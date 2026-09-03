namespace T3mmyvsa.Features.Auth.RefreshToken;

public record RefreshTokenCommand(
    string? AccessToken,
    [Required] string RefreshToken
) : ICommand<RefreshTokenResponse>;
