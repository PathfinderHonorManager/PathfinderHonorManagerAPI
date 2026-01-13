# Repository Guidelines

## Project Structure & Module Organization
- `PathfinderHonorManager/` hosts the ASP.NET Core API. Key areas: `Controllers/`, `Service/`, `DataAccess/`, `Model/`, `Dto/`, `Validators/`, `Healthcheck/`, `Migrations/`, `Swagger/`, `Mapping/`, and `Converters/`.
- `PathfinderHonorManager.Tests/` contains NUnit tests plus test helpers and seeders.
- Configuration lives in `PathfinderHonorManager/appsettings.json`, `PathfinderHonorManager/appsettings.Development.json`, and `PathfinderHonorManager/Properties/launchSettings.json`.

## Build, Test, and Development Commands
- `dotnet build PathfinderHonorManager.sln` builds the solution.
- `dotnet run --project PathfinderHonorManager` runs the API locally.
- `dotnet test PathfinderHonorManager.Tests/PathfinderHonorManager.Tests.csproj` runs the test suite.
- `dotnet ef migrations add DescriptiveMigrationName --project PathfinderHonorManager` creates a migration.
- `dotnet ef database update --project PathfinderHonorManager` applies migrations (see `EF_MIGRATIONS_README.md` for baseline setup).

## Coding Style & Naming Conventions
- Follow C# conventions: 4-space indentation; PascalCase for types and public members; camelCase for locals and parameters.
- Interfaces use the `I*` prefix (for example, `IHonorService`).
- File/class naming mirrors feature type: `*Controller`, `*Service`, `*Validator`, `*Dto`.
- Validators use FluentValidation and live in `PathfinderHonorManager/Validators`.

## Testing Guidelines
- NUnit is the primary framework; tests live in `PathfinderHonorManager.Tests` and use `[Test]`/`[TestCase]`.
- Name test files and classes with a `*Tests` suffix (for example, `HonorsControllerTests.cs`).
- Prefer in-memory EF providers or seeded helpers (`Helpers/DatabaseSeeder.cs`) for data setup.

## Commit & Pull Request Guidelines
- Recent commits use short, imperative sentence-case summaries (for example, "Fix SonarQube issues", "Add background worker...").
- Keep commits focused on a single change set.
- PRs should include: a clear description, testing notes/commands run, migration impacts (if any), and any required configuration or env var changes.

## Configuration & Migrations
- Required env vars include `PathfinderCS` and Auth0 settings (`Auth0:Domain`, `Auth0:Audience`, `Auth0:ClientId`).
- The baseline migration is tracked in `PathfinderHonorManager/Migrations`; follow `EF_MIGRATIONS_README.md` for local setup and safe rollback steps.
- Health endpoints are available at `/health`, `/health/ready`, and `/health/live`.
