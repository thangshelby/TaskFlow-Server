using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;
using MongoDB.Bson.Serialization;
using System.Globalization;

namespace MainService.Infras.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly IMongoCollection<Project> _projects;
    private readonly IMongoCollection<ProjectColumn> _projectColumns;
    private readonly IMongoCollection<Issue> _issues;
    private readonly IMongoCollection<ProjectMember> _teamMembers;  // Changed to match DB collection name
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Sprint> _sprints;
    
    private readonly ILogger<ProjectRepository> _logger;
    private readonly IMapper _mapper;

    public ProjectRepository(MongoDbService mongoDbService, IMapper mapper, ILogger<ProjectRepository> logger)
    {
        var database = mongoDbService.Database;
        _projects = database.GetCollection<Project>("projects");
        _issues = database.GetCollection<Issue>("issues");
        _projectColumns = database.GetCollection<ProjectColumn>("project_column");
        _teamMembers = database.GetCollection<ProjectMember>("team_members");
        _users = database.GetCollection<User>("users");
        _sprints = database.GetCollection<Sprint>("sprints");
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
        var project = await _projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (project == null)
            throw new Exception("Project not found");

        return _mapper.Map<ProjectDomain>(project);
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
        var filterBuilder = Builders<Project>.Filter;
        var filters = new List<FilterDefinition<Project>>();

        // Filter by project IDs (preserving order is not supported directly with .In)
        if (query.ProjectIds != null && query.ProjectIds.Any())
        {
            filters.Add(filterBuilder.In(x => x.Id, query.ProjectIds));
        }

        // Filter by owner ID
        if (!string.IsNullOrEmpty(query.UserId))
        {
            filters.Add(filterBuilder.Eq(x => x.OwnerId, query.UserId));
        }

        // Filter by keyword (case-insensitive search on name)
        if (!string.IsNullOrEmpty(query.Kw))
        {
            filters.Add(filterBuilder.Regex(x => x.Name, new BsonRegularExpression(query.Kw, "i")));
        }

        var matchFilter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

        // Sorting
        var sortField = "created_at";
        var sortDescending = true;

        if (!string.IsNullOrEmpty(query.Sort))
        {
            sortField = query.Sort.TrimStart('-');
            sortDescending = query.Sort.StartsWith("-");
            sortField = sortField.ToLower() switch
            {
                "name" => "name",
                "key" => "key",
                "created_at" => "created_at",
                "updated_at" => "updated_at",
                _ => "created_at"
            };
        }

        var sortDoc = new BsonDocument(sortField, sortDescending ? -1 : 1);

        // Query database
        var totalCount = await _projects.CountDocumentsAsync(matchFilter);
        var projects = await _projects.Find(matchFilter)
            .Sort(sortDoc)
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

    public async Task<List<ProjectColumnDomain>> FindColumnsByProjectId(ListProjectColumnsParams param)
    {
        // Get active sprint IDs if filtering is requested
      
        List<string>? activeSprintIds = null;
        if (param.ActiveSprintOnly == true)
        {
            var currentDate = DateTime.UtcNow;
            // Find active sprints: DateStarted <= currentDate <= DateEnded
            var sprintFilter = Builders<Sprint>.Filter.And(
                Builders<Sprint>.Filter.Eq(s => s.ProjectId, param.ProjectId),
                Builders<Sprint>.Filter.Lte(s => s.DateStarted, currentDate),
                Builders<Sprint>.Filter.Gte(s => s.DateEnded, currentDate)
            );
            
            var activeSprints = await _sprints.Find(sprintFilter).ToListAsync();
            _logger.LogInformation("Found {Count} active sprints", activeSprints.Count);
            
            activeSprintIds = activeSprints.Select(s => s.Id!).ToList();
        }

        _logger.LogInformation("Active sprint IDs: {ActiveSprintIds}", string.Join(',', activeSprintIds ?? []));

        var issueQuery = MongoUtils.BuildExprMongo(param, new Dictionary<string, (string field, string op, string? extra)>
        {
            { "DueDateFrom", ("due_date_from", "$gte", null) },
            { "DueDateTo", ("due_date_to", "$lte", null) },
            { "CreatedAtFrom", ("created_at", "$gte", null) },
            { "CreatedAtTo", ("created_at", "$lte", null) },
            { "AssigneeIds", ("assignee_id", "$in", null) },
            { "SprintIds", ("sprint_id", "$in", null) },
            { "Types", ("type", "$in", null) },
            { "Priorities", ("priority", "$in", null) },
            { "Keyword", ("title", "$regex", null) }
        }, excludeProps: ["ProjectId", "ColumnIds", "ActiveSprintOnly"]);
        issueQuery.Insert(0, new BsonDocument("$in", new BsonArray { "$_id", "$$issueIds" }));
        
        // Add active sprint filtering to issue query if needed
        if (param.ActiveSprintOnly == true && activeSprintIds != null && activeSprintIds.Any())
        {
            _logger.LogInformation("Adding active sprint filtering to issue query: {ActiveSprintOnly}", param.ActiveSprintOnly);
            issueQuery.Add(new BsonDocument("$in", new BsonArray { "$sprint_id", new BsonArray(activeSprintIds.Select(id => new BsonString(id)).ToArray()) }));
        }
        
        var issueMatch = new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$and", issueQuery)));

        var columnQuery = MongoUtils.BuildExprMongo(param, new Dictionary<string, (string field, string op, string? extra)>
        {
            { "ColumnIds", ("_id", "$in", "is_object_id") },
            { "ProjectId", ("project_id", "$eq", "is_object_id") },
        }, excludeProps: ["Keyword", "DueDateFrom", "DueDateTo", "CreatedAtFrom", "CreatedAtTo", "AssigneeIds", "SprintIds", "Types", "Priorities", "ActiveSprintOnly"]);
        var columnMatch = new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$and", columnQuery)));

        var pipeline = new[]
        {
            columnMatch,
            new BsonDocument { { "$sort", new BsonDocument { { "order", 1 } } } },
            new BsonDocument {
                {
                    "$lookup", new BsonDocument {
                        { "from", "issues" },
                        { "let", new BsonDocument("issueIds", "$issue_ids") },
                        { "pipeline", new BsonArray {
                            issueMatch,
                            new BsonDocument {
                                { "$sort", new BsonDocument("created_at", 1) }
                            }
                        }},
                        { "as", "issues" }
                    }
                }
            },
        };

        var rawResult = await _projectColumns.Aggregate<BsonDocument>(pipeline).ToListAsync();
        var columnsEntity = new List<ProjectColumn>();

        foreach (var bsonDoc in rawResult)
        {
            var column = BsonSerializer.Deserialize<ProjectColumn>(bsonDoc);
            columnsEntity.Add(column);
        }

        var columnsDomain = _mapper.Map<List<ProjectColumnDomain>>(columnsEntity);
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
    public async Task DeleteColumn(DeleteColumnParams param)
    {
        var filter = Builders<ProjectColumn>.Filter
            .Where(c => c.Id == param.ColumnId);

        await _projectColumns.FindOneAndDeleteAsync(filter);
    }

    public async Task IncrementIssuesCount(string projectId)
    {
        var filter = Builders<Project>.Filter.Eq(p => p.Id, projectId);
        var update = Builders<Project>.Update
            .Inc(p => p.IssuesCount, 1)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        await _projects.UpdateOneAsync(filter, update);
    }

    public async Task<(string Key, int IssuesCount)> GetProjectKeyAndIssuesCount(string projectId)
    {
        var project = await _projects.Find(p => p.Id == projectId)
            .Project(p => new { p.Key, p.IssuesCount })
            .FirstOrDefaultAsync();

        if (project == null)
            throw new Exception("Project not found");

        return (project.Key, project.IssuesCount ?? 0);
    }

    public async Task<ProjectSummaryDomain> GetProjectSummary(GetProjectSummaryParams param)
    {
        var filterBuilder = Builders<Issue>.Filter;
        var filter = filterBuilder.Eq(x => x.ProjectId, param.ProjectId);

        if (!string.IsNullOrWhiteSpace(param.SprintId))
        {
            filter &= filterBuilder.Eq(x => x.SprintId, param.SprintId);
        }

        if (param.DateFrom.HasValue)
        {
            filter &= filterBuilder.Gte(x => x.CreatedAt, param.DateFrom.Value);
        }

        if (param.DateTo.HasValue)
        {
            filter &= filterBuilder.Lte(x => x.CreatedAt, param.DateTo.Value);
        }

        var issues = await _issues.Find(filter).ToListAsync();
        var totalIssues = issues.Count;
        if (totalIssues == 0)
        {
            return new ProjectSummaryDomain();
        }

        var columnIds = issues
            .Select(x => x.ColumnId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var columns = await _projectColumns.Find(x => columnIds.Contains(x.Id!)).ToListAsync();
        var columnById = columns
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(x => x.Id!, x => x);

        var isDone = issues.ToDictionary(
            issue => issue.Id!,
            issue => columnById.TryGetValue(issue.ColumnId, out var col) && IsDoneColumn(col.Name)
        );

        var doneIssues = isDone.Values.Count(v => v);
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);
        var sixHoursAgo = DateTime.UtcNow.AddHours(-6);

        var byStatus = issues
            .GroupBy(issue =>
            {
                if (columnById.TryGetValue(issue.ColumnId, out var col))
                {
                    return new { Name = col.Name, Order = col.Order };
                }

                return new { Name = "UNASSIGNED", Order = int.MaxValue };
            })
            .OrderBy(x => x.Key.Order)
            .Select(x => new ProjectSummaryStatusCountDomain
            {
                Name = x.Key.Name,
                Count = x.Count()
            })
            .ToList();

        var byPriority = issues
            .GroupBy(x => x.Priority?.ToString() ?? "Unknown")
            .OrderByDescending(x => x.Count())
            .Select(x => new ProjectSummaryPriorityCountDomain
            {
                Priority = x.Key,
                Count = x.Count()
            })
            .ToList();

        var byType = issues
            .GroupBy(x => x.Type?.ToString() ?? "Unknown")
            .OrderByDescending(x => x.Count())
            .Select(x => new ProjectSummaryTypeCountDomain
            {
                Type = x.Key,
                Count = x.Count()
            })
            .ToList();

        var assigneeIds = issues
            .Where(x => !string.IsNullOrWhiteSpace(x.AssigneeId))
            .Select(x => x.AssigneeId!)
            .Distinct()
            .ToList();

        var users = assigneeIds.Count == 0
            ? new List<User>()
            : await _users.Find(x => assigneeIds.Contains(x.Id!)).ToListAsync();
        var userById = users
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(x => x.Id!, x => x);

        var topContributors = issues
            .Where(x => !string.IsNullOrWhiteSpace(x.AssigneeId))
            .GroupBy(x => x.AssigneeId!)
            .Select(group =>
            {
                var resolvedCount = group.Count();
                userById.TryGetValue(group.Key, out var user);
                return new ProjectSummaryContributorDomain
                {
                    UserId = group.Key,
                    DisplayName = user == null ? "Unknown User" : $"{user.FirstName} {user.LastName}".Trim(),
                    Avatar = user?.Avatar ?? string.Empty,
                    ResolvedCount = resolvedCount,
                    ContributionPercent = doneIssues == 0 ? 0 : (double)resolvedCount / totalIssues * 100
                };
            })
            .OrderByDescending(x => x.ResolvedCount)
            .Take(5)
            .ToList();

        var timeline = BuildTimeline(issues, isDone);

        return new ProjectSummaryDomain
        {
            ByStatus = byStatus,
            ByPriority = byPriority,
            ByType = byType,
            TopContributors = topContributors,
            Timeline = timeline,
            TotalIssues = totalIssues,
            DoneIssues = doneIssues,
            NewIssuesCount = issues.Count(x => x.CreatedAt >= oneDayAgo),
            RecentlyUpdatedCount = issues.Count(x => x.UpdatedAt >= sixHoursAgo),
        };
    }

    private static bool IsDoneColumn(string? columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return false;
        }

        var normalized = columnName.Trim().ToUpperInvariant();
        return normalized.Contains("DONE") || normalized.Contains("CLOSED");
    }

    private static List<ProjectSummaryTimelinePointDomain> BuildTimeline(
        List<Issue> issues,
        Dictionary<string, bool> doneLookup)
    {
        var firstDate = issues.Min(x => x.CreatedAt).Date;
        var lastCreatedDate = issues.Max(x => x.CreatedAt).Date;
        var lastCompletedDate = issues
            .Where(x => x.CompletedAt > DateTime.MinValue)
            .Select(x => x.CompletedAt.Date)
            .DefaultIfEmpty(lastCreatedDate)
            .Max();
        var lastDate = lastCreatedDate > lastCompletedDate ? lastCreatedDate : lastCompletedDate;

        var timeline = new List<ProjectSummaryTimelinePointDomain>();
        var cumulativeCreated = 0;
        var cumulativeDone = 0;
        var createdByDate = issues
            .GroupBy(x => x.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());
        var doneByDate = issues
            .Where(x => doneLookup.TryGetValue(x.Id!, out var done) && done && x.CompletedAt > DateTime.MinValue)
            .GroupBy(x => x.CompletedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
        {
            createdByDate.TryGetValue(date, out var addedScope);
            doneByDate.TryGetValue(date, out var doneInDate);

            cumulativeCreated += addedScope;
            cumulativeDone += doneInDate;

            timeline.Add(new ProjectSummaryTimelinePointDomain
            {
                Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                AddedScope = addedScope,
                DoneIssues = cumulativeDone,
                RemainingScope = Math.Max(cumulativeCreated - cumulativeDone, 0)
            });
        }

        return timeline;
    }
}
