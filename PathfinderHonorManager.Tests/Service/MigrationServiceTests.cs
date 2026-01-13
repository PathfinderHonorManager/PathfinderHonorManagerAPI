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
        public void StartAsync_MissingConnectionString_Throws()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            var logger = NullLogger<MigrationService>.Instance;
            var service = new MigrationService(services, logger, configuration);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                () => service.StartAsync(CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("Migration connection string not configured"));
        }

        [Test]
        public void StopAsync_ReturnsCompletedTask()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            var logger = NullLogger<MigrationService>.Instance;
            var service = new MigrationService(services, logger, configuration);

            Assert.DoesNotThrowAsync(() => service.StopAsync(CancellationToken.None));
        }
    }
}
