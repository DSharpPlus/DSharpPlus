using System.Threading.Channels;

using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Net.Gateway;

public interface IGatewayClient
{
    public ChannelReader<GatewayPayload> DispatchEvents { get; }
}
