

using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;
using TaskFlow.ProjectService;

namespace CamQuizzBE.Applications.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Project, ProjectDomain>().ReverseMap();
        CreateMap<User, UserDomain>().ReverseMap();
        CreateMap<Sprint, SprintDomain>().ReverseMap();

        CreateMap<CreateProjectReq, ProjectDomain>();
        CreateMap<UpdateProjectReq, ProjectDomain>();
        CreateMap<ProjectDomain, ProjectRes>();
    }
}