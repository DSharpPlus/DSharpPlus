using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.VoiceNext;

public static class DiscordClientExtensions
{
    /// <summary>
    /// Creates a new VoiceNext client with default settings.
    /// </summary>
    /// <param name="client">Discord client to create VoiceNext instance for.</param>
    /// <returns>VoiceNext client instance.</returns>
    public static VoiceNextExtension UseVoiceNext(this DiscordClient client)
        => UseVoiceNext(client, new VoiceNextConfiguration());

    /// <summary>
    /// Creates a new VoiceNext client with specified settings.
    /// </summary>
    /// <param name="client">Discord client to create VoiceNext instance for.</param>
    /// <param name="config">Configuration for the VoiceNext client.</param>
    /// <returns>VoiceNext client instance.</returns>
    public static VoiceNextExtension UseVoiceNext(this DiscordClient client, VoiceNextConfiguration config)
    {
        if (client.GetExtension<VoiceNextExtension>() != null)
        {
            throw new InvalidOperationException("VoiceNext is already enabled for that client.");
        }

        VoiceNextExtension vnext = new(config);
        client.AddExtension(vnext);
        return vnext;
    }

    /// <summary>
    /// Gets the active instance of VoiceNext client for the DiscordClient.
    /// </summary>
    /// <param name="client">Discord client to get VoiceNext instance for.</param>
    /// <returns>VoiceNext client instance.</returns>
    public static VoiceNextExtension GetVoiceNext(this DiscordClient client)
        => client.GetExtension<VoiceNextExtension>();

    /// <summary>
    /// Connects to this voice channel using VoiceNext.
    /// </summary>
    /// <param name="channel">Channel to connect to.</param>
    /// <returns>If successful, the VoiceNext connection.</returns>
    public static Task<VoiceNextConnection> ConnectAsync(this DiscordChannel channel)
    {
        if (channel == null)
        {
            throw new NullReferenceException();
        }

        if (channel.Guild == null)
        {
            throw new InvalidOperationException("VoiceNext can only be used with guild channels.");
        }

        if (channel.Type is not DiscordChannelType.Voice and not DiscordChannelType.Stage)
        {
            throw new InvalidOperationException("You can only connect to voice or stage channels.");
        }

        if (channel.Discord is not DiscordClient discord || discord == null)
        {
            throw new NullReferenceException();
        }

        VoiceNextExtension vnext = discord.GetVoiceNext() ?? throw new InvalidOperationException("VoiceNext is not initialized for this Discord client.");
        VoiceNextConnection? vnc = vnext.GetConnection(channel.Guild);
        return vnc != null
            ? throw new InvalidOperationException("VoiceNext is already connected in this guild.")
            : vnext.ConnectAsync(channel);
    }
}
