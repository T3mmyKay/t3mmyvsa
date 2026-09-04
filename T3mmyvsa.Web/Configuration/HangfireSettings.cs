namespace T3mmyvsa.Configuration;

public sealed class HangfireSettings
{
    public bool Enabled { get; set; } = true;
    public string ConnectionStringName { get; set; } = "sqlConnection";
    public string SchemaName { get; set; } = "HangFire";
    public int? WorkerCount { get; set; }
    public string[] Queues { get; set; } = ["critical", "default"];
    public int AutomaticRetryAttempts { get; set; } = 3;
    public bool PrepareSchemaIfNecessary { get; set; } = true;
    public HangfireDashboardSettings Dashboard { get; set; } = new();
}
