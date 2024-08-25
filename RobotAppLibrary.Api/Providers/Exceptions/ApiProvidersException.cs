using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibrary.Api.Providers.Exceptions;

[ExcludeFromCodeCoverage]
public class ApiProvidersException : Exception
{
    public string? ErrorCode { get; set; }
    public ApiProvidersException()
    {
    }

    protected ApiProvidersException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ApiProvidersException(string? message) : base(message)
    {
    }

    public ApiProvidersException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}