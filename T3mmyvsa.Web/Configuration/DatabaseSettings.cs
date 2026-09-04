namespace T3mmyvsa.Configuration;

public sealed class DatabaseSettings
{
    public string Provider { get; set; } = DatabaseProviders.SqlServer;
    public string ConnectionStringName { get; set; } = "appDatabase";

    // Compatibility alias for projects created before v2.0.
    // New applications should configure DatabaseSettings:Provider.
    public string? DBProvider { get; set; }

    public string ConfiguredProvider =>
        string.IsNullOrWhiteSpace(DBProvider) ? Provider : DBProvider;
}
