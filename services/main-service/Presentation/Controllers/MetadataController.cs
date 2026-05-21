using Grpc.Core;
using MainService.Domain.UseCases;
using TaskFlow.MetadataService;

public class MetadataController : MetadataService.MetadataServiceBase
{
    private readonly MetadataUseCase _metadataUseCase;
    private readonly ILogger<MetadataController> _logger;

    public MetadataController(MetadataUseCase metadataUseCase, ILogger<MetadataController> logger)
    {
        _metadataUseCase = metadataUseCase;
        _logger = logger;
    }

    public override async Task<CreatePresignedURLRes> CreatePresignedURL(
        CreatePresignedURLReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId")
            ? context.UserState["UserId"] as string
            : null;

        if (string.IsNullOrEmpty(userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated to upload files"));

        return await _metadataUseCase.CreatePresignedURL(new CreatePresignedURLReq
        {
            ProjectId = request.ProjectId,
            UserId = userId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            UploadType = request.UploadType
        });
    }

    public override async Task<DeleteFileRes> DeleteFile(
        DeleteFileReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId")
            ? context.UserState["UserId"] as string
            : null;

        if (string.IsNullOrEmpty(userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated to delete files"));

        return await _metadataUseCase.DeleteFile(request);
    }
}