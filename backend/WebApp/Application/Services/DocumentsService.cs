using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Utils;
using Core.Enums;
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

    public async Task<Result> SetUserPermission(Guid ownerId, DocumentRole documentRole, Guid documentId, Guid userId)
    {
        Result result = documentRole switch
        {
            DocumentRole.Editor => await _documentsRepository.AddUserAsEditor(ownerId, documentId, userId),
            DocumentRole.Reader => await _documentsRepository.AddUserAsReader(ownerId, documentId, userId),
            DocumentRole.NoAccess => await _documentsRepository.ClearUserPermissions(ownerId, documentId, userId),
            _ => Result.Failure(new Error("Такой роли не существует", ErrorType.AuthorizationError))
        };

        return result;
    }

    public async Task<Result<ICollection<UserUserNameDto>>> GetAllEditors(Guid userId, Guid documentId)
    {
        var getAllEditorsResult = await _documentsRepository.GetAllEditors(userId, documentId);

        return getAllEditorsResult.IsSuccess
            ? Result<ICollection<UserUserNameDto>>.Success(getAllEditorsResult.Value
                .Select(u => new UserUserNameDto(u.UserName)).ToList())
            : Result<ICollection<UserUserNameDto>>.Failure(getAllEditorsResult.Error);
    }

    public async Task<Result<ICollection<UserUserNameDto>>> GetAllReaders(Guid userId, Guid documentId)
    {
        var getAllReadersResult = await _documentsRepository.GetAllReaders(userId, documentId);

        return getAllReadersResult.IsSuccess
            ? Result<ICollection<UserUserNameDto>>.Success(getAllReadersResult.Value
                .Select(u => new UserUserNameDto(u.UserName)).ToList())
            : Result<ICollection<UserUserNameDto>>.Failure(getAllReadersResult.Error);
    }

    public async Task<Result<ICollection<UserWithDocumentRoleDto>>> GetAllUsers(Guid ownerId, Guid documentId)
    {
        var allUsersResult = await _documentsRepository.GetAllUsers(ownerId, documentId);

        return allUsersResult.IsSuccess
            ? Result<ICollection<UserWithDocumentRoleDto>>.Success(allUsersResult.Value)
            : Result<ICollection<UserWithDocumentRoleDto>>.Failure(allUsersResult.Error);
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

    public async Task<Result<DocumentRole>> GetUserRole(Guid ownerId, Guid documentId, Guid userId)
    {
        var userRoleResult = await _documentsRepository.GetUserRole(ownerId, documentId, userId);

        return userRoleResult.IsSuccess
            ? Result<DocumentRole>.Success(userRoleResult.Value)
            : Result<DocumentRole>.Failure(userRoleResult.Error);
    }

    public async Task<Result> Update(Guid userId, Guid documentId)
    {
        var updateResult = await _documentsRepository.Update(userId, documentId);

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