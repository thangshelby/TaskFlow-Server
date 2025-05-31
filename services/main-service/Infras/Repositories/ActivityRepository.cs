using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class ActivitiesRepository : IActivitiesRepository
{
    private readonly ILogger<ActivitiesRepository> _logger;
    private readonly IMongoCollection<Activity> _activities;
    private readonly IMapper _mapper;

    public ActivitiesRepository(MongoDbService mongoDbService, IMapper mapper, ILogger<ActivitiesRepository> logger)
    {
        _logger = logger;
        _mapper = mapper;
        var database = mongoDbService.Database;
        _activities = database.GetCollection<Activity>("activities");
    }

    public async Task<ActivityDomain> CreateActivity(ActivityDomain activityDomain)
    {
        try
        {
            var entity = _mapper.Map<Activity>(activityDomain);
            await _activities.InsertOneAsync(entity);
            activityDomain.Id = entity.Id;
            return activityDomain;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert activity");
            throw;
        }
    }

    public async Task<ActivityDomain> GetActivity(string id)
    {
        var entity = await _activities.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (entity == null)
            throw new Exception("Activity not found");

        return _mapper.Map<ActivityDomain>(entity);
    }

    public async Task<(List<ActivityDomain>, int totalCount)> ListActivities(GetActivityParams param)
    {
        var filter = Builders<Activity>.Filter.Empty;

        if (!string.IsNullOrEmpty(param.IssueId))
        {
            filter = Builders<Activity>.Filter.Eq(a => a.IssueId, param.IssueId);
        }

        var totalCount = (int)await _activities.CountDocumentsAsync(filter);

        var entities = await _activities
            .Find(filter)
            .SortByDescending(a => a.CreatedAt)
            .Skip((param.Page - 1) * param.Limit)
            .Limit(param.Limit)
            .ToListAsync();

        var domains = _mapper.Map<List<ActivityDomain>>(entities);
        return (domains, totalCount);
    }

    public async Task DeleteActivity(string id)
    {
        var result = await _activities.DeleteOneAsync(a => a.Id == id);
        if (result.DeletedCount == 0)
            throw new Exception("Activity not found");
    }
}
