// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;

namespace Pondhawk.Context;

/// <summary>
/// A request-scoped property bag that flows through async execution without dependency injection
/// or a parameter threaded through every call.
/// </summary>
/// <remarks>
/// <para>
/// This is plumbing, not a contract. It deliberately ships no interfaces, no attributes and no
/// opinion about what belongs in it — tenant, subject and audit fields are application concerns,
/// and every application needs a different shape. Consumers build whatever typed view they want
/// over <see cref="Get{T}"/>, <see cref="Set{T}"/> and <see cref="SetShared"/>.
/// </para>
/// <para>
/// Values live in a scope (see <see cref="CreateScope"/>), which is both the lifetime boundary and
/// the storage anchor. The scope allocates a holder and the ambient slot carries a
/// <em>reference</em> to it, so a write is visible everywhere inside that scope — to callers, to
/// callees, and across parallel tasks. Storing the values directly in the ambient slot instead
/// would make a write visible only downward, and only until an <c>await</c> copied the execution
/// context, which is the kind of asymmetry nobody should have to debug.
/// </para>
/// <para>
/// Values are in-process by default. <see cref="SetShared"/> additionally writes to
/// <see cref="Activity"/> baggage, which travels: the runtime's propagator injects it into outbound
/// HTTP calls, so a shared value reaches downstream services — and any third party that is called.
/// Sharing is therefore opt-in per write, and should stay rare.
/// </para>
/// </remarks>
public static class AmbientContext
{
    /// <summary>
    /// The baggage key <c>Pondhawk.Logging</c> reads the correlation id from. It is a well-known
    /// string rather than a reference to that package's constant, because Core does not depend on
    /// it. See <see cref="CorrelationId"/>.
    /// </summary>
    public const string CorrelationKey = "pondhawk.correlation";

    private const string AnchorName = "Pondhawk.AmbientScope";

    private static readonly AsyncLocal<Holder?> Ambient = new();

    /// <summary>Whether a scope is currently open on this execution flow.</summary>
    public static bool IsActive => Ambient.Value is not null;

    /// <summary>
    /// The correlation id for this operation, shared so it reaches downstream services and the log
    /// pipeline. Reading falls back to baggage, so an id an upstream process sent is visible here
    /// even though nothing in this process set it.
    /// </summary>
    /// <remarks>
    /// A convenience over <see cref="CorrelationKey"/>. <c>Pondhawk.Logging</c>'s
    /// <c>CorrelationManager</c> reads the same key, so it consumes whatever is set here without
    /// knowing Core exists.
    /// </remarks>
    public static string? CorrelationId
    {
        get => Get<string>(CorrelationKey);
        set => SetShared(CorrelationKey, value);
    }

    /// <summary>
    /// Opens a scope, and closes it when the returned handle is disposed. Every operation that
    /// stores context needs one: a request, a queue message, a scheduled run, a test.
    /// </summary>
    /// <returns>A handle that restores the previous scope when disposed.</returns>
    /// <remarks>
    /// <para>
    /// Scopes nest. An inner scope starts from a snapshot of the outer one's values and reverts to
    /// it on dispose, so a temporary override cannot escape the block that made it. Without a
    /// boundary nothing is ever cleared, and a value left over from the last message is worse than
    /// a missing one — a plausible wrong tenant rather than an obvious null.
    /// </para>
    /// <para>
    /// The scope guarantees an <see cref="Activity"/> exists, starting one only when there is none,
    /// because baggage has nowhere to go without it. <see cref="Activity.Current"/> is null unless
    /// something is listening, so this is what stops a shared write from silently vanishing in a
    /// host with no telemetry configured.
    /// </para>
    /// <para>
    /// Shared values are the one thing a nested scope does <b>not</b> revert: they live on the
    /// activity, not in the holder, and once propagated they cannot be recalled. Set them at the
    /// edge, once.
    /// </para>
    /// </remarks>
    public static IDisposable CreateScope()
    {
        var previous = Ambient.Value;
        Activity? started = null;

        if (Activity.Current is null)
        {
            started = new Activity(AnchorName);
            started.Start();
        }

        var holder = new Holder { Anchor = Activity.Current };

        if (previous is not null)
        {
            holder.Values = previous.Values;
        }

        Ambient.Value = holder;

        var scope = new Scope(previous, started);

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
    /// <para>
    /// A miss falls back to baggage when <typeparamref name="T"/> is <see cref="string"/>, which is
    /// what surfaces a shared value an upstream process sent us.
    /// </para>
    /// </remarks>
    public static T? Get<T>(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var holder = Ambient.Value;

        if (holder is not null && holder.Values.TryGetValue(name, out var value) && value is not null)
        {
            if (value is not T stored)
            {
                throw new InvalidOperationException(
                    $"Ambient value '{name}' is a {value.GetType().Name}, and was read as a {typeof(T).Name}.");
            }

            return stored;
        }

        if (typeof(T) != typeof(string))
        {
            return default;
        }

        var inbound = Activity.Current?.GetBaggageItem(name);

        if (inbound is null)
        {
            return default;
        }

        var shared = (T)(object)inbound;

        return shared;
    }

    /// <summary>Stores a value for this process only, replacing anything already held under that name.</summary>
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

        var holder = Require(name);

        holder.Store(name, value);
    }

    /// <summary>
    /// Stores a value that travels: readable here, and carried to downstream services on outbound
    /// calls.
    /// </summary>
    /// <param name="name">The name to store it under.</param>
    /// <param name="value">The value. Baggage carries strings, so this is not generic.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or blank.</exception>
    /// <exception cref="InvalidOperationException">No scope is open.</exception>
    /// <remarks>
    /// The runtime's propagator injects baggage into every outbound HTTP call, including calls to
    /// third parties, and headers are size-limited. Share identifiers that a downstream service
    /// must agree with us about — a correlation id, a diagnostic flag — and keep subject and tenant
    /// in-process with <see cref="Set{T}"/>, where they cannot leak.
    /// </remarks>
    public static void SetShared(string name, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var holder = Require(name);

        holder.Store(name, value);
        holder.Anchor?.SetBaggage(name, value);
    }

    /// <summary>The open scope's holder, or an explanation of which write had nowhere to go.</summary>
    /// <param name="name">The name being written, for the message.</param>
    /// <returns>The holder.</returns>
    private static Holder Require(string name)
    {
        var holder = Ambient.Value;

        if (holder is null)
        {
            throw new InvalidOperationException(
                $"No ambient scope is open, so '{name}' has nowhere to go. Open one with AmbientContext.CreateScope().");
        }

        return holder;
    }

    /// <summary>
    /// The mutable anchor a scope hands out. Shared by reference so a write inside the scope is
    /// seen by the whole scope, rather than only by the flow that made it.
    /// </summary>
    private sealed class Holder
    {
        private ImmutableDictionary<string, object?> _values = ImmutableDictionary<string, object?>.Empty;

        /// <summary>
        /// The activity shared writes land on — the one current when the scope opened. Writing to
        /// <see cref="Activity.Current"/> instead would put baggage on whatever child span happens
        /// to be running, and lose it when that span ends.
        /// </summary>
        public Activity? Anchor { get; init; }

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
    /// <param name="started">The activity this scope started, or null when it adopted an existing one.</param>
    private sealed class Scope(Holder? previous, Activity? started) : IDisposable
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

            if (started is null)
            {
                return;
            }

            started.Stop();
            started.Dispose();
        }
    }
}
