using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.Entities;
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
        /// Gets whether this channel is still connected.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref this._isDisposed);
        private bool _isDisposed = false;

        private LavalinkNodeConnection Node { get; }
        internal DiscordChannel Channel { get; set; }
        internal string GuildIdString => this.GuildId.ToString(CultureInfo.InvariantCulture);
        internal ulong GuildId => this.Channel.Guild.Id;
        internal VoiceStateUpdateEventArgs VoiceStateUpdate { get; }

        internal LavalinkGuildConnection(LavalinkNodeConnection node, DiscordChannel channel, VoiceStateUpdateEventArgs vstu)
        {
            this.Node = node;
            this.Channel = channel;
            this.VoiceStateUpdate = vstu;

            Volatile.Write(ref this._isDisposed, false);
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

            if (this.ChannelDisconnected != null)
                this.ChannelDisconnected(this);
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

        internal event ChannelDisconnectedEventHandler ChannelDisconnected;
    }
}
