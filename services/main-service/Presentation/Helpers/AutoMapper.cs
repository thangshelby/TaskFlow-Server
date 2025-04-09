using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;
using TaskFlow.IssueService;
using TaskFlow.ProjectService;
using TaskFlow.SprintService;
using TaskFlow.TeamService;
using TaskFlow.UserService;

namespace CamQuizzBE.Applications.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Project, ProjectDomain>().ReverseMap();
        CreateMap<Issue, IssueDomain>().ReverseMap();
        CreateMap<User, UserDomain>().ReverseMap();
        CreateMap<Sprint, SprintDomain>().ReverseMap();
        CreateMap<TeamMember, TeamMemberDomain>().ReverseMap();

        CreateMap<CreateSprintReq, SprintDomain>();
        CreateMap<UpdateSprintReq, SprintDomain>();
        CreateMap<CreateProjectReq, ProjectDomain>();
        CreateMap<UpdateProjectReq, ProjectDomain>();
        CreateMap<CreateIssueReq, IssueDomain>();
        CreateMap<AddTeamMemberReq, TeamMemberDomain>();
        CreateMap<UpdateTeamMemberRoleReq, TeamMemberDomain>();

        CreateMap<ProjectDomain, ProjectRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")));
        
        CreateMap<IssueDomain, IssueRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")));

        CreateMap<UserDomain, UserRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<SprintDomain, SprintRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.DateStarted, opt => opt.MapFrom(src => src.DateStarted.ToString("o")))
            .ForMember(dest => dest.DateEnded, opt => opt.MapFrom(src => src.DateEnded.ToString("o")));
        
        CreateMap<TeamMemberDomain, TeamMemberRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")));
       
    }
}