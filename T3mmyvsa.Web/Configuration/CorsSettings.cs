namespace T3mmyvsa.Configuration;

public sealed class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];
    public string[] AllowedHeaders { get; set; } = ["Authorization", "Content-Type", "Accept", "X-Api-Version"];
    public bool AllowCredentials { get; set; }
    public int PreflightMaxAgeSeconds { get; set; } = 600;
}
