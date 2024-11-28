using Application.Utils;
using Core.Models;

namespace Application.Interfaces.Repositories;

public interface IDocumentsRepository
{
    Task<Result> Create(Guid userId, string name);
    Task<Result> Rename(Guid userId, Guid documentId, string name);
    Task<Result<Document>> Get(Guid userId, Guid documentId);
    Task<Result> Update(Guid userId, Guid documentId, string newContent);
    Task<Result> Delete(Guid userId, Guid documentId);
    Task<Result<ICollection<User>>> GetAllEditors(Guid userId, Guid documentId);
    Task<Result> AddUserAsEditor(Guid userId, Guid documentId, Guid newUserId);
    Task<Result> RemoveUserFromEditors(Guid userId, Guid documentId, Guid oldUserId);
}