using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace PathfinderHonorManager.Tests.Controllers
{
    [TestFixture]
    public class HealthCheckControllerTests
    {
        [Test]
        public async Task HealthCheck_WhenHealthy_ReturnsOk()
        {
            await using var factory = new WebApplicationFactory<Startup>();
            var client = factory.CreateClient();
            var response = await client.GetAsync("/health");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
} 