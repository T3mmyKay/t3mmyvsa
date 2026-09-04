namespace T3mmyvsa.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;
