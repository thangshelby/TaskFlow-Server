using AutoMapper;
using FluentValidation;
using Grpc.Core;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using TaskFlow.IssueService;
using Google.Protobuf.WellKnownTypes;
using System;
using MainService.Domain.Interfaces;

public class IssueController : IssueService.IssueServiceBase
{
    private readonly IssueUseCase _issueUseCase;
    private readonly UserUseCase _userUseCase;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateIssueReq> _createIssueValidator;
    private readonly IValidator<UpdateIssueReq> _updateIssueValidator;
    private readonly IValidator<ListIssuesReq> _listIssuesValidator;

    private readonly ILogger<IssueController> _logger;

    public IssueController(
        IssueUseCase issueUseCase,
        UserUseCase userUseCase,
        IMapper mapper,
        ILogger<IssueController> logger,
        IValidator<CreateIssueReq> createIssueValidator,
        IValidator<UpdateIssueReq> updateIssueValidator,
        IValidator<ListIssuesReq> listIssuesValidator)
    {
        _issueUseCase = issueUseCase ?? throw new ArgumentNullException(nameof(issueUseCase));
        _userUseCase = userUseCase ?? throw new ArgumentNullException(nameof(userUseCase));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
        _createIssueValidator = createIssueValidator ?? throw new ArgumentNullException(nameof(createIssueValidator));
        _updateIssueValidator = updateIssueValidator ?? throw new ArgumentNullException(nameof(updateIssueValidator));
        _listIssuesValidator = listIssuesValidator ?? throw new ArgumentNullException(nameof(listIssuesValidator));
    }

    public override async Task<CreateIssueRes> CreateIssue(CreateIssueReq request, ServerCallContext context)
    {
        var userId = context.UserState.ContainsKey("UserId") ? context.UserState["UserId"] as string : null;
        if (string.IsNullOrEmpty(userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User must be authenticated to create issues"));
        }

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

        try
        {
            var issueRequest = new CreateIssueReq
            {
                ProjectId = request.ProjectId,
                Title = request.Title,
                Summary = request.Summary,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                Type = request.Type,
                SprintId = request.SprintId,
                AssigneeId = request.AssigneeId,
                ParentId = request.ParentId,
                ReporterId = userId,
                StoryPoint = request.StoryPoint
            };

            if (request.Attachments != null)
            {
                issueRequest.Attachments.AddRange(request.Attachments);
            }

            var result = await _issueUseCase.CreateIssue(issueRequest);

            return new CreateIssueRes
            {
                Data = _mapper.Map<IssueRes>(result)
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Failed to create issue"));
        }
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
        var validationResult = await _updateIssueValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var issueDomain = _mapper.Map<IssueDomain>(request);
        var result = await _issueUseCase.UpdateIssue(issueDomain);
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
        var validationResult = await _listIssuesValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var (issues, totalCount) = await _issueUseCase.ListIssues(new GetIssuesParams
        {
            Limit = request.Limit == 0 ? 10 : request.Limit,
            Page = request.Page == 0 ? 1 : request.Page,
            ProjectId = request.ProjectId,
            Status = request.Status
        });
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
