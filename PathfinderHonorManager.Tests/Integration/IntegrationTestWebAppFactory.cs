using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PathfinderHonorManager;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service;

namespace PathfinderHonorManager.Tests.Integration
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Startup>
    {
        private readonly SqliteConnection _connection;

        private readonly IReadOnlyCollection<string> _permissions;

        public IntegrationTestWebAppFactory(IReadOnlyCollection<string> permissions = null)
        {
            _permissions = permissions ?? TestAuthHandler.DefaultPermissions;
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var overrides = new Dictionary<string, string>
                {
                    ["Auth0:Domain"] = "test",
                    ["Auth0:Audience"] = "test-audience",
                    ["AzureAD:ApiScope"] = "test-scope",
                    ["ConnectionStrings:PathfinderCS"] = "DataSource=:memory:"
                };

                config.AddInMemoryCollection(overrides);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<PathfinderContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<IDatabaseProvider>();
                services.RemoveAll<IDbContextOptionsConfiguration<PathfinderContext>>();

                RemoveHostedService<MigrationService>(services);
                RemoveHostedService<AchievementSyncBackgroundService>(services);

                services.AddDbContext<PathfinderContext>(options =>
                {
                    options.UseSqlite(_connection);
                    options.ReplaceService<IModelCustomizer, TestModelCustomizer>();
                    options.AddInterceptors(new TimestampSaveChangesInterceptor());
                });

                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<TestAuthOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        options => { options.Permissions = _permissions; });
            });
        }

        public async Task InitializeAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PathfinderContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.Dispose();
            }

            base.Dispose(disposing);
        }

        private static void RemoveHostedService<TService>(IServiceCollection services)
            where TService : class, IHostedService
        {
            var descriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService) &&
                    descriptor.ImplementationType == typeof(TService))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
        }
    }
}
