using AutoMapper;
using robot_project_v3.Database.Modeles;
using robot_project_v3.Server.Dto.Response;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Strategy;

namespace robot_project_v3.Server.Mapper;

public class MappingProfilesBackgroundServices : Profile
{
    public MappingProfilesBackgroundServices()
    {
        CreateMap<StrategyBase, StrategyInfoDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(x => x.Symbol, opt => opt.MapFrom(src => src.Symbol))
            .ForMember(x => x.StrategyName, opt => opt.MapFrom(src => src.StrategyName))
            .ForMember(x => x.CanRun, opt => opt.MapFrom(src => src.CanRun))
            .ForMember(x => x.StrategyDisabled, opt => opt.MapFrom(src => src.StrategyDisabled))
            .ForMember(x => x.SecureControlPosition, opt => opt.MapFrom(src => src.StrategyResult.SecureControlPosition))
            .ForMember(x => x.LastCandle, opt => opt.MapFrom(src => src.MainChart.CurrentCandle))
            .ForMember(x => x.LastTick, opt => opt.MapFrom(src => src.MainChart.LastPrice));
        
        CreateMap<Result, ResultDto>();
        CreateMap<MonthlyResult, MonthlyResultDto>();
        CreateMap<GlobalResults, GlobalResultsDto>();
        
        

     
        
        CreateMap<AccountBalance, AccountBalanceDto>();
        CreateMap<Candle, CandleDto>();
        CreateMap<Tick, TickDto>();


        // Mapper position
        CreateMap<ReasonClosed, string>().ConvertUsing(src => src.ToString());
        CreateMap<StatusPosition, string>().ConvertUsing(src => src.ToString());
        CreateMap<TypeOperation, string>().ConvertUsing(src => src.ToString());

        CreateMap<Timeframe, string>().ConvertUsing(src => src.ToString());
        CreateMap<Position, PositionDto>();

        CreateMap<StrategyFile, StrategyFileDto>().ReverseMap();
    }
}