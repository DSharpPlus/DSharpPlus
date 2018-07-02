using DSharpPlus.Entities;
using DSharpPlus.Net.Udp;
using System.Threading.Tasks;

namespace DSharpPlus.Lavalink
{
    public sealed class LavalinkConnection
    {
        /// <summary>
        /// Gets the endpoint to which this client is connected.
        /// </summary>
        public ConnectionEndpoint Endpoint { get; }

        private DiscordClient Client { get; }
        private string Password { get; }

        internal LavalinkConnection(DiscordClient client, ConnectionEndpoint ep, string password)
        {
            this.Client = client;
            this.Endpoint = ep;
            this.Password = password;
        }

        /// <summary>
        /// Connects this Lavalink node to specified Discord channel.
        /// </summary>
        /// <param name="channel">Voice channel to connect to.</param>
        /// <returns></returns>
        public async Task ConnectAsync(DiscordChannel channel)
        {

        }
    }
}
