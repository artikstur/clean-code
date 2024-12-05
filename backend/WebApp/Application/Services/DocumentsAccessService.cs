using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services;

public class DocumentsAccessService : IDocumentsAccessService
{
    private readonly IDocumentsAccessRepository _documentsAccessRepository;

    public DocumentsAccessService(IDocumentsAccessRepository documentsAccessRepository)
    {
        _documentsAccessRepository = documentsAccessRepository;
    }

    public async Task<bool> IsAuthor(Guid ownerId, Guid documentId)
    {
        return await _documentsAccessRepository.IsAuthor(ownerId, documentId);
    }

    public async Task<bool> ExistById(Guid documentId)
    {
        return await _documentsAccessRepository.ExistById(documentId);
    }

    public async Task<bool> CheckAccessToRead(Guid userId, Guid documentId)
    {
        return await _documentsAccessRepository.CheckAccessToRead(userId, documentId);
    }

    public async Task<bool> CheckAccessToEdit(Guid userId, Guid documentId)
    {
        return await _documentsAccessRepository.CheckAccessToEdit(userId, documentId);
    }
}