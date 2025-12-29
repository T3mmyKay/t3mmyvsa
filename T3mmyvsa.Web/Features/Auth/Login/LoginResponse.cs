namespace T3mmyvsa.Features.Auth.Login;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime Expiration);
