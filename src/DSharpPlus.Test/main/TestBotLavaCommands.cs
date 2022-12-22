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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;

namespace DSharpPlus.Test;

[Group("lavalink"), Description("Provides audio playback via lavalink."), Aliases("lava", "ll"), ModuleLifespan(ModuleLifespan.Singleton)]
public class TestBotLavaCommands : BaseCommandModule
{
    private LavalinkNodeConnection Lavalink { get; set; }
    private LavalinkGuildConnection LavalinkVoice { get; set; }
    private DiscordChannel ContextChannel { get; set; }

    [Command, Description("Connects to Lavalink")]
    public async Task ConnectAsync(CommandContext ctx, string hostname, int port, string password)
    {
        if (Lavalink != null)
        {
            return;
        }

        LavalinkExtension lava = ctx.Client.GetLavalink();
        if (lava == null)
        {
            await ctx.RespondAsync("Lavalink is not enabled.").ConfigureAwait(false);
            return;
        }

        Lavalink = await lava.ConnectAsync(new LavalinkConfiguration
        {
            RestEndpoint = new ConnectionEndpoint(hostname, port),
            SocketEndpoint = new ConnectionEndpoint(hostname, port),
            Password = password
        }).ConfigureAwait(false);
        Lavalink.Disconnected += Lavalink_Disconnected;
        await ctx.RespondAsync("Connected to lavalink node.").ConfigureAwait(false);
    }

    private Task Lavalink_Disconnected(LavalinkNodeConnection ll, NodeDisconnectedEventArgs e)
    {
        if (e.IsCleanClose)
        {
            Lavalink = null;
            LavalinkVoice = null;
        }
        return Task.CompletedTask;
    }

    [Command, Description("Disconnects from Lavalink")]
    public async Task DisconnectAsync(CommandContext ctx)
    {
        if (Lavalink == null)
        {
            return;
        }

        LavalinkExtension lava = ctx.Client.GetLavalink();
        if (lava == null)
        {
            await ctx.RespondAsync("Lavalink is not enabled.").ConfigureAwait(false);
            return;
        }

        await Lavalink.StopAsync().ConfigureAwait(false);
        Lavalink = null;
        await ctx.RespondAsync("Disconnected from Lavalink node.").ConfigureAwait(false);
    }

    [Command, Description("Joins a voice channel.")]
    public async Task JoinAsync(CommandContext ctx, DiscordChannel chn = null)
    {
        if (Lavalink == null)
        {
            await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
            return;
        }

        DiscordChannel vc = chn ?? ctx.Member.VoiceState.Channel;
        if (vc == null)
        {
            await ctx.RespondAsync("You are not in a voice channel or you did not specify a voice channel.").ConfigureAwait(false);
            return;
        }

        LavalinkVoice = await Lavalink.ConnectAsync(vc).ConfigureAwait(false);
        LavalinkVoice.PlaybackFinished += LavalinkVoice_PlaybackFinished;
        LavalinkVoice.DiscordWebSocketClosed += (s, e) => ctx.RespondAsync("discord websocket close event");
        await ctx.RespondAsync("Connected.").ConfigureAwait(false);
    }

    private async Task LavalinkVoice_PlaybackFinished(LavalinkGuildConnection ll, TrackFinishEventArgs e)
    {
        if (ContextChannel == null)
        {
            return;
        }

        await ContextChannel.SendMessageAsync($"Playback of {Formatter.Bold(Formatter.Sanitize(e.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(e.Track.Author))} finished.").ConfigureAwait(false);
        ContextChannel = null;
    }

    [Command, Description("Leaves a voice channel.")]
    public async Task LeaveAsync(CommandContext ctx)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.DisconnectAsync().ConfigureAwait(false);
        LavalinkVoice = null;
        await ctx.RespondAsync("Disconnected.").ConfigureAwait(false);
    }

    [Command, Description("Queues tracks for playback.")]
    public async Task PlayAsync(CommandContext ctx, [RemainingText] Uri uri)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        ContextChannel = ctx.Channel;

        LavalinkLoadResult trackLoad = await Lavalink.Rest.GetTracksAsync(uri).ConfigureAwait(false);
        LavalinkTrack track = trackLoad.Tracks.First();
        await LavalinkVoice.PlayAsync(track).ConfigureAwait(false);

        await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
    }

    [Command, Description("Queues tracks for playback.")]
    public async Task PlayFileAsync(CommandContext ctx, [RemainingText] string path)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        LavalinkLoadResult trackLoad = await Lavalink.Rest.GetTracksAsync(new FileInfo(path)).ConfigureAwait(false);
        LavalinkTrack track = trackLoad.Tracks.First();
        await LavalinkVoice.PlayAsync(track).ConfigureAwait(false);

        await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
    }

    [Command, Description("Queues track for playback.")]
    public async Task PlaySoundCloudAsync(CommandContext ctx, string search)
    {
        if (Lavalink == null)
        {
            return;
        }

        LavalinkLoadResult result = await Lavalink.Rest.GetTracksAsync(search, LavalinkSearchType.SoundCloud).ConfigureAwait(false);
        LavalinkTrack track = result.Tracks.First();
        await LavalinkVoice.PlayAsync(track).ConfigureAwait(false);

        await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
    }

    [Command, Description("Queues tracks for playback.")]
    public async Task PlayPartialAsync(CommandContext ctx, TimeSpan start, TimeSpan stop, [RemainingText] Uri uri)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        LavalinkLoadResult trackLoad = await Lavalink.Rest.GetTracksAsync(uri).ConfigureAwait(false);
        LavalinkTrack track = trackLoad.Tracks.First();
        await LavalinkVoice.PlayPartialAsync(track, start, stop).ConfigureAwait(false);

        await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
    }

    [Command, Description("Pauses playback.")]
    public async Task PauseAsync(CommandContext ctx)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.PauseAsync().ConfigureAwait(false);
        await ctx.RespondAsync("Paused.").ConfigureAwait(false);
    }

    [Command, Description("Resumes playback.")]
    public async Task ResumeAsync(CommandContext ctx)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.ResumeAsync().ConfigureAwait(false);
        await ctx.RespondAsync("Resumed.").ConfigureAwait(false);
    }

    [Command, Description("Stops playback.")]
    public async Task StopAsync(CommandContext ctx)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.StopAsync().ConfigureAwait(false);
        await ctx.RespondAsync("Stopped.").ConfigureAwait(false);
    }

    [Command, Description("Seeks in the current track.")]
    public async Task SeekAsync(CommandContext ctx, TimeSpan position)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.SeekAsync(position).ConfigureAwait(false);
        await ctx.RespondAsync($"Seeking to {position}.").ConfigureAwait(false);
    }

    [Command, Description("Changes playback volume.")]
    public async Task VolumeAsync(CommandContext ctx, int volume)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.SetVolumeAsync(volume).ConfigureAwait(false);
        await ctx.RespondAsync($"Volume set to {volume}%.").ConfigureAwait(false);
    }

    [Command, Description("Shows what's being currently played."), Aliases("np")]
    public async Task NowPlayingAsync(CommandContext ctx)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        Lavalink.Entities.LavalinkPlayerState state = LavalinkVoice.CurrentState;
        LavalinkTrack track = state.CurrentTrack;
        await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))} [{state.PlaybackPosition}/{track.Length}].").ConfigureAwait(false);
    }

    [Command, Description("Sets or resets equalizer settings."), Aliases("eq")]
    public async Task EqualizerAsync(CommandContext ctx)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.ResetEqualizerAsync().ConfigureAwait(false);
        await ctx.RespondAsync("All equalizer bands were reset.").ConfigureAwait(false);
    }

    [Command]
    public async Task EqualizerAsync(CommandContext ctx, int band, float gain)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        await LavalinkVoice.AdjustEqualizerAsync(new LavalinkBandAdjustment(band, gain)).ConfigureAwait(false);
        await ctx.RespondAsync($"Band {band} adjusted by {gain}").ConfigureAwait(false);
    }

    [Command, Description("Displays Lavalink statistics.")]
    public async Task StatsAsync(CommandContext ctx)
    {
        if (LavalinkVoice == null)
        {
            return;
        }

        Lavalink.Entities.LavalinkStatistics stats = Lavalink.Statistics;
        StringBuilder sb = new StringBuilder();
        sb.Append("Lavalink resources usage statistics: ```")
            .Append("Uptime:                    ").Append(stats.Uptime).AppendLine()
            .Append("Players:                   ").AppendFormat("{0} active / {1} total", stats.ActivePlayers, stats.TotalPlayers).AppendLine()
            .Append("CPU Cores:                 ").Append(stats.CpuCoreCount).AppendLine()
            .Append("CPU Usage:                 ").AppendFormat("{0:#,##0.0%} lavalink / {1:#,##0.0%} system", stats.CpuLavalinkLoad, stats.CpuSystemLoad).AppendLine()
            .Append("RAM Usage:                 ").AppendFormat("{0} allocated / {1} used / {2} free / {3} reservable", SizeToString(stats.RamAllocated), SizeToString(stats.RamUsed), SizeToString(stats.RamFree), SizeToString(stats.RamReservable)).AppendLine()
            .Append("Audio frames (per minute): ").AppendFormat("{0:#,##0} sent / {1:#,##0} nulled / {2:#,##0} deficit", stats.AverageSentFramesPerMinute, stats.AverageNulledFramesPerMinute, stats.AverageDeficitFramesPerMinute).AppendLine()
            .Append("```");
        await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
    }

    private static readonly string[] _units = new[] { "", "ki", "Mi", "Gi" };
    private static string SizeToString(long l)
    {
        double d = l;
        int u = 0;
        while (d >= 900 && u < _units.Length - 2)
        {
            u++;
            d /= 1024;
        }

        return $"{d:#,##0.00} {_units[u]}B";
    }
}
