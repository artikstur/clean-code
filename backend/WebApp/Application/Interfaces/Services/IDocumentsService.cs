using Application.Dtos;
using Application.Utils;
using Core.Enums;
using Core.Models;

namespace Application.Interfaces.Services;

public interface IDocumentsService
{
    Task<Result> Create(Guid userId, string name);
    Task<Result> SetUserPermission(DocumentRole documentRole, Guid documentId, Guid userId);
    Task<Result<ICollection<UserUserNameDto>>> GetAllEditors(Guid documentId);
    Task<Result<ICollection<UserUserNameDto>>> GetAllReaders(Guid documentId);
    Task<Result<ICollection<UserWithDocumentRoleDto>>> GetAllUsers(Guid documentId);
    Task<Result> Rename(Guid documentId, string name);
    Task<Result<Document>> Get(Guid documentId);
    Task<Result<ICollection<Document>>> GetUserDocuments(Guid userId);
    Task<Result<DocumentRole>> GetUserRole(Guid documentId, Guid userId);
    Task<Result> Delete(Guid documentId);
    Task<Result<string>> GetDownloadUrl(Guid documentId);
}