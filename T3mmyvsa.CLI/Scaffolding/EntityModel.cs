namespace T3mmyvsa.CLI.Scaffolding;

internal sealed record EntityProperty(string Type, string Name, bool CanUpdate);

internal sealed record EntityModel(
    IReadOnlyList<EntityProperty> Properties,
    bool IsAuditable,
    IReadOnlyList<string> SkippedNavigationProperties,
    IReadOnlyList<string> UnsupportedRequiredNavigationProperties);

internal sealed record GeneratedFile(string RelativePath, string Content);
