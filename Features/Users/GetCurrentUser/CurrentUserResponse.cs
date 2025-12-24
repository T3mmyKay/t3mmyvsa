namespace T3mmyvsa.Features.Users.GetCurrentUser;

public record CurrentUserResponse(
    string Id,
    string? UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    List<string> Roles,
    List<string> Permissions
);
