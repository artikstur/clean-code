using Application.Dtos;
using Application.Utils;
using Core.Enums;
using Core.Models;

namespace Application.Interfaces.Repositories;

public interface IDocumentsRepository
{
    Task<Result> Create(Guid userId, string name);
    Task<Result> Rename(Guid userId, Guid documentId, string name);
    Task<Result<Document>> Get(Guid userId, Guid documentId);
    Task<Result> Update(Guid userId, Guid documentId);
    Task<Result> Delete(Guid userId, Guid documentId);
    Task<Result<ICollection<User>>> GetAllEditors(Guid ownerId, Guid documentId);
    Task<Result<ICollection<User>>> GetAllReaders(Guid ownerId, Guid documentId);
    Task<Result<ICollection<UserWithDocumentRoleDto>>> GetAllUsers(Guid ownerId, Guid documentId);
    Task<Result> AddUserAsEditor(Guid ownerId, Guid documentId, Guid userId);
    Task<Result> AddUserAsReader(Guid ownerId, Guid documentId, Guid userId);
    Task<Result> ClearUserPermissions(Guid ownerId, Guid documentId, Guid userId);
    Task<Result<DocumentRole>> GetUserRole(Guid ownerId, Guid documentId, Guid userId);
    Task<Result<bool>> CheckAccessToRead(Guid userId, Guid documentId);
    Task<Result<bool>> CheckAccessToEdit(Guid userId, Guid documentId);
}