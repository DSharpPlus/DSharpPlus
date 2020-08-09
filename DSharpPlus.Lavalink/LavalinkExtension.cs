using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;

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
        /// Gets the Lavalink node connection for specified endpoint.
        /// </summary>
        /// <param name="endpoint">Endpoint at which the node resides.</param>
        /// <returns>Lavalink node connection.</returns>
        public LavalinkNodeConnection GetNodeConnection(ConnectionEndpoint endpoint)
            => this.ConnectedNodes.ContainsKey(endpoint) ? this.ConnectedNodes[endpoint] : null;

        /// <summary>
        /// Gets a Lavalink node connection based on load balancing and an optional voice region.
        /// </summary>
        /// <param name="region">The region to filter by, if any.</param>
        /// <returns></returns>
        public LavalinkNodeConnection GetNodeConnection(DiscordVoiceRegion region = null)
        {
            if (this.ConnectedNodes.Count <= 1)
                return this.ConnectedNodes.Values.FirstOrDefault();

            var nodes = this.ConnectedNodes.Values.ToArray();

            var regionPredicate = new Func<LavalinkNodeConnection, bool>(x => x.Configuration.Region == region);
            
            if(region != null && nodes.Any(regionPredicate))
                nodes = nodes.Where(regionPredicate).ToArray();

            if (this.ConnectedNodes.Count <= 1)
                return this.ConnectedNodes.Values.FirstOrDefault();

            return this.FilterByLoad(nodes);
        }

        /// <summary>
        /// Gets a Lavalink guild connection from a <see cref="DiscordGuild"/>.
        /// </summary>
        /// <param name="guild">The guild the connection is on.</param>
        /// <returns></returns>
        public LavalinkGuildConnection GetGuildConnection(DiscordGuild guild)
        {
            foreach (var node in this.ConnectedNodes.Values)
            {
                if (node.ConnectedGuilds.TryGetValue(guild.Id, out var gc))
                    return gc;
            }

            return null;
        }

        private LavalinkNodeConnection FilterByLoad(LavalinkNodeConnection[] nodes)
        {
            Array.Sort(nodes, (a, b) => 
            {
                if (!a.Statistics.Updated || !b.Statistics.Updated)
                    return 0;

                //https://github.com/FredBoat/Lavalink-Client/blob/48bc27784f57be5b95d2ff2eff6665451b9366f5/src/main/java/lavalink/client/io/LavalinkLoadBalancer.java#L122
                //https://github.com/briantanner/eris-lavalink/blob/master/src/PlayerManager.js#L329

                //player count
                var aPenaltyCount = a.Statistics.ActivePlayers;
                var bPenaltyCount = b.Statistics.ActivePlayers;

                //cpu load
                aPenaltyCount += (int)Math.Pow(1.05d, 100 * (a.Statistics.CpuSystemLoad / a.Statistics.CpuCoreCount) * 10 - 10);
                bPenaltyCount += (int)Math.Pow(1.05d, 100 * (b.Statistics.CpuSystemLoad / a.Statistics.CpuCoreCount) * 10 - 10);

                //frame load
                if(a.Statistics.AverageDeficitFramesPerMinute > 0)
                {
                    //deficit frame load
                    aPenaltyCount += (int)(Math.Pow(1.03d, 500f * (a.Statistics.AverageDeficitFramesPerMinute / 3000f)) * 600 - 600);

                    //null frame load
                    aPenaltyCount += (int)(Math.Pow(1.03d, 500f * (a.Statistics.AverageNulledFramesPerMinute / 3000f)) * 300 - 300);
                }

                //frame load
                if (b.Statistics.AverageDeficitFramesPerMinute > 0)
                {
                    //deficit frame load
                    bPenaltyCount += (int)(Math.Pow(1.03d, 500f * (b.Statistics.AverageDeficitFramesPerMinute / 3000f)) * 600 - 600);

                    //null frame load
                    bPenaltyCount += (int)(Math.Pow(1.03d, 500f * (b.Statistics.AverageNulledFramesPerMinute / 3000f)) * 300 - 300);
                }

                return aPenaltyCount - bPenaltyCount;
            });

            return nodes[0];
        }

        private void Con_NodeDisconnected(LavalinkNodeConnection node)
            => this.ConnectedNodes.TryRemove(node.NodeEndpoint, out _);

        private Task Con_Disconnected(NodeDisconnectedEventArgs e)
            => this._nodeDisconnected.InvokeAsync(e);
    }
}
