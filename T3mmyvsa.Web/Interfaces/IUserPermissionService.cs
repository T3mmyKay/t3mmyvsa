namespace T3mmyvsa.Interfaces;

public interface IUserPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default);
}
