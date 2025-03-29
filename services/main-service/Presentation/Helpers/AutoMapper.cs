

using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;
using TaskFlow.IssueService;
using TaskFlow.ProjectService;

namespace CamQuizzBE.Applications.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Project, ProjectDomain>().ReverseMap();
        CreateMap<Issue, IssueDomain>().ReverseMap();
        CreateMap<User, UserDomain>().ReverseMap();
        CreateMap<Sprint, SprintDomain>().ReverseMap();

        CreateMap<CreateProjectReq, ProjectDomain>();
        CreateMap<UpdateProjectReq, ProjectDomain>();
        CreateMap<CreateIssueReq, IssueDomain>();


        CreateMap<ProjectDomain, ProjectRes>();
        CreateMap<IssueDomain, IssueRes>();
    }
}