using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;

namespace MainService.Domain.UseCases;

public class IssueUseCase
{
    private readonly IIssueRepository _issueRepository;

    public IssueUseCase(IIssueRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task<IssueDomain> CreateIssue(string projectId, string title, string reporterId, string? sprintId = null, string? assigneeId = null)
    {
        var issue = new IssueDomain(projectId, reporterId)
        {
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

        if (string.IsNullOrEmpty(updateData.Title))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue title cannot be empty"));

        var existingIssue = await _issueRepository.GetIssue(updateData.Id);

        if (existingIssue == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {updateData.Id} not found"));

        // Update basic fields
        existingIssue.Title = updateData.Title;
        existingIssue.Description = updateData.Description;
        existingIssue.Status = updateData.Status;
        existingIssue.Priority = updateData.Priority;
        
        // Use domain methods for sprint and assignee updates
        existingIssue.AssignToSprint(updateData.SprintId);
        existingIssue.AssignToUser(updateData.AssigneeId);

        return await _issueRepository.UpdateIssue(existingIssue);
    }

    public async Task<IssueDomain> AssignToSprint(string issueId, string? sprintId)
    {
        var issue = await GetIssue(issueId);
        issue.AssignToSprint(sprintId);
        return await _issueRepository.UpdateIssue(issue);
    }

    public async Task<IssueDomain> AssignToUser(string issueId, string? assigneeId)
    {
        var issue = await GetIssue(issueId);
        issue.AssignToUser(assigneeId);
        return await _issueRepository.UpdateIssue(issue);
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
