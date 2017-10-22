using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CorePush.Utils;

namespace CorePush.Apple
{
    public class APNSender : IDisposable
    {
        private const int port = 2195;
        private const string hostnameSandbox = "gateway.sandbox.push.apple.com";
        private const string hostnameProduction = "gateway.push.apple.com";

        private readonly GatewayType gatewayType;
        private readonly byte[] certificateBytes;
        private readonly string certificatePassword;
        private readonly bool validateCertificate;

        private TcpClient tcp;
        private X509Certificate2 certificate;
        private SslStream sslStream;
        private BinaryWriter binaryWriter;

        public APNSender(GatewayType gatewayType, string certificatePath, string certificatePassword, bool validateCertificate) : this(gatewayType, File.ReadAllBytes(certificatePath), certificatePassword, validateCertificate)
        {
        }

        public APNSender(GatewayType gatewayType, byte[] certificateBytes, string certificatePassword, bool validateCertificate)
        {
            this.gatewayType = gatewayType;
            this.certificateBytes = certificateBytes;
            this.certificatePassword = certificatePassword;
            this.validateCertificate = validateCertificate;
        }

        public async Task SendAsync(string deviceId, object payload)
        {
            var payloadString = JsonHelper.Serialize(payload);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadString);

            await ConnectAsync();

            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)32);
            binaryWriter.Write(HexStringToByteArray(deviceId.ToUpper()));
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)payloadString.Length);
            binaryWriter.Write(payloadBytes);
            binaryWriter.Flush();
            sslStream.Flush();
        }

        private async Task ConnectAsync()
        {
            if (binaryWriter == null)
            {
                var hostname = GetHostname();
                tcp = new TcpClient(AddressFamily.InterNetwork);
                await tcp.ConnectAsync(hostname, port);
                var removeCertificateValidation = validateCertificate ? new RemoteCertificateValidationCallback(ValidateServerCertificate) : null;
                sslStream = new SslStream(tcp.GetStream(), false, removeCertificateValidation, null);
                certificate = new X509Certificate2(certificateBytes, certificatePassword);
                var certificatesCollection = new X509Certificate2Collection(certificate);
                await sslStream.AuthenticateAsClientAsync(hostname, certificatesCollection, SslProtocols.Tls, false);
                binaryWriter = new BinaryWriter(sslStream);
            }
        }

        private string GetHostname()
        {
            return gatewayType == GatewayType.Production ? hostnameProduction : hostnameSandbox;
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            throw new APNCertificateException(sslPolicyErrors);
        }

        public void Dispose()
        {
            tcp?.Dispose();
            sslStream?.Dispose();
            certificate?.Dispose();
            binaryWriter?.Dispose();
        }
    }
}
