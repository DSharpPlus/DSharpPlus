using System;
using System.Globalization;
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
        /// Gets whether this channel is still connected.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref this._isDisposed);
        private bool _isDisposed = false;

        /// <summary>
        /// Gets the current player state.
        /// </summary>
        public LavalinkPlayerState CurrentState { get; }

        /// <summary>
        /// Gets the voice channel associated with this connection.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the guild associated with this connection.
        /// </summary>
        public DiscordGuild Guild => this.Channel.Guild;

        private LavalinkNodeConnection Node { get; }
        internal string GuildIdString => this.GuildId.ToString(CultureInfo.InvariantCulture);
        internal ulong GuildId => this.Channel.Guild.Id;
        internal VoiceStateUpdateEventArgs VoiceStateUpdate { get; }

        internal LavalinkGuildConnection(LavalinkNodeConnection node, DiscordChannel channel, VoiceStateUpdateEventArgs vstu)
        {
            this.Node = node;
            this.Channel = channel;
            this.VoiceStateUpdate = vstu;
            this.CurrentState = new LavalinkPlayerState();

            Volatile.Write(ref this._isDisposed, false);

            this._playerUpdated = new AsyncEvent<PlayerUpdateEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_PLAYER_UPDATE");
            this._playbackFinished = new AsyncEvent<TrackFinishEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_PLAYBACK_FINISHED");
            this._trackStuck = new AsyncEvent<TrackStuckEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_TRACK_STUCK");
            this._trackException = new AsyncEvent<TrackExceptionEventArgs>(this.Node.Discord.EventErrorHandler, "LAVALINK_TRACK_EXCEPTION");
        }

        /// <summary>
        /// Disconnect from this voice channel.
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            Volatile.Write(ref this._isDisposed, true);

            this.Node.SendPayload(new LavalinkDestroy(this));
            this.SendVoiceUpdate();

            if (this.ChannelDisconnected != null)
                this.ChannelDisconnected(this);
        }

        internal void SendVoiceUpdate()
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
            (this.Channel.Discord as DiscordClient)._webSocketClient.SendMessage(vsj);
        }

        /// <summary>
        /// Queues the specified track for playback.
        /// </summary>
        /// <param name="track">Track to play.</param>
        /// <returns></returns>
        public void Play(LavalinkTrack track)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            this.CurrentState.CurrentTrack = track;
            this.Node.SendPayload(new LavalinkPlay(this, track));
        }

        /// <summary>
        /// Queues the specified track for playback. The track will be played from specified start timestamp to specified end timestamp.
        /// </summary>
        /// <param name="track">Track to play.</param>
        /// <param name="start">Timestamp to start playback at.</param>
        /// <param name="end">Timestamp to stop playback at.</param>
        /// <returns></returns>
        public void PlayPartial(LavalinkTrack track, TimeSpan start, TimeSpan end)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (start.TotalMilliseconds < 0 || end <= start)
                throw new ArgumentException("Both start and end timestamps need to be greater or equal to zero, and the end timestamp needs to be greater than start timestamp.");

            this.CurrentState.CurrentTrack = track;
            this.Node.SendPayload(new LavalinkPlayPartial(this, track, start, end));
        }

        /// <summary>
        /// Stops the player completely.
        /// </summary>
        /// <returns></returns>
        public void Stop()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            this.Node.SendPayload(new LavalinkStop(this));
        }

        /// <summary>
        /// Pauses the player.
        /// </summary>
        /// <returns></returns>
        public void Pause()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            this.Node.SendPayload(new LavalinkPause(this, true));
        }

        /// <summary>
        /// Resumes playback.
        /// </summary>
        /// <returns></returns>
        public void Resume()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            this.Node.SendPayload(new LavalinkPause(this, false));
        }

        /// <summary>
        /// Seeks the current track to specified position.
        /// </summary>
        /// <param name="position">Position to seek to.</param>
        /// <returns></returns>
        public void Seek(TimeSpan position)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            this.Node.SendPayload(new LavalinkSeek(this, position));
        }

        /// <summary>
        /// Sets the playback volume. This might incur a lot of CPU usage.
        /// </summary>
        /// <param name="volume">Volume to set. Needs to be greater or equal to 0, and less than or equal to 150. 100 means 100% and is the default value.</param>
        /// <returns></returns>
        public void SetVolume(int volume)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (volume < 0 || volume > 150)
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume needs to range from 0 to 150.");

            this.Node.SendPayload(new LavalinkVolume(this, volume));
        }

        internal Task InternalUpdatePlayerStateAsync(LavalinkState newState)
        {
            this.CurrentState.LastUpdate = newState.Time;
            this.CurrentState.PlaybackPosition = newState.Position;

            return this._playerUpdated.InvokeAsync(new PlayerUpdateEventArgs(this, newState.Time, newState.Position));
        }

        internal Task InternalPlaybackFinishedAsync(TrackFinishData e)
        {
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

        internal event ChannelDisconnectedEventHandler ChannelDisconnected;
    }
}
