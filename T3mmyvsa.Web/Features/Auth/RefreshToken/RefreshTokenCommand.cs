namespace T3mmyvsa.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string? AccessToken, string RefreshToken) : ICommand<RefreshTokenResponse>;
