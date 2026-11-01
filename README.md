<p align="center">
  <img src="pht-small-logo.png" alt="Pondhawk.Utilities" width="120" />
</p>

<h1 align="center">Pondhawk.Utilities</h1>

<p align="center">
  Small shared helpers for Pondhawk applications.
</p>

| Namespace | Purpose |
|---|---|
| `Pondhawk.Utilities.Configuration` | `IServiceModule`: DI modules bound from `IConfiguration` |
| `Pondhawk.Utilities.Hosting` | `AddSingletonWithStart`, `AppLifecycleService` |
| `Pondhawk.Utilities.Pipeline` | Composable step-based execution |
| `Pondhawk.Utilities.Types` | Date ranges, type and assembly helpers |

See the [package README](src/Pondhawk.Utilities/README.md). The package targets `net10.0`.

## Migrate from Pondhawk.Core or Pondhawk.Hosting

1. Replace the package reference with `Pondhawk.Utilities`.
2. Change `using Pondhawk.Configuration;` to `using Pondhawk.Utilities.Configuration;`.
3. Change `using Pondhawk.Hosting;` to `using Pondhawk.Utilities.Hosting;`.
4. For the mediator and exception types, use [pondhawk-mediator](https://github.com/pondhawktech/pondhawk-mediator).

Do not reference `Pondhawk.Core` in the same project: the duplicate types cause CS0433.

## Retired code

This repo was `pondhawktech/tools`. The tag `api-final` holds the last version of `Pondhawk.Core`, `Pondhawk.Hosting` and `Pondhawk.Api`.

## Build

```bash
dotnet run --project build/Build.csproj -- --target=Test
```
