using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using TaskFlow.MetadataService;

namespace MainService.Domain.UseCases;

public class MetadataUseCase
{
    private readonly ILogger<MetadataUseCase> _logger;
    private readonly IS3Repository _s3Repository;

    public MetadataUseCase(ILogger<MetadataUseCase> logger, IPublisherService publisher, IS3Repository s3Repository)
    {
        _logger = logger;
        _s3Repository = s3Repository;
    }

    public async Task<CreatePresignedtURLImageRes> CreatePresignedtURLImage(CreatePresignedURLImageReq req)
    {
        _logger.LogInformation("Creating presigned URL for project {ProjectId} and user {UserId}", req.ProjectId, req.UserId, req.FileName, req.ContentType);
        var url = await _s3Repository.CreatePresignedURLImage(req.ProjectId, req.UserId, req.FileName, req.ContentType);

        _logger.LogInformation("Created presigned URL for {Url}", url);
      
        return new CreatePresignedtURLImageRes
        {
            Url = url
        };
    }
}
