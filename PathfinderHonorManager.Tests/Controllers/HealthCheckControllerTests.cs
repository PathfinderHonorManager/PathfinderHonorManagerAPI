using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Net;
using PathfinderHonorManager.Healthcheck;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace PathfinderHonorManager.Tests.Controllers
{
    [TestFixture]
    public class HealthCheckControllerTests
    {
        [Test]
        public async Task HealthCheck_WhenDatabaseIsHealthy_ReturnsOk()
        {
            await using var factory = new WebApplicationFactory<Startup>();
            var client = factory.CreateClient();
            var response = await client.GetAsync("/health");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task HealthCheck_WhenDatabaseIsUnhealthy_ReturnsServiceUnavailable()
        {
            await using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            var response = await client.GetAsync("/health");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        }
    }

    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing PathfinderContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PathfinderContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Add a PathfinderContext with an invalid connection string
                services.AddDbContext<PathfinderContext>(options =>
                    options.UseNpgsql("Host=invalid;Port=5432;Database=test;Username=test;Password=test"));
            });
        }
    }
} 