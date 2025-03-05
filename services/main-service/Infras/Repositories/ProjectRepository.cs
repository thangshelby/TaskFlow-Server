using AutoMapper;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class projectRepository : IProjectRepository
{
    private readonly IMongoCollection<Project> _projects;
    private readonly IMapper _mapper;
    public projectRepository(MongoDbService mongoDbService, IMapper mapper)
    {
        var database = mongoDbService.Database;
        _projects = database.GetCollection<Project>("projects");
        _mapper = mapper;
    }

    public async Task<ProjectDomain> CreateProject(ProjectDomain projectDomain)
    {
        // Convert Domain to Entity
        var projectEntity = _mapper.Map<Project>(projectDomain);

        await _projects.InsertOneAsync(projectEntity);

        // Update domain with new ID
        projectEntity.Id = projectEntity.Id;
        return projectDomain;
    }

}
