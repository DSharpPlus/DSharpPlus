using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.VoiceNext.Entities;

namespace DSharpPlus.VoiceNext;

/// <summary>
/// Represents VoiceNext extension, which acts as Discord voice client.
/// </summary>
public sealed class VoiceNextExtension : BaseExtension
{
    private VoiceNextConfiguration Configuration { get; set; }

    private ConcurrentDictionary<ulong, VoiceNextConnection> ActiveConnections { get; set; }
    private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdatedEventArgs>> VoiceStateUpdates { get; set; }
    private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdatedEventArgs>> VoiceServerUpdates { get; set; }

    /// <summary>
    /// Gets whether this connection has incoming voice enabled.
    /// </summary>
    public bool IsIncomingEnabled { get; }

    internal VoiceNextExtension(VoiceNextConfiguration config)
    {
        this.Configuration = new VoiceNextConfiguration(config);
        this.IsIncomingEnabled = config.EnableIncoming;

        this.ActiveConnections = new ConcurrentDictionary<ulong, VoiceNextConnection>();
        this.VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdatedEventArgs>>();
        this.VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdatedEventArgs>>();
    }

    /// <summary>
    /// DO NOT USE THIS MANUALLY.
    /// </summary>
    /// <param name="client">DO NOT USE THIS MANUALLY.</param>
    /// <exception cref="InvalidOperationException"/>
    public override void Setup(DiscordClient client)
    {
        if (this.Client != null)
        {
            throw new InvalidOperationException("What did I tell you?");
        }

        this.Client = client;
    }

    /// <summary>
    /// Create a VoiceNext connection for the specified channel.
    /// </summary>
    /// <param name="channel">Channel to connect to.</param>
    /// <returns>VoiceNext connection for this channel.</returns>
    public async Task<VoiceNextConnection> ConnectAsync(DiscordChannel channel)
    {
        if (channel.Type is not DiscordChannelType.Voice and not DiscordChannelType.Stage)
        {
            throw new ArgumentException("Invalid channel specified; needs to be voice or stage channel", nameof(channel));
        }

        if (channel.Guild is null)
        {
            throw new ArgumentException("Invalid channel specified; needs to be guild channel", nameof(channel));
        }

        if (!channel.PermissionsFor(channel.Guild.CurrentMember).HasPermission(DiscordPermissions.AccessChannels | DiscordPermissions.UseVoice))
        {
            throw new InvalidOperationException("You need AccessChannels and UseVoice permission to connect to this voice channel");
        }

        DiscordGuild gld = channel.Guild;
        if (this.ActiveConnections.ContainsKey(gld.Id))
        {
            throw new InvalidOperationException("This guild already has a voice connection");
        }

        TaskCompletionSource<VoiceStateUpdatedEventArgs> vstut = new();
        TaskCompletionSource<VoiceServerUpdatedEventArgs> vsrut = new();
        this.VoiceStateUpdates[gld.Id] = vstut;
        this.VoiceServerUpdates[gld.Id] = vsrut;

        VoiceStateUpdatePayload payload = new()
        {
            GuildId = gld.Id,
            ChannelId = channel.Id,
            Deafened = false,
            Muted = false
        };

#pragma warning disable DSP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        await (channel.Discord as DiscordClient).SendPayloadAsync(GatewayOpCode.VoiceStateUpdate, payload, gld.Id);
#pragma warning restore DSP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        VoiceStateUpdatedEventArgs vstu = await vstut.Task;
        VoiceStateUpdatePayload vstup = new()
        {
            SessionId = vstu.SessionId,
            UserId = vstu.User.Id
        };
        VoiceServerUpdatedEventArgs vsru = await vsrut.Task;
        VoiceServerUpdatePayload vsrup = new()
        {
            Endpoint = vsru.Endpoint,
            GuildId = vsru.Guild.Id,
            Token = vsru.VoiceToken
        };

        VoiceNextConnection vnc = new(this.Client, gld, channel, this.Configuration, vsrup, vstup);
        vnc.VoiceDisconnected += Vnc_VoiceDisconnectedAsync;
        await vnc.ConnectAsync();
        await vnc.WaitForReadyAsync();
        this.ActiveConnections[gld.Id] = vnc;
        return vnc;
    }

    /// <summary>
    /// Gets a VoiceNext connection for specified guild.
    /// </summary>
    /// <param name="guild">Guild to get VoiceNext connection for.</param>
    /// <returns>VoiceNext connection for the specified guild.</returns>
    public VoiceNextConnection? GetConnection(DiscordGuild guild)
        => this.ActiveConnections.TryGetValue(guild.Id, out VoiceNextConnection value) ? value : null;

    private async Task Vnc_VoiceDisconnectedAsync(DiscordGuild guild)
    {
        if (this.ActiveConnections.ContainsKey(guild.Id))
        {
            this.ActiveConnections.TryRemove(guild.Id, out _);
        }

        VoiceStateUpdatePayload payload = new()
        {
            GuildId = guild.Id,
            ChannelId = null
        };

#pragma warning disable DSP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        await (guild.Discord as DiscordClient).SendPayloadAsync(GatewayOpCode.VoiceStateUpdate, payload, guild.Id);
#pragma warning restore DSP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    internal Task Client_VoiceStateUpdate(DiscordClient client, VoiceStateUpdatedEventArgs e)
    {
        DiscordGuild gld = e.Guild;
        if (gld == null)
        {
            return Task.CompletedTask;
        }

        if (e.User == null)
        {
            return Task.CompletedTask;
        }

        if (e.User.Id == this.Client.CurrentUser.Id)
        {
            if (e.After.Channel == null && this.ActiveConnections.TryRemove(gld.Id, out VoiceNextConnection? ac))
            {
                ac.Disconnect();
            }

            if (this.ActiveConnections.TryGetValue(e.Guild.Id, out VoiceNextConnection? vnc))
            {
                vnc.TargetChannel = e.Channel;
            }

            if (!string.IsNullOrWhiteSpace(e.SessionId) && e.Channel != null && this.VoiceStateUpdates.TryRemove(gld.Id, out TaskCompletionSource<VoiceStateUpdatedEventArgs>? xe))
            {
                xe.SetResult(e);
            }
        }

        return Task.CompletedTask;
    }

    internal async Task Client_VoiceServerUpdateAsync(DiscordClient client, VoiceServerUpdatedEventArgs e)
    {
        DiscordGuild gld = e.Guild;
        if (gld == null)
        {
            return;
        }

        if (this.ActiveConnections.TryGetValue(e.Guild.Id, out VoiceNextConnection? vnc))
        {
            vnc.ServerData = new VoiceServerUpdatePayload
            {
                Endpoint = e.Endpoint,
                GuildId = e.Guild.Id,
                Token = e.VoiceToken
            };

            string eps = e.Endpoint;
            int epi = eps.LastIndexOf(':');
            string eph;
            int epp = 443;
            if (epi != -1)
            {
                eph = eps[..epi];
                epp = int.Parse(eps[(epi + 1)..]);
            }
            else
            {
                eph = eps;
            }
            vnc.WebSocketEndpoint = new ConnectionEndpoint { Hostname = eph, Port = epp };

            vnc.Resume = false;
            await vnc.ReconnectAsync();
        }

        if (this.VoiceServerUpdates.ContainsKey(gld.Id))
        {
            this.VoiceServerUpdates.TryRemove(gld.Id, out TaskCompletionSource<VoiceServerUpdatedEventArgs>? xe);
            xe.SetResult(e);
        }
    }

    public override void Dispose()
    {
        foreach (System.Collections.Generic.KeyValuePair<ulong, VoiceNextConnection> conn in this.ActiveConnections)
        {
            conn.Value?.Dispose();
        }

        // Lo and behold, the audacious man who dared lay his hand upon VoiceNext hath once more trespassed upon its profane ground!

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
