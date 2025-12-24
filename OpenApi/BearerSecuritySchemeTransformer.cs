using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace T3mmyvsa.OpenApi;

internal sealed class BearerSecuritySchemeTransformer(IServiceProvider serviceProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Check if Bearer authentication is actually configured in the app
        var authenticationSchemeProvider = serviceProvider.GetService<IAuthenticationSchemeProvider>();
        var isBearerConfigured = false;

        if (authenticationSchemeProvider != null)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            isBearerConfigured = authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer");
        }

        // Always add Bearer security scheme for Scalar documentation
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            In = ParameterLocation.Header,
            BearerFormat = "JWT",
            Description = isBearerConfigured
                ? "JWT Bearer token authentication"
                : "JWT Bearer token authentication. Enter your token in the format: Bearer {token}"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = securityScheme
        };

        // Apply it as a requirement for all operations using OpenApiSecuritySchemeReference
        var securitySchemeRef = new OpenApiSecuritySchemeReference("Bearer");
        
        if (document.Paths is null)
            return;
            
        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [securitySchemeRef] = []
            });
        }
    }
}
