using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PathfinderHonorManager.Tests.Integration
{
    public class TestAuthOptions : AuthenticationSchemeOptions
    {
        public IReadOnlyCollection<string> Permissions { get; set; } = TestAuthHandler.DefaultPermissions;
    }

    public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
    {
        public const string SchemeName = "TestAuth";
        public const string Issuer = "https://test/";
        public const string ClubCode = "VALIDCLUBCODE";
        public static readonly IReadOnlyCollection<string> DefaultPermissions = new[]
        {
            "read:clubs",
            "create:clubs",
            "update:clubs",
            "read:pathfinders",
            "create:pathfinders",
            "update:pathfinders",
            "read:honors",
            "create:honors",
            "update:honors"
        };

        public TestAuthHandler(
            IOptionsMonitor<TestAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            // TODO: Switch to TimeProvider when AuthenticationHandler supports it for this target framework.
#pragma warning disable CS0618
            ISystemClock clock)
            : base(options, logger, encoder, clock)
#pragma warning restore CS0618
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim> { new Claim("clubCode", ClubCode) };

            var permissions = Options?.Permissions ?? DefaultPermissions;
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permissions", permission, ClaimValueTypes.String, Issuer));
            }

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
