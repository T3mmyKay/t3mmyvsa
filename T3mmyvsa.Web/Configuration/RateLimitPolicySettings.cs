namespace T3mmyvsa.Configuration;

public sealed class RateLimitPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}
