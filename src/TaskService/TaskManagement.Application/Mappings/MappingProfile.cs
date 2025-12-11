using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Task, TaskDto>();
        CreateMap<TaskDto, Task>();
    }
}

