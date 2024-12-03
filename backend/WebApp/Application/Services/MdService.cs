using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Utils;

namespace Application.Services;

public class MdService : IMdService
{
    private readonly IDocumentsRepository _documentsRepository;

    public MdService(IDocumentsRepository documentsRepository)
    {
        _documentsRepository = documentsRepository;
    }

    // Здесь будет работа с S3 хранилищем
    public async Task<Result> Push(Guid userId, string newContent, Guid documentId)
    {
        var accessResult = await _documentsRepository.CheckAccessToEdit(userId, documentId);

        if (!accessResult.IsSuccess)
        {
            return Result.Failure(accessResult.Error);
        }

        throw new NotImplementedException();
    }

    public async Task<Result<string>> Pull(Guid userId, Guid documentId)
    {
        var accessResult = await _documentsRepository.CheckAccessToRead(userId, documentId);

        if (!accessResult.IsSuccess)
        {
            return Result<string>.Failure(accessResult.Error);
        }

        throw new NotImplementedException();
    }

    public Task<Result<string>> GetHtml(string rawMarkdown)
    {
        throw new NotImplementedException();
    }
}