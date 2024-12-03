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
}