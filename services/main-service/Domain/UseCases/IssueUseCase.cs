using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;

namespace MainService.Domain.UseCases;

public class IssueUseCase
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IProjectRepository _projectRepository;
    private readonly IIssueRepository _issueRepository;

    public IssueUseCase(IIssueRepository issueRepository, IProjectRepository projectRepository, ITransactionRepo transactionRepo)
    {
        _issueRepository = issueRepository;
        _transactionRepo = transactionRepo;
        _projectRepository = projectRepository;
    }

    public async Task<IssueDomain> CreateIssue(string projectId, string title, string reporterId, string? sprintId = null, string? assigneeId = null)
    {
        var issue = new IssueDomain
        {
            ProjectId = projectId,
            ReporterId = reporterId,
            Title = title
        };

        if (sprintId != null)
        {
            issue.AssignToSprint(sprintId);
        }

        if (assigneeId != null)
        {
            issue.AssignToUser(assigneeId);
        }

        return await _issueRepository.CreateIssue(issue);
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
            var updatedIssueBody = GetUpdatedIssueBody(updateData, existingIssue);

            if (updateData.Status != existingIssue.Status)
            {
                // Remove and add issue_id -> columns project
                // await _projectRepository.UpdateColumn(new UpdateColumnParams
                // {
                //     AddIssueId = updateData.Id,
                //     ColumnId = "sad",
                // });
                // await _projectRepository.UpdateColumn(new UpdateColumnParams
                // {
                //     RemoveIssueId = existingIssue.Id,
                //     ColumnId = "sad",
                // });
            }

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

        if (newIssue.StoryPoint != existingIssue.StoryPoint)
            existingIssue.StoryPoint = newIssue.StoryPoint;

        if (!string.IsNullOrEmpty(newIssue.AssigneeId))
            existingIssue.AssigneeId = newIssue.AssigneeId;

        if (!string.IsNullOrEmpty(newIssue.ParentId))
            existingIssue.ParentId = newIssue.ParentId;

        if (!string.IsNullOrEmpty(newIssue.ReporterId))
            existingIssue.ReporterId = newIssue.ReporterId;

        if (newIssue.Type != existingIssue.Type)
            existingIssue.Type = newIssue.Type;

        if (newIssue.Priority != existingIssue.Priority)
            existingIssue.Priority = newIssue.Priority;

        if (newIssue.Status != existingIssue.Status)
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

        await _issueRepository.DeleteIssue(id);
    }
    public async Task<(List<IssueDomain> Issues, int TotalCount)> ListIssues(string projectId, int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Page number and size must be greater than 0"));

        return await _issueRepository.ListIssues(projectId, page, pageSize);
    }
}
