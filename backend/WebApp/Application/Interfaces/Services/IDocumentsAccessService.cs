namespace Application.Interfaces.Services;

public interface IDocumentsAccessService
{
    Task<bool> IsAuthor(Guid ownerId, Guid documentId);
    Task<bool> ExistById(Guid documentId);
    Task<bool> CheckAccessToRead(Guid userId, Guid documentId);
    Task<bool> CheckAccessToEdit(Guid userId, Guid documentId);
}