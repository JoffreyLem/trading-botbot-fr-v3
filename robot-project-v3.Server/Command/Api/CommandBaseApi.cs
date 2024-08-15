namespace robot_project_v3.Server.Command.Api;

public abstract class CommandeBaseApiAbstract : CommandeBaseAbstract
{
}

public class CommandBaseApi<T> : CommandeBaseApiAbstract
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();

    public override void SetException(Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}