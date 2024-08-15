using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace RobotAppLibrary.Tests.Integrations.Api.Connector.Tcp;

public static class CertificateGenerator
{
    public static X509Certificate2 CreateSelfSignedCertificate(string commonName)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            new X500DistinguishedName($"CN={commonName}"),
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));
        return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
    }
}