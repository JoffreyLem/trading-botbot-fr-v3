using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibrary.Strategy;

[ExcludeFromCodeCoverage]
public class StrategyException : Exception
{
    public StrategyException()
    {
    }

    protected StrategyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public StrategyException(string? message) : base(message)
    {
    }

    public StrategyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}