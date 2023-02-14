// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using Emzi0767.Utilities;
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
        public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
        {
            add { this._playerUpdated.Register(value); }
            remove { this._playerUpdated.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> _playerUpdated;

        /// <summary>
        /// Triggered whenever playback of a track starts.
        /// <para>This is only available for version 3.3.1 and greater.</para>
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
        {
            add { this._playbackStarted.Register(value); }
            remove { this._playbackStarted.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> _playbackStarted;

        /// <summary>
        /// Triggered whenever playback of a track finishes.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
        {
            add { this._playbackFinished.Register(value); }
            remove { this._playbackFinished.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> _playbackFinished;

        /// <summary>
        /// Triggered whenever playback of a track gets stuck.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
        {
            add { this._trackStuck.Register(value); }
            remove { this._trackStuck.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> _trackStuck;

        /// <summary>
        /// Triggered whenever playback of a track encounters an error.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
        {
            add { this._trackException.Register(value); }
            remove { this._trackException.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> _trackException;

        /// <summary>
        /// Triggered whenever Discord Voice WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, WebSocketCloseEventArgs> DiscordWebSocketClosed
        {
            add { this._webSocketClosed.Register(value); }
            remove { this._webSocketClosed.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, WebSocketCloseEventArgs> _webSocketClosed;

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

        /// <summary>
        /// Gets the Lavalink node associated with this connection.
        /// </summary>
        public LavalinkNodeConnection Node { get; }

        internal string GuildIdString => this.GuildId.ToString(CultureInfo.InvariantCulture);
        internal ulong GuildId => this.Channel.Guild.Id;
        internal VoiceStateUpdateEventArgs VoiceStateUpdate { get; set; }
        internal TaskCompletionSource<bool> VoiceWsDisconnectTcs { get; set; }

        internal LavalinkGuildConnection(LavalinkNodeConnection node, DiscordChannel channel, VoiceStateUpdateEventArgs vstu)
        {
            this.Node = node;
            this.VoiceStateUpdate = vstu;
            this.CurrentState = new LavalinkPlayerState();
            this.VoiceWsDisconnectTcs = new TaskCompletionSource<bool>();

            Volatile.Write(ref this._isDisposed, false);

            this._playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("LAVALINK_PLAYER_UPDATE", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>("LAVALINK_PLAYBACK_STARTED", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>("LAVALINK_PLAYBACK_FINISHED", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>("LAVALINK_TRACK_STUCK", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_TRACK_EXCEPTION", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._webSocketClosed = new AsyncEvent<LavalinkGuildConnection, WebSocketCloseEventArgs>("LAVALINK_DISCORD_WEBSOCKET_CLOSED", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
        }

        /// <summary>
        /// Disconnects the connection from the voice channel.
        /// </summary>
        /// <param name="shouldDestroy">Whether the connection should be destroyed on the Lavalink server when leaving.</param>

        public Task DisconnectAsync(bool shouldDestroy = true)
            => this.DisconnectInternalAsync(shouldDestroy);

        internal async Task DisconnectInternalAsync(bool shouldDestroy, bool isManualDisconnection = false)
        {
            if (!this.IsConnected && !isManualDisconnection)
                throw new InvalidOperationException("This connection is not valid.");

            Volatile.Write(ref this._isDisposed, true);

            if (shouldDestroy)
                await this.Node.SendPayloadAsync(new LavalinkDestroy(this)).ConfigureAwait(false);

            if (!isManualDisconnection)
            {
                await this.SendVoiceUpdateAsync().ConfigureAwait(false);
                this.ChannelDisconnected?.Invoke(this);
            }
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
            await (this.Channel.Discord as DiscordClient).WsSendAsync(vsj).ConfigureAwait(false);
        }

        /// <summary>
        /// Searches for specified terms.
        /// </summary>
        /// <param name="searchQuery">What to search for.</param>
        /// <param name="type">What platform will search for.</param>
        /// <returns>A collection of tracks matching the criteria.</returns>
        public Task<LavalinkLoadResult> GetTracksAsync(string searchQuery, LavalinkSearchType type = LavalinkSearchType.Youtube)
            => this.Node.Rest.GetTracksAsync(searchQuery, type);

        /// <summary>
        /// Loads tracks from specified URL.
        /// </summary>
        /// <param name="uri">URL to load tracks from.</param>
        /// <returns>A collection of tracks from the URL.</returns>
        public Task<LavalinkLoadResult> GetTracksAsync(Uri uri)
            => this.Node.Rest.GetTracksAsync(uri);

        /// <summary>
        /// Loads tracks from a local file.
        /// </summary>
        /// <param name="file">File to load tracks from.</param>
        /// <returns>A collection of tracks from the file.</returns>
        public Task<LavalinkLoadResult> GetTracksAsync(FileInfo file)
            => this.Node.Rest.GetTracksAsync(file);

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

            return this._playerUpdated.InvokeAsync(this, new PlayerUpdateEventArgs(this, newState.Time, newState.Position));
        }

        internal Task InternalPlaybackStartedAsync(string track)
        {
            var ea = new TrackStartEventArgs(this, LavalinkUtilities.DecodeTrack(track));
            return this._playbackStarted.InvokeAsync(this, ea);
        }

        internal Task InternalPlaybackFinishedAsync(TrackFinishData e)
        {
            if (e.Reason != TrackEndReason.Replaced)
                this.CurrentState.CurrentTrack = default;

            var ea = new TrackFinishEventArgs(this, LavalinkUtilities.DecodeTrack(e.Track), e.Reason);
            return this._playbackFinished.InvokeAsync(this, ea);
        }

        internal Task InternalTrackStuckAsync(TrackStuckData e)
        {
            var ea = new TrackStuckEventArgs(this, e.Threshold, LavalinkUtilities.DecodeTrack(e.Track));
            return this._trackStuck.InvokeAsync(this, ea);
        }

        internal Task InternalTrackExceptionAsync(TrackExceptionData e)
        {
            var ea = new TrackExceptionEventArgs(this, e.Error, LavalinkUtilities.DecodeTrack(e.Track));
            return this._trackException.InvokeAsync(this, ea);
        }

        internal Task InternalWebSocketClosedAsync(WebSocketCloseEventArgs e)
            => this._webSocketClosed.InvokeAsync(this, e);

        internal event ChannelDisconnectedEventHandler ChannelDisconnected;
    }
}
