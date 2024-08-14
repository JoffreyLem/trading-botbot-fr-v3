using RobotAppLibrary.Api.Modeles;

namespace RobotAppLibrary.Api.Providers.Xtb;

public static class XtbServer
{
    public static Server DemoTcp => new("xapi.xtb.com", 5124, 5125, "DEMO SSL");

    public static Server RealTcp => new("xapi.xtb.com", 5112, 5113, "REAL SSL");


    public static Server DemoWss => new("wss://ws.xtb.com/demo", "DEMO WSS");

    public static Server DemoWssStreaming => new("wss://ws.xtb.com/demoStream", "DEMO WSS STREAMING");

    public static Server RealWss => new("wss://ws.xtb.com/real", "REAL WSS");

    public static Server RealWssStreaming => new("wss://ws.xtb.com/realStream", "REAL WSS STREAMING");
}