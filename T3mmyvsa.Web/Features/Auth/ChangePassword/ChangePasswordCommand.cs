namespace T3mmyvsa.Features.Auth.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand;
