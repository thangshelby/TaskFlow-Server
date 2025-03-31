

using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;
using TaskFlow.IssueService;
using TaskFlow.ProjectService;
using TaskFlow.UserService;

namespace CamQuizzBE.Applications.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        // CreateMap<Project, ProjectDomain>().ReverseMap();
        CreateMap<Issue, IssueDomain>().ReverseMap();
        CreateMap<User, UserDomain>().ReverseMap();
        //Project
        CreateMap<Project, ProjectDomain>().ReverseMap();
        CreateMap<CreateProjectReq, ProjectDomain>();
        CreateMap<UpdateProjectReq, ProjectDomain>();
        CreateMap<CreateIssueReq, IssueDomain>();


        CreateMap<ProjectDomain, ProjectRes>();
        CreateMap<IssueDomain, IssueRes>();
        CreateMap<UserDomain, UserRes>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}