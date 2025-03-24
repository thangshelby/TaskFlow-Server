

using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;
using TaskFlow.ProjectService;
using TaskFlow.SprintService;

namespace CamQuizzBE.Applications.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        //User
        CreateMap<User, UserDomain>().ReverseMap();
        //Project
        CreateMap<Project, ProjectDomain>().ReverseMap();
        CreateMap<CreateProjectReq, ProjectDomain>();
        CreateMap<UpdateProjectReq, ProjectDomain>();
        CreateMap<ProjectDomain, ProjectRes>();

        //Sprint
        CreateMap<Sprint, SprintDomain>().ReverseMap();
        CreateMap<CreateSprintReq, SprintDomain>(); 
        CreateMap<UpdateSprintReq, SprintDomain>();
        CreateMap<SprintDomain, SprintRes>();   
    }
}