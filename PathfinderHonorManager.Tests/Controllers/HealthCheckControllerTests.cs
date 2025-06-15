using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Healthcheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PathfinderHonorManager.Tests.Helpers;

namespace PathfinderHonorManager.Tests.Controllers
{
    [TestFixture]
    public class HealthCheckControllerTests
    {
        [Test]
        public async Task PostgresHealthCheck_ReturnsHealthy_WhenDbIsAccessible()
        {
            var options = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            using var context = new PathfinderContext(options);
            await DatabaseSeeder.SeedClubs(context);

            var healthCheck = new PostgresHealthCheck(context);
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));

            await DatabaseCleaner.CleanDatabase(context);
        }

        [Test]
        public async Task PostgresHealthCheck_ReturnsUnhealthy_WhenNoClubsExist()
        {
            var options = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            using var context = new PathfinderContext(options);
            await DatabaseCleaner.CleanDatabase(context);

            var healthCheck = new PostgresHealthCheck(context);
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        }
    }
} 