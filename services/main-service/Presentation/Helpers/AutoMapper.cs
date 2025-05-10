using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;
using TaskFlow.IssueService;
using TaskFlow.ProjectService;
using TaskFlow.SprintService;
using TaskFlow.UserService;
using BaseService;
using TaskFlow.ProjectMemberService;

namespace CamQuizzBE.Applications.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Project, ProjectDomain>().ReverseMap();
        CreateMap<ProjectColumn, ProjectColumnDomain>().ReverseMap();
        CreateMap<Issue, IssueDomain>()
            .ForMember(dest => dest.SprintId, opt => opt.MapFrom(src => src.SprintId ?? string.Empty))
            .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src => src.AssigneeId ?? string.Empty))
            .ReverseMap();
        CreateMap<User, UserDomain>().ReverseMap();
        CreateMap<Sprint, SprintDomain>().ReverseMap();
        CreateMap<ProjectMember, ProjectMemberDomain>().ReverseMap();

        CreateMap<CreateSprintReq, SprintDomain>();
        CreateMap<UpdateSprintReq, SprintDomain>();
        CreateMap<CreateProjectReq, ProjectDomain>();
        CreateMap<UpdateProjectReq, ProjectDomain>();
        CreateMap<CreateIssueReq, IssueDomain>()
            .ForMember(dest => dest.SprintId, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.SprintId) ? null : src.SprintId))
            .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.AssigneeId) ? null : src.AssigneeId));

        CreateMap<UpdateIssueReq, IssueDomain>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SprintId, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.SprintId) ? null : src.SprintId))
            .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.AssigneeId) ? null : src.AssigneeId));

        CreateMap<AddProjectMemberReq, ProjectMemberDomain>();
        CreateMap<UpdateProjectMemberRoleReq, ProjectMemberDomain>();

        CreateMap<ProjectDomain, ProjectRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.ProjectMembers, opt => opt.MapFrom(src => src.ProjectMembers));

        CreateMap<ProjectColumnDomain, ColumnRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")));

        CreateMap<IssueDomain, IssueRes>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.SprintId, opt => opt.MapFrom(src => src.SprintId ?? string.Empty))
            .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src => src.AssigneeId ?? string.Empty));

        CreateMap<UserDomain, UserRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<SprintDomain, SprintRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.DateStarted, opt => opt.MapFrom(src => src.DateStarted.ToString("o")))
            .ForMember(dest => dest.DateEnded, opt => opt.MapFrom(src => src.DateEnded.ToString("o")));

        CreateMap<UserDomain, UserInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<ProjectMemberDomain, ProjectMemberRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

        CreateMap<ProjectMemberDomain, UserMembershipRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")));
    }
}