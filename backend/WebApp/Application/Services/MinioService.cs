using Application.Utils;
using Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Application.Services;

public class MinioService
{
    private readonly IMinioClient _minioClient;
    private readonly MinIoRequirement _minioConfig;

    public MinioService(IOptions<MinIoRequirement> minioOptions)
    {
        _minioConfig = minioOptions.Value;
        _minioClient = new MinioClient()
            .WithEndpoint(_minioConfig.Endpoint)
            .WithCredentials(_minioConfig.AccessKey, _minioConfig.SecretKey)
            .WithSSL(false)
            .Build();
    }

    public async Task UploadFileAsync(string bucketName, string fileName, Stream fileStream, string contentType)
    {
        var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        }

        await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType)
        );
    }

    public async Task<string> GetFileContentAsync(string bucketName, Guid documentId)
    {
        var objectName = $"{documentId}.txt";

        var objectExists = await _minioClient.StatObjectAsync(
            new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
        );

        if (objectExists is null)
        {
            throw new Exception("Файл не найден.");
        }

        using var memoryStream = new MemoryStream();

        await _minioClient.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream))
        );

        memoryStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(memoryStream);

        return await reader.ReadToEndAsync();
    }

    public async Task OverwriteFileContentAsync(string bucketName, Guid documentId, string newContent)
    {
        var objectName = $"{documentId}.txt";

        var objectExists = await _minioClient.StatObjectAsync(
            new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
        );

        if (objectExists is null)
        {
            throw new Exception("Файл не найден.");
        }

        using var memoryStream = new MemoryStream();

        await using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        {
            await writer.WriteAsync(newContent);
            await writer.FlushAsync();
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length)
                .WithContentType("text/plain")
        );
    }

    public async Task DeleteFileAsync(string bucketName, string fileName)
    {
        var objectExists = await _minioClient.StatObjectAsync(
            new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
        );

        if (objectExists is null)
        {
            throw new Exception("Файл не найден.");
        }
        
        await _minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
        );
    }
}
