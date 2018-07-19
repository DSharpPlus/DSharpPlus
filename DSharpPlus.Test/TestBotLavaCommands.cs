using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Net.Udp;

namespace DSharpPlus.Test
{
    [Group("lavalink"), Description("Provides audio playback via lavalink."), Aliases("lava")]
    public class TestBotLavaCommands : BaseCommandModule
    {
        private LavalinkNodeConnection Lavalink { get; set; }
        private LavalinkGuildConnection LavalinkVoice { get; set; }

        [Command, Description("Connects to Lavalink")]
        public async Task ConnectAsync(CommandContext ctx, string hostname, int port, string password)
        {
            if (this.Lavalink != null)
                return;

            var lava = ctx.Client.GetLavalink();
            if (lava == null)
            {
                await ctx.RespondAsync("Lavalink is not enabled.").ConfigureAwait(false);
                return;
            }

            this.Lavalink = await lava.ConnectAsync(new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint { Hostname = hostname, Port = 2333 },
                SocketEndpoint = new ConnectionEndpoint { Hostname = hostname, Port = port },
                Password = password
            }).ConfigureAwait(false);
            this.Lavalink.Disconnected += this.Lavalink_Disconnected;
            await ctx.RespondAsync("Connected to lavalink node.").ConfigureAwait(false);
        }

        private Task Lavalink_Disconnected(Lavalink.EventArgs.NodeDisconnectedEventArgs e)
        {
            this.Lavalink = null;
            this.LavalinkVoice = null;
            return Task.CompletedTask;
        }

        [Command, Description("Disconnects from Lavalink")]
        public async Task DisconnectAsync(CommandContext ctx)
        {
            if (this.Lavalink == null)
                return;

            var lava = ctx.Client.GetLavalink();
            if (lava == null)
            {
                await ctx.RespondAsync("Lavalink is not enabled.").ConfigureAwait(false);
                return;
            }

            await this.Lavalink.StopAsync().ConfigureAwait(false);
            this.Lavalink = null;
            await ctx.RespondAsync("Disconnected from Lavalink node.").ConfigureAwait(false);
        }

        [Command, Description("Joins a voice channel.")]
        public async Task JoinAsync(CommandContext ctx, DiscordChannel chn = null)
        {
            if (this.Lavalink == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
                return;
            }

            var vc = chn ?? ctx.Member.VoiceState.Channel;
            if (vc == null)
            {
                await ctx.RespondAsync("You are not in a voice channel or you did not specify a voice channel.").ConfigureAwait(false);
                return;
            }

            this.LavalinkVoice = await this.Lavalink.ConnectAsync(vc);
            await ctx.RespondAsync("Connected.").ConfigureAwait(false);
        }

        [Command, Description("Leaves a voice channel.")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            this.LavalinkVoice.Disconnect();
            this.LavalinkVoice = null;
            await ctx.RespondAsync("Disconnected.").ConfigureAwait(false);
        }

        [Command, Description("Queues tracks for playback.")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] string uri)
        {
            if (this.LavalinkVoice == null)
                return;

            var tracks = await this.Lavalink.GetTracksAsync(new Uri(uri));
            var track = tracks.First();
            this.LavalinkVoice.Play(track);

            await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
        }

        [Command, Description("Queues tracks for playback.")]
        public async Task PlayFileAsync(CommandContext ctx, [RemainingText] string path)
        {
            if (this.LavalinkVoice == null)
                return;

            var tracks = await this.Lavalink.GetTracksAsync(new FileInfo(path));
            var track = tracks.First();
            this.LavalinkVoice.Play(track);

            await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
        }

        [Command, Description("Queues tracks for playback.")]
        public async Task PlayPartialAsync(CommandContext ctx, TimeSpan start, TimeSpan stop, [RemainingText] string uri)
        {
            if (this.LavalinkVoice == null)
                return;

            var tracks = await this.Lavalink.GetTracksAsync(new Uri(uri));
            var track = tracks.First();
            this.LavalinkVoice.PlayPartial(track, start, stop);

            await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
        }

        [Command, Description("Pauses playback.")]
        public async Task PauseAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            this.LavalinkVoice.Pause();
            await ctx.RespondAsync("Paused.").ConfigureAwait(false);
        }

        [Command, Description("Resumes playback.")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            this.LavalinkVoice.Resume();
            await ctx.RespondAsync("Resumed.").ConfigureAwait(false);
        }

        [Command, Description("Stops playback.")]
        public async Task StopAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            this.LavalinkVoice.Stop();
            await ctx.RespondAsync("Stopped.").ConfigureAwait(false);
        }

        [Command, Description("Seeks in the current track.")]
        public async Task SeekAsync(CommandContext ctx, TimeSpan position)
        {
            if (this.LavalinkVoice == null)
                return;

            this.LavalinkVoice.Seek(position);
            await ctx.RespondAsync($"Seeking to {position}.").ConfigureAwait(false);
        }

        [Command, Description("Changes playback volume.")]
        public async Task VolumeAsync(CommandContext ctx, int volume)
        {
            if (this.LavalinkVoice == null)
                return;

            this.LavalinkVoice.SetVolume(volume);
            await ctx.RespondAsync($"Volume set to {volume}%.").ConfigureAwait(false);
        }
    }
}
