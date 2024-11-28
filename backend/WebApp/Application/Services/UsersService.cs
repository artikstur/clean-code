using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Utils;
using Core.Enums;
using Core.Models;

namespace Application.Services;

public class UsersService: IUsersService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUsersRepository _usersRepository;
    private readonly IJwtProvider _jwtProvider;
    public UsersService(
        IUsersRepository usersRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _passwordHasher = passwordHasher;
        _usersRepository = usersRepository;
        _jwtProvider = jwtProvider;
    }
    public async Task<Result<List<User>>> GetAllUsers()
    {
        var usersResult = await _usersRepository.GetAllUsers();

        return !usersResult.IsSuccess ? Result<List<User>>.Failure(usersResult.Error) : Result<List<User>>.Success(usersResult.Value);
    }

    public async Task<Result<HashSet<Permission>>> GetPermissionsByUserId(Guid userId)
    {
        var permissionsResult = await _usersRepository.GetUserPermissions(userId);

        return !permissionsResult.IsSuccess ? Result<HashSet<Permission>>.Failure(permissionsResult.Error) : Result<HashSet<Permission>>.Success(permissionsResult.Value);
    }

    public async Task<Result<string>> Login(string email, string password)
    {
        var userResult = await _usersRepository.GetByEmail(email);

        if (!userResult.IsSuccess)
        {
            return Result<string>.Failure(userResult.Error);
        }

        var user = userResult.Value;
        var passwordIsValid = _passwordHasher.Verify(password, user.PasswordHash);

        if (!passwordIsValid)
        {
            return Result<string>.Failure(new Error("Неверный пароль.", ErrorType.AuthenticationError));
        }

        var token = _jwtProvider.Generate(user);
        return Result<string>.Success(token);
    }


    public async Task<Result> Register(string userName, string email, string password)
    {
        var hashedPassword = _passwordHasher.Generate(password);

        var user = User.Create(Guid.NewGuid(), userName, hashedPassword, email);

        var addUserResult = await _usersRepository.Add(user);

        if (!addUserResult.IsSuccess)
        {
            Result.Failure(addUserResult.Error);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteAllUsers()
    {
        var deleteUsersResult = await _usersRepository.DeleteAllUsers();

        return !deleteUsersResult.IsSuccess ? Result.Failure(deleteUsersResult.Error) : Result.Success();
    }

    public async Task<Result<HashSet<Role>>> GetUserRolesByUserId(Guid userId)
    {
        var userRolesResult = await _usersRepository.GetUserRoles(userId);
        
        return !userRolesResult.IsSuccess ? Result<HashSet<Role>>.Failure(userRolesResult.Error) : userRolesResult;
    }

    public async Task<Result> ChangeUserRolesById(Guid userId, List<Role> newRoles)
    {
        var changeUserRolesResult = await _usersRepository.ChangeUserRolesById(userId, newRoles);

        return !changeUserRolesResult.IsSuccess ? Result.Failure(changeUserRolesResult.Error) : Result.Success();
    }

    public async Task<Result<User>> GetUserByUserName(string userName)
    {
        var userResult = await _usersRepository.GetByUserName(userName);

        return !userResult.IsSuccess ? Result<User>.Failure(userResult.Error) : userResult;
    }
}