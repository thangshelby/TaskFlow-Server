using Grpc.Core;
using MainService.Domain.UseCases;
using MainService.Domain.Entities;
using TaskFlow.TeamService;
using Google.Protobuf.WellKnownTypes;
using BaseService;
using DomainTeamMemberRole = MainService.Domain.Enums.TeamMemberRole;
using GrpcTeamMemberRole = TaskFlow.TeamService.TeamMemberRole;
using AutoMapper;

public class TeamController : TeamService.TeamServiceBase
{
    private readonly TeamMemberUseCase _teamMemberUseCase;
    private readonly ProjectUseCase _projectUseCase;
    private readonly IMapper _mapper;
    private readonly ILogger<TeamController> _logger;

    public TeamController(
        TeamMemberUseCase teamMemberUseCase,
        ProjectUseCase projectUseCase,
        IMapper mapper,
        ILogger<TeamController> logger)
    {
        _teamMemberUseCase = teamMemberUseCase ?? throw new ArgumentNullException(nameof(teamMemberUseCase));
        _projectUseCase = projectUseCase ?? throw new ArgumentNullException(nameof(projectUseCase));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<TeamMemberRes> AddTeamMember(AddTeamMemberReq request, ServerCallContext context)
    {
        var member = await _teamMemberUseCase.AddTeamMemberAsync(
            request.ProjectId,
            request.UserId,
            (DomainTeamMemberRole)request.Role
        );

        return MapToTeamMemberResponse(member);
    }

    public override async Task<TeamMemberRes> UpdateTeamMemberRole(UpdateTeamMemberRoleReq request, ServerCallContext context)
    {
        var member = await _teamMemberUseCase.UpdateTeamMemberRoleAsync(
            request.ProjectId,
            request.UserId,
            (DomainTeamMemberRole)request.Role
        );

        return MapToTeamMemberResponse(member);
    }

    public override async Task<Empty> RemoveTeamMember(RemoveTeamMemberReq request, ServerCallContext context)
    {
        await _teamMemberUseCase.RemoveTeamMemberAsync(request.ProjectId, request.UserId);
        return new Empty();
    }

    public override async Task<ListTeamMembersRes> ListTeamMembers(ListTeamMembersReq request, ServerCallContext context)
    {
        var (members, totalCount) = await _teamMemberUseCase.GetProjectMembersAsync(
            request.ProjectId,
            (int)request.Page,
            (int)request.Limit
        );

        var response = new ListTeamMembersRes
        {
            Pagination = new PaginationRes
            {
                TotalItems = totalCount,
                CurrentPage = request.Page,
                Limit = request.Limit,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit)
            }
        };

        response.Data.AddRange(members.Select(MapToTeamMemberResponse));
        return response;
    }

    public override async Task<GetUserTeamsRes> GetUserTeams(UserTeamMembersReq request, ServerCallContext context)
    {
        try
        {
            if (request.Page <= 0) request.Page = 1;
            if (request.Limit <= 0) request.Limit = 10;
            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));
            }

            _logger.LogInformation("Getting teams for user {UserId}", request.UserId);
            var (teams, totalCount) = await _teamMemberUseCase.GetMyTeamsAsync(
                request.UserId,
                (int)request.Page,
                (int)request.Limit
            );

            var response = new GetUserTeamsRes
            {
                Pagination = new PaginationRes
                {
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit),
                    CurrentPage = request.Page,
                    Limit = request.Limit
                }
            };

            foreach (var team in teams)
            {
                var project = await _projectUseCase.GetProject(team.ProjectId);
                var userTeam = new UserTeamRes
                {
                    Id = team.Id ?? string.Empty,
                    ProjectId = team.ProjectId,
                    UserId = team.UserId,
                    Role = (GrpcTeamMemberRole)team.Role,
                    CreatedAt = team.CreatedAt.ToString("O"),
                    UpdatedAt = team.UpdatedAt.ToString("O"),
                    Project = new ProjectInfo
                    {
                        Id = project.Id ?? string.Empty,
                        Name = project.Name,
                        Description = project.Key,
                        Status = project.Access.ToString(),
                        CreatedAt = project.CreatedAt.ToString("O"),
                        UpdatedAt = project.UpdatedAt.ToString("O")
                    }
                };
                response.Data.Add(userTeam);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams for user {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Error getting user teams"));
        }
    }

    private static TeamMemberRes MapToTeamMemberResponse(TeamMemberDomain member)
    {
        return new TeamMemberRes
        {
            Id = member.Id ?? string.Empty,
            ProjectId = member.ProjectId,
            UserId = member.UserId,
            Role = (GrpcTeamMemberRole)member.Role,
            CreatedAt = member.CreatedAt.ToString("O"),
            UpdatedAt = member.UpdatedAt.ToString("O"),
        };
    }
}