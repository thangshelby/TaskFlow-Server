using AutoMapper;
using FluentValidation;
using Grpc.Core;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using TaskFlow.IssueService;
using Google.Protobuf.WellKnownTypes;
using System;

public class IssueController : IssueService.IssueServiceBase
{
    private readonly IssueUseCase _issueUseCase;
    private readonly UserUseCase _userUseCase;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateIssueReq> _createIssueValidator;
    private readonly IValidator<UpdateIssueReq> _updateIssueValidator;
    private readonly IValidator<ListIssuesReq> _listIssuesValidator;

    public IssueController(
        IssueUseCase issueUseCase,
        UserUseCase userUseCase,
        IMapper mapper,
        IValidator<CreateIssueReq> createIssueValidator,
        IValidator<UpdateIssueReq> updateIssueValidator,
        IValidator<ListIssuesReq> listIssuesValidator)
    {
        _issueUseCase = issueUseCase ?? throw new ArgumentNullException(nameof(issueUseCase));
        _userUseCase = userUseCase ?? throw new ArgumentNullException(nameof(userUseCase));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _createIssueValidator = createIssueValidator ?? throw new ArgumentNullException(nameof(createIssueValidator));
        _updateIssueValidator = updateIssueValidator ?? throw new ArgumentNullException(nameof(updateIssueValidator));
        _listIssuesValidator = listIssuesValidator ?? throw new ArgumentNullException(nameof(listIssuesValidator));
    }

    public override async Task<CreateIssueRes> CreateIssue(CreateIssueReq request, ServerCallContext context)
    {
        // Get the current user ID from context
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated to create issues"));
        }

        // Get the current user to verify they exist
        var currentUser = await _userUseCase.GetById(userId);
        if (currentUser == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Current user not found"));
        }

        var validationResult = await _createIssueValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        // Create issue with the current user as reporter
        var result = await _issueUseCase.CreateIssue(
            projectId: request.ProjectId,
            title: request.Title,
            reporterId: userId, // Set the reporter ID to the current user
            sprintId: string.IsNullOrEmpty(request.SprintId) ? null : request.SprintId,
            assigneeId: string.IsNullOrEmpty(request.AssigneeId) ? null : request.AssigneeId
        );

        return new CreateIssueRes { Data = _mapper.Map<IssueRes>(result) };
    }

    public override async Task<IssueRes> GetIssue(GetIssueReq request, ServerCallContext context)
    {
        try
        {
            var result = await _issueUseCase.GetIssue(request.Id);
            return _mapper.Map<IssueRes>(result);
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<IssueRes> UpdateIssue(UpdateIssueReq request, ServerCallContext context)
    {
        // ID and ProjectId are now part of the request message from URL path
        if (string.IsNullOrEmpty(request.ProjectId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Project ID is required"));
        }

        if (string.IsNullOrEmpty(request.Id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID is required"));
        }

        var validationResult = await _updateIssueValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        // Get existing issue and update its properties
        var existingIssue = await _issueUseCase.GetIssue(request.Id);

        if (!string.IsNullOrEmpty(request.Title))
            existingIssue.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Description))
            existingIssue.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Type))
            existingIssue.Type = System.Enum.Parse<IssueType>(request.Type, true);
        if (!string.IsNullOrEmpty(request.Status))
            existingIssue.Status = System.Enum.Parse<IssueStatus>(request.Status, true);
        if (!string.IsNullOrEmpty(request.Priority))
            existingIssue.Priority = System.Enum.Parse<IssuePriority>(request.Priority, true);
        if (request.StoryPoint > 0)
            existingIssue.StoryPoint = request.StoryPoint;

        // Use domain methods for sprint and assignee updates
        if (request.SprintId != null)
            existingIssue.AssignToSprint(request.SprintId);
        if (request.AssigneeId != null)
            existingIssue.AssignToUser(request.AssigneeId);

        var result = await _issueUseCase.UpdateIssue(existingIssue);
        return _mapper.Map<IssueRes>(result);
    }

    public override async Task<Empty> DeleteIssue(DeleteIssueReq request, ServerCallContext context)
    {
        try
        {
            await _issueUseCase.DeleteIssue(request.Id);
            return new Empty();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ListIssuesRes> ListIssues(ListIssuesReq request, ServerCallContext context)
    {
        if (request.Page <= 0) request.Page = 1;
        if (request.Limit <= 0) request.Limit = 10;

        var validationResult = await _listIssuesValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var (issues, totalCount) = await _issueUseCase.ListIssues(request.ProjectId, request.Page, request.Limit);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

        var response = new ListIssuesRes();
        response.Data.AddRange(_mapper.Map<List<IssueRes>>(issues));
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
