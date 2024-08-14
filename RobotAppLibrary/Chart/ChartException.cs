using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibrary.Chart;

[ExcludeFromCodeCoverage]
public class ChartException : Exception
{
    public ChartException()
    {
    }

    protected ChartException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ChartException(string? message) : base(message)
    {
    }

    public ChartException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}