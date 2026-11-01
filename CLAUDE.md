# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore, build, and test via the Cake build script
dotnet run --project build/Build.csproj -- --target=Test

# Run tests directly
dotnet test pondhawk-utilities.slnx

# Pack NuGet packages (writes to ./artifacts)
dotnet run --project build/Build.csproj -- --target=Pack --build-number=<n>
```

## Project Setup

- `net10.0`, nullable enabled in `src`, central package management, `TreatWarningsAsErrors` and Meziantou analyzer in `src`.
- `version.json` holds `major.minor`; the Cake `Pack` target appends the build number.
- `build.yml` pushes dev packages to GitHub Packages. `publish.yml` is a manual promotion to nuget.org with Trusted Publishing.

## Architecture

One package, `Pondhawk.Utilities`, with all namespaces under `Pondhawk.Utilities`:

- **Configuration**: `IServiceModule` properties bind from `IConfiguration`, then `Build()` registers services. `AddServiceModule<TModule>(configuration)` does both.
- **Hosting**: `AddSingletonWithStart<TService>` registers a singleton with start and optional stop lambdas. `ServiceStarterHostedService` runs starts on host startup and stops in reverse order. `AppLifecycleService` does file-based lifecycle signaling (`started.flag`, `stopped.flag`, `muststop.flag`).
- **Pipeline**: composable step-based execution with Before/After hooks.
- **Types**: date range helpers, `TypeSource`, `TypeExtensions`, `AssemblyExtensions`.

## Related Repositories

- [pondhawk-mediator](https://github.com/pondhawktech/pondhawk-mediator): `Pondhawk.Mediator`, `Pondhawk.Exceptions`.
- This repo was `tools`. The tag `api-final` holds the last `Pondhawk.Core`, `Pondhawk.Hosting` and `Pondhawk.Api`, including `docs/`.
