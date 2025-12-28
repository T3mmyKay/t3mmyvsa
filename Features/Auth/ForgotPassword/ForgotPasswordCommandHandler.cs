using Microsoft.Extensions.Options;
using T3mmyvsa.Configuration;
using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler(UserManager<User> userManager, IEmailService emailService, IOptions<AppSettings> appSettings)
    : ICommandHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var appUrl = appSettings.Value.AppUrl;
        var resetLink = $"{appUrl}/auth/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";

        await emailService.SendPasswordResetEmailAsync(request.Email, resetLink);
    }
}
