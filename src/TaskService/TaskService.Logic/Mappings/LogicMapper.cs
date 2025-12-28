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

            config.CreateMap<TaskUpdateRequest, ServerTask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RetryCount, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt =>
                    opt.PreCondition(src => src.Name != null))
                .ForMember(dest => dest.Code, opt =>
                    opt.PreCondition(src => src.Code != null))
                .ForMember(dest => dest.InputData, opt =>
                    opt.PreCondition(src => src.InputData != null))
                .ForMember(dest => dest.Result, opt =>
                    opt.PreCondition(src => src.Result != null))
                .ForMember(dest => dest.ErrorMessage, opt =>
                    opt.PreCondition(src => src.ErrorMessage != null))
                .ForMember(dest => dest.Language, opt =>
                    opt.PreCondition(src => src.Language.HasValue))
                .ForMember(dest => dest.UserId, opt =>
                    opt.PreCondition(src => src.UserId.HasValue))
                .ForMember(dest => dest.Status, opt =>
                    opt.PreCondition(src => src.Status.HasValue))
                .ForMember(dest => dest.StartedAt, opt =>
                    opt.PreCondition(src => src.StartedAt.HasValue))
                .ForMember(dest => dest.CompletedAt, opt =>
                    opt.PreCondition(src => src.CompletedAt.HasValue))
                .ForMember(dest => dest.Ttl, opt =>
                    opt.PreCondition(src => src.Ttl.HasValue));
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

    public static ServerTask UpdateServerTaskFromRequest(this ServerTask existingTask, TaskUpdateRequest updateRequest)
    {
        return Mapper.Map(updateRequest, existingTask);
    }
}