using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Healthcheck;

namespace PathfinderHonorManager.Tests.Healthcheck
{
    [TestFixture]
    public class MigrationHealthCheckTests
    {
        [Test]
        public async Task CheckHealthAsync_PendingMigrations_ReturnsDegradedOrHealthy()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<PathfinderContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new PathfinderContext(options);
            var healthCheck = new MigrationHealthCheck(context);

            var result = await healthCheck.CheckHealthAsync(
                new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext(),
                CancellationToken.None);

            Assert.That(result.Status, Is.Not.EqualTo(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy));
        }

        [Test]
        public async Task CheckHealthAsync_DisposedContext_ReturnsUnhealthy()
        {
            var options = new DbContextOptionsBuilder<PathfinderContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            var context = new PathfinderContext(options);
            context.Dispose();

            var healthCheck = new MigrationHealthCheck(context);
            var result = await healthCheck.CheckHealthAsync(
                new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext(),
                CancellationToken.None);

            Assert.That(result.Status, Is.EqualTo(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy));
        }
    }
}
