using System.Collections.Generic;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PathfinderHonorManager.Swagger
{
    public class HealthCheckEndpointFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var path = new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    {
                        OperationType.Get,
                        new OpenApiOperation
                        {
                            Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Health" } },
                            Summary = "Health Check",
                            Description = "Checks the health of the API and its dependencies",
                            Responses = new OpenApiResponses
                            {
                                {
                                    "200",
                                    new OpenApiResponse
                                    {
                                        Description = "Healthy",
                                        Content = new Dictionary<string, OpenApiMediaType>
                                        {
                                            {
                                                "text/plain",
                                                new OpenApiMediaType
                                                {
                                                    Schema = new OpenApiSchema
                                                    {
                                                        Type = "string",
                                                        Example = new Microsoft.OpenApi.Any.OpenApiString("Healthy")
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "503",
                                    new OpenApiResponse
                                    {
                                        Description = "Unhealthy",
                                        Content = new Dictionary<string, OpenApiMediaType>
                                        {
                                            {
                                                "text/plain",
                                                new OpenApiMediaType
                                                {
                                                    Schema = new OpenApiSchema
                                                    {
                                                        Type = "string",
                                                        Example = new Microsoft.OpenApi.Any.OpenApiString("Unhealthy")
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            swaggerDoc.Paths.Add("/health", path);
        }
    }
} 