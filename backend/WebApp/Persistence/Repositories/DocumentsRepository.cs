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
            AllowedToReadUsers = new List<UserEntity>(),
            Name = name,
            LastModifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        documentEntity.AllowedToEditUsers.Add(userEntity);
        documentEntity.AllowedToReadUsers.Add(userEntity);

        await _dbContext.Documents.AddAsync(documentEntity);
        await _dbContext.SaveChangesAsync();

        return Result<Guid>.Success(documentEntity.DocumentId);
    }

    public async Task<Result> Rename(Guid documentId, string name)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        documentEntity.Name = name;
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<Document>> Get(Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        return Result<Document>.Success(Document.Create(documentEntity.DocumentId, documentEntity.AuthorId,
            documentEntity.Name, documentEntity.CreatedAt, documentEntity.LastModifiedAt));
    }

    public async Task<Result<string>> Delete(Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        _dbContext.Documents.Remove(documentEntity!);
        await _dbContext.SaveChangesAsync();

        return Result<string>.Success(documentEntity!.Name);
    }

    public async Task<Result<ICollection<Document>>> GetUserDocuments(Guid userId)
    {
        var documentEntities = await _dbContext.Documents
            .Where(d => d.AuthorId == userId)
            .Select(d => Document.Create(d.DocumentId, d.AuthorId, 
                d.Name, d.CreatedAt, d.LastModifiedAt))
            .ToListAsync();

        return Result<ICollection<Document>>.Success(documentEntities);
    }

    public async Task<Result<ICollection<User>>> GetAllEditors(Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        return Result<ICollection<User>>.Success(documentEntity.AllowedToEditUsers
            .Select(u => User.Create(u.Id, u.UserName, u.PasswordHash, u.Email)).ToList());
    }

    public async Task<Result<ICollection<User>>> GetAllReaders(Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        return Result<ICollection<User>>.Success(documentEntity.AllowedToReadUsers
            .Select(u => User.Create(u.Id, u.UserName, u.PasswordHash, u.Email)).ToList());
    }

    public async Task<Result<ICollection<UserWithDocumentRoleDto>>> GetAllUsers(Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        var editUsers = documentEntity.AllowedToEditUsers
            .Select(u => new UserWithDocumentRoleDto(u.UserName, u.Id, DocumentRole.Editor));

        var readUsers = documentEntity.AllowedToReadUsers
            .Select(u => new UserWithDocumentRoleDto(u.UserName, u.Id, DocumentRole.Reader));

        var allUsers = editUsers.Union(readUsers).ToList();

        return Result<ICollection<UserWithDocumentRoleDto>>.Success(allUsers);
    }

    public async Task<Result> AddUserAsEditor(Guid documentId, Guid newUserId)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == newUserId);

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToEditUsers)
            .Include(documentEntity => documentEntity.AllowedToReadUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

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

    public async Task<Result> AddUserAsReader(Guid documentId, Guid newUserId)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == newUserId);

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        documentEntity.AllowedToEditUsers.Remove(userEntity!);

        if (documentEntity.AllowedToReadUsers.Contains(userEntity!))
        {
            return Result.Success();
        }

        documentEntity.AllowedToReadUsers.Add(userEntity!);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ClearUserPermissions(Guid documentId, Guid userId)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity.AuthorId == userId)
        {
            return Result.Failure(new Error("Недостаточно прав", ErrorType.AuthorizationError));
        }

        documentEntity.AllowedToEditUsers.Remove(userEntity!);
        documentEntity.AllowedToReadUsers.Remove(userEntity!);

        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<DocumentRole>> GetUserRole(Guid documentId, Guid userId)
    {
        var userEntity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

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