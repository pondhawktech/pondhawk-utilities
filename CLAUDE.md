# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build pondhawk-tools.slnx

# Build a specific project
dotnet build src/Pondhawk.Core/Pondhawk.Core.csproj
dotnet build src/Pondhawk.Hosting/Pondhawk.Hosting.csproj
dotnet build src/Pondhawk.Api/Pondhawk.Api.csproj
```

## Project Setup

- **.NET 10** targeting `net10.0` (SDK 10.0.103)
- **Central package management** via `Directory.Packages.props`
- **Nullable reference types** enabled in Core, Watch, and Hosting projects
## Architecture

This repository contains class libraries under `src/` that form the **Pondhawk** toolkit by Pond Hawk Technologies:

### Pondhawk.Core — Shared Foundation

Core utilities, pipeline infrastructure, mediator, and common exception types. Key subsystems:

- **Mediator** (`Pondhawk.Mediator`): Lightweight mediator pattern implementation for CQRS-style request/response dispatch with pipeline behaviors.
  - `IMediator` / `Mediator` — routes requests through pipeline behaviors to handlers. Uses cached handler wrappers to avoid reflection on every request. `SendAsync` returns a `Response<TResponse>` envelope (it is the single seam that converts a thrown error into a structured failure). The mediator is request/response only — exactly one handler per request. There is deliberately no notification/publish (one-to-many broadcast) channel; add one only when a real need for domain events arises, designing fan-out and error-aggregation semantics explicitly at that point.
  - `Response<T>` — success/failure envelope (`Ok`/`Value`/`Error`, plus `IsError`). The mediator maps a thrown `ExternalException` to `Response.Failure` with an `ErrorInfo` (preserving `ErrorKind`); unexpected exceptions become `ErrorKind.System` and are logged at `Error`; `OperationCanceledException` and missing-handler configuration errors still throw. Handlers/behaviors are unchanged — they keep returning `Task<TResponse>` and throwing. Ergonomic accessors for the app-side CRUD framework: `AsEntity` (success value, alias of `GetValueOrThrow`), `AsError` (the `ErrorInfo` to propagate; throws on success), and an implicit `ErrorInfo → Response<T>` conversion. Together these enable cross-type error propagation — `return source.AsError;` re-raises a failure from a `Response<X>` into a differently-typed `Response<T>` carrying the same `ErrorInfo`. There is deliberately no non-generic `Response`. Because `Response<T>` is a value type, `default(Response<T>)` (an uninitialized field, an unpopulated array slot, a dictionary miss) reads as a coherent `ErrorKind.System` failure rather than a success carrying a null value — the `Error` accessor coalesces a null-backed failure to a synthetic "not initialized" error, so the "error non-null iff not Ok" invariant holds on read and `Match`/`AsError`/`GetValueOrThrow` never NRE. `Response<T>` also implements a non-generic `IResponse` (`Ok`/`ErrorInfo? Error`/`object? Value`) — the type-erased view the ASP.NET layer (`Pondhawk.Api`) reads to render an envelope to `Ok`/JSON or ProblemDetails without knowing `T`.
  - `Receipt` — the payload a mutation returns when it has no entity to hand back (a delete/bulk command uses `Response<Receipt>`). Carries `Affected` (entities affected: 1 for a single command, the real count for bulk, 0 = no-op). Factories: `Receipt.One` and `Receipt.Of(int)`.
  - `ErrorInfo` (`Pondhawk.Exceptions`) — transport-agnostic error shape shared by exceptions and the envelope. `ErrorKindPolicy.IsTransient(ErrorKind)` is the canonical retry/dead-letter default (`System`/`Remote`/`Concurrency`). `NotFoundException`/`ConflictException`/`NotAuthorizedException` are common `FluentException<T>` subclasses fixing their `Kind`. Note: the HTTP `ErrorKind → status` mapping deliberately lives in the ASP.NET layer, not Core.
  - `IRequest<TResponse>` — unified marker interface. `ICommand<TResponse>` and `IQuery<TResponse>` are convenience aliases preserving semantic intent.
  - `IRequestHandler<TRequest, TResponse>` — handler interface. `ICommandHandler` and `IQueryHandler` are convenience aliases.
  - `IPipelineBehavior<TRequest, TResponse>` — cross-cutting concerns (logging, validation) via delegate chain wrapping.
  - `ServiceCollectionExtensions.AddMediator(assemblies)` — registers mediator + auto-discovers handlers from assemblies. A single class that implements `IRequestHandler<,>` more than once is registered for **every** request type it handles, not just the first discovered. Discovery is resilient to unloadable types: a `ReflectionTypeLoadException` from an assembly (a missing optional dependency, a version mismatch) is caught, the loadable types are salvaged, and the skipped ones are reported via a `Warning` — a broken type no longer aborts startup, but such a handler is silently skipped and only surfaces as "No handler registered" at send time (the warning is the breadcrumb). An `AddMediator(ILoggerFactory?, assemblies)` overload supplies the factory for that warning (falls back to `NullLoggerFactory` when null). `AddPipelineBehavior(type)` registers open-generic behaviors.
  - `BatchExecutionContext` — `AsyncLocal`-based batch state tracking (nesting depth, batch ID). `BeginBatch(id)` returns disposable scope.
  - `BatchCommandResult` — type-erased result wrapper for heterogeneous batch command responses. Failures carry an `ErrorInfo` (with `ErrorKind`) via `Failed(commandType, entityUid, ErrorInfo)`; `ErrorMessage` is derived from `ErrorInfo.Explanation`, so the error kind survives a batch.
- **Configuration** (`Pondhawk.Configuration`): Configuration-driven DI module pattern.
  - `IServiceModule` — interface for modules that bundle related service registrations. Properties are populated via `IConfiguration` model binding, then `Build()` registers services.
  - `ServiceModuleExtensions.AddServiceModule<TModule>(configuration)` — binds a module from config and calls `Build()`. Overload accepts `Action<TModule>` for post-binding overrides.
- **Utilities**: Pipeline infrastructure (`IServiceCollection`-based DI registration), type extensions, date/time helpers.
- **Exceptions**: Common exception types.

### Pondhawk.Api — ASP.NET Core web kit (references Pondhawk.Core + the Pondhawk.Logging & Pondhawk.Rules NuGet packages)

The ASP.NET Core web-app kit for Pondhawk Tools (replaces AppCommon.Api). Targets `net10.0` with a `FrameworkReference` to `Microsoft.AspNetCore.App`. References Pondhawk.Core (mediator + `Response<T>`/`IResponse`) as a project, and the **Pondhawk.Logging** (structured logging, from [pondhawktech/pondhawk-logging](https://github.com/pondhawktech/pondhawk-logging)) and **Pondhawk.Rules** (validation, from [pondhawktech/pondhawk-rules](https://github.com/pondhawktech/pondhawk-rules)) NuGet packages. Organized by concern:

- **Context**: `IRequestContext` (`CorrelationId`, `Caller` ClaimsPrincipal, `CallerGatewayToken`). The default `RequestContext` owns a stable correlation id (explicit > ambient `CorrelationManager.Current` > generated). Registered by `AddPondhawkApi()` alongside `IHttpContextAccessor`.
- **Endpoints**: `IEndpointModule` (`BasePath`, `Configure(RouteGroupBuilder)`, `AddRoutes(IEndpointRouteBuilder)`) — self-contained minimal-API route groups. `AddEndpointModules(assemblies)` discovers and DI-registers modules; `MapEndpointModules(basePath)` maps each module's group. Mapping is resilient — a module that throws is logged and skipped so one bad module cannot break startup.
- **Filters**: `ResponseEndpointFilter` renders a handler's `Response<T>` (via the non-generic `IResponse`) to `Ok`/JSON/stream on success or a `ProblemDetail` (`application/problem+json`) on failure, with the `ErrorKind → HTTP status` map (see below). `ApiKeyEndpointFilter` + `IApiKeyValidator`/`SimpleApiKeyValidator` do constant-time `x-api-key` checks.
- **Exceptions**: `ApiExceptionHandler` (`IExceptionHandler`) converts unhandled exceptions to `ProblemDetail` (ExternalException keeps its kind, `JsonException` → 400, else System/500).
- **Json**: `CompactJsonTypeInfoResolver` (omits empty strings/collections/zeros/min-dates), `PascalJsonNamingPolicy`, and `AddPondhawkJson(configure)` (Microsoft.Extensions.DI — no Autofac).
- **Behaviors** (mediator `IPipelineBehavior`s): `ValidationBehavior<TReq,TResp>` runs Pondhawk.Rules `IRuleSet.TryValidate` over the request; violations become a `FailedValidationException` the mediator envelopes as `Predicate`, which the filter renders as 422. `LoggingBehavior<TReq,TResp>` does request logging via Pondhawk.Logging with correlation.
- **Identity**: gateway authentication — ONE scheme (`IdentityConstants.Scheme`) with TWO inbound handlers. **Token mode** (`AddGatewayTokenAuthentication(base64Key)`) validates an HS256 JWT in `X-Gateway-Identity-Token` via `Microsoft.IdentityModel.JsonWebTokens`. **Header mode** (`AddGatewayHeaderAuthentication()`) reads an unsigned JSON claim set from `X-Gateway-Identity`. Minimal `ClaimSet` (UserId, UserName, FirstName, LastName, Email, Roles) with `ClaimSetPrincipal` claim↔principal mapping and `IGatewayTokenEncoder`/`GatewayTokenJwtEncoder` (HS256). This is inbound authentication only — there is deliberately no outbound/client token propagation (that HTTP-client concern would live in a future `Pondhawk.Identity`).
- **Middleware**: `DiagnosticsEnrichmentMiddleware` (populates `IRequestContext` — correlation/caller/token; register after auth), `DiagnosticsMonitorMiddleware` (register EARLY: whole-pipeline begin/end + elapsed, catches short-circuited requests), and `RequestLoggingMiddleware` (register DEEP: full request dump — method/path/query/redacted-headers/buffered-body — when the `Pondhawk.Diagnostics.Http` category is debug-enabled). The two diagnostics middlewares are complementary by pipeline position, not redundant. Registered via `UseDiagnosticsEnrichment`/`UseDiagnosticsMonitor`/`UseRequestLogging`.
- **Root**: `AddPondhawkApi()` registers `IHttpContextAccessor` + a scoped `IRequestContext`; the other concerns are opted into by their own extensions.

**ErrorKind → HTTP status** (shared by the response filter and the exception handler): None→200, NotFound→404, NotImplemented→501, Predicate→422, Conflict→409, Concurrency→410, BadRequest→400, AuthenticationRequired→401, NotAuthorized→403, Remote→502, System/Functional/Unknown→500.

### Pondhawk.Hosting — Service Startup Extensions for Generic Host

Lightweight service lifecycle management for `Microsoft.Extensions.Hosting`. Standalone — no dependency on any other Pondhawk project. Only depends on `Microsoft.Extensions.Hosting.Abstractions`.

- **`AddSingletonWithStart<TService>(startAction)`** — registers a singleton and a start lambda, co-located at the registration site. Supports sync, async, and optional stop lambdas:
  ```csharp
  services.AddSingletonWithStart<RuleSetFactory>(f => f.Start());
  services.AddSingletonWithStart<RuleSetFactory>(f => f.Start(), f => f.Stop());
  services.AddSingletonWithStart<MyService>((svc, ct) => svc.InitAsync(ct), (svc, ct) => svc.StopAsync(ct));
  ```
- **ServiceStarterHostedService**: `IHostedService` that resolves all registered descriptors and calls start lambdas on host startup, stop lambdas in reverse order on shutdown. Auto-registered via `TryAddEnumerable` — only one instance regardless of how many services are registered. Logs each service start/stop via `[LoggerMessage]` source-generated methods.
- **AppLifecycleService**: `IHostedService` providing file-based lifecycle signaling against a flag directory (defaults to `AppContext.BaseDirectory`). On start it clears stale flags, then writes `started.flag` / `stopped.flag` from the `IHostApplicationLifetime` callbacks, and uses a `FileSystemWatcher` to watch for an externally-created `muststop.flag` — which triggers a graceful `StopApplication()`. Useful for out-of-band shutdown signaling (e.g. orchestrators dropping a flag file). Logs via a source-generated `[LoggerMessage]`.
- Falls back to `NullLoggerFactory` when `AddLogging()` hasn't been called (e.g. in test scenarios).

### Dependency Graph

```
Pondhawk.Core          (foundation — mediator, pipeline infrastructure, utilities, exceptions)
Pondhawk.Hosting  (standalone)
Pondhawk.Api  ──→ Pondhawk.Core (+ external Pondhawk.Logging & Pondhawk.Rules NuGet packages)  (ASP.NET web kit — endpoint modules, Response<T>→ProblemDetails, gateway identity, diagnostics)
```

## Related Repository

**[pondhawk/watch-server](https://github.com/pondhawk/watch-server)** (`E:\repository\watch-server`) — The Pondhawk Watch Server, a log event aggregation server with ASP.NET Core Web API, SQLite storage, React UI, and Winston transport for Node.js. Consumes the `Pondhawk.Logging.Watch` NuGet package (now from [pondhawktech/pondhawk-logging](https://github.com/pondhawktech/pondhawk-logging)) as its client-side logging sink.

**[pondhawktech/pondhawk-logging](https://github.com/pondhawktech/pondhawk-logging)** — The structured logging API (`Pondhawk.Logging`) and its Watch Server provider (`Pondhawk.Logging.Watch`), extracted from this repo and published to NuGet.org. `Pondhawk.Api` consumes `Pondhawk.Logging` from this package.

**[pondhawktech/pondhawk-rql](https://github.com/pondhawktech/pondhawk-rql)** — The Resource Query Language (RQL) implementation, extracted from this repo into its own standalone repository and published to NuGet.org as `Pondhawk.Rql`. It had no internal dependents in this solution.

**[pondhawktech/pondhawk-rules](https://github.com/pondhawktech/pondhawk-rules)** — The rule engine (`Pondhawk.Rules`) and its EF Core SaveChanges adapter (`Pondhawk.Rules.EFCore`), extracted from this repo and published to NuGet.org. `Pondhawk.Api` consumes `Pondhawk.Rules` from this package.

## Conventions

- Namespaces match project/folder structure: `Pondhawk.Mediator`, `Pondhawk.Configuration`, `Pondhawk.Hosting`
- Exception: `Pondhawk.Core` project uses `RootNamespace=Pondhawk`
- `LangVersion` varies: `default` in Hosting, `latest` in Watch
