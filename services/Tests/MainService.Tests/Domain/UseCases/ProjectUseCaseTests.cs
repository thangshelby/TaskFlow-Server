using Xunit;
using Moq;
using FluentAssertions;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using MainService.Domain.Interfaces;
using MainService.Domain.Enums;

namespace MainService.Tests.Domain.UseCases;

public class ProjectUseCaseTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ITransactionRepo> _transactionRepoMock;
    private readonly ProjectUseCase _projectUseCase;

    public ProjectUseCaseTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _transactionRepoMock = new Mock<ITransactionRepo>();
        _projectUseCase = new ProjectUseCase(_projectRepositoryMock.Object, _transactionRepoMock.Object);
    }

    [Fact]
    public async Task CreateProject_ValidProject_ReturnsCreatedProject()
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

        _projectRepositoryMock
            .Setup(repo => repo.CreateProject(It.IsAny<ProjectDomain>()))
            .ReturnsAsync((ProjectDomain p) => new ProjectDomain
            {
                Id = "test-id",
                Name = p.Name,
                Key = p.Key,
                Access = p.Access,
                Type = p.Type,
                OwnerId = p.OwnerId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

        // Act
        var result = await _projectUseCase.CreateProject(project);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("test-id");
        result.Name.Should().Be(project.Name);
        result.Key.Should().Be(project.Key);
        result.Access.Should().Be(project.Access);
        result.Type.Should().Be(project.Type);
        result.OwnerId.Should().Be(project.OwnerId);

        _projectRepositoryMock.Verify(
            repo => repo.CreateProject(It.Is<ProjectDomain>(p => 
                p.Name == project.Name && 
                p.Key == project.Key)),
            Times.Once
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateProject_EmptyName_ThrowsArgumentException(string name)
    {
        // Arrange
        var project = new ProjectDomain
        {
            Name = name,
            Key = "TEST"
        };

        // Act
        var act = () => _projectUseCase.CreateProject(project);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Project name cannot be empty");
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
            .ReturnsAsync((ProjectDomain)null);

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
            OwnerId = "user-1"
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
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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