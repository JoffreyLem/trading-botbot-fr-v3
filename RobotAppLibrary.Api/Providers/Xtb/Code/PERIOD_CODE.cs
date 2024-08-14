namespace RobotAppLibrary.Api.Providers.Xtb.Code;

public class PERIOD_CODE(long code) : BaseCode(code)
{
    public static readonly PERIOD_CODE PERIOD_M1 = new(1L);
    public static readonly PERIOD_CODE PERIOD_M5 = new(5L);
    public static readonly PERIOD_CODE PERIOD_M15 = new(15L);
    public static readonly PERIOD_CODE PERIOD_M30 = new(30L);
    public static readonly PERIOD_CODE PERIOD_H1 = new(60L);
    public static readonly PERIOD_CODE PERIOD_H4 = new(240L);
    public static readonly PERIOD_CODE PERIOD_D1 = new(1440L);
    public static readonly PERIOD_CODE PERIOD_W1 = new(10080L);
    public static readonly PERIOD_CODE PERIOD_MN1 = new(43200L);
}