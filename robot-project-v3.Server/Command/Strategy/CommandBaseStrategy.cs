namespace robot_project_v3.Server.Command.Strategy;

public abstract class CommandeBaseStrategyAbstract : CommandeBaseAbstract
{
}

public class CommandBaseStrategy<T, TU> : CommandeBaseStrategyAbstract where TU : new()
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();
    
    public  TU Data = new TU();

    public string Id { get; set; }

    public override void SetException(Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}