using System.Collections.Generic;
using System.Linq;
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

        [TestCase("GET", "GET")]
        [TestCase("POST", "POST")]
        [TestCase("PUT", "PUT")]
        [TestCase("PATCH", "PATCH")]
        [TestCase("DELETE", "DELETE")]
        [TestCase("HEAD", "HEAD")]
        [TestCase("OPTIONS", "OPTIONS")]
        [TestCase("TRACE", "TRACE")]
        [TestCase("get", "GET")]
        public void Apply_AddsSecurityRequirement_ForAllSupportedHttpMethods(string httpMethod, string operationMethod)
        {
            var mappedOperationMethod = new HttpMethod(operationMethod);
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
                    [mappedOperationMethod] = new OpenApiOperation()
                }
            });

            var apiDescription = new ApiDescription
            {
                RelativePath = "api/test",
                HttpMethod = httpMethod,
                ActionDescriptor = new ActionDescriptor
                {
                    EndpointMetadata = new List<object> { new AuthorizeAttribute() }
                }
            };

            var context = CreateContext(apiDescription);
            var filter = new AuthorizeCheckDocumentFilter();

            filter.Apply(swaggerDoc, context);

            var operation = swaggerDoc.Paths["/api/test"].Operations[mappedOperationMethod];
            Assert.That(operation.Security, Is.Not.Null);
            Assert.That(operation.Security!.Count, Is.EqualTo(1));
        }

        [Test]
        public void Apply_DoesNotAddSecurity_WhenAllowAnonymousIsPresent()
        {
            var swaggerDoc = new OpenApiDocument { Paths = new OpenApiPaths() };
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
                    EndpointMetadata = new List<object> { new AuthorizeAttribute(), new AllowAnonymousAttribute() }
                }
            };

            var context = CreateContext(apiDescription);
            var filter = new AuthorizeCheckDocumentFilter();

            filter.Apply(swaggerDoc, context);

            Assert.That(swaggerDoc.Paths["/api/test"].Operations[HttpMethod.Get].Security, Is.Null);
        }

        [Test]
        public void Apply_DoesNotAddSecurity_WhenHttpMethodIsUnsupported()
        {
            var swaggerDoc = new OpenApiDocument { Paths = new OpenApiPaths() };
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
                HttpMethod = "CONNECT",
                ActionDescriptor = new ActionDescriptor
                {
                    EndpointMetadata = new List<object> { new AuthorizeAttribute() }
                }
            };

            var context = CreateContext(apiDescription);
            var filter = new AuthorizeCheckDocumentFilter();

            filter.Apply(swaggerDoc, context);

            Assert.That(swaggerDoc.Paths["/api/test"].Operations[HttpMethod.Get].Security, Is.Null);
        }

        [Test]
        public void Apply_DoesNotAddSecurity_WhenPathOrOperationCannotBeResolved()
        {
            var swaggerDoc = new OpenApiDocument { Paths = new OpenApiPaths() };
            swaggerDoc.Paths.Add("/api/other", new OpenApiPathItem
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>
                {
                    [HttpMethod.Get] = new OpenApiOperation()
                }
            });

            var apiDescriptions = new List<ApiDescription>
            {
                new()
                {
                    RelativePath = "api/missing",
                    HttpMethod = "GET",
                    ActionDescriptor = new ActionDescriptor
                    {
                        EndpointMetadata = new List<object> { new AuthorizeAttribute() }
                    }
                },
                new()
                {
                    RelativePath = "api/other",
                    HttpMethod = "POST",
                    ActionDescriptor = new ActionDescriptor
                    {
                        EndpointMetadata = new List<object> { new AuthorizeAttribute() }
                    }
                }
            };

            var context = CreateContext(apiDescriptions.ToArray());
            var filter = new AuthorizeCheckDocumentFilter();

            filter.Apply(swaggerDoc, context);

            Assert.That(swaggerDoc.Paths["/api/other"].Operations[HttpMethod.Get].Security, Is.Null);
        }

        [Test]
        public void Apply_TrimsQueryAndTrailingSlash_WhenMatchingPath()
        {
            var swaggerDoc = new OpenApiDocument { Paths = new OpenApiPaths() };
            swaggerDoc.Paths.Add("/api/test", new OpenApiPathItem
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>
                {
                    [HttpMethod.Get] = new OpenApiOperation()
                }
            });

            var apiDescription = new ApiDescription
            {
                RelativePath = "api/test/?v=1",
                HttpMethod = "GET",
                ActionDescriptor = new ActionDescriptor
                {
                    EndpointMetadata = new List<object> { new AuthorizeAttribute() }
                }
            };

            var context = CreateContext(apiDescription);
            var filter = new AuthorizeCheckDocumentFilter();

            filter.Apply(swaggerDoc, context);

            Assert.That(swaggerDoc.Paths["/api/test"].Operations[HttpMethod.Get].Security, Is.Not.Null);
        }

        private static DocumentFilterContext CreateContext(params ApiDescription[] apiDescriptions)
        {
            var schemaGenerator = new SchemaGenerator(
                new SchemaGeneratorOptions(),
                new JsonSerializerDataContractResolver(new JsonSerializerOptions()));

            return new DocumentFilterContext(
                apiDescriptions,
                schemaGenerator,
                new SchemaRepository());
        }
    }
}
