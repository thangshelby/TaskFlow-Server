using MainService.Domain.Entities;
using MainService.Domain.UseCases;
using MainService.Presentation.Controllers;
using MainService.Domain.Enums;
using Moq;
using AutoMapper;
using Grpc.Core;
using TaskFlow.ProjectMemberService;
using BaseService;

namespace MainService.Tests.Presentation.Controllers;

public class ProjectMemberControllerTests
{
    private readonly Mock<ProjectMemberUseCase> _mockProjectMemberUseCase;
    private readonly Mock<ProjectUseCase> _mockProjectUseCase;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProjectMemberController>> _mockLogger;
    private readonly ProjectMemberController _controller;
    private readonly ServerCallContext _context;

    public ProjectMemberControllerTests()
    {
        _mockProjectMemberUseCase = new Mock<ProjectMemberUseCase>();
        _mockProjectUseCase = new Mock<ProjectUseCase>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProjectMemberController>>();
        _controller = new ProjectMemberController(
            _mockProjectMemberUseCase.Object,
            _mockProjectUseCase.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );

        // Setup mock context with authenticated user
        var mockHttpContext = new DefaultHttpContext();
        var metadata = new Metadata();
        var mockServerCallContext = new TestServerCallContext();
        mockServerCallContext.UserStateCore["UserId"] = "test-user-id";
        _context = mockServerCallContext;
    }

    [Fact]
    public async Task AddProjectMember_WhenAuthenticated_ShouldReturnMember()
    {
        // Arrange
        var request = new AddProjectMemberReq
        {
            ProjectId = "test-project",
            UserId = "test-member",
            Role = ProjectMemberRole.Member
        };

        var domainMember = new ProjectMemberDomain
        {
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = TeamMemberRole.Member
        };

        var response = new ProjectMemberRes
        {
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = ProjectMemberRole.Member
        };

        _mockProjectMemberUseCase.Setup(x => x.AddProjectMemberAsync(
            request.ProjectId,
            "test-user-id",
            request.UserId,
            TeamMemberRole.Member
        )).ReturnsAsync(domainMember);

        _mockMapper.Setup(x => x.Map<ProjectMemberRes>(domainMember))
            .Returns(response);

        // Act
        var result = await _controller.AddProjectMember(request, _context);

        // Assert
        Assert.Equal(response, result);
    }

    [Fact]
    public async Task AddProjectMember_WhenNotAuthenticated_ShouldThrowException()
    {
        // Arrange
        var request = new AddProjectMemberReq();
        var mockContext = new TestServerCallContext(); // No user state set

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _controller.AddProjectMember(request, mockContext));
        Assert.Equal(StatusCode.Unauthenticated, exception.Status.StatusCode);
    }

    [Fact]
    public async Task ApproveMember_WhenAuthenticatedAndOwner_ShouldApproveMember()
    {
        // Arrange
        var request = new ApproveMemberReq
        {
            ProjectId = "test-project",
            UserId = "test-member"
        };

        var domainMember = new ProjectMemberDomain
        {
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = TeamMemberRole.Member,
            IsPending = false
        };

        var response = new ProjectMemberRes
        {
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = ProjectMemberRole.Member,
            IsPending = false
        };

        _mockProjectMemberUseCase.Setup(x => x.ApproveProjectMemberAsync(
            request.ProjectId,
            "test-user-id",
            request.UserId
        )).ReturnsAsync(domainMember);

        _mockMapper.Setup(x => x.Map<ProjectMemberRes>(domainMember))
            .Returns(response);

        // Act
        var result = await _controller.ApproveMember(request, _context);

        // Assert
        Assert.Equal(response, result);
        Assert.False(result.IsPending);
    }

    [Fact]
    public async Task ApproveMember_WhenUnauthorized_ShouldThrowPermissionDenied()
    {
        // Arrange
        var request = new ApproveMemberReq
        {
            ProjectId = "test-project",
            UserId = "test-member"
        };

        _mockProjectMemberUseCase.Setup(x => x.ApproveProjectMemberAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _controller.ApproveMember(request, _context));
        Assert.Equal(StatusCode.PermissionDenied, exception.Status.StatusCode);
    }

    [Fact]
    public async Task RejectMember_WhenAuthenticatedAndOwner_ShouldRejectMember()
    {
        // Arrange
        var request = new RejectMemberReq
        {
            ProjectId = "test-project",
            UserId = "test-member"
        };

        _mockProjectMemberUseCase.Setup(x => x.RejectProjectMemberAsync(
            request.ProjectId,
            "test-user-id",
            request.UserId
        )).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RejectMember(request, _context);

        // Assert
        Assert.IsType<Empty>(result);
        _mockProjectMemberUseCase.Verify(x => x.RejectProjectMemberAsync(
            request.ProjectId,
            "test-user-id",
            request.UserId
        ), Times.Once);
    }

    [Fact]
    public async Task RejectMember_WhenNotFound_ShouldThrowNotFound()
    {
        // Arrange
        var request = new RejectMemberReq
        {
            ProjectId = "test-project",
            UserId = "test-member"
        };

        _mockProjectMemberUseCase.Setup(x => x.RejectProjectMemberAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ThrowsAsync(new KeyNotFoundException("Member not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _controller.RejectMember(request, _context));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    // Helper class for testing
    public class TestServerCallContext : ServerCallContext
    {
        public override IDictionary<object, object> UserState { get; } = new Dictionary<object, object>();
        protected override string MethodCore => "TestMethod";
        protected override string HostCore => "TestHost";
        protected override string PeerCore => "TestPeer";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => new Metadata();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => new Metadata();
        protected override Status StatusCore { get; set; } = Status.DefaultSuccess;
        protected override WriteOptions WriteOptionsCore { get; set; } = new WriteOptions();
        protected override AuthContext AuthContextCore => null;
        public override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => null;
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
    }
}