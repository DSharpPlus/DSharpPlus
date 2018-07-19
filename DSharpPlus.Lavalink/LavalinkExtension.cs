using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DSharpPlus.Net.Udp;

namespace DSharpPlus.Lavalink
{
    public sealed class LavalinkExtension : BaseExtension
    {

        private ConcurrentDictionary<ConnectionEndpoint, LavalinkNodeConnection> ConnectedNodes { get; }

        /// <summary>
        /// Creates a new instance of this Lavalink extension.
        /// </summary>
        internal LavalinkExtension()
        {
            this.ConnectedNodes = new ConcurrentDictionary<ConnectionEndpoint, LavalinkNodeConnection>();
        }

        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="InvalidOperationException"/>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;
        }

        /// <summary>
        /// Connect to a Lavalink node.
        /// </summary>
        /// <param name="config">Lavalink client configuration.</param>
        /// <returns>The established Lavalink connection.</returns>
        public async Task<LavalinkNodeConnection> ConnectAsync(LavalinkConfiguration config)
        {
            if (this.ConnectedNodes.ContainsKey(config.SocketEndpoint))
                return this.ConnectedNodes[config.SocketEndpoint];

            var con = new LavalinkNodeConnection(this.Client, config);
            con.NodeDisconnected += this.Con_NodeDisconnected;
            this.ConnectedNodes[con.NodeEndpoint] = con;
            await con.StartAsync().ConfigureAwait(false);

            return con;
        }

        /// <summary>
        /// Gets the lavalink node connection for specified endpoint.
        /// </summary>
        /// <param name="endpoint">Endpoint at which the node resides.</param>
        /// <returns>Lavalink node connection.</returns>
        public LavalinkNodeConnection GetNodeConnection(ConnectionEndpoint endpoint)
            => this.ConnectedNodes[endpoint];

        private void Con_NodeDisconnected(LavalinkNodeConnection node)
            => this.ConnectedNodes.TryRemove(node.NodeEndpoint, out _);
    }
}
