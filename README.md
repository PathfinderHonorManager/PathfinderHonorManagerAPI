# Pathfinder Honor Manager API

ASP.NET Core API for managing Pathfinder clubs, pathfinders, honors, achievements, and related status workflows. The solution targets .NET 10 and uses EF Core with PostgreSQL.

## Project Layout
- `PathfinderHonorManager/`: API project (controllers, services, EF Core, DTOs, validators, health checks).
- `PathfinderHonorManager.Tests/`: NUnit test project with in-memory EF helpers and controller/service tests.
- `PathfinderHonorManager/Pathfinder-DB/`: database scripts.
- `EF_MIGRATIONS_README.md`: migration workflow and baseline setup details.

## Prerequisites
- .NET SDK 10.0
- PostgreSQL (for local database work)

## Configuration
Set environment variables or user secrets as needed:
- `PathfinderCS` (connection string)
- `Auth0:Domain`
- `Auth0:Audience`
- `Auth0:ClientId`

Example local connection string:
```
Host=localhost;Database=pathfinder;Username=dbuser;Password=yourpassword
```

## Build, Run, Test
- Build: `dotnet build PathfinderHonorManager.sln`
- Run: `dotnet run --project PathfinderHonorManager`
- Test: `dotnet test PathfinderHonorManager.Tests/PathfinderHonorManager.Tests.csproj`

## EF Core Migrations
This repo uses a baseline migration for existing databases. See `EF_MIGRATIONS_README.md` for the full workflow. Common commands:
- List migrations: `dotnet ef migrations list --project PathfinderHonorManager`
- Add migration: `dotnet ef migrations add DescriptiveMigrationName --project PathfinderHonorManager`
- Apply migration: `dotnet ef database update --project PathfinderHonorManager`

## Health Endpoints
- `/health`
- `/health/ready`
- `/health/live`

## Notes
- Tests use NUnit (`[Test]`, `[TestCase]`) and EF Core in-memory/SQLite helpers.
- Validators use FluentValidation (`PathfinderHonorManager/Validators`).
