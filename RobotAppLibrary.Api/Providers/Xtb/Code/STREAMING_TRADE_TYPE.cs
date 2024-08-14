namespace RobotAppLibrary.Api.Providers.Xtb.Code;

public class STREAMING_TRADE_TYPE(long code) : BaseCode(code)
{
    public static readonly STREAMING_TRADE_TYPE OPEN = new(0L);
    public static readonly STREAMING_TRADE_TYPE PENDING = new(1L);
    public static readonly STREAMING_TRADE_TYPE CLOSE = new(2L);
}