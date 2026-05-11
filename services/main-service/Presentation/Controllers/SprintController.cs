using AutoMapper;
using FluentValidation;
using Grpc.Core;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using TaskFlow.SprintService;
using MainService.Domain.Interfaces;
using BaseService;
// namespace MainService.Presentation.Services;

public class SprintController : SprintService.SprintServiceBase
{
    private readonly SprintUseCase _sprintUseCase;
    private readonly IMapper _mapper;
    private readonly ILogger<SprintController> _logger;
    private readonly IValidator<CreateSprintReq> _createSprintValidator;
    private readonly IValidator<UpdateSprintReq> _updateSprintValidator;
    private readonly IValidator<ListSprintsReq> _listSprintsValidator;

    public SprintController(
        SprintUseCase sprintUseCase,
        IMapper mapper, ILogger<SprintController> logger,
        IValidator<CreateSprintReq> createSprintValidator,
        IValidator<UpdateSprintReq> updateSprintValidator,
        IValidator<ListSprintsReq> listSprintsValidator)
    {
        _sprintUseCase = sprintUseCase ?? throw new ArgumentNullException(nameof(sprintUseCase));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
        _createSprintValidator = createSprintValidator ?? throw new ArgumentNullException(nameof(createSprintValidator));
        _updateSprintValidator = updateSprintValidator ?? throw new ArgumentNullException(nameof(updateSprintValidator));
        _listSprintsValidator = listSprintsValidator ?? throw new ArgumentNullException(nameof(listSprintsValidator));
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
        if (request.Page <= 0) request.Page = 1;
        if (request.Limit <= 0) request.Limit = 10;

        var validationResult = await _listSprintsValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var (sprints, totalCount) = await _sprintUseCase.ListSprints(new ListSprintParams
        {
            Limit = request.Limit,
            Page = request.Page,
            ProjectId = request.ProjectId,
            SprintIds = request.SprintIds.ToList(),
        });
        var totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

        var response = new ListSprintsRes();
        response.Data.AddRange(_mapper.Map<List<SprintRes>>(sprints));
        response.Pagination = new BaseService.PaginationRes
        {
            TotalItems = totalCount,
            TotalPages = totalPages,
            CurrentPage = request.Page,
            Limit = request.Limit
        };
        return response;
    }
    public override async Task<GetSrintStatsRes> GetSprintStats(GetSrintStatsReq request, ServerCallContext context)
    {
        var (sprint, sprintStats, dailyData) = await _sprintUseCase.GetStats(request.SprintId);
        var data = new SprintStatData
        {
            Sprint = _mapper.Map<SprintRes>(sprint),
            SprintStats = sprintStats
        };
        data.SprintDailyStats.AddRange(dailyData);
        var response = new GetSrintStatsRes
        {
            Data = data,
            Message = "Get sprint stats successfully",
            Status = "success"
        };

        return response;
    }
}