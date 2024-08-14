using Xunit;

namespace RobotAppLibrary.Tests.Integrations.Api.Connector.Tcp.Fixture;

[CollectionDefinition("TcpServer collection")]
public class TcpServerCollection : ICollectionFixture<TcpServerFixture>
{
}