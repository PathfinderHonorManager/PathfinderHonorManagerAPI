using Microsoft.EntityFrameworkCore;
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

        public MigrationService(IServiceProvider serviceProvider, ILogger<MigrationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database migration check...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PathfinderContext>();

            try
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Found {Count} pending migrations. Applying...", pendingMigrations.Count());
                    
                    foreach (var migration in pendingMigrations)
                    {
                        _logger.LogInformation("Pending migration: {Migration}", migration);
                    }

                    await context.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Database migrations completed successfully");
                }
                else
                {
                    _logger.LogInformation("Database is up-to-date, no migrations needed");
                }

                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
                _logger.LogInformation("Total applied migrations: {Count}", appliedMigrations.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply database migrations. Application startup will be aborted.");
                throw new InvalidOperationException("Database migration failed during application startup. See inner exception for details.", ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
