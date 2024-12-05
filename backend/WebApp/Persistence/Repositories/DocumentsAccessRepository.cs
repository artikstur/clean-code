using Application.Interfaces.Repositories;
using Application.Utils;
using Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

public class DocumentsAccessRepository : IDocumentsAccessRepository
{
    private readonly WebDbContext _dbContext;

    public DocumentsAccessRepository(WebDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsAuthor(Guid ownerId, Guid documentId)
    {
        var documentEntity = await _dbContext.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId);
        
        return documentEntity!.AuthorId == ownerId;
    }

    public async Task<bool> ExistById(Guid documentId)
    {
        var documentEntity = await _dbContext.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (documentEntity is null)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> CheckAccessToRead(Guid userId, Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToReadUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        return documentEntity!.AllowedToReadUsers.Any(u => u.Id == userId);
    }

    public async Task<bool> CheckAccessToEdit(Guid userId, Guid documentId)
    {
        var documentEntity = await _dbContext.Documents
            .Include(d => d.AllowedToEditUsers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        return documentEntity!.AllowedToEditUsers.Any(u => u.Id == userId);
    }
}