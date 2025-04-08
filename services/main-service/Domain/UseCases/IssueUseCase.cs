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

    public async Task<IssueDomain> CreateIssue(IssueDomain issue)
    {
        return await _issueRepository.CreateIssue(issue);
    }

    public async Task<IssueDomain> GetIssue(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID cannot be empty"));

        var issue = await _issueRepository.GetIssue(id) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {id} not found"));

        return issue;
    }

    public async Task<IssueDomain> UpdateIssue(IssueDomain issue)
    {
        if (string.IsNullOrEmpty(issue.Id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue ID cannot be empty"));

        if (string.IsNullOrEmpty(issue.Title))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Issue title cannot be empty"));


        var existingIssue = await _issueRepository.GetIssue(issue.Id);

        if (existingIssue == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Issue with ID {issue.Id} not found"));

        // Update only provided fields
        existingIssue.Title = issue.Title;
        existingIssue.Description = issue.Description;
        existingIssue.Status = issue.Status;
        existingIssue.Priority = issue.Priority;
        existingIssue.AssigneeId = issue.AssigneeId;
        existingIssue.UpdatedAt = DateTime.UtcNow;

        return await _issueRepository.UpdateIssue(existingIssue);
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
