using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace T3mmyvsa.OpenApi;

internal sealed class ServerUrlTransformer : IOpenApiDocumentTransformer
{

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Servers = [];
        return Task.CompletedTask;
    }
}
