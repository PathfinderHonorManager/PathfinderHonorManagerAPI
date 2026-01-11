using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NUnit.Framework;
using PathfinderHonorManager.Auth;

namespace PathfinderHonorManager.Tests.Auth
{
    [TestFixture]
    public class HasScopeHandlerTests
    {
        [Test]
        public async Task HandleAsync_NoPermissionsClaim_DoesNotSucceed()
        {
            var requirement = new HasScopeRequirement("read:honors", "https://issuer/");
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
            var handler = new HasScopeHandler();

            await handler.HandleAsync(context);

            Assert.That(context.HasSucceeded, Is.False);
        }

        [Test]
        public async Task HandleAsync_PermissionsClaimWithMatchingScope_Succeeds()
        {
            var requirement = new HasScopeRequirement("read:honors", "https://issuer/");
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("permissions", "read:honors", ClaimValueTypes.String, "https://issuer/")
            });
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
            var handler = new HasScopeHandler();

            await handler.HandleAsync(context);

            Assert.That(context.HasSucceeded, Is.True);
        }

        [Test]
        public async Task HandleAsync_PermissionsClaimWithWrongIssuer_DoesNotSucceed()
        {
            var requirement = new HasScopeRequirement("read:honors", "https://issuer/");
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("permissions", "read:honors", ClaimValueTypes.String, "https://other/")
            });
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
            var handler = new HasScopeHandler();

            await handler.HandleAsync(context);

            Assert.That(context.HasSucceeded, Is.False);
        }

        [Test]
        public void Constructor_NullScope_Throws()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HasScopeRequirement(null, "https://issuer/"));
            Assert.That(ex!.ParamName, Is.EqualTo("scope"));
        }

        [Test]
        public void Constructor_NullIssuer_Throws()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HasScopeRequirement("read:honors", null));
            Assert.That(ex!.ParamName, Is.EqualTo("issuer"));
        }
    }
}
