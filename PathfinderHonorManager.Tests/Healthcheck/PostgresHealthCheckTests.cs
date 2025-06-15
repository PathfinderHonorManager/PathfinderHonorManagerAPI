using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Healthcheck;
using PathfinderHonorManager.Tests.Helpers;

namespace PathfinderHonorManager.Tests.Healthcheck
{
    [TestFixture]
    public class PostgresHealthCheckTests
    {
        private DbContextOptions<PathfinderContext> _dbContextOptions;

        [SetUp]
        public void SetUp()
        {
            _dbContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            using var dbContext = new PathfinderContext(_dbContextOptions);
            DatabaseCleaner.CleanDatabase(dbContext).Wait();
            DatabaseSeeder.SeedDatabase(_dbContextOptions).Wait();
        }

        [Test]
        public async Task CheckHealthAsync_WhenDatabaseIsHealthy_ReturnsHealthy()
        {
            using var dbContext = new PathfinderContext(_dbContextOptions);
            var healthCheck = new PostgresHealthCheck(dbContext);
            var context = new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, null)
            };
            var result = await healthCheck.CheckHealthAsync(context, System.Threading.CancellationToken.None);
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
        }

        [Test]
        public async Task CheckHealthAsync_WhenDatabaseIsUnreachable_ReturnsUnhealthy()
        {
            var badOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseNpgsql("Host=invalid;Port=5432;Database=test;Username=test;Password=test")
                .Options;
            using var dbContext = new PathfinderContext(badOptions);
            var healthCheck = new PostgresHealthCheck(dbContext);
            var context = new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, null)
            };
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        }
    }
} 