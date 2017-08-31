using System;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Udp
{
    internal class DspUdpClient : BaseUdpClient
    {
        public override int DataAvailable => throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");

        public DspUdpClient()
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        public override void Setup(ConnectionEndpoint endpoint)
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        public override Task SendAsync(byte[] data, int data_length)
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        public override Task<byte[]> ReceiveAsync()
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        public override void Close()
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }
    }
}
