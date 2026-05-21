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

    /// <summary>
    /// Tạo presigned PUT URL để client upload file trực tiếp lên S3.
    /// S3 key được tổ chức theo: {category}/project_{projectId}/user_{userId}/{fileId}_{safeName}.{ext}
    /// Trong đó category = phần đầu của content_type (image, application, video, audio, text...)
    /// </summary>
    public async Task<(string presignedUrl, string fileUrl)> CreatePresignedURL(
        string projectId,
        string userId,
        string fileName,
        string contentType,
        string uploadType = "attachment")
    {
        var fileId = Guid.NewGuid();

        // Xác định category từ content_type (e.g. "image/png" → "image")
        var category = contentType.Split("/")[0];

        // Sanitize filename gốc để dùng trong key
        var safeName = Path.GetFileNameWithoutExtension(fileName)
            .Replace(" ", "_")
            .Replace("..", "");
        var extension = Path.GetExtension(fileName).TrimStart('.');

        // Tổ chức key theo uploadType: avatar/ hoặc attachment/
        var key = uploadType.ToLower() == "avatar"
            ? $"avatar/project_{projectId}/user_{userId}/{fileId}_{safeName}.{extension}"
            : $"attachment/{category}/raw/project_{projectId}/user_{userId}/{fileId}_{safeName}.{extension}";

        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(10),
            Verb = HttpVerb.PUT,
            ContentType = contentType
        };

        var presignedUrl = _s3Client.GetPreSignedURL(presignRequest);

        // Xây dựng file URL sạch: bỏ query string khỏi presigned URL
        var uri = new Uri(presignedUrl);
        var fileUrl = $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";

        _logger.LogInformation(
            "Created presigned URL for project {ProjectId}, user {UserId}, file {FileName}, category {Category}",
            projectId, userId, fileName, category);

        return await Task.FromResult((presignedUrl, fileUrl));
    }

    /// <summary>
    /// Xóa file khỏi S3 sử dụng file URL.
    /// </summary>
    public async Task<bool> DeleteFile(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            string key = uri.AbsolutePath.TrimStart('/');
            if (key.StartsWith(_bucketName + "/"))
            {
                key = key.Substring(_bucketName.Length + 1);
            }

            _logger.LogInformation("Deleting object from S3: Bucket={Bucket}, Key={Key}", _bucketName, key);

            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete S3 file: {FileUrl}", fileUrl);
            return false;
        }
    }
}