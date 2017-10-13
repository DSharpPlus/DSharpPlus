using System.Net.Sockets;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Udp
{
    internal class DspUdpClient : BaseUdpClient
    {
        public override int DataAvailable => Client.Available;

        private UdpClient Client { get; set; }
        private ConnectionEndpoint EndPoint { get; set; }

        public override void Setup(ConnectionEndpoint endpoint)
        {
            EndPoint = endpoint;
            Client = new UdpClient();
            // TODO: Solve for .NET Standard, this is possibly default behaviour (???)
#if HAS_NAT_TRAVERSAL
            Client.AllowNatTraversal(true);
#endif
        }

        public override async Task SendAsync(byte[] data, int dataLength)
        {
            await Client.SendAsync(data, dataLength, EndPoint.Hostname, EndPoint.Port);
        }

        public override async Task<byte[]> ReceiveAsync()
        {
            var result = await Client.ReceiveAsync();
            return result.Buffer;
        }

        public override void Close()
        {
            // TODO: Solve later
        }
    }
}
