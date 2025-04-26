using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;
using MongoDB.Bson.Serialization;

namespace MainService.Infras.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ILogger<ProjectRepository> _logger;
    private readonly IMongoCollection<Project> _projects;
    private readonly IMongoCollection<ProjectColumn> _projectColumns;
    private readonly IMongoCollection<Issue> _issues;
    private readonly IMapper _mapper;

    public ProjectRepository(MongoDbService mongoDbService, IMapper mapper, ILogger<ProjectRepository> logger)
    {
        var database = mongoDbService.Database;
        _projects = database.GetCollection<Project>("projects");
        _issues = database.GetCollection<Issue>("issues");
        _projectColumns = database.GetCollection<ProjectColumn>("project_column");
        _mapper = mapper;
        _logger = logger;
        // Ensure index on Key
        var indexKeysDefinition = Builders<Project>.IndexKeys.Ascending(p => p.Key);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<Project>(indexKeysDefinition, indexOptions);
        _projects.Indexes.CreateOne(indexModel);
    }

    public async Task<ProjectDomain> CreateProject(ProjectDomain projectDomain)
    {
        var projectEntity = _mapper.Map<Project>(projectDomain);
        await _projects.InsertOneAsync(projectEntity);
        projectDomain.Id = projectEntity.Id;
        return projectDomain;
    }
    public async Task<ProjectDomain> GetProject(string id)
    {
        var projectEntity = await _projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (projectEntity == null)
            throw new Exception("Project not found");
        return _mapper.Map<ProjectDomain>(projectEntity);
    }
    public async Task<ProjectDomain> UpdateProject(ProjectDomain projectDomain)
    {
        var projectEntity = _mapper.Map<Project>(projectDomain);
        var result = await _projects.ReplaceOneAsync(p => p.Id == projectDomain.Id, projectEntity);
        if (result.MatchedCount == 0)
            throw new Exception("Project not found");
        return projectDomain;
    }
    public async Task DeleteProject(string id)
    {
        var result = await _projects.DeleteOneAsync(p => p.Id == id);
        if (result.DeletedCount == 0)
            throw new Exception("Project not found");
    }
    public async Task<(List<ProjectDomain> Projects, int TotalCount)> ListProjects(ListProjectParams query)
    {
        var builder = Builders<Project>.Filter;
        var filter = builder.Empty;
        // Query builder
        if (!string.IsNullOrEmpty(query.UserId))
        {
            filter = builder.Eq(p => p.OwnerId, query.UserId);
        }
        if (!string.IsNullOrEmpty(query.Kw))
        {
            var keywordFilter = builder.Regex(p => p.Name, new BsonRegularExpression(query.Kw, "i"));
            filter = builder.And(filter, keywordFilter);
        }

        // Sort builder
        var sortBuilder = Builders<Project>.Sort;
        SortDefinition<Project> sort = sortBuilder.Descending("createdAt");
        if (!string.IsNullOrEmpty(query.Sort))
        {
            var sortField = query.Sort.TrimStart('-');
            var descending = query.Sort.StartsWith("-");

            sort = sortField.ToLower() switch
            {
                "name" => descending ? sortBuilder.Descending(p => p.Name) : sortBuilder.Ascending(p => p.Name),
                "key" => descending ? sortBuilder.Descending(p => p.Key) : sortBuilder.Ascending(p => p.Key),
                "created_at" => descending ? sortBuilder.Descending(p => p.CreatedAt) : sortBuilder.Ascending(p => p.CreatedAt),
                "updated_at" => descending ? sortBuilder.Descending(p => p.UpdatedAt) : sortBuilder.Ascending(p => p.UpdatedAt),
                _ => sortBuilder.Descending(p => p.CreatedAt)
            };
        }

        var totalCount = await _projects.CountDocumentsAsync(filter);

        var projects = await _projects.Find(filter)
            .Sort(sort)
            .Skip((query.Page - 1) * query.Limit)
            .Limit(query.Limit)
            .ToListAsync();

        return (_mapper.Map<List<ProjectDomain>>(projects), (int)totalCount);
    }
    public async Task<ProjectColumnDomain> CreateColumn(ProjectColumnDomain projectColumn)
    {
        var projectColumnEntity = _mapper.Map<ProjectColumn>(projectColumn);
        await _projectColumns.InsertOneAsync(projectColumnEntity);

        projectColumn.Id = projectColumnEntity.Id;
        return projectColumn;
    }
    public async Task<List<ProjectColumnDomain>> FindColumnsByProjectId(string projectId)
    {
        var pipeline = new[]
        {
            new BsonDocument {
                {
                    "$match", new BsonDocument {
                        { "project_id", new ObjectId(projectId) }
                    }
                }
            },
            new BsonDocument { { "$sort", new BsonDocument { { "order", 1 } } } },
            new BsonDocument {
                {
                    "$lookup", new BsonDocument {
                        { "from", "issues" },
                        { "localField", "issue_ids" },
                        { "foreignField", "_id" },
                        { "as", "issues" }
                    }
                }
            },
        };

        var rawResult = await _projectColumns.Aggregate<BsonDocument>(pipeline).ToListAsync();
        MongoDocumentLogUtil.LogBsonDocuments(_logger, rawResult);

        var columnsEntity = rawResult.Select(bson => BsonSerializer.Deserialize<ProjectColumn>(bson)).ToList();

        var columnsDomain = _mapper.Map<List<ProjectColumnDomain>>(columnsEntity);

        foreach (var column in columnsDomain)
        {
            var columnEntity = columnsEntity.First(c => c.Id == column.Id);
            if (columnEntity.Issues != null)
            {
                column.Issues = _mapper.Map<List<IssueDomain>>(columnEntity.Issues);
            }
        }
        return columnsDomain;
    }
    public async Task<ProjectColumnDomain> FindColumn(GetColumnParams param)
    {
        var filterBuilder = Builders<ProjectColumn>.Filter;
        FilterDefinition<ProjectColumn> filter = FilterDefinition<ProjectColumn>.Empty;

        if (!string.IsNullOrEmpty(param.ColumnId))
        {
            filter = filterBuilder.Eq(c => c.Id, param.ColumnId);
        }
        else if (!string.IsNullOrEmpty(param.Name))
        {
            filter = filterBuilder.Eq(c => c.Name, param.Name);
        }

        var column = await _projectColumns.Find(filter).FirstOrDefaultAsync();
        return _mapper.Map<ProjectColumnDomain>(column);
    }

    public async Task UpdateColumnOrder(string projectId, string columnId, int order)
    {
        var filter = Builders<ProjectColumn>.Filter
        .Where(c => c.ProjectId == projectId && c.Id == columnId);

        var update = Builders<ProjectColumn>.Update
        .Set(c => c.Order, order)
        .Set(c => c.UpdatedAt, DateTime.UtcNow);

        await _projectColumns.UpdateOneAsync(filter, update);
    }
    public async Task<ProjectColumnDomain> UpdateColumn(UpdateColumnParams param)
    {
        var filter = Builders<ProjectColumn>.Filter.Eq(c => c.Id, param.ColumnId);

        var updateDefs = new List<UpdateDefinition<ProjectColumn>>();

        if (!string.IsNullOrEmpty(param.Name))
            updateDefs.Add(Builders<ProjectColumn>.Update.Set(c => c.Name, param.Name));

        if (!string.IsNullOrEmpty(param.RemoveIssueId))
            updateDefs.Add(Builders<ProjectColumn>.Update.Pull(c => c.IssueIds, new ObjectId(param.RemoveIssueId)));

        if (!string.IsNullOrEmpty(param.AddIssueId))
            updateDefs.Add(Builders<ProjectColumn>.Update.AddToSet(c => c.IssueIds, new ObjectId(param.AddIssueId)));

        updateDefs.Add(Builders<ProjectColumn>.Update.Set(c => c.UpdatedAt, DateTime.UtcNow));

        var update = Builders<ProjectColumn>.Update.Combine(updateDefs);


        var updatedColumn = await _projectColumns.FindOneAndUpdateAsync(filter, update);

        return _mapper.Map<ProjectColumnDomain>(updatedColumn);
    }
}
