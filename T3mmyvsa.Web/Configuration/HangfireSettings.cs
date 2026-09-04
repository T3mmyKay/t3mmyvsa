namespace T3mmyvsa.Configuration;

public sealed class HangfireSettings
{
    public bool Enabled { get; set; } = true;

    // The web/API process is enqueue-only by default. The dedicated Worker process
    // overrides this setting to true before registering Hangfire.
    public bool ServerEnabled { get; set; } = false;

    // "inherit" follows the application database only when it is SQL Server or PostgreSQL.
    public string StorageProvider { get; set; } = "inherit";

    // Empty means use DatabaseSettings:ConnectionStringName.
    public string ConnectionStringName { get; set; } = string.Empty;

    public string SchemaName { get; set; } = "hangfire";
    public int? WorkerCount { get; set; }
    public string[] Queues { get; set; } = ["critical", "default"];
    public int AutomaticRetryAttempts { get; set; } = 3;
    public bool PrepareSchemaIfNecessary { get; set; } = true;
    public HangfireDashboardSettings Dashboard { get; set; } = new();
}
