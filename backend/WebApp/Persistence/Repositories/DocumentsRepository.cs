using Application.Interfaces.Repositories;
using Application.Utils;
using Core.Enums;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;

namespace Persistence.Repositories;

public class DocumentsRepository : IDocumentsRepository
{
    private readonly WebDbContext _dbContext;

    public DocumentsRepository(WebDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Create(Guid userId, string name)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (userEntity is null)
        {
            return Result.Failure(new Error("Пользователь не найден", ErrorType.NotFound));
        }

        var documentEntity = new DocumentEntity()
        {
            DocumentId = Guid.NewGuid(),
            AuthorId = userId,
            Author = userEntity,
            AllowedUsers = new List<UserEntity>(),
            Name = name,
            LastModifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Documents.AddAsync(documentEntity);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> Rename(Guid userId, Guid documentId, string name)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Document>> Get(Guid userId, Guid documentId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> Update(Guid userId, Guid documentId, string newContent)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> Delete(Guid userId, Guid documentId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<ICollection<User>>> GetAllEditors(Guid userId, Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity is null)
        {
            return Result<ICollection<User>>.Failure(new Error("Документ не найден", ErrorType.NotFound));
        }

        if (documentEntity.AuthorId != userId
            && documentEntity.AllowedUsers.All(u => u.Id != userId))
        {
            return Result<ICollection<User>>.Failure(new Error("У вас недостачно прав", ErrorType.AuthorizationError));
        }

        return Result<ICollection<User>>.Success(documentEntity.AllowedUsers
            .Select(u => User.Create(u.Id, u.UserName, u.PasswordHash, u.Email)).ToList());
    }

    public async Task<Result> AddUserAsEditor(Guid userId, Guid documentId, Guid newUserId)
    {
        var adminEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var newUserEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == newUserId);

        if (adminEntity is null || newUserEntity is null)
        {
            return Result.Failure(new Error("Пользователь не найден", ErrorType.NotFound));
        }

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity is null)
        {
            return Result.Failure(new Error("Документ не найден", ErrorType.NotFound));
        }

        if (documentEntity.AuthorId != userId)
        {
            return Result.Failure(new Error("У вас недостачно прав", ErrorType.AuthorizationError));
        }

        if (documentEntity.AllowedUsers.Contains(newUserEntity))
        {
            return Result.Failure(new Error("Пользователь уже добавлен", ErrorType.BadRequest));
        }

        documentEntity.AllowedUsers.Add(newUserEntity);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> RemoveUserFromEditors(Guid userId, Guid documentId, Guid oldUserId)
    {
        var adminEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var newUserEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == oldUserId);

        if (adminEntity is null || newUserEntity is null)
        {
            return Result.Failure(new Error("Пользователь не найден", ErrorType.NotFound));
        }

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity is null)
        {
            return Result.Failure(new Error("Документ не найден", ErrorType.NotFound));
        }

        if (documentEntity.AuthorId != userId)
        {
            return Result.Failure(new Error("У вас недостачно прав", ErrorType.AuthorizationError));
        }

        if (!documentEntity.AllowedUsers.Contains(newUserEntity))
        {
            return Result.Failure(new Error("Пользователь не был добавлен в доверенный список", ErrorType.BadRequest));
        }

        documentEntity.AllowedUsers.Remove(newUserEntity);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}