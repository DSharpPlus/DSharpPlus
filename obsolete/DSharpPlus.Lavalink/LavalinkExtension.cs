using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;

namespace DSharpPlus.Lavalink;

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
public sealed class LavalinkExtension : BaseExtension
{
    /// <summary>
    /// Triggered whenever a node disconnects.
    /// </summary>
    public event AsyncEventHandler<LavalinkNodeConnection, NodeDisconnectedEventArgs> NodeDisconnected
    {
        add => this.nodeDisconnected.Register(value);
        remove => this.nodeDisconnected.Unregister(value);
    }
    private AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs> nodeDisconnected;

    /// <summary>
    /// Gets a dictionary of connected Lavalink nodes for the extension.
    /// </summary>
    public IReadOnlyDictionary<ConnectionEndpoint, LavalinkNodeConnection> ConnectedNodes => this.connectedNodes;
    private readonly ConcurrentDictionary<ConnectionEndpoint, LavalinkNodeConnection> connectedNodes = new();

    /// <summary>
    /// Creates a new instance of this Lavalink extension.
    /// </summary>
    internal LavalinkExtension() { }

    /// <summary>
    /// DO NOT USE THIS MANUALLY.
    /// </summary>
    /// <param name="client">DO NOT USE THIS MANUALLY.</param>
    /// <exception cref="InvalidOperationException"/>
    protected internal override void Setup(DiscordClient client)
    {
        if (this.Client != null)
        {
            throw new InvalidOperationException("What did I tell you?");
        }

        this.Client = client;

        DefaultClientErrorHandler errorHandler = new(client.Logger);

        this.nodeDisconnected = new AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs>(errorHandler);
    }

    /// <summary>
    /// Connect to a Lavalink node.
    /// </summary>
    /// <param name="config">Lavalink client configuration.</param>
    /// <returns>The established Lavalink connection.</returns>
    public async Task<LavalinkNodeConnection> ConnectAsync(LavalinkConfiguration config)
    {
        if (this.connectedNodes.TryGetValue(config.SocketEndpoint, out LavalinkNodeConnection value))
        {
            return value;
        }

        LavalinkNodeConnection con = new(this.Client, this, config);
        con.NodeDisconnected += Con_NodeDisconnected;
        con.Disconnected += Con_Disconnected;
        this.connectedNodes[con.NodeEndpoint] = con;
        try
        {
            await con.StartAsync();
        }
        catch
        {
            Con_NodeDisconnected(con);
            throw;
        }

        return con;
    }

    /// <summary>
    /// Gets the Lavalink node connection for the specified endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint at which the node resides.</param>
    /// <returns>Lavalink node connection.</returns>
    public LavalinkNodeConnection GetNodeConnection(ConnectionEndpoint endpoint)
        => this.connectedNodes.TryGetValue(endpoint, out LavalinkNodeConnection value) ? value : null;

    /// <summary>
    /// Gets a Lavalink node connection based on load balancing and an optional voice region.
    /// </summary>
    /// <param name="region">The region to compare with the node's <see cref="LavalinkConfiguration.Region"/>, if any.</param>
    /// <returns>The least load affected node connection, or null if no nodes are present.</returns>
    public LavalinkNodeConnection GetIdealNodeConnection(DiscordVoiceRegion region = null)
    {
        if (this.connectedNodes.Count <= 1)
        {
            return this.connectedNodes.Values.FirstOrDefault();
        }

        LavalinkNodeConnection[] nodes = [.. this.connectedNodes.Values];

        if (region != null)
        {
            Func<LavalinkNodeConnection, bool> regionPredicate = new(x => x.Region == region);

            if (nodes.Any(regionPredicate))
            {
                nodes = nodes.Where(regionPredicate).ToArray();
            }

            if (nodes.Length <= 1)
            {
                return nodes.FirstOrDefault();
            }
        }

        return FilterByLoad(nodes);
    }

    /// <summary>
    /// Gets a Lavalink guild connection from a <see cref="DiscordGuild"/>.
    /// </summary>
    /// <param name="guild">The guild the connection is on.</param>
    /// <returns>The found guild connection, or null if one could not be found.</returns>
    public LavalinkGuildConnection GetGuildConnection(DiscordGuild guild)
    {
        ICollection<LavalinkNodeConnection> nodes = this.connectedNodes.Values;
        LavalinkNodeConnection? node = nodes.FirstOrDefault(x => x.connectedGuilds.ContainsKey(guild.Id));
        return node?.GetGuildConnection(guild);
    }

    private LavalinkNodeConnection FilterByLoad(LavalinkNodeConnection[] nodes)
    {
        Array.Sort(nodes, (a, b) =>
        {
            if (!a.Statistics.updated || !b.Statistics.updated)
            {
                return 0;
            }

            //https://github.com/FredBoat/Lavalink-Client/blob/48bc27784f57be5b95d2ff2eff6665451b9366f5/src/main/java/lavalink/client/io/LavalinkLoadBalancer.java#L122
            //https://github.com/briantanner/eris-lavalink/blob/master/src/PlayerManager.js#L329

            //player count
            int aPenaltyCount = a.Statistics.ActivePlayers;
            int bPenaltyCount = b.Statistics.ActivePlayers;

            //cpu load
            aPenaltyCount += (int)Math.Pow(1.05d, (100 * (a.Statistics.CpuSystemLoad / a.Statistics.CpuCoreCount) * 10) - 10);
            bPenaltyCount += (int)Math.Pow(1.05d, (100 * (b.Statistics.CpuSystemLoad / a.Statistics.CpuCoreCount) * 10) - 10);

            //frame load
            if (a.Statistics.AverageDeficitFramesPerMinute > 0)
            {
                //deficit frame load
                aPenaltyCount += (int)((Math.Pow(1.03d, 500f * (a.Statistics.AverageDeficitFramesPerMinute / 3000f)) * 600) - 600);

                //null frame load
                aPenaltyCount += (int)((Math.Pow(1.03d, 500f * (a.Statistics.AverageNulledFramesPerMinute / 3000f)) * 300) - 300);
            }

            //frame load
            if (b.Statistics.AverageDeficitFramesPerMinute > 0)
            {
                //deficit frame load
                bPenaltyCount += (int)((Math.Pow(1.03d, 500f * (b.Statistics.AverageDeficitFramesPerMinute / 3000f)) * 600) - 600);

                //null frame load
                bPenaltyCount += (int)((Math.Pow(1.03d, 500f * (b.Statistics.AverageNulledFramesPerMinute / 3000f)) * 300) - 300);
            }

            return aPenaltyCount - bPenaltyCount;
        });

        return nodes[0];
    }

    private void Con_NodeDisconnected(LavalinkNodeConnection node)
        => this.connectedNodes.TryRemove(node.NodeEndpoint, out _);

    private Task Con_Disconnected(LavalinkNodeConnection node, NodeDisconnectedEventArgs e)
        => this.nodeDisconnected.InvokeAsync(node, e);

    public override void Dispose()
    {
        foreach (KeyValuePair<ConnectionEndpoint, LavalinkNodeConnection> node in this.connectedNodes)
        {
            // undoubtedly there will be some GitHub comments about this. Help.
            node.Value.StopAsync().GetAwaiter().GetResult();
        }

        this.connectedNodes?.Clear();

        // unhook events
        this.nodeDisconnected?.UnregisterAll();

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
