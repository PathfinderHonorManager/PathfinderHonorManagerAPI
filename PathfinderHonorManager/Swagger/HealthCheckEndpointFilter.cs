using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Swagger
{
    [ExcludeFromCodeCoverage]
    public class HealthCheckEndpointFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var getOperation = new OpenApiOperation
            {
                Summary = "Health Check",
                Description = "Checks the health of the API and its dependencies",
                Responses = new OpenApiResponses()
            };

            getOperation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Healthy"
            });

            getOperation.Responses.Add("503", new OpenApiResponse
            {
                Description = "Unhealthy"
            });

            var path = new OpenApiPathItem();
            path.AddOperation(HttpMethod.Get, getOperation);

            swaggerDoc.Paths.Add("/health", path);
        }
    }
} 
