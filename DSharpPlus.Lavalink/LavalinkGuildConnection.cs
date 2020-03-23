﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using Newtonsoft.Json;

namespace DSharpPlus.Lavalink
{
    internal delegate void ChannelDisconnectedEventHandler(LavalinkGuildConnection node);

    /// <summary>
    /// Represents a Lavalink connection to a channel.
    /// </summary>
    public sealed class LavalinkGuildConnection
    {
        /// <summary>
        /// Triggered whenever Lavalink updates player status.
        /// </summary>
        public event AsyncEventHandler<PlayerUpdateEventArgs> PlayerUpdated
        {
            add { this._playerUpdated.Register(value); }
            remove { this._playerUpdated.Unregister(value); }
        }
        private AsyncEvent<PlayerUpdateEventArgs> _playerUpdated;

        /// <summary>
        /// Triggered whenever playback of a track finishes.
        /// </summary>
        public event AsyncEventHandler<TrackFinishEventArgs> PlaybackFinished
        {
            add { this._playbackFinished.Register(value); }
            remove { this._playbackFinished.Unregister(value); }
        }
        private AsyncEvent<TrackFinishEventArgs> _playbackFinished;

        /// <summary>
        /// Triggered whenever playback of a track gets stuck.
        /// </summary>
        public event AsyncEventHandler<TrackStuckEventArgs> TrackStuck
        {
            add { this._trackStuck.Register(value); }
            remove { this._trackStuck.Unregister(value); }
        }
        private AsyncEvent<TrackStuckEventArgs> _trackStuck;

        /// <summary>
        /// Triggered whenever playback of a track encounters an error.
        /// </summary>
        public event AsyncEventHandler<TrackExceptionEventArgs> TrackException
        {
            add { this._trackException.Register(value); }
            remove { this._trackException.Unregister(value); }
        }
        private AsyncEvent<TrackExceptionEventArgs> _trackException;

        /// <summary>
        /// Triggered whenever Discord Voice WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<WebSocketCloseEventArgs> DiscordWebSocketClosed
        {
            add { this._webSocketClosed.Register(value); }
            remove { this._webSocketClosed.Unregister(value); }
        }
        private AsyncEvent<WebSocketCloseEventArgs> _webSocketClosed;

        /// <summary>
        /// Gets whether this channel is still connected.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref this._isDisposed) && this.Channel != null;
        private bool _isDisposed = false;

        /// <summary>
        /// Gets the current player state.
        /// </summary>
        public LavalinkPlayerState CurrentState { get; }

        /// <summary>
        /// Gets the voice channel associated with this connection.
        /// </summary>
        public DiscordChannel Channel => this.VoiceStateUpdate.Channel;

        /// <summary>
        /// Gets the guild associated with this connection.
        /// </summary>
        public DiscordGuild Guild => this.Channel.Guild;

        private LavalinkNodeConnection Node { get; }
        internal string GuildIdString => this.GuildId.ToString(CultureInfo.InvariantCulture);
        internal ulong GuildId => this.Channel.Guild.Id;
        internal VoiceStateUpdateEventArgs VoiceStateUpdate { get; set; }
        internal bool ManuallyDisconnected { get; set; } = false;

        internal LavalinkGuildConnection(LavalinkNodeConnection node, DiscordChannel channel, VoiceStateUpdateEventArgs vstu)
        {
            this.Node = node;
            this.VoiceStateUpdate = vstu;
            this.CurrentState = new LavalinkPlayerState();

            Volatile.Write(ref this._isDisposed, false);

            this._playerUpdated = new AsyncEvent<PlayerUpdateEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_PLAYER_UPDATE");
            this._playbackFinished = new AsyncEvent<TrackFinishEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_PLAYBACK_FINISHED");
            this._trackStuck = new AsyncEvent<TrackStuckEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_TRACK_STUCK");
            this._trackException = new AsyncEvent<TrackExceptionEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_TRACK_EXCEPTION");
            this._webSocketClosed = new AsyncEvent<WebSocketCloseEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_DISCORD_WEBSOCKET_CLOSED");
        }

        /// <summary>
        /// Disconnect from this voice channel.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            Volatile.Write(ref this._isDisposed, true);

            await this.Node.SendPayloadAsync(new LavalinkDestroy(this)).ConfigureAwait(false);

            if(!this.ManuallyDisconnected)
                await this.SendVoiceUpdateAsync().ConfigureAwait(false);

            this.ChannelDisconnected?.Invoke(this);
        }

        internal async Task SendVoiceUpdateAsync()
        {
            var vsd = new VoiceDispatch
            {
                OpCode = 4,
                Payload = new VoiceStateUpdatePayload
                {
                    GuildId = this.GuildId,
                    ChannelId = null,
                    Deafened = false,
                    Muted = false
                }
            };
            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            await (this.Channel.Discord as DiscordClient)._webSocketClient.SendMessageAsync(vsj).ConfigureAwait(false);
        }

        /// <summary>
        /// Queues the specified track for playback.
        /// </summary>
        /// <param name="track">Track to play.</param>
        public async Task PlayAsync(LavalinkTrack track)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            this.CurrentState.CurrentTrack = track;
            await this.Node.SendPayloadAsync(new LavalinkPlay(this, track)).ConfigureAwait(false);
        }

        /// <summary>
        /// Queues the specified track for playback. The track will be played from specified start timestamp to specified end timestamp.
        /// </summary>
        /// <param name="track">Track to play.</param>
        /// <param name="start">Timestamp to start playback at.</param>
        /// <param name="end">Timestamp to stop playback at.</param>
        public async Task PlayPartialAsync(LavalinkTrack track, TimeSpan start, TimeSpan end)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (start.TotalMilliseconds < 0 || end <= start)
                throw new ArgumentException("Both start and end timestamps need to be greater or equal to zero, and the end timestamp needs to be greater than start timestamp.");

            this.CurrentState.CurrentTrack = track;
            await this.Node.SendPayloadAsync(new LavalinkPlayPartial(this, track, start, end)).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the player completely.
        /// </summary>
        public async Task StopAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkStop(this)).ConfigureAwait(false);
        }

        /// <summary>
        /// Pauses the player.
        /// </summary>
        public async Task PauseAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkPause(this, true)).ConfigureAwait(false);
        }

        /// <summary>
        /// Resumes playback.
        /// </summary>
        public async Task ResumeAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkPause(this, false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Seeks the current track to specified position.
        /// </summary>
        /// <param name="position">Position to seek to.</param>
        public async Task SeekAsync(TimeSpan position)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkSeek(this, position)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the playback volume. This might incur a lot of CPU usage.
        /// </summary>
        /// <param name="volume">Volume to set. Needs to be greater or equal to 0, and less than or equal to 100. 100 means 100% and is the default value.</param>
        public async Task SetVolumeAsync(int volume)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (volume < 0 || volume > 1000)
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume needs to range from 0 to 1000.");

            await this.Node.SendPayloadAsync(new LavalinkVolume(this, volume)).ConfigureAwait(false);
        }

        /// <summary>
        /// Adjusts the specified bands in the audio equalizer. This will alter the sound output, and might incur a lot of CPU usage.
        /// </summary>
        /// <param name="bands">Bands adjustments to make. You must specify one adjustment per band at most.</param>
        public async Task AdjustEqualizerAsync(params LavalinkBandAdjustment[] bands)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (bands?.Any() != true)
                return;

            if (bands.Distinct(new LavalinkBandAdjustmentComparer()).Count() != bands.Count())
                throw new InvalidOperationException("You cannot specify multiple modifiers for the same band.");

            await this.Node.SendPayloadAsync(new LavalinkEqualizer(this, bands)).ConfigureAwait(false);
        }

        /// <summary>
        /// Resets the audio equalizer to default values.
        /// </summary>
        public async Task ResetEqualizerAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkEqualizer(this, Enumerable.Range(0, 15).Select(x => new LavalinkBandAdjustment(x, 0)))).ConfigureAwait(false);
        }

        internal Task InternalUpdatePlayerStateAsync(LavalinkState newState)
        {
            this.CurrentState.LastUpdate = newState.Time;
            this.CurrentState.PlaybackPosition = newState.Position;

            return this._playerUpdated.InvokeAsync(new PlayerUpdateEventArgs(this, newState.Time, newState.Position));
        }

        internal Task InternalPlaybackFinishedAsync(TrackFinishData e)
        {
            if (e.Reason != TrackEndReason.Replaced)
                this.CurrentState.CurrentTrack = default;

            var ea = new TrackFinishEventArgs(this, LavalinkUtilities.DecodeTrack(e.Track), e.Reason);
            return this._playbackFinished.InvokeAsync(ea);
        }

        internal Task InternalTrackStuckAsync(TrackStuckData e)
        {
            var ea = new TrackStuckEventArgs(this, e.Threshold, LavalinkUtilities.DecodeTrack(e.Track));
            return this._trackStuck.InvokeAsync(ea);
        }

        internal Task InternalTrackExceptionAsync(TrackExceptionData e)
        {
            var ea = new TrackExceptionEventArgs(this, e.Error, LavalinkUtilities.DecodeTrack(e.Track));
            return this._trackException.InvokeAsync(ea);
        }

        internal Task InternalWebSocketClosedAsync(WebSocketCloseEventArgs e)
            => this._webSocketClosed.InvokeAsync(e);

        internal event ChannelDisconnectedEventHandler ChannelDisconnected;
    }
}
