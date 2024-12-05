using System.Linq.Expressions;
using Application.Dtos;
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

    public async Task<Result<Guid>> Create(Guid userId, string name)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var documentEntity = new DocumentEntity()
        {
            DocumentId = Guid.NewGuid(),
            AuthorId = userId,
            Author = userEntity!,
            AllowedToEditUsers = new List<UserEntity>(),
            Name = name,
            LastModifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Documents.AddAsync(documentEntity);
        await _dbContext.SaveChangesAsync();

        return Result<Guid>.Success(documentEntity.DocumentId);
    }

    public async Task<Result> Rename(Guid userId, Guid documentId, string name)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (!documentEntity!.AllowedToEditUsers.Contains(userEntity!) && documentEntity.AuthorId != userId)
            return Result.Failure(new Error("У вас недостаточно прав", ErrorType.AuthorizationError));
        
        documentEntity.Name = name;
        await _dbContext.SaveChangesAsync();
        return Result.Success();

    }

    public async Task<Result<Document>> Get(Guid userId, Guid documentId)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
        
        if (documentEntity!.AllowedToReadUsers.Contains(userEntity!) || documentEntity.AuthorId == userId)
        {
            return Result<Document>.Success(Document.Create(documentEntity.DocumentId, documentEntity.AuthorId,
                documentEntity.Name, documentEntity.CreatedAt, documentEntity.LastModifiedAt));
        }

        return Result<Document>.Failure(new Error("У вас недостаточно прав", ErrorType.AuthorizationError));
    }


    // сделать поддержку LastModifiedAt
    public async Task<Result> Update(Guid userId, Guid documentId)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        throw new NotImplementedException();
    }

    public async Task<Result<string>> Delete(Guid userId, Guid documentId)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        var documentEntity = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        _dbContext.Documents.Remove(documentEntity!);
        await _dbContext.SaveChangesAsync();

        return Result<string>.Success(documentEntity!.Name);
    }

    public async Task<Result<ICollection<User>>> GetAllEditors(Guid userId, Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity!.AuthorId != userId)
        {
            return Result<ICollection<User>>.Failure(new Error("У вас недостаточно прав",
                ErrorType.AuthorizationError));
        }

        return Result<ICollection<User>>.Success(documentEntity.AllowedToEditUsers
            .Select(u => User.Create(u.Id, u.UserName, u.PasswordHash, u.Email)).ToList());
    }

    public async Task<Result<ICollection<User>>> GetAllReaders(Guid userId, Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
        
        if (documentEntity!.AuthorId != userId)
        {
            return Result<ICollection<User>>.Failure(new Error("У вас недостаточно прав",
                ErrorType.AuthorizationError));
        }

        return Result<ICollection<User>>.Success(documentEntity.AllowedToReadUsers
            .Select(u => User.Create(u.Id, u.UserName, u.PasswordHash, u.Email)).ToList());
    }

    public async Task<Result<ICollection<UserWithDocumentRoleDto>>> GetAllUsers(Guid ownerId, Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity!.AuthorId != ownerId)
        {
            return Result<ICollection<UserWithDocumentRoleDto>>.Failure(
                new Error("У вас недостаточно прав", ErrorType.AuthorizationError));
        }

        var editUsers = documentEntity.AllowedToEditUsers
            .Select(u => new UserWithDocumentRoleDto(u.UserName, u.Id, DocumentRole.Editor));

        var readUsers = documentEntity.AllowedToReadUsers
            .Select(u => new UserWithDocumentRoleDto(u.UserName, u.Id, DocumentRole.Reader));

        var allUsers = editUsers.Union(readUsers).ToList();

        return Result<ICollection<UserWithDocumentRoleDto>>.Success(allUsers);
    }

    public async Task<Result> AddUserAsEditor(Guid ownerId, Guid documentId, Guid newUserId)
    {
        var adminEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == ownerId);

        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == newUserId);
        
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToEditUsers)
            .Include(documentEntity => documentEntity.AllowedToReadUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity!.AuthorId != ownerId)
        {
            return Result.Failure(new Error("У вас недостаточно прав", ErrorType.AuthorizationError));
        }

        if (documentEntity.AllowedToEditUsers.Contains(userEntity!))
        {
            return Result.Success();
        }

        documentEntity.AllowedToEditUsers.Add(userEntity!);

        if (!documentEntity.AllowedToReadUsers.Contains(userEntity!))
        {
            documentEntity.AllowedToReadUsers.Add(userEntity!);
        }

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AddUserAsReader(Guid ownerId, Guid documentId, Guid newUserId)
    {
        var adminEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == ownerId);

        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == newUserId);
        
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity!.AuthorId != ownerId)
        {
            return Result.Failure(new Error("У вас недостаточно прав", ErrorType.AuthorizationError));
        }

        documentEntity.AllowedToEditUsers.Remove(userEntity!);

        if (documentEntity.AllowedToReadUsers.Contains(userEntity!))
        {
            return Result.Success();
        }

        documentEntity.AllowedToReadUsers.Add(userEntity!);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ClearUserPermissions(Guid ownerId, Guid documentId, Guid userId)
    {
        var adminEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == ownerId);

        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity!.AuthorId != ownerId)
        {
            return Result.Failure(new Error("У вас недостаточно прав", ErrorType.AuthorizationError));
        }

        documentEntity.AllowedToEditUsers.Remove(userEntity!);
        documentEntity.AllowedToReadUsers.Remove(userEntity!);

        return Result.Success();
    }

    public async Task<Result<DocumentRole>> GetUserRole(Guid ownerId, Guid documentId, Guid userId)
    {
        var adminEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == ownerId);

        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
        
        if (documentEntity!.AuthorId != ownerId)
        {
            return Result<DocumentRole>.Failure(new Error("У вас недостаточно прав", ErrorType.AuthorizationError));
        }

        if (documentEntity.AllowedToEditUsers.Contains(userEntity!))
        {
            return Result<DocumentRole>.Success(DocumentRole.Editor);
        }

        if (documentEntity.AllowedToReadUsers.Contains(userEntity!))
        {
            return Result<DocumentRole>.Success(DocumentRole.Reader);
        }

        return Result<DocumentRole>.Success(DocumentRole.NoAccess);
    }
}