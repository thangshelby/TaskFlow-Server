using Amazon.S3;
using Amazon.S3.Model;
using MainService.Domain.Interfaces;

namespace MainService.Infras.Repositories;

public class S3Repository : IS3Repository
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Repository> _logger;
    private readonly string _bucketName;

    public S3Repository(
        IAmazonS3 s3Client,
        ILogger<S3Repository> logger,
        IConfiguration config)
    {
        var accessKey = config["AWS:AccessKey"];
        var secretKey = config["AWS:SecretKey"];
        var bucketName = config["AWS:S3:Bucket"];

        _s3Client = new AmazonS3Client(accessKey, secretKey);
        _logger = logger;
        _bucketName = bucketName ?? throw new InvalidOperationException("AWS:S3:Bucket configuration is required.");
    }

 public async Task<string> CreatePresignedURLImage(
    string projectId,
    string userId,
    string fileExtension,
    string contentType)
{
    var fileId = Guid.NewGuid();
    _logger.LogInformation(
        "Creating presigned URL for project {ProjectId}, user {UserId}, extension {FileExtension}, contentType {ContentType}",
        projectId,
        userId,
        fileExtension,
        contentType);
    var key =
        $"images/raw/project_{projectId}/user_{userId}/{fileId}.{fileExtension}";

    var request = new GetPreSignedUrlRequest
    {
        BucketName = _bucketName,
        Key = key,
        Expires = DateTime.UtcNow.AddMinutes(5),
        Verb= HttpVerb.PUT,
        ContentType = "image/jpeg"
        
    };

    var url = _s3Client.GetPreSignedURL(request);

    return await Task.FromResult(url);
}
}