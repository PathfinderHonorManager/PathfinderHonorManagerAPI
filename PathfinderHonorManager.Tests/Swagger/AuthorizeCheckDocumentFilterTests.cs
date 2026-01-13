using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using NUnit.Framework;
using PathfinderHonorManager.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PathfinderHonorManager.Tests.Swagger
{
    public class AuthorizeCheckDocumentFilterTests
    {
        [Test]
        public void Apply_AddsSecurityRequirement_ForAuthorizedEndpoint()
        {
            var swaggerDoc = new OpenApiDocument
            {
                Components = new OpenApiComponents
                {
                    SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        ["oauth2"] = new OpenApiSecurityScheme()
                    }
                },
                Paths = new OpenApiPaths()
            };

            swaggerDoc.Paths.Add("/api/test", new OpenApiPathItem
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>
                {
                    [HttpMethod.Get] = new OpenApiOperation()
                }
            });

            var apiDescription = new ApiDescription
            {
                RelativePath = "api/test",
                HttpMethod = "GET",
                ActionDescriptor = new ActionDescriptor
                {
                    EndpointMetadata = new List<object> { new AuthorizeAttribute() }
                }
            };

            var schemaGenerator = new SchemaGenerator(
                new SchemaGeneratorOptions(),
                new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
            var context = new DocumentFilterContext(
                new List<ApiDescription> { apiDescription },
                schemaGenerator,
                new SchemaRepository());

            var filter = new AuthorizeCheckDocumentFilter();

            filter.Apply(swaggerDoc, context);

            var operation = swaggerDoc.Paths["/api/test"].Operations[HttpMethod.Get];
            Assert.That(operation.Security, Is.Not.Null);
            Assert.That(operation.Security.Count, Is.EqualTo(1));
        }
    }
}
