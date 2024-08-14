using Serilog.Core;
using Serilog.Events;

namespace robot_project_v3.Server;

public class RemovePropertiesEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent le, ILogEventPropertyFactory lepf)
    {
        le.RemovePropertyIfPresent("RequestId");
        le.RemovePropertyIfPresent("RequestPath");
        le.RemovePropertyIfPresent("ActionName");
        le.RemovePropertyIfPresent("ActionId");
    }
}