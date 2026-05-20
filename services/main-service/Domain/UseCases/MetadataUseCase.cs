using MainService.Domain.Interfaces;
using TaskFlow.MetadataService;

namespace MainService.Domain.UseCases;

public class MetadataUseCase
{
    private readonly ILogger<MetadataUseCase> _logger;
    private readonly IS3Repository _s3Repository;

    public MetadataUseCase(ILogger<MetadataUseCase> logger, IS3Repository s3Repository)
    {
        _logger = logger;
        _s3Repository = s3Repository;
    }

    public async Task<CreatePresignedURLRes> CreatePresignedURL(CreatePresignedURLReq req)
    {
        _logger.LogInformation(
            "Create presigned URL for project {ProjectId}, user {UserId}, file {FileName}, contentType {ContentType}, uploadType {UploadType}",
            req.ProjectId, req.UserId, req.FileName, req.ContentType, req.UploadType);

        var (presignedUrl, fileUrl) = await _s3Repository.CreatePresignedURL(
            req.ProjectId, req.UserId, req.FileName, req.ContentType, req.UploadType);

        return new CreatePresignedURLRes
        {
            PresignedUrl = presignedUrl,
            FileUrl = fileUrl
        };
    }
}
