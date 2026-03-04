using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Voice.Codec;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides an extension method for connecting to a discord channel.
/// </summary>
public static class ConnectionExtensions
{
    extension(DiscordChannel channel)
    {
        public async Task<VoiceConnection> ConnectAsync(AudioType type = AudioType.Auto, bool isStreamingConnection = true)
        {
            if (channel.IsPrivate)
            {
                throw new InvalidOperationException("Cannot connect to a call in a direct message.");
            }

            if (channel.Type is not DiscordChannelType.Voice and not DiscordChannelType.Stage)
            {
                throw new InvalidOperationException($"Cannot connect to non-voice channel of type {channel.Type}.");
            }

            if (channel.Discord is not DiscordClient client)
            {
                throw new InvalidOperationException("This overload cannot connect to a channel without backing DiscordClient. " 
                    + "Please call DiscordChannel.ConnectAsync(IServiceScope, ulong, AudioType, bool) directly.");
            }

            IServiceScope scope = client.ServiceProvider.CreateAsyncScope();
            ulong userId = client.CurrentUser.Id;

            return await channel.ConnectAsync(scope, userId, type, isStreamingConnection);
        }

        public async Task<VoiceConnection> ConnectAsync
        (
            IServiceScope scope,
            ulong currentUserId,
            AudioType type = AudioType.Auto,
            bool isStreamingConnection = true
        )
        {
            VoiceConnection connection = new(scope, currentUserId, channel.Id, channel.GuildId.Value, channel.Bitrate.Value, type, isStreamingConnection);

            await connection.ConnectAsync();

            return connection;
        }
    }
}