using Application.Dtos;
using Application.Utils;
using Core.Enums;
using Core.Models;

namespace Application.Interfaces.Repositories;

public interface IDocumentsRepository
{
    Task<Result<Guid>> Create(Guid userId, string name);
    Task<Result> Rename(Guid documentId, string name);
    Task<Result<Document>> Get(Guid documentId);
    Task<Result<string>> Delete(Guid documentId);
    Task<Result<ICollection<Document>>> GetUserDocuments(Guid userId);
    Task<Result<ICollection<User>>> GetAllEditors(Guid documentId);
    Task<Result<ICollection<User>>> GetAllReaders(Guid documentId);
    Task<Result<ICollection<UserWithDocumentRoleDto>>> GetAllUsers(Guid documentId);
    Task<Result> AddUserAsEditor(Guid documentId, Guid userId);
    Task<Result> AddUserAsReader(Guid documentId, Guid userId);
    Task<Result> ClearUserPermissions(Guid documentId, Guid userId);
    Task<Result<DocumentRole>> GetUserRole(Guid documentId, Guid userId);
}