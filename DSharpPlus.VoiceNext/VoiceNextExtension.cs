using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Core.VoiceGatewayEntities;
using DSharpPlus.Core.VoiceGatewayEntities.Payloads;
using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;

namespace DSharpPlus.VoiceNext
{
    public sealed class VoiceNextExtension : BaseExtension
    {
        public VoiceNextConfiguration Configuration { get; }
        public Dictionary<ulong, VoiceNextConnection> Connections => new(this._connections);
        internal ConcurrentDictionary<ulong, VoiceNextConnection> _connections { get; } = new();

        internal ConcurrentDictionary<ulong, TaskCompletionSource<DiscordVoiceStateUpdate>> _voiceStateUpdates = new();
        internal ConcurrentDictionary<ulong, TaskCompletionSource<DiscordVoiceServerUpdatePayload>> _voiceServerUpdates = new();

        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("Extension has already been setup.");
            this.Client = client;
        }

        public async Task<VoiceNextConnection> ConnectAsync(DiscordChannel voiceChannel, bool selfMuted = false, bool selfDeafened = true)
        {
            var botPermissions = voiceChannel.PermissionsFor(voiceChannel.Guild.CurrentMember);
            if (voiceChannel.Type != ChannelType.Voice && voiceChannel.Type != ChannelType.Stage)
            {
                throw new ArgumentException($"Cannot connect to a {voiceChannel.Type} channel. The channel type must be {ChannelType.Voice} or {ChannelType.Stage}.", nameof(voiceChannel));
            }
            else if (voiceChannel.Guild == null)
            {
                throw new ArgumentException("Bots can only connect to guild channels.", nameof(voiceChannel));
            }
            else if (this._connections.ContainsKey(voiceChannel.Guild.Id))
            {
                throw new InvalidOperationException("Bots may only join 1 voice or stage channel per guild.");
            }
            else if (!botPermissions.HasPermission(Permissions.AccessChannels | Permissions.UseVoice))
            {
                throw new InvalidOperationException($"The bot must have the {Permissions.AccessChannels} and {Permissions.UseVoice} permissions to connect to a channel.");
            }
            else if (!botPermissions.HasPermission(Permissions.Speak) && !selfMuted)
            {
                throw new InvalidOperationException($"The bot must have the {Permissions.Speak} permission to speak in a voice channel.");
            }
            else if (voiceChannel.UserLimit > voiceChannel.Users.Count && !botPermissions.HasPermission(Permissions.ManageChannels))
            {
                throw new InvalidOperationException($"The voice channel is full and the bot must have the {Permissions.ManageChannels} permission to connect to override the channel user limit.");
            }

            // From the Discord Docs (https://discord.com/developers/docs/topics/voice-connections#connecting-to-voice):
            // > If our request succeeded, the gateway will respond with two events—a Voice State Update event and a Voice Server Update event—meaning your
            // > library must properly wait for both events before continuing. The first will contain a new key, session_id, and the second will provide
            // > voice server information we can use to establish a new voice connection:
            // As such, we will pre-emptively create an event handler for both events, and wait for both to be received before continuing.
            var voiceStateUpdateEvent = new TaskCompletionSource<DiscordVoiceStateUpdate>();
            var voiceServerUpdateEvent = new TaskCompletionSource<DiscordVoiceServerUpdatePayload>();
            this._voiceStateUpdates[voiceChannel.Guild.Id] = voiceStateUpdateEvent;
            this._voiceServerUpdates[voiceChannel.Guild.Id] = voiceServerUpdateEvent;

            // Let the server know we're connecting to a voice channel.
            var voiceDispatch = new GatewayPayload
            {
                OpCode = GatewayOpCode.VoiceStateUpdate,
                Data = new DiscordVoiceStateUpdate
                {
                    GuildId = voiceChannel.Guild.Id,
                    ChannelId = voiceChannel.Id,
                    SelfMute = selfMuted,
                    SelfDeaf = selfDeafened
                }
            };
            await this.Client.WsSendAsync(DiscordJson.SerializeObject(voiceDispatch)).ConfigureAwait(false);

            // Wait for the Voice State Update and Voice Server Update events to be received.
            var voiceStateUpdateTask = await voiceStateUpdateEvent.Task.ConfigureAwait(false); // Task<DiscordVoiceStateUpdate>
            var voiceServerUpdateTask = await voiceServerUpdateEvent.Task.ConfigureAwait(false); // Task<DiscordVoiceServerUpdatePayload>

            var voiceNextConnection = new VoiceNextConnection(this.Client, voiceChannel.Guild, voiceChannel, this.Configuration, voiceStateUpdateTask, voiceServerUpdateTask);
            await voiceNextConnection.ConnectAsync().ConfigureAwait(false);
            return voiceNextConnection;
        }
    }
}
