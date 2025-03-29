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
            throw new ArgumentException("Issue ID cannot be empty");

        var issue = await _issueRepository.GetIssue(id);
        return issue ?? throw new KeyNotFoundException($"Issue with ID {id} not found");
    }

    public async Task<IssueDomain> UpdateIssue(IssueDomain issue)
    {
        if (string.IsNullOrEmpty(issue.Id))
            throw new ArgumentException("Issue ID cannot be empty");

        if (string.IsNullOrEmpty(issue.Title))
            throw new ArgumentException("Issue title cannot be empty");

        var existingIssue = await _issueRepository.GetIssue(issue.Id);
        if (existingIssue == null)
            throw new KeyNotFoundException($"Issue with ID {issue.Id} not found");

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
            throw new ArgumentException("Issue ID cannot be empty");

        var issue = await _issueRepository.GetIssue(id);
        if (issue == null)
            throw new KeyNotFoundException($"Issue with ID {id} not found");

        await _issueRepository.DeleteIssue(id);
    }

    public async Task<(List<IssueDomain> Issues, int TotalCount)> ListIssues(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Page number must be greater than 0");

        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        return await _issueRepository.ListIssues(page, pageSize);
    }
}
