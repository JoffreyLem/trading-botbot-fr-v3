using System.Security.Cryptography.X509Certificates;
using RobotAppLibrary.Tests.Integrations.Api.Connector.Tcp.Mock;

namespace RobotAppLibrary.Tests.Integrations.Api.Connector.Tcp.Fixture;

public class TcpServerFixture : IDisposable, IAsyncDisposable
{

    public const int ServerPort = 1234;
    public const int StreamingServerPort = 5678;
    public TcpServerMock Server { get; private set; }
    public TcpServerMock StreamingServer { get; private set; }

    public X509Certificate2 Certificate { get; private set; }

    public TcpServerFixture()
    {
        Certificate = CertificateGenerator.CreateSelfSignedCertificate("localhost");
        Server = new TcpServerMock(ServerPort, Certificate);
        StreamingServer = new TcpServerMock(StreamingServerPort, Certificate);
    }

    public async ValueTask DisposeAsync()
    {
        await CastAndDispose(Server);
        await CastAndDispose(StreamingServer);
        await CastAndDispose(Certificate);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }

    public void Dispose()
    {
        Server.Dispose();
        StreamingServer.Dispose();
        Certificate.Dispose();
    }
}