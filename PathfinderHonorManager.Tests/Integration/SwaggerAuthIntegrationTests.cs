using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PathfinderHonorManager.Tests.Integration
{
    [TestFixture]
    public class SwaggerAuthIntegrationTests
    {
        [Test]
        public async Task SwaggerDocument_SecuredEndpointsDeclareOAuth2()
        {
            using var factory = new IntegrationTestWebAppFactory();
            await factory.InitializeAsync();

            using var client = factory.CreateClient();
            var response = await client.GetAsync("/swagger/v1/swagger.json");

            Assert.That(response.IsSuccessStatusCode, Is.True, "Swagger document should be reachable.");

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.That(root.TryGetProperty("paths", out var paths), Is.True, "Swagger document should include paths.");
            Assert.That(paths.TryGetProperty("/api/Achievements", out var achievements), Is.True, "Swagger should include /api/Achievements.");
            Assert.That(achievements.TryGetProperty("get", out var getOperation), Is.True, "Swagger should include GET /api/Achievements.");
            Assert.That(getOperation.TryGetProperty("security", out var security), Is.True, "Secured operations should declare security.");
            Assert.That(security.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(security.GetArrayLength(), Is.GreaterThan(0));

            var requirement = security[0];
            Assert.That(requirement.TryGetProperty("oauth2", out var oauth2), Is.True, "Security requirement should reference oauth2.");
            Assert.That(oauth2.ValueKind, Is.EqualTo(JsonValueKind.Array));
        }
    }
}
