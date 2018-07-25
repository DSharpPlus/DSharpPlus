using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net.Udp;

namespace DSharpPlus.Lavalink
{
    public sealed class LavalinkExtension : BaseExtension
    {
        /// <summary>
        /// Triggered whenever a node disconnects.
        /// </summary>
        public event AsyncEventHandler<NodeDisconnectedEventArgs> NodeDisconnected
        {
            add { this._nodeDisconnected.Register(value); }
            remove { this._nodeDisconnected.Unregister(value); }
        }
        private AsyncEvent<NodeDisconnectedEventArgs> _nodeDisconnected;

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

            this._nodeDisconnected = new AsyncEvent<NodeDisconnectedEventArgs>(this.Client.EventErrorHandler, "LAVALINK_NODE_DISCONNECTED");
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
            con.Disconnected += this.Con_Disconnected;
            this.ConnectedNodes[con.NodeEndpoint] = con;
            try
            {
                await con.StartAsync().ConfigureAwait(false);
            }
            catch
            {
                this.Con_NodeDisconnected(con);
                throw;
            }

            return con;
        }

        /// <summary>
        /// Gets the lavalink node connection for specified endpoint.
        /// </summary>
        /// <param name="endpoint">Endpoint at which the node resides.</param>
        /// <returns>Lavalink node connection.</returns>
        public LavalinkNodeConnection GetNodeConnection(ConnectionEndpoint endpoint)
            => this.ConnectedNodes.ContainsKey(endpoint) ? this.ConnectedNodes[endpoint] : null;

        private void Con_NodeDisconnected(LavalinkNodeConnection node)
            => this.ConnectedNodes.TryRemove(node.NodeEndpoint, out _);

        private Task Con_Disconnected(NodeDisconnectedEventArgs e)
            => this._nodeDisconnected.InvokeAsync(e);
    }
}
