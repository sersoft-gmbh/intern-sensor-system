using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

namespace SensorServer.OpenApi.DocumentTransformers;

public sealed class AuthorizationDocumentTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (!authenticationSchemes.Any(static authScheme => authScheme.Name == "Bearer"))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Please enter a valid token",
            Scheme = "Bearer",
            BearerFormat = "Custom",
        };
    }
}
