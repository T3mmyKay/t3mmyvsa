namespace T3mmyvsa.Configuration;

public sealed class BootstrapAdminSettings
{
    public bool Enabled { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string FirstName { get; set; } = "System";
    public string LastName { get; set; } = "Administrator";
}
