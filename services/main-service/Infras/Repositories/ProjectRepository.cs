using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;
using MongoDB.Bson;

namespace MainService.Infras.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly IMongoCollection<Project> _projects;
    private readonly IMapper _mapper;

    public ProjectRepository(MongoDbService mongoDbService, IMapper mapper)
    {
        var database = mongoDbService.Database;
        _projects = database.GetCollection<Project>("projects");
        _mapper = mapper;

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
}
