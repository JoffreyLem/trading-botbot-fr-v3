using System.Runtime.Serialization;

namespace robot_project_v3.Server.Exceptions;

public class UnhandledCommandException : Exception
{
    public UnhandledCommandException()
    {
    }

    protected UnhandledCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public UnhandledCommandException(string? message) : base(message)
    {
    }

    public UnhandledCommandException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}