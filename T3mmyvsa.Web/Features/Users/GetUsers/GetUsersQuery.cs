using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetUsers;

public record GetUsersQuery(GetUsersRequest Request) : IQuery<PaginatedResponse<UserResponse>>;
