using AutoMapper;
using FluentValidation;
using Grpc.Core;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using TaskFlow.SprintService;

// namespace MainService.Presentation.Services;

public class SprintServiceImpl : SprintService.SprintServiceBase
{
    private readonly SprintUseCase _sprintUseCase;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateSprintReq> _createSprintValidator;
    private readonly IValidator<UpdateSprintReq> _updateSprintValidator;

    public SprintServiceImpl(
    SprintUseCase sprintUseCase,
    IMapper mapper,
    IValidator<CreateSprintReq> createSprintValidator,
    IValidator<UpdateSprintReq> updateSprintValidator)
    {
        _sprintUseCase = sprintUseCase ?? throw new ArgumentNullException(nameof(sprintUseCase));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _createSprintValidator = createSprintValidator ?? throw new ArgumentNullException(nameof(createSprintValidator));
        _updateSprintValidator = updateSprintValidator ?? throw new ArgumentNullException(nameof(updateSprintValidator));
    }

    // Sprint Service Methods
    public override async Task<CreateSprintRes> CreateSprint(CreateSprintReq request, ServerCallContext context)
    {
        var validationResult = await _createSprintValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var sprintDomain = _mapper.Map<SprintDomain>(request);
        var result = await _sprintUseCase.CreateSprint(sprintDomain);
        return new CreateSprintRes 
        { 
            Data = _mapper.Map<SprintRes>(result)
        };
    }

    public override async Task<SprintRes> GetSprint(GetSprintReq request, ServerCallContext context)
    {
        try
        {
            var result = await _sprintUseCase.GetSprint(request.Id);
            return _mapper.Map<SprintRes>(result);
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<SprintRes> UpdateSprint(UpdateSprintReq request, ServerCallContext context)
    {
        var validationResult = await _updateSprintValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var sprintDomain = _mapper.Map<SprintDomain>(request);
        var result = await _sprintUseCase.UpdateSprint(sprintDomain);
        return _mapper.Map<SprintRes>(result);
    }

    public override async Task<Empty> DeleteSprint(DeleteSprintReq request, ServerCallContext context)
    {
        try
        {
            await _sprintUseCase.DeleteSprint(request.Id);
            return new Empty();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ListSprintsRes> ListSprints(ListSprintsReq request, ServerCallContext context)
    {
        // Apply default values if not provided
        int page = request.Page > 0 ? request.Page : 1;
        int limit = request.Limit > 0 ? request.Limit : 10;

        var (sprints, totalCount) = await _sprintUseCase.ListSprints(request.ProjectId, page, limit);
        var totalPages = (int)Math.Ceiling((double)totalCount / limit);

        return new ListSprintsRes
        {
            Data = { _mapper.Map<IEnumerable<SprintRes>>(sprints) },
            Pagination = new PaginationRes
            {
                TotalItems = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                Limit = limit
            }
        };
    }

}