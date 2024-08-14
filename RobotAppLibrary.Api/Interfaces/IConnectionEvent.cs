namespace RobotAppLibrary.Api.Interfaces;

public interface IConnectionEvent
{
    event EventHandler Connected;
    event EventHandler Disconnected;

}