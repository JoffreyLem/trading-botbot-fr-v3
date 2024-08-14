namespace RobotAppLibrary.Api.Providers.Xtb.Code;

public class REQUEST_STATUS(long code) : BaseCode(code)
{
    public static readonly REQUEST_STATUS ERROR = new(0L);
    public static readonly REQUEST_STATUS PENDING = new(1L);
    public static readonly REQUEST_STATUS ACCEPTED = new(3L);
    public static readonly REQUEST_STATUS REJECTED = new(4L);
}