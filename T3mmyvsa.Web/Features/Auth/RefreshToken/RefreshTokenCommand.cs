namespace T3mmyvsa.Features.Auth.RefreshToken;

public record RefreshTokenCommand(
    [Required] string AccessToken,
    [Required] string RefreshToken
) : ICommand<RefreshTokenResponse>;
