using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using TaskFlow.SprintService;

namespace MainService.Domain.UseCases;

public class SprintUseCase
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IPublisherService _publisher;

    private readonly IIssueRepository _issueRepository;

    public SprintUseCase(
        ISprintRepository sprintRepository,
        IProjectMemberRepository projectMemberRepository,
        IPublisherService publisher,
        IIssueRepository issueRepository)
    {
        _sprintRepository = sprintRepository;
        _projectMemberRepository = projectMemberRepository;
        _publisher = publisher;
        _issueRepository = issueRepository;
    }

    public async Task<SprintDomain> CreateSprint(SprintDomain sprint)
    {
        // Add any business logic/validation here
        if (string.IsNullOrEmpty(sprint.Name))
            throw new ArgumentException("Sprint name cannot be empty");

        if (string.IsNullOrEmpty(sprint.ProjectId))
            throw new ArgumentException("Project ID cannot be empty");

        if (sprint.DateStarted >= sprint.DateEnded)
            throw new ArgumentException("Start date must be before end date");

        var newSprint = await _sprintRepository.CreateSprint(sprint);

        // Notify all project members if the sprint is starting today or in the future
        if (sprint.DateStarted.Date >= DateTime.UtcNow.Date)
        {
            // Get all project members
            var members = await _projectMemberRepository.GetProjectMembersAsync(sprint.ProjectId, 1, int.MaxValue);

            var notificationTasks = members.Select(member =>
                _publisher.EmitQueue(QueueTopicName.NOTIFICATIONS, QueueMessageAction.NOTIFICATIONS_CREATE_NEW_NOTIFICATION, new INotificationMessage
                {
                    Type = NotificationType.SPRINT_STARTED.ToString(),
                    RecipientId = member.Id
                })
            ).ToList();

            await Task.WhenAll(notificationTasks);
        }

        return newSprint;
    }

    public async Task<SprintDomain> GetSprint(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Sprint ID cannot be empty");

        var sprint = await _sprintRepository.GetSprint(id);
        return sprint ?? throw new KeyNotFoundException($"Sprint with ID {id} not found");
    }

    public async Task<SprintDomain> UpdateSprint(SprintDomain sprint)
    {
        if (string.IsNullOrEmpty(sprint.Id))
            throw new ArgumentException("Sprint ID cannot be empty");

        if (string.IsNullOrEmpty(sprint.Name))
            throw new ArgumentException("Sprint name cannot be empty");

        var existingSprint = await _sprintRepository.GetSprint(sprint.Id);
        if (existingSprint == null)
            throw new KeyNotFoundException($"Sprint with ID {sprint.Id} not found");

        // Update only provided fields
        existingSprint.Name = sprint.Name;
        existingSprint.DateStarted = sprint.DateStarted;
        existingSprint.DateEnded = sprint.DateEnded;
        existingSprint.Duration = sprint.Duration;
        existingSprint.Goal = sprint.Goal;
        existingSprint.ProjectId = sprint.ProjectId;
        existingSprint.UpdatedAt = DateTime.UtcNow;

        return await _sprintRepository.UpdateSprint(existingSprint);
    }

    public async Task DeleteSprint(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Sprint ID cannot be empty");

        var sprint = await _sprintRepository.GetSprint(id);
        if (sprint == null)
            throw new KeyNotFoundException($"Sprint with ID {id} not found");

        await _sprintRepository.DeleteSprint(id);
    }

    public async Task<(List<SprintDomain> Sprints, int TotalCount)> ListSprints(ListSprintParams param)
    {
        return await _sprintRepository.ListSprints(param);
    }
    public async Task<(SprintDomain, SprintStats, List<SprintDailyStats>)> GetStats(string sprint_id)
    {
        var sprint = await GetSprint(sprint_id);
        SprintStats sprintStats = await _sprintRepository.GetSprintStats(sprint_id, sprint.ProjectId);
        List<SprintDailyStats> dailyData = await GetSprintDailyStats(sprint);
        return (sprint, sprintStats, dailyData);
    }

    private async Task<List<SprintDailyStats>> GetSprintDailyStats(SprintDomain sprint)
    {

        var issues = await _issueRepository.ListIssues(new GetIssuesParams
        {
            SprintIds = [sprint.Id],
            Unpaged = true
        });

        var start = sprint.DateStarted.ToUniversalTime().Date;
        var end = sprint.DateEnded.ToUniversalTime().Date;

        var dailyStats = new List<SprintDailyStats>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var completedCount = issues.Items.Count(i =>
                i.CompletedAt != DateTime.MinValue &&
                i.CompletedAt.ToUniversalTime().Date <= date);

            dailyStats.Add(new SprintDailyStats
            {
                Date = date.ToString("yyyy-MM-dd"),
                CompletedIssues = completedCount,
                RemainingIssues = issues.TotalCount - completedCount
            });
        }
        return dailyStats;
    }
}