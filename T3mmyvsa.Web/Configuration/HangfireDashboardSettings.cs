namespace T3mmyvsa.Configuration;

public sealed class HangfireDashboardSettings
{
    public bool Enabled { get; set; }
    public string Path { get; set; } = "/jobs";
    public bool RequireHttps { get; set; } = true;
    public bool ReadOnly { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string[] AllowedIpAddresses { get; set; } = [];
}
