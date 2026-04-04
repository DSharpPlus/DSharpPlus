using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Receivers;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides an extension method for connecting to a discord channel.
/// </summary>
public static class ConnectionExtensions
{
    extension(DiscordChannel channel)
    {
        /// <summary>
        /// Connects to a voice channel.
        /// </summary>
        /// <param name="audioType">The type of audio to optimize for.</param>
        /// <param name="receiverType">The type of audio receiver to use.</param>
        /// <returns>The initialized and active voice connection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this channel could not be connected to.</exception>
        public async Task<VoiceConnection> ConnectAsync(AudioType audioType = AudioType.Auto, Type? receiverType = null)
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
                    + "Please call DiscordChannel.ConnectAsync(IServiceScope, ulong, AudioType, Type?) directly.");
            }

            IServiceScope scope = client.ServiceProvider.CreateAsyncScope();
            ulong userId = client.CurrentUser.Id;

            return await channel.ConnectAsync(scope, userId, audioType, receiverType);
        }

        /// <summary>
        /// Connects to a voice channel.
        /// </summary>
        /// <param name="scope">A service scope to use for this connection.</param>
        /// <param name="currentUserId">The snowflake identifier of the current user.</param>
        /// <param name="audioType">The type of audio to optimize for.</param>
        /// <param name="receiverType">The type of audio receiver to use.</param>
        /// <returns>The initialized and active voice connection.</returns>
        public async Task<VoiceConnection> ConnectAsync
        (
            IServiceScope scope,
            ulong currentUserId,
            AudioType audioType = AudioType.Auto,
            Type? receiverType = null
        )
        {
            receiverType ??= typeof(NullAudioReceiver);

            VoiceConnection connection = new
            (
                scope,
                currentUserId,
                channel.Id,
                channel.GuildId.Value,
                channel.Bitrate.Value,
                audioType,
                channel.Guild.VoiceStates.Where(x => x.Value.ChannelId == channel.Id).Select(x => x.Value.UserId),
                receiverType
            );

            await connection.ConnectAsync();

            return connection;
        }
    }
}