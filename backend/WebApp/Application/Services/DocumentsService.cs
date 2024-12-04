using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Utils;
using Core.Enums;
using Core.Models;
using Microsoft.Extensions.Options;
using Exception = System.Exception;

namespace Application.Services;

public class DocumentsService : IDocumentsService
{
    private readonly IDocumentsRepository _documentsRepository;
    private readonly MinioService _minioService;
    private readonly MinIoRequirement _minioConfig;

    public DocumentsService(IDocumentsRepository documentsRepository, MinioService minioService,
        IOptions<MinIoRequirement> minIoOptions)
    {
        _documentsRepository = documentsRepository;
        _minioService = minioService;
        _minioConfig = minIoOptions.Value;
    }

    public async Task<Result> Create(Guid userId, string name)
    {
        var createResult = await _documentsRepository.Create(userId, name);

        if (!createResult.IsSuccess)
        {
            return Result.Failure(createResult.Error);
        }

        var fileName = $"{createResult.Value}.txt";
        var fileContent = $"This is a document for {name}";

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

        try
        {
            await _minioService.UploadFileAsync(_minioConfig.BucketName, fileName, stream, "text/plain");
        }
        catch (Exception e)
        {
            await _documentsRepository.Delete(userId, createResult.Value);
            return Result.Failure(new Error(e.Message, ErrorType.ServerError));
        }

        return Result.Success();
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

    public async Task<Result> Delete(Guid userId, Guid documentId)
    {
        var deleteResult = await _documentsRepository.Delete(userId, documentId);

        if (!deleteResult.IsSuccess)
        {
            Result.Failure(deleteResult.Error);
        }

        try
        {
            await _minioService.DeleteFileAsync(_minioConfig.BucketName, deleteResult.Value);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error(e.Message, ErrorType.ServerError));
        }
    }

    public async Task<Result> Download(Guid userId, Guid documentId)
    {
        throw new NotImplementedException();
    }
}