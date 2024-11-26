using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace SensorServer.OpenApi.SchemaTransformers;

public sealed class DescriptionSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(schema.Description) || context.JsonPropertyInfo is not null) return Task.CompletedTask;
        if (context.JsonTypeInfo.Type.GetCustomAttribute<DescriptionAttribute>() is {} description)
            schema.Description = description.Description;
        return Task.CompletedTask;
    }
}
