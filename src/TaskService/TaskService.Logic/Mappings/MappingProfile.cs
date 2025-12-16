using AutoMapper;
using TaskService.Client.Models.Tasks;
using TaskService.Model.Data;

namespace TaskService.Logic.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        this.CreateMap<ClientTask, ServerTask>().ReverseMap();
    }
}