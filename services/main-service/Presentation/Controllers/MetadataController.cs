using AutoMapper;
using Grpc.Core;
using MainService.Domain.UseCases;
using TaskFlow.MetadataService;

public class MetadataController : MetadataService.MetadataServiceBase
{
    private readonly MetadataUseCase _metadataUseCase;
    private readonly IMapper _mapper;
    private readonly ILogger<MetadataController> _logger;

    public MetadataController(MetadataUseCase metadataUseCase, IMapper mapper, ILogger<MetadataController> logger)
    {
        _metadataUseCase = metadataUseCase;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<CreatePresignedtURLImageRes> CreatePresignedtURLImage(CreatePresignedURLImageReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated to create issues"));
        }

        return await _metadataUseCase.CreatePresignedtURLImage(new CreatePresignedURLImageReq
        {
            ProjectId = request.ProjectId,
        });
    } 
}