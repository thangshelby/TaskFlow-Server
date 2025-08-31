
using MainService.Domain.Entities;

namespace MainService.Domain.Interfaces;

public interface IActivitiesRepository
{
    Task<ActivityDomain> CreateActivity(ActivityDomain activity);
    Task<ActivityDomain> GetActivity(string id);
    Task<(List<ActivityDomain>, int totalCount)> ListActivities(GetActivityParams param);
    Task DeleteActivity(string id);
}

public class GetActivityParams
{
    public string? IssueId;
    public string? ProjectId;
    public int Page;
    public int Limit;
}