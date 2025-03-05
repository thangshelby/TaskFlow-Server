

using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;

namespace CamQuizzBE.Applications.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Project, ProjectDomain>().ReverseMap();
    }
}