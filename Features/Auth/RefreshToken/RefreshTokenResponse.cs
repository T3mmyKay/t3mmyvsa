namespace T3mmyvsa.Features.Auth.RefreshToken;

public record RefreshTokenResponse(string AccessToken, string RefreshToken, DateTime Expiration);
