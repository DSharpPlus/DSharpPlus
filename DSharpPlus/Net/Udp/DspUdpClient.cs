using System.Net.Sockets;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Udp
{
    internal class DspUdpClient : BaseUdpClient
    {
        public override int DataAvailable => this.Client.Available;

        private UdpClient Client { get; set; }
        private ConnectionEndpoint EndPoint { get; set; }

        public DspUdpClient() { }

        public override void Setup(ConnectionEndpoint endpoint)
        {
            this.EndPoint = endpoint;
            this.Client = new UdpClient();
            // TODO: Solve for .NET Standard, this is possibly default behaviour (???)
#if HAS_NAT_TRAVERSAL
            this.Client.AllowNatTraversal(true);
#endif
        }

        public override async Task SendAsync(byte[] data, int data_length)
        {
            await this.Client.SendAsync(data, data_length, this.EndPoint.Hostname, this.EndPoint.Port);
        }

        public override async Task<byte[]> ReceiveAsync()
        {
            var result = await this.Client.ReceiveAsync();
            return result.Buffer;
        }

        public override void Close()
        {
            // TODO: Solve later
        }
    }
}
