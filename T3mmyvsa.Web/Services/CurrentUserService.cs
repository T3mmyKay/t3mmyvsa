using System.Security.Claims;
using T3mmyvsa.Attributes;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Services;

[ScopedService]
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
