namespace robot_project_v3.Server.Command.Api;

public abstract class CommandeBaseApiAbstract : CommandeBaseAbstract
{
}

public class CommandBaseApi<T, TU> : CommandeBaseApiAbstract where TU : new()
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();

    public  TU Data = new TU();

    public override void SetException(Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}