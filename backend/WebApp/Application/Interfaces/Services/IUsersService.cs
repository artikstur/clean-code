using Application.Utils;
using Core.Enums;
using Core.Models;

namespace Application.Interfaces.Services;

public interface IUsersService
{
    Task<Result<List<User>>> GetAllUsers();
    Task<Result<HashSet<Permission>>> GetPermissionsByUserId(Guid userId);
    Task<Result<string>> Login(string email, string password);
    Task<Result> Register(string userName, string email, string password);
    Task<Result> DeleteAllUsers();
    Task<Result<HashSet<Role>>> GetUserRolesByUserId(Guid userId);
    Task<Result> ChangeUserRolesById(Guid userId, List<Role> newRoles);
    Task<Result<User>> GetUserByUserName(string userName);
}