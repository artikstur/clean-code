using Application.Utils;
using Core.Enums;

namespace Application.Interfaces.Auth;

public interface IPermissionService
{
    Task<Result<HashSet<Permission>>> GetPermissionsAsync(Guid userId);
}