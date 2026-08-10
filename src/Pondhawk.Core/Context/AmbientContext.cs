// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Immutable;

namespace Pondhawk.Context;

/// <summary>
/// A request-scoped property bag that flows through async execution without dependency injection
/// or a parameter threaded through every call.
/// </summary>
/// <remarks>
/// <para>
/// This is plumbing, not a contract. It deliberately ships no interfaces, no attributes and no
/// opinion about what belongs in it — tenant, subject, feature flags and audit fields are
/// application concerns, and every application needs a different shape. Consumers build whatever
/// typed view they want over <see cref="Get{T}"/> and <see cref="Set{T}"/>.
/// </para>
/// <para>
/// Values live in a scope (see <see cref="CreateScope"/>), which is both the lifetime boundary and
/// the storage anchor. The scope allocates a holder and the ambient slot carries a
/// <em>reference</em> to it, so a write is visible everywhere inside that scope — to callers, to
/// callees, and across parallel tasks. Storing the values directly in the ambient slot instead
/// would make a write visible only downward, and only until an <c>await</c> copied the execution
/// context, which is the kind of asymmetry nobody should have to debug.
/// </para>
/// </remarks>
public static class AmbientContext
{
    private static readonly AsyncLocal<Holder?> Ambient = new();

    /// <summary>Whether a scope is currently open on this execution flow.</summary>
    public static bool IsActive => Ambient.Value is not null;

    /// <summary>
    /// Opens a scope, and closes it when the returned handle is disposed. Every operation that
    /// stores context needs one: a request, a queue message, a scheduled run, a test.
    /// </summary>
    /// <returns>A handle that restores the previous scope when disposed.</returns>
    /// <remarks>
    /// Scopes nest. An inner scope starts from a snapshot of the outer one's values and reverts to
    /// it on dispose, so a temporary override cannot escape the block that made it. Without a
    /// boundary nothing is ever cleared, and a value left over from the last message is worse than
    /// a missing one — a plausible wrong tenant rather than an obvious null.
    /// </remarks>
    public static IDisposable CreateScope()
    {
        var previous = Ambient.Value;
        var holder = new Holder();

        if (previous is not null)
        {
            holder.Values = previous.Values;
        }

        Ambient.Value = holder;

        var scope = new Scope(previous);

        return scope;
    }

    /// <summary>Reads a value, or <see langword="default"/> when it is absent.</summary>
    /// <typeparam name="T">The type the value was stored as.</typeparam>
    /// <param name="name">The name it was stored under.</param>
    /// <returns>The value, or <see langword="default"/> when no scope is open or nothing is stored.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or blank.</exception>
    /// <exception cref="InvalidOperationException">A value is present but is not a <typeparamref name="T"/>.</exception>
    /// <remarks>
    /// Reading outside a scope yields <see langword="default"/> rather than throwing: library code
    /// legitimately runs both inside a request and outside one, in a console host or a unit test,
    /// and should not have to ask which. Writing outside a scope does throw — see
    /// <see cref="Set{T}"/>.
    /// </remarks>
    public static T? Get<T>(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var holder = Ambient.Value;

        if (holder is null || !holder.Values.TryGetValue(name, out var value) || value is null)
        {
            return default;
        }

        if (value is not T typed)
        {
            throw new InvalidOperationException(
                $"Ambient value '{name}' is a {value.GetType().Name}, and was read as a {typeof(T).Name}.");
        }

        return typed;
    }

    /// <summary>Stores a value, replacing anything already held under that name.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="name">The name to store it under.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or blank.</exception>
    /// <exception cref="InvalidOperationException">No scope is open.</exception>
    /// <remarks>
    /// Overwriting is allowed — whether a value may change mid-operation is the application's
    /// policy to enforce, not this store's. Writing with no scope open throws rather than silently
    /// discarding the value, because it always means the host never opened one, and a context that
    /// quietly drops what it is given is how a tenant goes missing.
    /// </remarks>
    public static void Set<T>(string name, T? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var holder = Ambient.Value;

        if (holder is null)
        {
            throw new InvalidOperationException(
                $"No ambient scope is open, so '{name}' has nowhere to go. Open one with AmbientContext.CreateScope().");
        }

        holder.Store(name, value);
    }

    /// <summary>
    /// The mutable anchor a scope hands out. Shared by reference so a write inside the scope is
    /// seen by the whole scope, rather than only by the flow that made it.
    /// </summary>
    private sealed class Holder
    {
        private ImmutableDictionary<string, object?> _values = ImmutableDictionary<string, object?>.Empty;

        /// <summary>The values held, as an immutable snapshot that readers can use without locking.</summary>
        public ImmutableDictionary<string, object?> Values
        {
            get => Volatile.Read(ref _values);
            set => Volatile.Write(ref _values, value);
        }

        /// <summary>Stores a value, retrying if a parallel task swapped the snapshot underneath.</summary>
        /// <param name="name">The name to store it under.</param>
        /// <param name="value">The value.</param>
        public void Store(string name, object? value)
        {
            while (true)
            {
                var before = Volatile.Read(ref _values);
                var after = before.SetItem(name, value);

                if (Interlocked.CompareExchange(ref _values, after, before) == before)
                {
                    return;
                }
            }
        }
    }

    /// <summary>The handle returned by <see cref="CreateScope"/>.</summary>
    /// <param name="previous">The scope to restore on dispose, or null when this is the outermost.</param>
    private sealed class Scope(Holder? previous) : IDisposable
    {
        private bool _disposed;

        /// <summary>Restores the scope that was open before this one. Disposing twice is harmless.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Ambient.Value = previous;
        }
    }
}
