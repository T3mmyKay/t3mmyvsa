namespace T3mmyvsa.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(
    [Required, EmailAddress] string Email
) : ICommand;
