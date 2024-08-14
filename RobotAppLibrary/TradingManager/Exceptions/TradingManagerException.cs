using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibrary.TradingManager.Exceptions;

[ExcludeFromCodeCoverage]
public class TradingManagerException : Exception
{
    public TradingManagerException()
    {
    }

    protected TradingManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public TradingManagerException(string? message) : base(message)
    {
    }

    public TradingManagerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}