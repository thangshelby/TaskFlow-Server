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
using MongoDB.Driver;
using ProtoProjectAccess = TaskFlow.ProjectService.ProjectAccess;
using ProtoProjectType = TaskFlow.ProjectService.ProjectType;
using DomainProjectAccess = MainService.Domain.Enums.ProjectAccess;
using DomainProjectType = MainService.Domain.Enums.ProjectType;

namespace MainService.Tests.Presentation.Controllers;

public class TestServerCallContext : ServerCallContext
{
    private readonly IDictionary<object, object> _userState;

    public TestServerCallContext(IDictionary<object, object> userState)
    {
        _userState = userState;
    }

    protected override IDictionary<object, object> UserStateCore => _userState;

    protected override string MethodCore => "";
    protected override string HostCore => "";
    protected override string PeerCore => "";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore => new Metadata();
    protected override CancellationToken CancellationTokenCore => CancellationToken.None;
    protected override Metadata ResponseTrailersCore => new Metadata();
    protected override Status StatusCore { get; set; } = Status.DefaultSuccess;
    protected override WriteOptions WriteOptionsCore { get; set; } = new WriteOptions();

    protected override AuthContext AuthContextCore => throw new NotImplementedException();

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
    {
        throw new NotImplementedException();
    }

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        return Task.CompletedTask;
    }
}

[Collection("Project Tests")]
public class ProjectControllerTests : IDisposable
{
    private readonly ProjectUseCase _projectUseCase;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ITransactionRepo> _transactionRepoMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
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
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        // Setup transaction handling
        _transactionRepoMock
            .Setup(repo => repo.ExecuteAsync(It.IsAny<Func<IClientSessionHandle, Task>>()))
            .Returns<Func<IClientSessionHandle, Task>>(async func =>
            {
                await func(Mock.Of<IClientSessionHandle>());
                return true;
            });

        // Setup project member repository for owner member creation
        _projectMemberRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ProjectMemberDomain>()))
            .ReturnsAsync((ProjectMemberDomain member) => member);

        _projectUseCase = new ProjectUseCase(
            _projectRepositoryMock.Object,
            _transactionRepoMock.Object,
            _projectMemberRepositoryMock.Object,
            _userRepositoryMock.Object
        );
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
        
        // Create TestServerCallContext with user state
        var userState = new Dictionary<object, object>
        {
            { "UserId", "owner-1" }
        };
        _context = new TestServerCallContext(userState);
    }

    public void Dispose()
    {
        _projectRepositoryMock.VerifyAll();
        _transactionRepoMock.VerifyAll();
        _projectMemberRepositoryMock.VerifyAll();
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
            Type = ProtoProjectType.Scrum
        };

        var projectDomain = new ProjectDomain
        {
            Id = "new-id",
            Name = request.Name,
            Key = request.Key,
            Access = (DomainProjectAccess)request.Access,
            Type = (DomainProjectType)request.Type,
            OwnerId = "owner-1", // This should match what we get from the context
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

        // Setup validator to indicate success
        var validationResult = new FluentValidation.Results.ValidationResult();
        _createProjectValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Setup mapper to return our domain project with owner ID from context
        var mappedProject = new ProjectDomain
        {
            Name = request.Name,
            Key = request.Key,
            Access = (DomainProjectAccess)request.Access,
            Type = (DomainProjectType)request.Type,
            OwnerId = "owner-1"  // This should come from the context
        };
        _mapperMock
            .Setup(m => m.Map<ProjectDomain>(request))
            .Returns(mappedProject);

        // Setup repository to accept the project and return it with an ID
        _projectRepositoryMock
            .Setup(repo => repo.CreateProject(It.Is<ProjectDomain>(p =>
                p.Name == request.Name &&
                p.Key == request.Key &&
                p.OwnerId == "owner-1" &&
                p.Access == (DomainProjectAccess)request.Access &&
                p.Type == (DomainProjectType)request.Type)))
            .ReturnsAsync(projectDomain);

        // Setup mapper to convert domain project back to response
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
        response.Data.OwnerId.Should().Be("owner-1");  // Should match the owner ID from the context
        response.Status.Should().Be("success");
        response.Message.Should().Be("Create project success.");

        // Verify that the project was created with the correct owner ID from the context
        _projectRepositoryMock.Verify(
            repo => repo.CreateProject(It.Is<ProjectDomain>(p =>
                p.Name == request.Name &&
                p.Key == request.Key &&
                p.OwnerId == "owner-1" && // This should match what we get from the context
                p.Access == (DomainProjectAccess)request.Access &&
                p.Type == (DomainProjectType)request.Type)),
            Times.Once
        );
    }
}