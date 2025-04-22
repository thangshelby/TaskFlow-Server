using Xunit;
using Moq;
using FluentAssertions;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using MainService.Domain.Enums;
using MainService.Domain.Interfaces;
using MainService.Presentation;
using TaskFlow.ProjectService;
using FluentValidation;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using ProtoProjectAccess = TaskFlow.ProjectService.ProjectAccess;
using ProtoProjectType = TaskFlow.ProjectService.ProjectType;
using DomainProjectAccess = MainService.Domain.Enums.ProjectAccess;
using DomainProjectType = MainService.Domain.Enums.ProjectType;

namespace MainService.Tests.Presentation.Controllers;

[Collection("Project Tests")]
public class ProjectControllerTests : IDisposable
{
    private readonly ProjectUseCase _projectUseCase;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ITransactionRepo> _transactionRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ProjectController>> _loggerMock;
    private readonly Mock<IValidator<CreateProjectReq>> _createProjectValidatorMock;
    private readonly Mock<IValidator<UpdateProjectReq>> _updateProjectValidatorMock;
    private readonly Mock<IValidator<ListProjectsReq>> _listProjectsValidatorMock;
    private readonly ProjectController _controller;
    private readonly ServerCallContext _context;

    public ProjectControllerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _transactionRepoMock = new Mock<ITransactionRepo>();
        _projectUseCase = new ProjectUseCase(_projectRepositoryMock.Object, _transactionRepoMock.Object);
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ProjectController>>();
        _createProjectValidatorMock = new Mock<IValidator<CreateProjectReq>>();
        _updateProjectValidatorMock = new Mock<IValidator<UpdateProjectReq>>();
        _listProjectsValidatorMock = new Mock<IValidator<ListProjectsReq>>();

        _controller = new ProjectController(
            _projectUseCase,
            _mapperMock.Object,
            _loggerMock.Object,
            _createProjectValidatorMock.Object,
            _updateProjectValidatorMock.Object,
            _listProjectsValidatorMock.Object
        );
        
        // Create mock ServerCallContext
        var contextMock = new Mock<ServerCallContext>();
        _context = contextMock.Object;
    }

    public void Dispose()
    {
        _projectRepositoryMock.VerifyAll();
        _transactionRepoMock.VerifyAll();
    }

    [Fact]
    public async Task GetProject_ExistingProject_ReturnsProject()
    {
        // Arrange
        var projectId = "test-id";
        var projectDomain = new ProjectDomain
        {
            Id = projectId,
            Name = "Test Project",
            Key = "TEST",
            Access = DomainProjectAccess.Public,
            Type = DomainProjectType.Scrum,
            OwnerId = "owner-1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var expectedResponse = new ProjectRes
        {
            Id = projectId,
            Name = projectDomain.Name,
            Key = projectDomain.Key,
            Access = ProtoProjectAccess.Public,
            Type = ProtoProjectType.Scrum,
            OwnerId = projectDomain.OwnerId,
            CreatedAt = projectDomain.CreatedAt.ToString(),
            UpdatedAt = projectDomain.UpdatedAt.ToString()
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetProject(projectId))
            .ReturnsAsync(projectDomain);
            
        _mapperMock
            .Setup(m => m.Map<ProjectRes>(It.IsAny<ProjectDomain>()))
            .Returns(expectedResponse);

        var request = new GetProjectReq { Id = projectId };

        // Act
        var response = await _controller.GetProject(request, _context);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(projectId);
        response.Name.Should().Be(projectDomain.Name);
        response.Key.Should().Be(projectDomain.Key);
        response.Access.Should().Be(ProtoProjectAccess.Public);
        response.Type.Should().Be(ProtoProjectType.Scrum);
        response.OwnerId.Should().Be(projectDomain.OwnerId);
        
        _projectRepositoryMock.Verify(
            repo => repo.GetProject(projectId),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateProject_ValidRequest_ReturnsCreatedProject()
    {
        // Arrange
        var request = new CreateProjectReq
        {
            Name = "New Project",
            Key = "NEW",
            Access = ProtoProjectAccess.Public,
            Type = ProtoProjectType.Scrum,
            OwnerId = "owner-1"
        };

        var projectDomain = new ProjectDomain
        {
            Id = "new-id",
            Name = request.Name,
            Key = request.Key,
            Access = (DomainProjectAccess)request.Access,
            Type = (DomainProjectType)request.Type,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var expectedResponse = new ProjectRes
        {
            Id = projectDomain.Id,
            Name = projectDomain.Name,
            Key = projectDomain.Key,
            Access = (ProtoProjectAccess)projectDomain.Access,
            Type = (ProtoProjectType)projectDomain.Type,
            OwnerId = projectDomain.OwnerId,
            CreatedAt = projectDomain.CreatedAt.ToString(),
            UpdatedAt = projectDomain.UpdatedAt.ToString()
        };

        _createProjectValidatorMock
            .Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mapperMock
            .Setup(m => m.Map<ProjectDomain>(request))
            .Returns(projectDomain);

        _projectRepositoryMock
            .Setup(repo => repo.CreateProject(It.IsAny<ProjectDomain>()))
            .ReturnsAsync(projectDomain);
            
        _mapperMock
            .Setup(m => m.Map<ProjectRes>(It.IsAny<ProjectDomain>()))
            .Returns(expectedResponse);

        // Act
        var response = await _controller.CreateProject(request, _context);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(projectDomain.Id);
        response.Data.Name.Should().Be(request.Name);
        response.Data.Key.Should().Be(request.Key);
        response.Data.Access.Should().Be(request.Access);
        response.Data.Type.Should().Be(request.Type);
        response.Data.OwnerId.Should().Be(request.OwnerId);
        response.Status.Should().Be("success");
        response.Message.Should().Be("Craete project success.");

        _projectRepositoryMock.Verify(
            repo => repo.CreateProject(It.Is<ProjectDomain>(p =>
                p.Name == request.Name &&
                p.Key == request.Key &&
                p.OwnerId == request.OwnerId &&
                p.Access == (DomainProjectAccess)request.Access &&
                p.Type == (DomainProjectType)request.Type)),
            Times.Once
        );
    }
}