using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using T3mmyvsa.Attributes;
using T3mmyvsa.Configuration;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Services;

[TransientService]
public class EmailService(IOptions<MailSettings> mailSettings) : IEmailService
{
    private readonly MailSettings _mailSettings = mailSettings.Value;

    public async Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_mailSettings.DisplayName ?? "Support", _mailSettings.Mail ?? "no-reply@example.com"));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        var builder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_mailSettings.Host ?? "localhost", _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailSettings.Mail ?? "", _mailSettings.Password ?? "");
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
