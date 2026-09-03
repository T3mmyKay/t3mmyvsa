using T3mmyvsa.Entities;

namespace T3mmyvsa.Interfaces;

public interface IUserRoleService
{
    Task SetExactRoleAsync(User user, string roleName, CancellationToken cancellationToken = default);
}
