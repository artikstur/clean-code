using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Utils;
using Core.Enums;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class MdService : IMdService
{
    private readonly MinioService _minioService;
    private readonly MinIoRequirement _minioConfig;
    private readonly IDocumentsRepository _documentsRepository;

    public MdService(IDocumentsRepository documentsRepository, MinioService minioService,
        IOptions<MinIoRequirement> minIoOptions)
    {
        _documentsRepository = documentsRepository;
        _minioService = minioService;
        _minioConfig = minIoOptions.Value;
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

        try
        {
            var fileData = await _minioService.GetFileContentAsync(_minioConfig.BucketName, documentId);
            return Result<string>.Success(fileData);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error(ex.Message, ErrorType.ServerError));
        }
    }

    public Task<Result<string>> GetHtml(string rawMarkdown)
    {
        throw new NotImplementedException();
    }
}