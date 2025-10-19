using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PathfinderHonorManager.DataAccess;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PathfinderHonorManager.Service
{
    public class MigrationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationService> _logger;
        private readonly IConfiguration _configuration;

        public MigrationService(IServiceProvider serviceProvider, ILogger<MigrationService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database migration check...");

            using var scope = _serviceProvider.CreateScope();
            
            var migrationConnectionString = _configuration.GetConnectionString("PathfinderMigrationCS");
            var optionsBuilder = new DbContextOptionsBuilder<PathfinderContext>();
            optionsBuilder.UseNpgsql(migrationConnectionString, 
                npgsqlOptions => 
                {
                    npgsqlOptions.CommandTimeout(60);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
            
            var context = new PathfinderContext(optionsBuilder.Options);

            try
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(migrationConnectionString);
                _logger.LogDebug("Connecting to database for migrations: Host={Host}, Database={Database}, Username={Username}", 
                    builder.Host, builder.Database, builder.Username);
                
                var canConnect = await context.Database.CanConnectAsync(cancellationToken);
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database. Check connection string and database availability.");
                    throw new InvalidOperationException("Database connection failed");
                }

                _logger.LogInformation("Database connection verified. Checking migration status...");

                // Check if we have an existing database without migration history (baseline scenario)
                if (await NeedsBaselineAsync(context, cancellationToken))
                {
                    _logger.LogInformation("Existing database detected without migration history. Creating baseline...");
                    await CreateBaselineAsync(context, cancellationToken);
                }

                // Now apply any pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Found {Count} pending migrations. Applying...", pendingMigrations.Count());
                    foreach (var migration in pendingMigrations)
                    {
                        _logger.LogInformation("Applying migration: {Migration}", migration);
                    }
                    
                    await context.Database.MigrateAsync(cancellationToken);
                }
                else
                {
                    _logger.LogInformation("No pending migrations found.");
                }
                
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
                _logger.LogInformation("Database migrations completed. Total applied: {Count}", appliedMigrations.Count());
                
                if (appliedMigrations.Any())
                {
                    _logger.LogInformation("Latest migration: {Migration}", appliedMigrations.Last());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply database migrations. Application startup will be aborted.");
                throw new InvalidOperationException("Database migration failed during application startup. See inner exception for details.", ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<bool> NeedsBaselineAsync(PathfinderContext context, CancellationToken cancellationToken)
        {
            try
            {
                // First check if our application tables exist (pathfinder, club, honor)
                // If they don't exist, this is a fresh database - no baseline needed
                try
                {
                    await context.Pathfinders.AnyAsync(cancellationToken);
                    await context.Clubs.AnyAsync(cancellationToken);
                    await context.Honors.AnyAsync(cancellationToken);
                }
                catch
                {
                    // Tables don't exist, this is a fresh database
                    _logger.LogInformation("Application tables not found - fresh database, no baseline needed");
                    return false;
                }

                // Tables exist, now check if migration history exists
                try
                {
                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
                    var hasHistory = appliedMigrations.Any();
                    
                    if (!hasHistory)
                    {
                        _logger.LogInformation("Application tables exist but no migration history - baseline needed");
                        return true;
                    }
                    
                    return false;
                }
                catch
                {
                    // Migration table doesn't exist, need baseline
                    _logger.LogInformation("Cannot read migration history - baseline needed");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking baseline status, assuming no baseline needed");
                return false;
            }
        }

        private async Task CreateBaselineAsync(PathfinderContext context, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating migrations table and baseline entries...");

                // Create the migrations table
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                        ""MigrationId"" character varying(150) NOT NULL,
                        ""ProductVersion"" character varying(32) NOT NULL,
                        CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                    )
                ");

                // Add baseline migration entry for existing schema
                // Since the production database already has all the schema we need,
                // we just mark the initial migration as applied
                await context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                    VALUES ('20250826224824_InitialSchemaWithProperDeleteBehavior', '9.0.8')
                    ON CONFLICT (""MigrationId"") DO NOTHING
                ");

                _logger.LogInformation("Baseline migration history created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create baseline migration history");
                throw;
            }
        }
    }
}
