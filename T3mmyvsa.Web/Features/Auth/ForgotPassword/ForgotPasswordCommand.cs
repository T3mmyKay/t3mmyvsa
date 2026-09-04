namespace T3mmyvsa.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand;
