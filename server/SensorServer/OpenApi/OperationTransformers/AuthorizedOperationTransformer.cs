using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace SensorServer.OpenApi.OperationTransformers;

public sealed class AuthorizedOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (!context.Description.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
            return Task.CompletedTask;

        if (!operation.Responses.ContainsKey("401"))
            operation.Responses.Add("401", new OpenApiResponse());

        var jwtBearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer",
            },
        };
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [jwtBearerScheme] = Array.Empty<string>(),
            },
        };

        return Task.CompletedTask;
    }
}
