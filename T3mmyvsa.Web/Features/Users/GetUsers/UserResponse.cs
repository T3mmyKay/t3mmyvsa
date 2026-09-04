namespace T3mmyvsa.Features.Users.GetUsers;

public record UserResponse(
    string Id,
    string? UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Role,
    bool IsActive,
    bool EmailConfirmed,
    DateTimeOffset CreatedAt,
    List<string> Roles,
    List<string> Permissions
);
