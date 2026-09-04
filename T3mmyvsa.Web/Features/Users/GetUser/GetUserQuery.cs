using T3mmyvsa.Features.Users.GetUsers;

namespace T3mmyvsa.Features.Users.GetUser;

public record GetUserQuery(string Id) : IQuery<UserResponse?>;
