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

    public IssueUseCase(IIssueRepository issueRepository, IProjectRepository projectRepository, ITransactionRepo transactionRepo, ILogger<IssueUseCase> logger)
    {
        _issueRepository = issueRepository;
        _transactionRepo = transactionRepo;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<IssueDomain> CreateIssue(CreateIssueReq param)
    {
        var column = await _projectRepository.FindColumn(new GetColumnParams
        {
            Name = param.Status
        });

        if (column == null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ColumnId is invalid or does not exist."));

        var issue = new IssueDomain()
        {
            ProjectId = param.ProjectId,
            ReporterId = param.ReporterId ?? string.Empty,
            Status = column.Name,
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

        return result;
    }

    public async Task<IssueDomain> GetIssue(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID cannot be empty"));

        var issue = await _issueRepository.GetIssue(id) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {id} not found"));

        return issue;
    }

    public async Task<IssueDomain> UpdateIssue(IssueDomain updateData)
    {
        if (string.IsNullOrEmpty(updateData.Id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID cannot be empty"));

        var updatedIssue = await _transactionRepo.ExecuteAsync(async session =>
        {
            var existingIssue = await _issueRepository.GetIssue(updateData.Id);
            if (!string.IsNullOrEmpty(updateData.Status) && updateData.Status != existingIssue.Status)
            {
                var newColumn = await _projectRepository.FindColumn(new GetColumnParams
                {
                    Name = updateData.Status
                });

                if (newColumn == null)
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Column with name '{updateData.Status}' not found"));

                var oldColumn = await _projectRepository.FindColumn(new GetColumnParams
                {
                    Name = existingIssue.Status
                });

                if (oldColumn == null)
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Original column with name '{existingIssue.Status}' not found"));

                await _projectRepository.UpdateColumn(new UpdateColumnParams
                {
                    AddIssueId = updateData.Id,
                    ColumnId = newColumn.Id,
                });
                await _projectRepository.UpdateColumn(new UpdateColumnParams
                {
                    RemoveIssueId = existingIssue.Id,
                    ColumnId = oldColumn.Id,
                });
            }
            var updatedIssueBody = GetUpdatedIssueBody(updateData, existingIssue);
            return await _issueRepository.UpdateIssue(updatedIssueBody);
        });

        return updatedIssue;
    }

    private IssueDomain GetUpdatedIssueBody(IssueDomain newIssue, IssueDomain existingIssue)
    {
        if (existingIssue == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Issue not found"));

        if (!string.IsNullOrEmpty(newIssue.Title))
            existingIssue.Title = newIssue.Title;

        if (!string.IsNullOrEmpty(newIssue.Description))
            existingIssue.Description = newIssue.Description;

        if (!string.IsNullOrEmpty(newIssue.Summary))
            existingIssue.Summary = newIssue.Summary;

        if (newIssue.StoryPoint != null && newIssue.StoryPoint != existingIssue.StoryPoint)
            existingIssue.StoryPoint = newIssue.StoryPoint;

        if (!string.IsNullOrEmpty(newIssue.AssigneeId))
            existingIssue.AssigneeId = newIssue.AssigneeId;

        if (!string.IsNullOrEmpty(newIssue.ParentId))
            existingIssue.ParentId = newIssue.ParentId;

        if (!string.IsNullOrEmpty(newIssue.ReporterId))
            existingIssue.ReporterId = newIssue.ReporterId;

        if (newIssue.Type.HasValue && newIssue.Type.Value != existingIssue.Type)
            existingIssue.Type = newIssue.Type;

        if (newIssue.Priority.HasValue && newIssue.Priority.Value != existingIssue.Priority)
            existingIssue.Priority = newIssue.Priority;

        if (!string.IsNullOrEmpty(newIssue.Status) && newIssue.Status != existingIssue.Status)
            existingIssue.Status = newIssue.Status;

        if (newIssue.Attachments != null && newIssue.Attachments.Any())
            existingIssue.Attachments = newIssue.Attachments;

        if (!string.IsNullOrEmpty(newIssue.SprintId))
            existingIssue.SprintId = newIssue.SprintId;

        if (!string.IsNullOrEmpty(newIssue.ProjectId))
            existingIssue.ProjectId = newIssue.ProjectId;

        existingIssue.UpdatedAt = DateTime.UtcNow;

        return existingIssue;
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
            Name = issue.Status
        });
        await _projectRepository.UpdateColumn(new UpdateColumnParams
        {
            RemoveIssueId = issue.Id,
            ColumnId = column.Id,
        });
        await _issueRepository.DeleteIssue(id);
    }

    public async Task<(List<IssueDomain> Issues, int TotalCount)> ListIssues(GetIssuesParams param)
    {
        if (param.Page <= 0) param.Page = 1;
        if (param.Limit <= 0) param.Limit = 10;

        return await _issueRepository.ListIssues(param);
    }
}
