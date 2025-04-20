using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly IMongoCollection<Issue> _issues;
    private readonly IProjectRepository _projectRepository;

    private readonly IMapper _mapper;

    public IssueRepository(MongoDbService mongoDbService, IMapper mapper, IProjectRepository projectRepository)
    {
        var database = mongoDbService.Database;
        _issues = database.GetCollection<Issue>("issues");
        _mapper = mapper;
        _projectRepository = projectRepository;
    }

    public async Task<IssueDomain> CreateIssue(IssueDomain issueDomain)
    {
        var issueEntity = _mapper.Map<Issue>(issueDomain);
        await _issues.InsertOneAsync(issueEntity);
        issueDomain.Id = issueEntity.Id;
        return issueDomain;
    }

    public async Task<IssueDomain> GetIssue(string id)
    {
        var issueEntity = await _issues.Find(i => i.Id == id).FirstOrDefaultAsync();
        if (issueEntity == null)
            throw new Exception("Issue not found");
        return _mapper.Map<IssueDomain>(issueEntity);
    }

    public async Task<IssueDomain> UpdateIssue(IssueDomain issueDomain)
    {
        var issueEntity = _mapper.Map<Issue>(issueDomain);
        var result = await _issues.ReplaceOneAsync(i => i.Id == issueDomain.Id, issueEntity);
        if (result.MatchedCount == 0)
            throw new Exception("Issue not found");
        return issueDomain;
    }

    public async Task DeleteIssue(string id)
    {
        var result = await _issues.DeleteOneAsync(i => i.Id == id);
        if (result.DeletedCount == 0)
            throw new Exception("Issue not found");
    }

    public async Task<(List<IssueDomain> Issues, int TotalCount)> ListIssues(GetIssuesParams param)
    {
        var filterBuilder = Builders<Issue>.Filter;
        var filter = filterBuilder.Eq(i => i.ProjectId, param.ProjectId);

        if (!string.IsNullOrEmpty(param.Status))
        {
            filter &= filterBuilder.Eq(i => i.Status, param.Status);
        }

        var totalCount = await _issues.CountDocumentsAsync(filter);
        var issues = await _issues.Find(filter)
            .Skip((param.Page - 1) * param.Limit)
            .Limit(param.Limit)
            .ToListAsync();

        return (_mapper.Map<List<IssueDomain>>(issues), (int)totalCount);
    }
}