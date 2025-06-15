using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Healthcheck;

namespace PathfinderHonorManager.Tests.Healthcheck
{
    [TestFixture]
    public class PostgresHealthCheckTests
    {
        private string _connectionString;
        private DbContextOptions<PathfinderContext> _dbContextOptions;

        [SetUp]
        public void SetUp()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json")
                .Build();
            _connectionString = config.GetConnectionString("PathfinderCS");
            _dbContextOptions = new DbContextOptionsBuilder<PathfinderContext>()
                .UseNpgsql(_connectionString)
                .Options;
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
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);
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