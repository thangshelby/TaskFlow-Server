using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class SprintRepository : ISprintRepository
{
    private readonly IMongoCollection<Sprint> _sprints;
    private readonly IMapper _mapper;

    public SprintRepository(MongoDbService mongoDbService, IMapper mapper)
    {
        var database = mongoDbService.Database;
        _sprints = database.GetCollection<Sprint>("sprints");
        _mapper = mapper;
    }

    public async Task<SprintDomain> CreateSprint(SprintDomain sprintDomain)
    {
        var sprintEntity = _mapper.Map<Sprint>(sprintDomain);
        await _sprints.InsertOneAsync(sprintEntity);
        sprintDomain.Id = sprintEntity.Id;
        return sprintDomain;
    }

    public async Task<SprintDomain> GetSprint(string id)
    {
        var sprintEntity = await _sprints.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (sprintEntity == null)
            throw new Exception("Sprint not found");
        return _mapper.Map<SprintDomain>(sprintEntity);
    }

    public async Task<SprintDomain> UpdateSprint(SprintDomain sprintDomain)
    {
        var sprintEntity = _mapper.Map<Sprint>(sprintDomain);
        var result = await _sprints.ReplaceOneAsync(s => s.Id == sprintDomain.Id, sprintEntity);
        if (result.MatchedCount == 0)
            throw new Exception("Sprint not found");
        return sprintDomain;
    }

    public async Task DeleteSprint(string id)
    {
        var result = await _sprints.DeleteOneAsync(s => s.Id == id);
        if (result.DeletedCount == 0)
            throw new Exception("Sprint not found");
    }

    public async Task<(List<SprintDomain> Sprints, int TotalCount)> ListSprints(string projectId, int page, int pageSize)
    {
        var filter = Builders<Sprint>.Filter.Eq(s => s.ProjectId, projectId);
        var totalCount = await _sprints.CountDocumentsAsync(filter);
        var sprints = await _sprints.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        
        return (_mapper.Map<List<SprintDomain>>(sprints), (int)totalCount);
    }
}