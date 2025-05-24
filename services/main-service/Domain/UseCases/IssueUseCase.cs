using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;
using TaskFlow.IssueService;

namespace MainService.Domain.UseCases;

public class IssueUseCase
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IProjectRepository _projectRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly ILogger<IssueUseCase> _logger;
    private readonly IPublisherService _publisher;

    public IssueUseCase(IIssueRepository issueRepository, IProjectRepository projectRepository, ITransactionRepo transactionRepo, ILogger<IssueUseCase> logger, IPublisherService publisher)
    {
        _issueRepository = issueRepository;
        _transactionRepo = transactionRepo;
        _projectRepository = projectRepository;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<IssueDomain> CreateIssue(CreateIssueReq param)
    {
        var column = await _projectRepository.FindColumn(new GetColumnParams
        {
            ColumnId = param.ColumnId
        });

        if (column == null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ColumnId is invalid or does not exist."));

        var issue = new IssueDomain()
        {
            ProjectId = param.ProjectId,
            ReporterId = param.ReporterId ?? string.Empty,
            ColumnId = param.ColumnId,
            Title = param.Title,
            Summary = param.Summary ?? string.Empty,
            Description = param.Description ?? string.Empty,
            Type = Enum.TryParse<IssueType>(param.Type, true, out var type) ? type : IssueType.Task,
            Priority = Enum.TryParse<IssuePriority>(param.Priority, true, out var priority) ? priority : IssuePriority.Medium,
            StoryPoint = param.StoryPoint,
            ParentId = param.ParentId ?? string.Empty
        };

        if (param.SprintId != null)
        {
            issue.AssignToSprint(param.SprintId);
        }
        if (param.AssigneeId != null)
        {
            issue.AssignToUser(param.AssigneeId);
        }

        var result = await _transactionRepo.ExecuteAsync(async session =>
        {
            var newIssue = await _issueRepository.CreateIssue(issue);

            await _projectRepository.UpdateColumn(new UpdateColumnParams
            {
                AddIssueId = newIssue.Id,
                ColumnId = column.Id,
            });

            return newIssue;
        });

        await _publisher.Emit(new IActivitiesMessage
        {
            EventType = ActivitiesMessageAction.ISSUE_CREATED,
            NewIssue = result,
            OldIssue = null
        });

        return result;
    }

    public async Task<IssueDomain> GetIssue(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID cannot be empty"));

        var issue = await _issueRepository.GetIssue(id) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {id} not found"));

        return issue;
    }

    public async Task<IssueDomain> UpdateIssue(UpdateIssueParams updateData)
    {
        if (string.IsNullOrEmpty(updateData.IssueId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID cannot be empty"));

        var updatedIssue = await _transactionRepo.ExecuteAsync(async session =>
        {
            var existingIssue = await _issueRepository.GetIssue(updateData.IssueId);
            if (!string.IsNullOrEmpty(updateData.ColumnId) && updateData.ColumnId != existingIssue.ColumnId)
            {
                var newColumn = await _projectRepository.FindColumn(new GetColumnParams
                {
                    ColumnId = updateData.ColumnId
                });

                if (newColumn == null)
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Column with id '{updateData.ColumnId}' not found"));

                var oldColumn = await _projectRepository.FindColumn(new GetColumnParams
                {
                    ColumnId = existingIssue.ColumnId
                });

                if (oldColumn == null)
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Original column with id '{existingIssue.ColumnId}' not found"));

                await _projectRepository.UpdateColumn(new UpdateColumnParams
                {
                    AddIssueId = updateData.IssueId,
                    ColumnId = newColumn.Id,
                });
                await _projectRepository.UpdateColumn(new UpdateColumnParams
                {
                    RemoveIssueId = existingIssue.Id,
                    ColumnId = oldColumn.Id,
                });
            }

            return await _issueRepository.UpdateIssue(updateData);
        });

        return updatedIssue;
    }

    public async Task DeleteIssue(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID cannot be empty"));

        var issue = await _issueRepository.GetIssue(id);

        if (issue == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {id} not found"));

        var column = await _projectRepository.FindColumn(new GetColumnParams
        {
            ColumnId = issue.ColumnId
        });
        await _projectRepository.UpdateColumn(new UpdateColumnParams
        {
            RemoveIssueId = issue.Id,
            ColumnId = column.Id,
        });
        await _issueRepository.DeleteIssue(id);
    }

    public async Task OnIssueChanged(IssueDomain? oldIssue, IssueDomain newIssue)
    {
        _logger.LogInformation("receive message");
        MongoDocumentLogUtil.LogObject(_logger, newIssue);
    }
    public async Task<(List<IssueDomain> Issues, int TotalCount)> ListIssues(GetIssuesParams param)
    {
        if (param.Page <= 0) param.Page = 1;
        if (param.Limit <= 0) param.Limit = 10;

        return await _issueRepository.ListIssues(param);
    }
}
