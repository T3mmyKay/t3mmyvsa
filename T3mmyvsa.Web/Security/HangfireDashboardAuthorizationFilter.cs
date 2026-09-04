using System.Net;
using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;
using T3mmyvsa.Configuration;

namespace T3mmyvsa.Security;

public sealed class HangfireDashboardAuthorizationFilter(IOptions<HangfireSettings> options) : IDashboardAuthorizationFilter
{
    private readonly HangfireDashboardSettings _settings = options.Value.Dashboard;

    public bool Authorize(DashboardContext context)
    {
        if (!_settings.Enabled)
        {
            return false;
        }

        var httpContext = context.GetHttpContext();
        if (_settings.RequireHttps && !httpContext.Request.IsHttps)
        {
            return false;
        }

        if (_settings.AllowedIpAddresses.Length > 0)
        {
            var remoteIp = Normalize(httpContext.Connection.RemoteIpAddress);
            if (remoteIp is null || !_settings.AllowedIpAddresses.Any(value =>
                    IPAddress.TryParse(value, out var allowedIp) && Normalize(allowedIp)?.Equals(remoteIp) == true))
            {
                return false;
            }
        }

        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\", charset=\"UTF-8\"";
        var authorization = httpContext.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var encoded = authorization["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var separatorIndex = decoded.IndexOf(':');
            if (separatorIndex <= 0)
            {
                return false;
            }

            var username = decoded[..separatorIndex];
            var password = decoded[(separatorIndex + 1)..];
            return FixedTimeEquals(username, _settings.Username) && FixedTimeEquals(password, _settings.Password);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static IPAddress? Normalize(IPAddress? address)
    {
        return address?.IsIPv4MappedToIPv6 == true ? address.MapToIPv4() : address;
    }

    private static bool FixedTimeEquals(string candidate, string? expected)
    {
        if (string.IsNullOrEmpty(expected))
        {
            return false;
        }

        var candidateHash = SHA256.HashData(Encoding.UTF8.GetBytes(candidate));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(candidateHash, expectedHash);
    }
}
