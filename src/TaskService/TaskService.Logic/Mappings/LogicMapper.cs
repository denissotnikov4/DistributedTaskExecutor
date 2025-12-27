using AutoMapper;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;
using TaskService.Dal.Models;

namespace TaskService.Logic.Mappings;

public static class LogicMapper
{
    private static readonly IMapper Mapper;

    static LogicMapper()
    {
        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config
                .CreateMap<ClientTask, ServerTask>()
                .ReverseMap();

            config
                .CreateMap<TaskCreateRequest, ServerTask>()
                .ForMember(dest => dest.Id, _ => Guid.NewGuid())
                .ForMember(dest => dest.Result, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore())
                .ForMember(dest => dest.RetryCount, opt => opt.Ignore());
        });

        Mapper = mapperConfiguration.CreateMapper();
    }

    public static ClientTask ToClientModel(this ServerTask serverTask)
    {
        return Mapper.Map<ClientTask>(serverTask);
    }

    public static ServerTask ToServerModel(this TaskCreateRequest createRequest)
    {
        return Mapper.Map<ServerTask>(createRequest);
    }
}