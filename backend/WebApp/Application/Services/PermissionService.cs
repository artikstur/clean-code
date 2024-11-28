using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Utils;
using Core.Enums;

namespace Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IUsersRepository _usersRepository;

    public PermissionService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<HashSet<Permission>>> GetPermissionsAsync(Guid userId)
    {
        var permissionsResult = await _usersRepository.GetUserPermissions(userId);

        return !permissionsResult.IsSuccess ? Result<HashSet<Permission>>.Failure(permissionsResult.Error) : Result<HashSet<Permission>>.Success(permissionsResult.Value);
    }
}