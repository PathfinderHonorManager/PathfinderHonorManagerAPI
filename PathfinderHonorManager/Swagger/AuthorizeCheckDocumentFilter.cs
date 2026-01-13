using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PathfinderHonorManager.Swagger
{
    public class AuthorizeCheckDocumentFilter : IDocumentFilter
    {
        private const char UrlPathSeparator = '/';

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var schemeReference = new OpenApiSecuritySchemeReference("oauth2", swaggerDoc, null);

            foreach (var apiDescription in context.ApiDescriptions)
            {
                var hasAuthorize = apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<IAuthorizeData>()
                    .Any();
                var hasAllowAnonymous = apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<IAllowAnonymous>()
                    .Any();

                if (!hasAuthorize || hasAllowAnonymous)
                {
                    continue;
                }

                var pathKey = UrlPathSeparator + (apiDescription.RelativePath ?? string.Empty);
                pathKey = pathKey.Split('?', 2)[0].TrimEnd('/');

                if (!swaggerDoc.Paths.TryGetValue(pathKey, out var pathItem))
                {
                    continue;
                }

                var httpMethod = apiDescription.HttpMethod ?? string.Empty;
                if (!TryMapOperation(httpMethod, out var operationType))
                {
                    continue;
                }

                if (!pathItem.Operations.TryGetValue(operationType, out var operation))
                {
                    continue;
                }

                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    { schemeReference, new List<string>() }
                });
            }
        }

        private static bool TryMapOperation(string httpMethod, out HttpMethod operationType)
        {
            switch (httpMethod.ToUpperInvariant())
            {
                case "GET":
                    operationType = HttpMethod.Get;
                    return true;
                case "POST":
                    operationType = HttpMethod.Post;
                    return true;
                case "PUT":
                    operationType = HttpMethod.Put;
                    return true;
                case "PATCH":
                    operationType = HttpMethod.Patch;
                    return true;
                case "DELETE":
                    operationType = HttpMethod.Delete;
                    return true;
                case "HEAD":
                    operationType = HttpMethod.Head;
                    return true;
                case "OPTIONS":
                    operationType = HttpMethod.Options;
                    return true;
                case "TRACE":
                    operationType = HttpMethod.Trace;
                    return true;
                default:
                    operationType = default;
                    return false;
            }
        }
    }
}
