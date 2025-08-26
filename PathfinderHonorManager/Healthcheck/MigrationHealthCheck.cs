using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PathfinderHonorManager.DataAccess;

namespace PathfinderHonorManager.Healthcheck
{
    public class MigrationHealthCheck : IHealthCheck
    {
        private readonly PathfinderContext _context;
        
        public MigrationHealthCheck(PathfinderContext context)
        {
            _context = context;
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
                
                if (pendingMigrations.Any())
                {
                    return HealthCheckResult.Degraded(
                        "Database has pending migrations",
                        data: new Dictionary<string, object>
                        {
                            ["PendingMigrations"] = pendingMigrations.ToArray(),
                            ["PendingCount"] = pendingMigrations.Count()
                        });
                }
                
                return HealthCheckResult.Healthy("Database schema is up-to-date");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Failed to check migration status", ex);
            }
        }
    }
} 