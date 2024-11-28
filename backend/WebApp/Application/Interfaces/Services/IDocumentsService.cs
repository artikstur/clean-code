using Application.Utils;
using Core.Models;

namespace Application.Interfaces.Services;

public interface IDocumentsService
{
    Task<Result> Create(Guid userId, string name);
    Task<Result> AddUserAsEditor(Guid userId, Guid documentId, Guid newUserId);
    Task<Result> RemoveUserFromEditors(Guid userId, Guid documentId, Guid oldUserId);
    Task<Result<ICollection<User>>> GetAllEditors(Guid userId, Guid documentId);
    Task<Result> Rename(Guid userId, Guid documentId, string name);
    Task<Result<Document>> Get(Guid userId, Guid documentId);
    Task<Result> Update(Guid userId, Guid documentId, string newContent);
    Task<Result> Delete(Guid userId, Guid documentId);
    Task<Result> Download(Guid userId, Guid documentId);
}