using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class TeamMemberRepository : ITeamMemberRepository
{
    private readonly IMongoCollection<TeamMember> _collection;
    private readonly IMapper _mapper;

    public TeamMemberRepository(MongoDbService mongoDbService, IMapper mapper)
    {
        var database = mongoDbService.Database;
        _collection = database.GetCollection<TeamMember>("team_members");
        _mapper = mapper;
    }

    public async Task<TeamMemberDomain?> GetAsync(string id)
    {
        var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return entity == null ? null : _mapper.Map<TeamMemberDomain>(entity);
    }

    public async Task<TeamMemberDomain?> GetByProjectAndUserAsync(string projectId, string userId)
    {
        var entity = await _collection.Find(x => x.ProjectId == projectId && x.UserId == userId)
            .FirstOrDefaultAsync();
        return entity == null ? null : _mapper.Map<TeamMemberDomain>(entity);
    }

    public async Task<IEnumerable<TeamMemberDomain>> GetProjectMembersAsync(string projectId, int page, int limit)
    {
        var entities = await _collection.Find(x => x.ProjectId == projectId)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();
        return _mapper.Map<IEnumerable<TeamMemberDomain>>(entities);
    }

    public async Task<int> GetProjectMembersCountAsync(string projectId)
    {
        return (int)await _collection.CountDocumentsAsync(x => x.ProjectId == projectId);
    }

    public async Task<TeamMemberDomain> AddAsync(TeamMemberDomain member)
    {
        var entity = _mapper.Map<TeamMember>(member);
        await _collection.InsertOneAsync(entity);
        member.Id = entity.Id;
        return member;
    }

    public async Task<TeamMemberDomain> UpdateAsync(TeamMemberDomain member)
    {
        var entity = _mapper.Map<TeamMember>(member);
        var result = await _collection.ReplaceOneAsync(x => x.Id == member.Id, entity);
        if (result.MatchedCount == 0)
            throw new Exception("Team member not found");
        return member;
    }

    public async Task DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        if (result.DeletedCount == 0)
            throw new Exception("Team member not found");
    }

    public async Task<bool> IsUserProjectMemberAsync(string projectId, string userId)
    {
        var member = await GetByProjectAndUserAsync(projectId, userId);
        return member != null;
    }

    public async Task<bool> HasProjectRole(string projectId, string userId, TeamMemberRole role)
    {
        var member = await GetByProjectAndUserAsync(projectId, userId);
        return member?.Role == role;
    }

    public async Task<IEnumerable<TeamMemberDomain>> GetUserTeamsAsync(string userId, int page, int limit)
    {
        var entities = await _collection.Find(x => x.UserId == userId)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();
        return _mapper.Map<IEnumerable<TeamMemberDomain>>(entities);
    }

    public async Task<int> GetUserTeamsCountAsync(string userId)
    {
        return (int)await _collection.CountDocumentsAsync(x => x.UserId == userId);
    }
}