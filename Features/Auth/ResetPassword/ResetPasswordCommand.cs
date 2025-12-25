namespace T3mmyvsa.Features.Auth.ResetPassword;

public record ResetPasswordCommand(
    [Required, EmailAddress] string Email,
    [Required] string Token,
    [Required] string NewPassword
) : ICommand;
