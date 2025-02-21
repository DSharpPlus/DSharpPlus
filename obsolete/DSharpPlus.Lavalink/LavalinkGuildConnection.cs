using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Lavalink;

[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
internal delegate void ChannelDisconnectedEventHandler(LavalinkGuildConnection node);

/// <summary>
/// Represents a Lavalink connection to a channel.
/// </summary>
[Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
public sealed class LavalinkGuildConnection
{
    /// <summary>
    /// Triggered whenever Lavalink updates player status.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
    {
        add => this.playerUpdated.Register(value);
        remove => this.playerUpdated.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> playerUpdated;

    /// <summary>
    /// Triggered whenever playback of a track starts.
    /// <para>This is only available for version 3.3.1 and greater.</para>
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
    {
        add => this.playbackStarted.Register(value);
        remove => this.playbackStarted.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> playbackStarted;

    /// <summary>
    /// Triggered whenever playback of a track finishes.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
    {
        add => this.playbackFinished.Register(value);
        remove => this.playbackFinished.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> playbackFinished;

    /// <summary>
    /// Triggered whenever playback of a track gets stuck.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
    {
        add => this.trackStuck.Register(value);
        remove => this.trackStuck.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> trackStuck;

    /// <summary>
    /// Triggered whenever playback of a track encounters an error.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
    {
        add => this.trackException.Register(value);
        remove => this.trackException.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> trackException;

    /// <summary>
    /// Triggered whenever Discord Voice WebSocket connection is terminated.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, WebSocketCloseEventArgs> DiscordWebSocketClosed
    {
        add => this.webSocketClosed.Register(value);
        remove => this.webSocketClosed.Unregister(value);
    }
    private readonly AsyncEvent<LavalinkGuildConnection, WebSocketCloseEventArgs> webSocketClosed;

    /// <summary>
    /// Gets whether this channel is still connected.
    /// </summary>
    public bool IsConnected => !Volatile.Read(ref this.isDisposed) && this.Channel != null;
    private bool isDisposed = false;

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
    internal VoiceStateUpdatedEventArgs VoiceStateUpdate { get; set; }
    internal TaskCompletionSource<bool> VoiceWsDisconnectTcs { get; set; }

    internal LavalinkGuildConnection(LavalinkNodeConnection node, VoiceStateUpdatedEventArgs vstu)
    {
        this.Node = node;
        this.VoiceStateUpdate = vstu;
        this.CurrentState = new LavalinkPlayerState();
        this.VoiceWsDisconnectTcs = new TaskCompletionSource<bool>();

        Volatile.Write(ref this.isDisposed, false);

        DefaultClientErrorHandler errorHandler = new(vstu.Guild.Discord.Logger);

        this.playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>(errorHandler);
        this.playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>(errorHandler);
        this.playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>(errorHandler);
        this.trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>(errorHandler);
        this.trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>(errorHandler);
        this.webSocketClosed = new AsyncEvent<LavalinkGuildConnection, WebSocketCloseEventArgs>(errorHandler);
    }

    /// <summary>
    /// Disconnects the connection from the voice channel.
    /// </summary>
    /// <param name="shouldDestroy">Whether the connection should be destroyed on the Lavalink server when leaving.</param>

    public Task DisconnectAsync(bool shouldDestroy = true)
        => DisconnectInternalAsync(shouldDestroy);

    internal async Task DisconnectInternalAsync(bool shouldDestroy, bool isManualDisconnection = false)
    {
        if (!this.IsConnected && !isManualDisconnection)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        Volatile.Write(ref this.isDisposed, true);

        if (shouldDestroy)
        {
            await this.Node.SendPayloadAsync(new LavalinkDestroy(this));
        }

        if (!isManualDisconnection)
        {
            await SendVoiceUpdateAsync();
            ChannelDisconnected?.Invoke(this);
        }
    }

    internal async Task SendVoiceUpdateAsync()
    {
        VoiceStateUpdatePayload payload = new()
        {
            GuildId = this.GuildId,
            ChannelId = null,
            Deafened = false,
            Muted = false
        };

#pragma warning disable DSP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        await (this.Channel.Discord as DiscordClient).SendPayloadAsync(GatewayOpCode.VoiceStateUpdate, payload, this.GuildId);
#pragma warning restore DSP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
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
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        this.CurrentState.CurrentTrack = track;
        await this.Node.SendPayloadAsync(new LavalinkPlay(this, track));
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
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        if (start.TotalMilliseconds < 0 || end <= start)
        {
            throw new ArgumentException("Both start and end timestamps need to be greater or equal to zero, and the end timestamp needs to be greater than start timestamp.");
        }

        this.CurrentState.CurrentTrack = track;
        await this.Node.SendPayloadAsync(new LavalinkPlayPartial(this, track, start, end));
    }

    /// <summary>
    /// Stops the player completely.
    /// </summary>
    public async Task StopAsync()
    {
        if (!this.IsConnected)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        await this.Node.SendPayloadAsync(new LavalinkStop(this));
    }

    /// <summary>
    /// Pauses the player.
    /// </summary>
    public async Task PauseAsync()
    {
        if (!this.IsConnected)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        await this.Node.SendPayloadAsync(new LavalinkPause(this, true));
    }

    /// <summary>
    /// Resumes playback.
    /// </summary>
    public async Task ResumeAsync()
    {
        if (!this.IsConnected)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        await this.Node.SendPayloadAsync(new LavalinkPause(this, false));
    }

    /// <summary>
    /// Seeks the current track to specified position.
    /// </summary>
    /// <param name="position">Position to seek to.</param>
    public async Task SeekAsync(TimeSpan position)
    {
        if (!this.IsConnected)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        await this.Node.SendPayloadAsync(new LavalinkSeek(this, position));
    }

    /// <summary>
    /// Sets the playback volume. This might incur a lot of CPU usage.
    /// </summary>
    /// <param name="volume">Volume to set. Needs to be greater or equal to 0, and less than or equal to 100. 100 means 100% and is the default value.</param>
    public async Task SetVolumeAsync(int volume)
    {
        if (!this.IsConnected)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        if (volume is < 0 or > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume needs to range from 0 to 1000.");
        }

        await this.Node.SendPayloadAsync(new LavalinkVolume(this, volume));
    }

    /// <summary>
    /// Adjusts the specified bands in the audio equalizer. This will alter the sound output, and might incur a lot of CPU usage.
    /// </summary>
    /// <param name="bands">Bands adjustments to make. You must specify one adjustment per band at most.</param>
    public async Task AdjustEqualizerAsync(params LavalinkBandAdjustment[] bands)
    {
        if (!this.IsConnected)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        if (bands.Length == 0)
        {
            return;
        }

        if (bands.Distinct(new LavalinkBandAdjustmentComparer()).Count() != bands.Length)
        {
            throw new InvalidOperationException("You cannot specify multiple modifiers for the same band.");
        }

        await this.Node.SendPayloadAsync(new LavalinkEqualizer(this, bands));
    }

    /// <summary>
    /// Resets the audio equalizer to default values.
    /// </summary>
    public async Task ResetEqualizerAsync()
    {
        if (!this.IsConnected)
        {
            throw new InvalidOperationException("This connection is not valid.");
        }

        await this.Node.SendPayloadAsync(new LavalinkEqualizer(this, Enumerable.Range(0, 15).Select(x => new LavalinkBandAdjustment(x, 0))));
    }

    internal Task InternalUpdatePlayerStateAsync(LavalinkState newState)
    {
        this.CurrentState.LastUpdate = newState.Time;
        this.CurrentState.PlaybackPosition = newState.Position;

        return this.playerUpdated.InvokeAsync(this, new PlayerUpdateEventArgs(this, newState.Time, newState.Position));
    }

    internal Task InternalPlaybackStartedAsync(string track)
    {
        TrackStartEventArgs ea = new(this, LavalinkUtilities.DecodeTrack(track));
        return this.playbackStarted.InvokeAsync(this, ea);
    }

    internal Task InternalPlaybackFinishedAsync(TrackFinishData e)
    {
        if (e.Reason != TrackEndReason.Replaced)
        {
            this.CurrentState.CurrentTrack = default;
        }

        TrackFinishEventArgs ea = new(this, LavalinkUtilities.DecodeTrack(e.Track), e.Reason);
        return this.playbackFinished.InvokeAsync(this, ea);
    }

    internal Task InternalTrackStuckAsync(TrackStuckData e)
    {
        TrackStuckEventArgs ea = new(this, e.Threshold, LavalinkUtilities.DecodeTrack(e.Track));
        return this.trackStuck.InvokeAsync(this, ea);
    }

    internal Task InternalTrackExceptionAsync(TrackExceptionData e)
    {
        TrackExceptionEventArgs ea = new(this, e.Error, LavalinkUtilities.DecodeTrack(e.Track));
        return this.trackException.InvokeAsync(this, ea);
    }

    internal Task InternalWebSocketClosedAsync(WebSocketCloseEventArgs e)
        => this.webSocketClosed.InvokeAsync(this, e);

    internal event ChannelDisconnectedEventHandler ChannelDisconnected;
}
