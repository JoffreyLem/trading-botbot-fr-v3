using RobotAppLibrary.Api.Providers.Exceptions;
using Serilog.Core;
using Serilog.Events;

namespace robot_project_v3.Server;

public class ApiProvidersExceptionDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        if (value is ApiProvidersException apiEx)
        {
            var properties = new List<LogEventProperty>
            {
                new(nameof(apiEx.Message), new ScalarValue(apiEx.Message)),
                new(nameof(apiEx.ErrorCode), new ScalarValue(apiEx.ErrorCode ?? "null")),
                new(nameof(apiEx.StackTrace), new ScalarValue(apiEx.StackTrace))
            };

            result = new StructureValue(properties);
            return true;
        }

        result = null;
        return false;
    }
}

public class CustomExceptionEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Exception is ApiProvidersException apiEx)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(apiEx.ErrorCode), new ScalarValue(apiEx.ErrorCode)));
            logEvent.AddOrUpdateProperty(new LogEventProperty(nameof(apiEx.Message), new ScalarValue(apiEx.Message)));
        }
    }
}
