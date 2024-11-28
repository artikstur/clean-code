using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Utils;
using Core.Models;

namespace Application.Services;

public class DocumentsService : IDocumentsService
{
    private readonly IDocumentsRepository _documentsRepository;

    public DocumentsService(IDocumentsRepository documentsRepository)
    {
        _documentsRepository = documentsRepository;
    }

    public async Task<Result> Create(Guid userId, string name)
    {
        var createResult = await _documentsRepository.Create(userId, name);

        return createResult.IsSuccess
            ? Result.Success()
            : Result.Failure(createResult.Error);
    }

    public async Task<Result> AddUserAsEditor(Guid userId, Guid documentId, Guid newUserId)
    {
        var addUserResult = await _documentsRepository.AddUserAsEditor(userId, documentId, newUserId);

        return addUserResult.IsSuccess
            ? Result.Success()
            : Result.Failure(addUserResult.Error);
    }

    public async Task<Result> RemoveUserFromEditors(Guid userId, Guid documentId, Guid oldUserId)
    {
        var removeUserResult = await _documentsRepository.RemoveUserFromEditors(userId, documentId, oldUserId);

        return removeUserResult.IsSuccess
            ? Result.Success()
            : Result.Failure(removeUserResult.Error);
    }

    public async Task<Result<ICollection<User>>> GetAllEditors(Guid userId, Guid documentId)
    {
        var getAllEditorsResult = await _documentsRepository.GetAllEditors(userId, documentId);
        
        return getAllEditorsResult.IsSuccess
            ? Result<ICollection<User>>.Success(getAllEditorsResult.Value)
            : Result<ICollection<User>>.Failure(getAllEditorsResult.Error);
    }

    public async Task<Result> Rename(Guid userId, Guid documentId, string name)
    {
        var renameResult = await _documentsRepository.Rename(userId, documentId, name);

        return renameResult.IsSuccess
            ? Result.Success()
            : Result.Failure(renameResult.Error);
    }

    public async Task<Result<Document>> Get(Guid userId, Guid documentId)
    {
        var documentResult = await _documentsRepository.Get(userId, documentId);

        return documentResult.IsSuccess
            ? Result<Document>.Success(documentResult.Value)
            : Result<Document>.Failure(documentResult.Error);
    }

    public async Task<Result> Update(Guid userId, Guid documentId, string newContent)
    {
        var updateResult = await _documentsRepository.Update(userId, documentId, newContent);

        return updateResult.IsSuccess
            ? Result.Success()
            : Result.Failure(updateResult.Error);
    }

    public async Task<Result> Delete(Guid userId, Guid documentId)
    {
        var deleteResult = await _documentsRepository.Delete(userId, documentId);

        return deleteResult.IsSuccess
            ? Result.Success()
            : Result.Failure(deleteResult.Error);
    }

    public async Task<Result> Download(Guid userId, Guid documentId)
    {
        throw new NotImplementedException();
    }
}