using AutoMapper;
using MainService.Domain.Common;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using TaskFlow.UserService;

namespace MainService.Infras.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly IMongoCollection<Issue> _issues;
    private readonly ILogger<IssueRepository> _logger;
    private readonly IProjectRepository _projectRepository;

    private readonly IMapper _mapper;

    private static DateTime ParseToUtc(string dateString)
    {
        if (DateTime.TryParse(dateString, out var date))
        {
            return date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
        }
        return DateTime.MinValue;
    }

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
        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(id))),
        };
      
        pipeline.AddRange(GetEnrichedLookupStages());

        var result = await _issues.Aggregate<Issue>(pipeline).FirstOrDefaultAsync();
        _logger.LogInformation($"GetIssue result: {result}");
        if (result == null)
            throw new Exception("Issue not found");

        return _mapper.Map<IssueDomain>(result);
    }

    public async Task<IssueDomain> UpdateIssue(UpdateIssueParams body)
    {
        var update = MongoUtils.MakeMongoDataUpdate<Issue>(new MongoUtils.MongoUpdateInput
        {
            Data = body,
        });

        await _issues.UpdateOneAsync(i => i.Id == body.Id, update);

        // Fetch the updated issue with column information
        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(body.Id))),
        };
        pipeline.AddRange(GetEnrichedLookupStages());

        var rawResult = await _issues.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        var updatedIssue = BsonSerializer.Deserialize<Issue>(rawResult);
        return _mapper.Map<IssueDomain>(updatedIssue);
    }

    public async Task DeleteIssue(string id)
    {
        var result = await _issues.DeleteOneAsync(i => i.Id == id);
        if (result.DeletedCount == 0)
            throw new Exception("Issue not found");
    }

    public async Task<UserStats> GetStats(string id, bool isSprintId = false)
    {
        var now = DateTime.UtcNow;
        var oneDayAgo = now.AddDays(-1);
        var sixHoursAgo = now.AddHours(-6);
        var matchField = isSprintId ? "sprint_id" : "project_id";
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument(matchField, id)),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "project_column" },
                { "let", new BsonDocument("colId", "$column_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray
                                {
                                    "$_id",
                                    new BsonDocument("$toObjectId", "$$colId")
                                })
                            }
                        })
                    }
                },
                { "as", "column" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$column" },
                { "preserveNullAndEmptyArrays", true }
            }),
            new BsonDocument("$facet", new BsonDocument
            {
                {
                    "byStatus", new BsonArray
                    {
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", new BsonDocument {
                                { "name", "$column.name" },
                                { "order", "$column.order" }
                            }},
                            { "count", new BsonDocument("$sum", 1) }
                        }),
                        new BsonDocument("$sort", new BsonDocument("_id.order", 1)),
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "_id", 0 },
                            { "name", "$_id.name" },
                            { "count", 1 }
                        })
                    }
                },
                {
                    "byPriority", new BsonArray
                    {
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", "$priority" },
                            { "count", new BsonDocument("$sum", 1) }
                        })
                    }
                },
                {
                    "byType", new BsonArray
                    {
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", "$type" },
                            { "count", new BsonDocument("$sum", 1) }
                        })
                    }
                },
                {
                    "newIssuesCount", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "created_at", new BsonDocument("$gte", oneDayAgo) }
                        }),
                        new BsonDocument("$count", "count")
                    }
                },
                {
                    "updatedIssuesCount", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "updated_at", new BsonDocument("$gte", sixHoursAgo) }
                        }),
                        new BsonDocument("$count", "count")
                    }
                }
            })
        };

        var results = await _issues.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        var newIssuesCount = results["newIssuesCount"].IsBsonArray && results["newIssuesCount"].AsBsonArray.Count > 0
        ? results["newIssuesCount"][0]["count"].AsInt32
        : 0;
        var updatedIssuesCount = results["updatedIssuesCount"].IsBsonArray && results["updatedIssuesCount"].AsBsonArray.Count > 0
            ? results["updatedIssuesCount"][0]["count"].AsInt32
            : 0;

        var response = new UserStats
        {
            ByStatus = { results["byStatus"].AsBsonArray.Select(x => new StatusCount {
                Name = x["name"].AsString,
                Count = x["count"].AsInt32
            }) },
            ByPriority = { results["byPriority"].AsBsonArray.Select(x => new PriorityCount {
                Priority = x["_id"].AsString,
                Count = x["count"].AsInt32
            }) },
            ByType = { results["byType"].AsBsonArray.Select(x => new TypeCount {
                Type = x["_id"].AsString,
                Count = x["count"].AsInt32
            }) },
            NewIssuesCount = newIssuesCount,
            RecentlyUpdatedCount = updatedIssuesCount
        };

        return response;
    }

    public async Task<PagedResult<IssueDomain>> ListIssues(GetIssuesParams param)
    {
        var filterBuilder = Builders<Issue>.Filter;
        var filter = filterBuilder.Empty;

        if (param.IssueIds != null && param.IssueIds.Any())
        {
            filter &= filterBuilder.In(i => i.Id, param.IssueIds);
        }

        if (!string.IsNullOrEmpty(param.ProjectId))
        {
            filter &= filterBuilder.Eq(i => i.ProjectId, param.ProjectId);
        }

        if (param.ColumnIds != null && param.ColumnIds.Any())
        {
            filter &= filterBuilder.In(i => i.ColumnId, param.ColumnIds);
        }

        if (param.AssigneeIds != null && param.AssigneeIds.Any())
        {
            filter &= filterBuilder.In(i => i.AssigneeId, param.AssigneeIds);
        }

        if (param.SprintIds != null && param.SprintIds.Any())
        {
            filter &= filterBuilder.In(i => i.SprintId, param.SprintIds);
        }

        if (param.Priorities != null && param.Priorities.Any())
        {
            filter &= filterBuilder.In("priority", param.Priorities);
        }
        if (param.Types != null && param.Types.Any())
        {
            filter &= filterBuilder.In("type", param.Types);
        }

        // Handle TeamIds filter with NULL support
        if (param.TeamIds != null && param.TeamIds.Any())
        {
            var teamFilters = new List<FilterDefinition<Issue>>();
            
            foreach (var teamId in param.TeamIds)
            {
                var normalizedTeamId = teamId?.Trim();
                if (string.IsNullOrEmpty(normalizedTeamId) || string.Equals(normalizedTeamId, "NULL", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for null or empty team_id
                    teamFilters.Add(filterBuilder.Or(
                        filterBuilder.Eq(i => i.TeamId, null),
                        filterBuilder.Eq(i => i.TeamId, ""),
                        filterBuilder.Eq("team_id", BsonNull.Value)
                    ));
                }
                else
                {
                    // Filter for specific team_id
                    teamFilters.Add(filterBuilder.Eq(i => i.TeamId, normalizedTeamId));
                }
            }
            
            if (teamFilters.Any())
            {
                filter &= filterBuilder.Or(teamFilters);
            }
        }

        // Handle ParentIds filter with NULL support
        if (param.ParentIds != null && param.ParentIds.Any())
        {
            var parentFilters = new List<FilterDefinition<Issue>>();
            
            foreach (var parentId in param.ParentIds)
            {
                var normalizedParentId = parentId?.Trim();
                if (string.IsNullOrEmpty(normalizedParentId) || string.Equals(normalizedParentId, "NULL", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for null or empty parent_id
                    parentFilters.Add(filterBuilder.Or(
                        filterBuilder.Eq(i => i.ParentId, null),
                        filterBuilder.Eq(i => i.ParentId, ""),
                        filterBuilder.Eq("parent_id", BsonNull.Value)
                    ));
                }
                else
                {
                    // Filter for specific parent_id
                    parentFilters.Add(filterBuilder.Eq(i => i.ParentId, normalizedParentId));
                }
            }
            
            if (parentFilters.Any())
            {
                filter &= filterBuilder.Or(parentFilters);
            }
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
        if (!string.IsNullOrEmpty(param.DueDateFrom))
        {
            var dueFrom = ParseToUtc(param.DueDateFrom);
            if (dueFrom != DateTime.MinValue)
            {
                filter &= filterBuilder.Gte(i => i.DueDateFrom, dueFrom);
            }
        }

        if (!string.IsNullOrEmpty(param.DueDateTo))
        {
             filter &= BuildDueDateFilter(param.DueDateTo, filterBuilder);
        }
        if (!string.IsNullOrEmpty(param.CreatedAtFrom) && DateTime.TryParse(param.CreatedAtFrom, out var createdFrom))
        {
            filter &= filterBuilder.Gte(i => i.CreatedAt, createdFrom);
        }

        if (!string.IsNullOrEmpty(param.CreatedAtTo) && DateTime.TryParse(param.CreatedAtTo, out var createdTo))
        {
            createdTo = createdTo.Date.AddDays(1).AddTicks(-1);
            filter &= filterBuilder.Lte(i => i.CreatedAt, createdTo);
        }

        var totalCount = await _issues.CountDocumentsAsync(filter);
        var renderedFilter = filter.Render(new RenderArgs<Issue>(
            BsonSerializer.SerializerRegistry.GetSerializer<Issue>(),
            BsonSerializer.SerializerRegistry
        ));
        var pipelineStages = new List<BsonDocument>
        {
            new BsonDocument("$match", renderedFilter)
        };

        pipelineStages.AddRange(GetEnrichedLookupStages());

        if (!param.Unpaged)
        {
            pipelineStages.Add(new BsonDocument("$skip", (param.Page - 1) * param.Limit));
            pipelineStages.Add(new BsonDocument("$limit", param.Limit));
        }

        var rawResults = await _issues.Aggregate<BsonDocument>(pipelineStages).ToListAsync();
        var issues = rawResults.Select(bson => BsonSerializer.Deserialize<Issue>(bson)).ToList();
        return new PagedResult<IssueDomain>(_mapper.Map<List<IssueDomain>>(issues), (int)totalCount);
    }
    
    private FilterDefinition<Issue> BuildDueDateFilter(string dueDateParam, FilterDefinitionBuilder<Issue> filterBuilder)
    {
        if (string.IsNullOrWhiteSpace(dueDateParam))
            return FilterDefinition<Issue>.Empty;

        var param = dueDateParam.Trim().ToLower();

        // Case 1: unassigned → DueDateTo == null hoặc DateTime.MinValue
        if (param == "unassigned")
        {
            return filterBuilder.Or(
                filterBuilder.Eq(i => i.DueDateTo, DateTime.MinValue)
            );
        }

        // Case 2: assigned → DueDateTo != null và != MinValue
        if (param == "assigned")
        {
            return filterBuilder.And(
                filterBuilder.Ne(i => i.DueDateTo, DateTime.MinValue)
            );
        }

        // Case 3: cụ thể due date
        var dueTo = ParseToUtc(dueDateParam);
        if (dueTo == DateTime.MinValue)
            return FilterDefinition<Issue>.Empty;

        var inclusiveEnd = dueTo.Date.AddDays(1).AddTicks(-1);
        return filterBuilder.Lte(i => i.DueDateTo, inclusiveEnd);
    }

    private List<BsonDocument> GetEnrichedLookupStages()
    {
        BsonDocument SafeToObjectId(string varName) => new BsonDocument("$convert", new BsonDocument
        {
            { "input", varName },
            { "to", "objectId" },
            { "onError", BsonNull.Value },
            { "onNull", BsonNull.Value }
        });

        return new List<BsonDocument>
        {
            // Lookup Column
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "project_column" },
                { "let", new BsonDocument("colId", "$column_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$_id", SafeToObjectId("$$colId") }) }
                        })
                    }
                },
                { "as", "column" }
            }),
            new BsonDocument("$unwind", new BsonDocument { { "path", "$column" }, { "preserveNullAndEmptyArrays", true } }),

            // Lookup Sprint
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "sprints" },
                { "let", new BsonDocument("sprintId", "$sprint_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$_id", SafeToObjectId("$$sprintId") }) }
                        })
                    }
                },
                { "as", "sprint" }
            }),
            new BsonDocument("$unwind", new BsonDocument { { "path", "$sprint" }, { "preserveNullAndEmptyArrays", true } }),

            // Lookup Team
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "project_teams" },
                { "let", new BsonDocument("teamId", "$team_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$_id", SafeToObjectId("$$teamId") }) }
                        })
                    }
                },
                { "as", "team" }
            }),
            new BsonDocument("$unwind", new BsonDocument { { "path", "$team" }, { "preserveNullAndEmptyArrays", true } }),

            // Lookup Assignee
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "users" },
                { "let", new BsonDocument("assigneeId", "$assignee_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$_id", SafeToObjectId("$$assigneeId") }) }
                        })
                    }
                },
                { "as", "assignee" }
            }),
            new BsonDocument("$unwind", new BsonDocument { { "path", "$assignee" }, { "preserveNullAndEmptyArrays", true } }),

            // Lookup Reporter
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "users" },
                { "let", new BsonDocument("reporterId", "$reporter_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$_id", SafeToObjectId("$$reporterId") }) }
                        })
                    }
                },
                { "as", "reporter" }
            }),
            new BsonDocument("$unwind", new BsonDocument { { "path", "$reporter" }, { "preserveNullAndEmptyArrays", true } }),

            // Lookup Parent Issue
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "issues" },
                { "let", new BsonDocument("parentId", "$parent_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$_id", SafeToObjectId("$$parentId") }) }
                        })
                    }
                },
                { "as", "parent_issue" }
            }),
            new BsonDocument("$unwind", new BsonDocument { { "path", "$parent_issue" }, { "preserveNullAndEmptyArrays", true } }),
        };
    }
}
