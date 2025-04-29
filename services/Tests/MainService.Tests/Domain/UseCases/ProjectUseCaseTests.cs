using Xunit;
using Moq;
using FluentAssertions;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;
using MongoDB.Driver;

namespace MainService.Tests.Domain.UseCases;

public class ProjectUseCaseTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ITransactionRepo> _transactionRepoMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ProjectUseCase _projectUseCase;

    public ProjectUseCaseTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _transactionRepoMock = new Mock<ITransactionRepo>();
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _projectUseCase = new ProjectUseCase(
            _projectRepositoryMock.Object,
            _transactionRepoMock.Object,
            _projectMemberRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateProject_ValidProject_CreatesProjectAndOwnerMember()
    {
        // Arrange
        var project = new ProjectDomain
        {
            Name = "Test Project",
            Key = "TEST",
            Access = ProjectAccess.Private,
            Type = ProjectType.Scrum,
            OwnerId = "user-1"
        };

        var createdProject = new ProjectDomain
        {
            Id = "test-id",
            Name = project.Name,
            Key = project.Key,
            Access = project.Access,
            Type = project.Type,
            OwnerId = project.OwnerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _projectRepositoryMock
            .Setup(repo => repo.CreateProject(It.IsAny<ProjectDomain>()))
            .ReturnsAsync(createdProject);

        _transactionRepoMock
            .Setup(repo => repo.ExecuteAsync(It.IsAny<Func<IClientSessionHandle, Task>>()))
            .Returns((Func<IClientSessionHandle, Task> func) =>
            {
                return Task.Run(() =>
                {
                    func(Mock.Of<IClientSessionHandle>()).Wait();
                    return true;
                });
            });

        ProjectMemberDomain capturedMember = null!;
        _projectMemberRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ProjectMemberDomain>()))
            .Callback<ProjectMemberDomain>(member => capturedMember = member)
            .ReturnsAsync((ProjectMemberDomain m) => m);

        // Act
        var result = await _projectUseCase.CreateProject(project);

        // Assert
        result.Should().BeEquivalentTo(createdProject);

        _projectRepositoryMock.Verify(
            repo => repo.CreateProject(It.Is<ProjectDomain>(p => 
                p.Name == project.Name && 
                p.Key == project.Key)),
            Times.Once
        );

        capturedMember.Should().NotBeNull();
        capturedMember.ProjectId.Should().Be(createdProject.Id);
        capturedMember.UserId.Should().Be(project.OwnerId);
        capturedMember.Role.Should().Be(TeamMemberRole.Owner);
        capturedMember.IsPending.Should().BeFalse();

        _transactionRepoMock.Verify(
            repo => repo.ExecuteAsync(It.IsAny<Func<IClientSessionHandle, Task>>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData("")]
    public async Task CreateProject_EmptyName_ThrowsArgumentException(string name)
    {
        // Arrange
        var project = new ProjectDomain
        {
            Name = name,
            Key = "TEST",
            OwnerId = "user-1"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _projectUseCase.CreateProject(project));
        Assert.Equal("Project name cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData("")]
    public async Task CreateProject_EmptyOwnerId_ThrowsArgumentException(string ownerId)
    {
        // Arrange
        var project = new ProjectDomain
        {
            Name = "Test Project",
            Key = "TEST",
            OwnerId = ownerId
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _projectUseCase.CreateProject(project));
        Assert.Equal("Project owner ID cannot be empty", ex.Message);
    }

    [Fact]
    public async Task GetProject_ExistingProject_ReturnsProject()
    {
        // Arrange
        var projectId = "test-id";
        var project = new ProjectDomain 
        { 
            Id = projectId,
            Name = "Test Project",
            Key = "TEST"
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetProject(projectId))
            .ReturnsAsync(project);

        var projectMembers = new List<ProjectMemberDomain>
        {
            new ProjectMemberDomain { UserId = "user-1" }
        };

        _projectMemberRepositoryMock
            .Setup(repo => repo.GetProjectMembersAsync(projectId, 1, 100))
            .ReturnsAsync(projectMembers);

        var user = new UserDomain {
            Id = "user-1",
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock
            .Setup(repo => repo.FindUserAsync(It.Is<UserQueryParams>(p => p.UserId == "user-1")))
            .ReturnsAsync(user);

        // Act
        var result = await _projectUseCase.GetProject(projectId);

        // Assert
        result.Should().BeEquivalentTo(project);
        _projectRepositoryMock.Verify(repo => repo.GetProject(projectId), Times.Once);
    }

    [Fact]
    public async Task GetProject_NonExistingProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = "non-existing-id";
        _projectRepositoryMock
            .Setup(repo => repo.GetProject(projectId))
            .ReturnsAsync((ProjectDomain)null!);

        // Act
        var act = () => _projectUseCase.GetProject(projectId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Project with ID {projectId} not found");
    }

    [Fact]
    public async Task UpdateProject_ValidProject_ReturnsUpdatedProject()
    {
        // Arrange
        var project = new ProjectDomain
        {
            Id = "test-id",
            Name = "Updated Project",
            Key = "TEST",
            Access = ProjectAccess.Private,
            Type = ProjectType.Kanban,
            OwnerId = "user-1"  
        };

        var existingProject = new ProjectDomain
        {
            Id = project.Id,
            Name = "Original Project",
            Key = "ORIG",
            Access = ProjectAccess.Public,
            Type = ProjectType.Scrum,
            OwnerId = "user-1"  // Same owner
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetProject(project.Id))
            .ReturnsAsync(existingProject);

        _projectRepositoryMock
            .Setup(repo => repo.UpdateProject(It.IsAny<ProjectDomain>()))
            .ReturnsAsync((ProjectDomain p) => p);

        // Act
        var result = await _projectUseCase.UpdateProject(project);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(project.Name);
        result.Key.Should().Be(project.Key);
        result.Access.Should().Be(project.Access);
        result.Type.Should().Be(project.Type);
        result.OwnerId.Should().Be(existingProject.OwnerId); // Owner should not change
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateProject_AttemptToChangeOwner_ThrowsInvalidOperationException()
    {
        // Arrange
        var project = new ProjectDomain
        {
            Id = "test-id",
            Name = "Updated Project",
            Key = "TEST",
            OwnerId = "new-owner"  // Attempting to change owner
        };

        var existingProject = new ProjectDomain
        {
            Id = project.Id,
            Name = "Original Project",
            Key = "ORIG",
            OwnerId = "original-owner"
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetProject(project.Id))
            .ReturnsAsync(existingProject);

        // Act
        var act = () => _projectUseCase.UpdateProject(project);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot change project owner through update");
    }

    [Fact]
    public async Task CreateColumn_Success_ReturnsNewColumn()
    {
        // Arrange
        var param = new CreateColumnParams
        {
            ProjectId = "test-project",
            Name = "New Column"
        };

        var existingColumns = new List<ProjectColumnDomain>
        {
            new() { Order = 1 },
            new() { Order = 2 }
        };

        _projectRepositoryMock
            .Setup(repo => repo.FindColumnsByProjectId(param.ProjectId))
            .ReturnsAsync(existingColumns);

        _projectRepositoryMock
            .Setup(repo => repo.CreateColumn(It.IsAny<ProjectColumnDomain>()))
            .ReturnsAsync((ProjectColumnDomain col) => new ProjectColumnDomain
            {
                Id = "new-col-id",
                Name = col.Name,
                ProjectId = col.ProjectId,
                CreatedAt = col.CreatedAt,
                UpdatedAt = col.UpdatedAt,
                Issues = col.Issues,
                Order = col.Order
            });

        // Act
        var result = await _projectUseCase.CreateColumn(param);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("new-col-id");
        result.Name.Should().Be(param.Name);
        result.ProjectId.Should().Be(param.ProjectId);
        result.Order.Should().Be(3); // Highest existing order + 1
    }
}