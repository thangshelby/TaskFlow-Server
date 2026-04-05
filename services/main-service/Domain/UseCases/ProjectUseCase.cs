using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;

namespace MainService.Domain.UseCases;

public class ProjectUseCase
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IssueUseCase _issueUseCase;
    private readonly IPublisherService _publisher;
    public ProjectUseCase(
        IProjectRepository projectRepository,
        ITransactionRepo transactionRepo,
        IProjectMemberRepository projectMemberRepository,
        IssueUseCase issueUseCase,
        IUserRepository userRepository,
        IPublisherService publisher
        )
    {
        _projectRepository = projectRepository;
        _transactionRepo = transactionRepo;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
        _issueUseCase = issueUseCase;
        _publisher = publisher;
    }

    public async Task<ProjectDomain> CreateProject(ProjectDomain project)
    {
        // Add any business logic/validation here
        if (string.IsNullOrEmpty(project.Name))
            throw new ArgumentException("Project name cannot be empty");

        if (string.IsNullOrEmpty(project.Key))
            throw new ArgumentException("Project key cannot be empty");

        if (string.IsNullOrEmpty(project.OwnerId))
            throw new ArgumentException("Project owner ID cannot be empty");

        // Initialize issues count to 0 for new projects
        project.IssuesCount = 0;

        // Create project, owner member, and default columns in a transaction
        ProjectDomain createdProject = null!;
        ProjectMemberDomain ownerMember = null!;

        await _transactionRepo.ExecuteAsync(async session =>
        {
            // Create the project first
            createdProject = await _projectRepository.CreateProject(project);

            // Create owner member record
            ownerMember = new ProjectMemberDomain
            {
                ProjectId = createdProject.Id!,
                UserId = project.OwnerId,
                Role = TeamMemberRole.Owner,
                IsPending = false // Owner is automatically approved
            };

            ownerMember = await _projectMemberRepository.AddAsync(ownerMember);

            // Initialize project's team members with the owner
            createdProject.ProjectMembers = new List<ProjectMemberDomain> { ownerMember };
            await _projectRepository.UpdateProject(createdProject);

            // Create default columns (TODO, INPROGRESS, DONE)
            var defaultColumns = new[]
            {
                new { Name = "TODO", Order = 1 },
                new { Name = "INPROGRESS", Order = 2 },
                new { Name = "DONE", Order = 3 }
            };

            foreach (var columnInfo in defaultColumns)
            {
                var defaultColumn = new ProjectColumnDomain
                {
                    Name = columnInfo.Name,
                    ProjectId = createdProject.Id!,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Issues = new List<IssueDomain>(),
                    Order = columnInfo.Order
                };
                await _projectRepository.CreateColumn(defaultColumn);
            }
        });

        await _publisher.EmitKafka(TopicName.ACTIVITIES, KafkaMessageAction.ACTIVITIES_PROJECT_CREATED, new IProjectMessage
        {
            ProjectId = createdProject.Id,
            UserId = project.OwnerId,
        });

        return createdProject;
    }

    public async Task<ProjectDomain> GetProject(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Project ID cannot be empty");

        var project = await _projectRepository.GetProject(id);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found");

        // Get project members with user data
        var members = await _projectMemberRepository.GetProjectMembersAsync(id, 1, 100); // TODO: Handle pagination properly
        var membersList = members.ToList();

        foreach (var member in membersList)
        {
            var user = await _userRepository.FindUserAsync(new UserQueryParams { UserId = member.UserId });
            if (user != null)
            {
                member.User = user;
            }
        }

        // Set the members to the project
        project.ProjectMembers = membersList;
        return project;
    }

    public async Task<ProjectDomain> UpdateProject(ProjectDomain project)
    {
        if (string.IsNullOrEmpty(project.Id))
            throw new ArgumentException("Project ID cannot be empty");

        if (string.IsNullOrEmpty(project.Name))
            throw new ArgumentException("Project name cannot be empty");

        var existingProject = await _projectRepository.GetProject(project.Id);
        if (existingProject == null)
            throw new KeyNotFoundException($"Project with ID {project.Id} not found");

        // Prevent changing owner through update
        if (project.OwnerId != existingProject.OwnerId)
            throw new InvalidOperationException("Cannot change project owner through update");

        // Update only provided fields
        existingProject.Name = project.Name;
        existingProject.Key = project.Key;
        existingProject.Access = project.Access;
        existingProject.Type = project.Type;
        existingProject.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateProject(existingProject);
    }

    public async Task DeleteProject(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Project ID cannot be empty");

        var project = await _projectRepository.GetProject(id);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found");

        await _projectRepository.DeleteProject(id);
    }

    public async Task<(List<ProjectDomain> Projects, int TotalCount)> ListProjects(ListProjectParams param)
    {
        var (projects, totalCount) = await _projectRepository.ListProjects(param);

        // Get project members for each project
        // TODO: Fix performance
        // foreach (var project in projects)
        // {
        //     var members = await _projectMemberRepository.GetProjectMembersAsync(project.Id!, 1, 100);
        //     var membersList = members.ToList();

        //     foreach (var member in membersList)
        //     {
        //         var user = await _userRepository.FindUserAsync(new UserQueryParams { UserId = member.UserId });
        //         if (user != null)
        //         {
        //             member.User = user;
        //         }
        //     }

        //     project.ProjectMembers = membersList;
        // }

        return (projects, totalCount);
    }

    public async Task<ProjectColumnDomain> CreateColumn(CreateColumnParams param)
    {
        // TODO : FIX CORCUR
        var existingColumns = await _projectRepository.FindColumnsByProjectId(new ListProjectColumnsParams { ProjectId = param.ProjectId });

        var highestOrder = existingColumns.Count != 0 ? existingColumns.Max(c => c.Order) : 0;

        var newOrder = highestOrder + 1;

        var projectColumnDomain = new ProjectColumnDomain
        {
            Name = param.Name,
            ProjectId = param.ProjectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Issues = [],
            Order = newOrder
        };
        return await _projectRepository.CreateColumn(projectColumnDomain);
    }

    public async Task<List<ProjectColumnDomain>> GetAllColumns(ListProjectColumnsParams param)
    {
        return await _projectRepository.FindColumnsByProjectId(param);
    }

    public async Task<List<ProjectColumnDomain>> UpdateColumnsOrder(UpdateColumnOrdersParams param)
    {
        // TODO: validate columns_id belong to project

        var sortedColumns = param.Columns.OrderBy(c => c.Order).ToList();
        await _transactionRepo.ExecuteAsync(async session =>
        {
            foreach (var column in sortedColumns)
            {
                await _projectRepository.UpdateColumnOrder(param.ProjectId, column.Id, column.Order);
            }
        });

        var updatedColumns = await _projectRepository.FindColumnsByProjectId(new ListProjectColumnsParams { ProjectId = param.ProjectId });
        return updatedColumns;
    }

    public async Task<ProjectColumnDomain> UpdateColumn(UpdateColumnParams param)
    {
        return await _projectRepository.UpdateColumn(param);
    }

    public async Task DeleteColumn(DeleteColumnParams param)
    {
        var col = await _projectRepository.FindColumn(new GetColumnParams
        {
            ColumnId = param.ColumnId
        });
        if (col == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Column not found"));
        }
        if (col.IssueIds != null && col.IssueIds.Count > 0)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Cannot delete column that contains issues"));
        }
        await _projectRepository.DeleteColumn(param);
    }
}