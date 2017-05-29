using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DSharpPlus
{
    internal class DspUdpClient : BaseUdpClient
    {
        private UdpClient Client { get; set; }
        private ConnectionEndpoint EndPoint { get; set; }

        public DspUdpClient() { }

        public override void Setup(ConnectionEndpoint endpoint)
        {
            this.EndPoint = endpoint;
            this.Client = new UdpClient();
            // TODO: Solve later, this is possibly default behaviour (???)
            //this.Client.AllowNatTraversal(true);
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
