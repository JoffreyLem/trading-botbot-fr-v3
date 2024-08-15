namespace RobotAppLibrary.Api.Modeles;

public class Server
{
    public Server()
    {
    }

    public Server(string address, int mainPort, int streamingPort, string description)
    {
        Address = address;
        MainPort = mainPort;
        StreamingPort = streamingPort;
        Description = description;
    }

    public Server(string address, string description)
    {
        Address = address;
        Description = description;
    }

    public string Address { get; set; }

    public string Description { get; set; }

    public int MainPort { get; set; }

    public int StreamingPort { get; set; }


    public override string ToString()
    {
        return Description + " (" + Address + ":" + MainPort + "/" + StreamingPort + ")";
    }
}