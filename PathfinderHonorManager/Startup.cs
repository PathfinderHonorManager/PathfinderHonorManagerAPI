using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PathfinderHonorManager.Auth;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Healthcheck;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Validators;
using System.Diagnostics.CodeAnalysis;
using PathfinderHonorManager.Swagger;

namespace PathfinderHonorManager
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private static readonly string[] PathfinderDbTags = { "pathfinderdb" };
        private static readonly string[] MigrationTags = { "migrations" };

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var domain = $"https://{Configuration["Auth0:Domain"]}/";
            var tokenUrl = $"https://{Configuration["Auth0:Domain"]}/oauth/token";
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = domain;
                    options.Audience = Configuration["Auth0:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            if (!Environment.IsDevelopment())
            {
                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.EnableAdaptiveSampling = false;
                    options.EnableQuickPulseMetricStream = true;
                    options.EnablePerformanceCounterCollectionModule = true;
                    options.EnableDependencyTrackingTelemetryModule = true;
                    options.EnableRequestTrackingTelemetryModule = true;
                });
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadPathfinders", policy => policy.Requirements.Add(new HasScopeRequirement("read:pathfinders", domain)));
                options.AddPolicy("ReadHonors", policy => policy.Requirements.Add(new HasScopeRequirement("read:honors", domain)));
                options.AddPolicy("ReadClubs", policy => policy.Requirements.Add(new HasScopeRequirement("read:clubs", domain)));
                options.AddPolicy("CreatePathfinders", policy => policy.Requirements.Add(new HasScopeRequirement("create:pathfinders", domain)));
                options.AddPolicy("CreateHonors", policy => policy.Requirements.Add(new HasScopeRequirement("create:honors", domain)));
                options.AddPolicy("CreateClubs", policy => policy.Requirements.Add(new HasScopeRequirement("create:clubs", domain)));
                options.AddPolicy("UpdatePathfinders", policy => policy.Requirements.Add(new HasScopeRequirement("update:pathfinders", domain)));
                options.AddPolicy("UpdateHonors", policy => policy.Requirements.Add(new HasScopeRequirement("update:honors", domain)));
                options.AddPolicy("UpdateClubs", policy => policy.Requirements.Add(new HasScopeRequirement("update:clubs", domain)));
            });
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });
            services.AddControllers();

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pathfinder Honor Manager", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "OAuth2.0 Auth Code with PKCE",
                    Name = "oauth2",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(domain + "authorize?audience="
                                + Configuration["Auth0:Audience"]),
                            TokenUrl = new Uri(tokenUrl),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "openid" },
                                { "profile", "profile"},
                                { "email", "email"}
                            }
                        }
                    }
                });
                c.DocumentFilter<HealthCheckEndpointFilter>();
                c.DocumentFilter<AuthorizeCheckDocumentFilter>();
            });
            services
                .AddDbContext<PathfinderContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("PathfinderCS"), 
                        npgsqlOptions => 
                        {
                            npgsqlOptions.CommandTimeout(60);
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorCodesToAdd: null);
                        }));
            services
                .AddAutoMapper(typeof(AutoMapperConfig));
            services.Configure<AchievementSyncOptions>(
                Configuration.GetSection("AchievementSync"));
            services.AddSingleton<IGradeChangeQueue, InMemoryGradeChangeQueue>();
            services
                .AddScoped<IPathfinderService, PathfinderService>()
                .AddScoped<IHonorService, HonorService>()
                .AddScoped<IPathfinderHonorService, PathfinderHonorService>()
                .AddScoped<IClubService, ClubService>()
                .AddScoped<IAchievementService, AchievementService>()
                .AddScoped<IPathfinderAchievementService, PathfinderAchievementService>();
            
            services.AddHostedService<MigrationService>();
            services.AddHostedService<AchievementSyncBackgroundService>();
            services
                .AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssemblyContaining<PathfinderValidator>()
                .AddValidatorsFromAssemblyContaining<HonorValidator>();

            services.AddHttpLogging(logging =>
                {
                    logging.LoggingFields = HttpLoggingFields.RequestProtocol |
                        HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                        HttpLoggingFields.ResponseStatusCode;
                    logging.RequestBodyLogLimit = 4096;
                    logging.ResponseBodyLogLimit = 4096;

                });
            services.AddHealthChecks()
                .AddCheck<PostgresHealthCheck>(
                    "PathfinderDB-check",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: PathfinderDbTags)
                .AddCheck<MigrationHealthCheck>(
                    "Migration-check",
                    failureStatus: HealthStatus.Degraded,
                    tags: MigrationTags);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pathfinder Honor Manager v1");
                c.OAuthClientId(Configuration["Auth0:ClientId"]);
                c.OAuthUsePkce();
                c.OAuthScopeSeparator(" ");
                c.OAuthScopes("openid profile email");
            });

            app.UseHttpLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
