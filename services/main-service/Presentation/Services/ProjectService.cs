using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using TaskFlow.ProjectService;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using AutoMapper;

// namespace MainService.Presentation.Services;

public class ProjectServiceImpl : ProjectService.ProjectServiceBase
{
    private readonly ProjectUseCase _projectUseCase;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProjectReq> _createProjectValidator;
    private readonly IValidator<UpdateProjectReq> _updateProjectValidator;

    public ProjectServiceImpl(
        ProjectUseCase projectUseCase,
        IMapper mapper,
        IValidator<CreateProjectReq> createProjectValidator,
        IValidator<UpdateProjectReq> updateProjectValidator) 
    {
        _projectUseCase = projectUseCase;
        _mapper = mapper;
        _createProjectValidator = createProjectValidator;
        _updateProjectValidator = updateProjectValidator;
    }

    // Project Service Methods
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
        return new CreateProjectRes 
        { 
            Data = _mapper.Map<ProjectRes>(result)
        };
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

    public override async Task<Empty> DeleteProject(DeleteProjectReq request, ServerCallContext context)
    {
        try
        {
            await _projectUseCase.DeleteProject(request.Id);
            return new Empty();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ListProjectsRes> ListProjects(ListProjectsReq request, ServerCallContext context)
    {
        var (projects, totalCount) = await _projectUseCase.ListProjects(request.Page, request.Limit);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);
        
        return new ListProjectsRes
        {
            Data = { _mapper.Map<IEnumerable<ProjectRes>>(projects) },
            Pagination = new PaginationRes
            {
                TotalItems = totalCount,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                Limit = request.Limit
            }
        };
    }
}
