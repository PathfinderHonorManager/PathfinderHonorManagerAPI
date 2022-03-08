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
            // services
            //     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddMicrosoftAccount(microsoftOptions =>
            //         {
            //             microsoftOptions.ClientId = Configuration["AzureAD:ClientId"];
            //             microsoftOptions.ClientSecret = Configuration["AzureAD:ClientSecret"];
            //         })
            //     .AddJwtBearer("Bearer", 
            //         opt =>
            //         {
            //             opt.Audience = "https://localhost:5000/";
            //             opt.Authority = "https://login.microsoftonline.com/eb971100-7f436/";
            //         });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration);
            services.AddAuthorization(
                opt =>
                {
                    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        JwtBearerDefaults.AuthenticationScheme,
                        "Bearer");
                    defaultAuthorizationPolicyBuilder =
                        defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    opt.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                });
            services
                .AddControllers();

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
                            AuthorizationUrl = new Uri(Configuration["AzureAD:AuthURL"]),
                            TokenUrl = new Uri(Configuration["AzureAD:TokenURL"]),
                            Scopes = new Dictionary<string, string>
                            {
                                { Configuration["AzureAD:ApiScope"], "read the api" }
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
                .AddScoped<IPathfinderHonorService, PathfinderHonorService>();
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
                c.OAuthClientId(Configuration["AzureAD:ClientId"]);
                c.OAuthUsePkce();
                c.OAuthScopeSeparator(" ");
                c.OAuthScopes(Configuration["ApiScope"]);
            });
            
            // Commented out HTTPS redirection because Azure Healthcheck only uses HTTP and doesn't like 307
            // app.UseHttpsRedirection();
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