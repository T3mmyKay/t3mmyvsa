namespace T3mmyvsa.Configuration;

public sealed class RateLimitSettings
{
    public RateLimitPolicySettings Login { get; set; } = new() { PermitLimit = 5, WindowSeconds = 60 };
    public RateLimitPolicySettings Registration { get; set; } = new() { PermitLimit = 5, WindowSeconds = 600 };
    public RateLimitPolicySettings Recovery { get; set; } = new() { PermitLimit = 5, WindowSeconds = 900 };
    public RateLimitPolicySettings Refresh { get; set; } = new() { PermitLimit = 30, WindowSeconds = 60 };
}
