using T3mmyvsa.Features.Users.GetUser;
using T3mmyvsa.Features.Users.GetUsers;

namespace T3mmyvsa.Features.Users.GetUser;

public record GetUserQuery([Required] string Id) : IQuery<UserResponse?>;
