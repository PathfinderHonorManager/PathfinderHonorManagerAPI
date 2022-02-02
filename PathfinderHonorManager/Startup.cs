using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
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
            services
                .AddControllers();
            services
                .AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pathfinder Honor Manager", Version = "v1" }));
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
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<PathfinderValidator>());
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
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pathfinder Honor Manager v1"));
            // Commented out HTTPS redirection because Azure Healthcheck only uses HTTP and doesn't like 307
            // app.UseHttpsRedirection();
            app.UseHttpLogging();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}