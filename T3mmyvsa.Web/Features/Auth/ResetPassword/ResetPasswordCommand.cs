namespace T3mmyvsa.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand;
