using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PathfinderHonorManager.Service;

namespace PathfinderHonorManager.Tests.Service
{
    [TestFixture]
    public class MigrationServiceTests
    {
        [Test]
        public async Task StartAsync_MissingConnectionString_Throws()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            var logger = NullLogger<MigrationService>.Instance;
            var service = new MigrationService(services, logger, configuration);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.StartAsync(CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("Migration connection string not configured"));
        }

        [Test]
        public async Task StopAsync_ReturnsCompletedTask()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            var logger = NullLogger<MigrationService>.Instance;
            var service = new MigrationService(services, logger, configuration);

            await Assert.DoesNotThrowAsync(() => service.StopAsync(CancellationToken.None));
        }

        [Test]
        public async Task StartAsync_InvalidDatabaseConnection_ThrowsWrappedInvalidOperationException()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:PathfinderMigrationCS"] =
                        "Host=127.0.0.1;Port=1;Database=testdb;Username=test;Password=test;Timeout=1;Command Timeout=1"
                })
                .Build();
            var logger = NullLogger<MigrationService>.Instance;
            var service = new MigrationService(services, logger, configuration);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.StartAsync(CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("Database migration failed during application startup"));
            Assert.That(ex.InnerException, Is.Not.Null);
        }
    }
}
