using Application.Dtos;
using Application.Utils;
using Core.Enums;
using Core.Models;

namespace Application.Interfaces.Services;

public interface IDocumentsService
{
    Task<Result> Create(Guid userId, string name);
    Task<Result> SetUserPermission(Guid ownerId, DocumentRole documentRole, Guid documentId, Guid userId);
    Task<Result<ICollection<UserUserNameDto>>> GetAllEditors(Guid ownerId, Guid documentId);
    Task<Result<ICollection<UserUserNameDto>>> GetAllReaders(Guid ownerId, Guid documentId);
    Task<Result<ICollection<UserWithDocumentRoleDto>>> GetAllUsers(Guid ownerId, Guid documentId);
    Task<Result> Rename(Guid userId, Guid documentId, string name);
    Task<Result<Document>> Get(Guid userId, Guid documentId);
    Task<Result<DocumentRole>> GetUserRole(Guid ownerId, Guid documentId, Guid userId);
    Task<Result> Update(Guid userId, Guid documentId);
    Task<Result> Delete(Guid ownerId, Guid documentId);
    Task<Result> Download(Guid userId, Guid documentId);
}