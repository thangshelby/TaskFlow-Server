using AutoMapper;
using FluentValidation;
using Grpc.Core;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using TaskFlow.IssueService;

public class IssueController(
    IssueUseCase issueUseCase,
    IValidator<CreateIssueReq> createIssueValidator,
    IValidator<UpdateIssueReq> updateIssueValidator,

    IMapper mapper) : IssueService.IssueServiceBase
{
    private readonly IssueUseCase _issueUseCase = issueUseCase;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<CreateIssueReq> _createIssueValidator = createIssueValidator;
    private readonly IValidator<UpdateIssueReq> _updateIssueValidator = updateIssueValidator;

    public override async Task<CreateIssueRes> CreateIssue(CreateIssueReq request, ServerCallContext context)
    {
        var validationResult = await _createIssueValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
        }

        var issueDomain = _mapper.Map<IssueDomain>(request);
        var result = await _issueUseCase.CreateIssue(issueDomain);

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

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> DeleteIssue(DeleteIssueReq request, ServerCallContext context)
    {
        try
        {
            await _issueUseCase.DeleteIssue(request.Id);
            return new Google.Protobuf.WellKnownTypes.Empty();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }


    public override async Task<ListIssuesRes> ListIssues(ListIssuesReq request, ServerCallContext context)
    {
        var (issues, totalCount) = await _issueUseCase.ListIssues(request.Page, request.Limit);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);



        return new ListIssuesRes
        {
            Data = { _mapper.Map<IEnumerable<IssueRes>>(issues) },
            Pagination = {
                TotalItems = totalCount,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                Limit = request.Limit
            }
        };
    }

}
