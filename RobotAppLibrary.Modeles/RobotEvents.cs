namespace RobotAppLibrary.Modeles;

public class RobotEvent<T> : EventArgs
{
    public RobotEvent(string id)
    {
        Id = id;
    }

    public RobotEvent(T eventField, string id)
    {
        EventField = eventField;
        Id = id;
    }

    public T? EventField { get; set; }

    public string Id { get; set; }

    public DateTime Date => DateTime.UtcNow;
}