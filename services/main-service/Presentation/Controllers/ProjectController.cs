using AutoMapper;
using FluentValidation;
using Grpc.Core;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using TaskFlow.ProjectService;
using MainService.Domain.Interfaces;

public class ProjectController : ProjectService.ProjectServiceBase
{
    private readonly ProjectUseCase _projectUseCase;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProjectReq> _createProjectValidator;
    private readonly IValidator<UpdateProjectReq> _updateProjectValidator;
    private readonly IValidator<ListProjectsReq> _listProjectsValidator;
    private readonly ILogger<ProjectController> _logger;
    public ProjectController(
        ProjectUseCase projectUseCase,
        IMapper mapper,
        ILogger<ProjectController> logger,
        IValidator<CreateProjectReq> createProjectValidator,
        IValidator<UpdateProjectReq> updateProjectValidator,
        IValidator<ListProjectsReq> listProjectsValidator)
    {
        _projectUseCase = projectUseCase;
        _mapper = mapper;
        _logger = logger;
        _createProjectValidator = createProjectValidator;
        _updateProjectValidator = updateProjectValidator;
        _listProjectsValidator = listProjectsValidator;
    }

    public override async Task<CreateProjectRes> CreateProject(CreateProjectReq request, ServerCallContext context)
    {
        var validationResult = await _createProjectValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var projectDomain = _mapper.Map<ProjectDomain>(request);
        var result = await _projectUseCase.CreateProject(projectDomain);
        return new CreateProjectRes { Data = _mapper.Map<ProjectRes>(result) };
    }

    public override async Task<ProjectRes> GetProject(GetProjectReq request, ServerCallContext context)
    {
        try
        {
            var result = await _projectUseCase.GetProject(request.Id);
            return _mapper.Map<ProjectRes>(result);
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ProjectRes> UpdateProject(UpdateProjectReq request, ServerCallContext context)
    {
        var validationResult = await _updateProjectValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var projectDomain = _mapper.Map<ProjectDomain>(request);
        var result = await _projectUseCase.UpdateProject(projectDomain);
        return _mapper.Map<ProjectRes>(result);
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> DeleteProject(DeleteProjectReq request, ServerCallContext context)
    {
        try
        {
            await _projectUseCase.DeleteProject(request.Id);
            return new Google.Protobuf.WellKnownTypes.Empty();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ListProjectsRes> ListProjects(ListProjectsReq request, ServerCallContext context)
    {
        if (request.Page <= 0) request.Page = 1;
        if (request.Limit <= 0) request.Limit = 10;

        var validationResult = await _listProjectsValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var (projects, totalCount) = await _projectUseCase.ListProjects(new ListProjectParams
        {
            Limit = request.Limit,
            Page = request.Page,
            Kw = request.Kw,
            Sort = request.Sort
        });
        var totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

        var response = new ListProjectsRes();
        response.Data.AddRange(_mapper.Map<List<ProjectRes>>(projects));
        response.Pagination = new BaseService.PaginationRes
        {
            TotalItems = totalCount,
            TotalPages = totalPages,
            CurrentPage = request.Page,
            Limit = request.Limit
        };
        return response;
    }
    public override async Task<ListProjectsRes> GetUserProjects(UserProjectsReq request, ServerCallContext context)
    {
        if (request.Page <= 0) request.Page = 1;
        if (request.Limit <= 0) request.Limit = 10;
        if (string.IsNullOrEmpty(request.UserId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));
        }

        var (projects, totalCount) = await _projectUseCase.ListProjects(new ListProjectParams
        {
            Limit = request.Limit,
            Page = request.Page,
            UserId = request.UserId,
            Kw = request.Kw,
            Sort = request.Sort
        });
        var totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);
        var response = new ListProjectsRes();
        response.Data.AddRange(_mapper.Map<List<ProjectRes>>(projects));
        response.Pagination = new BaseService.PaginationRes
        {
            TotalItems = totalCount,
            TotalPages = totalPages,
            CurrentPage = request.Page,
            Limit = request.Limit
        };
        return response;
    }
}