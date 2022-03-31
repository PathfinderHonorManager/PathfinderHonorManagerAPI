using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Npgsql;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Validators;
using PathfinderHonorManager.Healthcheck;
using Microsoft.AspNetCore.HttpLogging;
using PathfinderHonorManager.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace PathfinderHonorManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddCors(options =>
            // {
            //     options.AddPolicy("AllowSpecificOrigin",
            //         builder =>
            //         {
            //             builder
            //             .WithOrigins("http://localhost:8080")
            //             .AllowAnyMethod()
            //             .AllowAnyHeader()
            //             .AllowCredentials();
            //         });
            // });
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
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadPathfinders", policy => policy.Requirements.Add(new HasScopeRequirement("read:pathfinders", domain)));
                options.AddPolicy("ReadHonors", policy => policy.Requirements.Add(new HasScopeRequirement("read:honors", domain)));
            });
            services.AddControllers();
            
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pathfinder Honor Manager", Version = "v1" });
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
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    new[] { Configuration["AzureAD:ApiScope"] }
                    }
                });
            });
            services
                .AddDbContext<PathfinderContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("PathfinderCS")));
            services
                .AddAutoMapper(typeof(AutoMapperConfig));
            services
                .AddScoped<IPathfinderService, PathfinderService>()
                .AddScoped<IHonorService, HonorService>()
                .AddScoped<IPathfinderHonorService, PathfinderHonorService>()
                .AddScoped<IClubService, ClubService>();
            services.AddMvc()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<PathfinderValidator>())
                .AddJsonOptions(options => 
                    {
                        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    });
            services.AddHttpLogging(logging =>
                {
                    logging.LoggingFields = HttpLoggingFields.RequestProtocol |
                        HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                        HttpLoggingFields.ResponseStatusCode;
                    logging.RequestBodyLogLimit = 4096;
                    logging.ResponseBodyLogLimit = 4096;

                });
            services.AddHealthChecks()
                .AddCheck(
                "PathfinderDB-check",
                new PostgresHealthCheck(Configuration.GetConnectionString("PathfinderCS")),
                HealthStatus.Unhealthy,
                new string[] { "pathfinderdb" });
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
            
            // Commented out HTTPS redirection because Azure Healthcheck only uses HTTP and doesn't like 307
            // app.UseHttpsRedirection();
            app.UseHttpLogging();

            app.UseRouting();

            // app.UseCors("AllowSpecificOrigin");

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