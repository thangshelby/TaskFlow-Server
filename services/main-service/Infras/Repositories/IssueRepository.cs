using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly IMongoCollection<Issue> _issues;
    private readonly ILogger<IssueRepository> _logger;
    private readonly IProjectRepository _projectRepository;

    private readonly IMapper _mapper;

    public IssueRepository(MongoDbService mongoDbService, IMapper mapper, IProjectRepository projectRepository, ILogger<IssueRepository> logger)
    {
        var database = mongoDbService.Database;
        _issues = database.GetCollection<Issue>("issues");
        _mapper = mapper;
        _logger = logger;
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

    public async Task<IssueDomain> UpdateIssue(UpdateIssueParams body)
    {
        var existingIssue = await _issues.Find(i => i.Id == body.IssueId).FirstOrDefaultAsync();
        _logger.LogInformation(">>>>>>");
        _logger.LogInformation(existingIssue.StoryPoint.ToString());
        _logger.LogInformation(body.StoryPoint.ToString());
        if (existingIssue == null)
            throw new Exception("Issue not found");

        // Update only if the field is not null
        if (!string.IsNullOrEmpty(body.Title)) existingIssue.Title = body.Title;
        if (!string.IsNullOrEmpty(body.ProjectId)) existingIssue.ProjectId = body.ProjectId;
        if (!string.IsNullOrEmpty(body.SprintId)) existingIssue.SprintId = body.SprintId;
        if (!string.IsNullOrEmpty(body.AssigneeId)) existingIssue.AssigneeId = body.AssigneeId;
        if (!string.IsNullOrEmpty(body.Description)) existingIssue.Description = body.Description;
        if (!string.IsNullOrEmpty(body.Summary)) existingIssue.Summary = body.Summary;
        if (body.StoryPoint.HasValue) existingIssue.StoryPoint = body.StoryPoint.Value;
        if (!string.IsNullOrEmpty(body.ReporterId)) existingIssue.ReporterId = body.ReporterId;
        if (!string.IsNullOrEmpty(body.Status)) existingIssue.Status = body.Status;
        if (!string.IsNullOrEmpty(body.ParentId)) existingIssue.ParentId = body.ParentId;
        if (body.Type.HasValue) existingIssue.Type = body.Type;
        if (body.Priority.HasValue) existingIssue.Priority = body.Priority;
        if (body.Attachments != null && body.Attachments.Count > 0) existingIssue.Attachments = body.Attachments;

        existingIssue.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation(">>>>>>2");
        _logger.LogInformation(existingIssue.StoryPoint.ToString());
        await _issues.ReplaceOneAsync(i => i.Id == body.IssueId, existingIssue);

        return _mapper.Map<IssueDomain>(existingIssue);
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
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(param.ProjectId))
        {
            filter &= filterBuilder.Eq(i => i.ProjectId, param.ProjectId);
        }

        if (!string.IsNullOrEmpty(param.Status))
        {
            filter &= filterBuilder.Eq(i => i.Status, param.Status);
        }

        if (!string.IsNullOrEmpty(param.AssigneeId))
        {
            filter &= filterBuilder.Eq(i => i.AssigneeId, param.AssigneeId);
        }

        if (!string.IsNullOrEmpty(param.SprintId))
        {
            filter &= filterBuilder.Eq(i => i.SprintId, param.SprintId);
        }

        if (!string.IsNullOrEmpty(param.Keyword))
        {
            var decodedKeyword = Uri.UnescapeDataString(param.Keyword.Replace("+", " "));

            var keywordFilter = filterBuilder.Or(
                filterBuilder.Regex(i => i.Title, new BsonRegularExpression(decodedKeyword, "i")),
                filterBuilder.Regex(i => i.Description, new BsonRegularExpression(decodedKeyword, "i"))
            );
            filter &= keywordFilter;
        }

        var totalCount = await _issues.CountDocumentsAsync(filter);

        var issues = await _issues.Find(filter)
            .Skip((param.Page - 1) * param.Limit)
            .Limit(param.Limit)
            .ToListAsync();
        return (_mapper.Map<List<IssueDomain>>(issues), (int)totalCount);
    }
}