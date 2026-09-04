namespace T3mmyvsa.Features.Users.GetCurrentUser;

public record CurrentUserResponse(
    string Id,
    string? UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Role,
    bool IsActive,
    bool EmailConfirmed,
    List<string> Roles,
    List<string> Permissions,
    DateTimeOffset CreatedAt
);
