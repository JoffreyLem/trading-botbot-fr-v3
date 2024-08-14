namespace robot_project_v3.Server.Command.Strategy;

public abstract class CommandeBaseStrategyAbstract : CommandeBaseAbstract
{
}

public class CommandBaseStrategy<T> : CommandeBaseStrategyAbstract 
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();

    public string Id { get; set; }

    public override void SetException(Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}