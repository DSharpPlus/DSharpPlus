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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;

namespace DSharpPlus.Test
{
    [Group("voice"), Description("Provides voice commands."), Aliases("audio")]
    public class TestBotVoiceCommands : BaseCommandModule
    {
        private CancellationTokenSource AudioLoopCancelTokenSource { get; set; }
        private CancellationToken AudioLoopCancelToken => this.AudioLoopCancelTokenSource.Token;
        private Task AudioLoopTask { get; set; }

        private ConcurrentDictionary<uint, ulong> _ssrcMap;
        private ConcurrentDictionary<uint, FileStream> _ssrcFilemap;
        private async Task OnVoiceReceived(VoiceNextConnection vnc, VoiceReceiveEventArgs e)
        {
            if (!this._ssrcFilemap.ContainsKey(e.SSRC))
                this._ssrcFilemap[e.SSRC] = File.Create($"{e.SSRC} ({e.AudioFormat.ChannelCount}).pcm");
            var fs = this._ssrcFilemap[e.SSRC];

            // e.Client.DebugLogger.LogMessage(LogLevel.Debug, "VNEXT RX", $"{e.User?.Username ?? "Unknown user"} sent voice data. {e.AudioFormat.ChannelCount}", DateTime.Now);
            var buff = e.PcmData.ToArray();
            await fs.WriteAsync(buff).ConfigureAwait(false);
            // await fs.FlushAsync().ConfigureAwait(false);
        }
        private Task OnUserSpeaking(VoiceNextConnection vnc, UserSpeakingEventArgs e)
        {
            if (this._ssrcMap.ContainsKey(e.SSRC))
                return Task.CompletedTask;

            if (e.User == null)
                return Task.CompletedTask;

            this._ssrcMap[e.SSRC] = e.User.Id;
            return Task.CompletedTask;
        }
        private Task OnUserJoined(VoiceNextConnection vnc, VoiceUserJoinEventArgs e)
        {
            this._ssrcMap.TryAdd(e.SSRC, e.User.Id);
            return Task.CompletedTask;
        }
        private Task OnUserLeft(VoiceNextConnection vnc, VoiceUserLeaveEventArgs e)
        {
            if (this._ssrcFilemap.TryRemove(e.SSRC, out var pcmFs))
                pcmFs.Dispose();
            this._ssrcMap.TryRemove(e.SSRC, out _);
            return Task.CompletedTask;
        }

        [Command("volume"), RequireOwner]
        public static async Task VolumeAsync(CommandContext ctx, double vol = 1.0)
        {
            if (vol < 0 || vol > 2.5)
                throw new ArgumentOutOfRangeException(nameof(vol), "Volume needs to be between 0 and 250% inclusive.");

            var voice = ctx.Client.GetVoiceNext();
            if (voice == null)
            {
                await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
                return;
            }

            var vnc = voice.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
                return;
            }

            var transmitStream = vnc.GetTransmitSink();
            transmitStream.VolumeModifier = vol;

            await ctx.RespondAsync($"Volume set to {vol * 100:0.00}%").ConfigureAwait(false);
        }

        [Command("join")]
        public async Task VoiceJoinAsync(CommandContext ctx)
        {
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.Message.RespondAsync("Your voice channel was not found or you are not connected").ConfigureAwait(false);
                return;
            }

            var vnc = await chn.ConnectAsync().ConfigureAwait(false);
            await ctx.Message.RespondAsync($"Tryina join `{chn.Name}` ({chn.Id})").ConfigureAwait(false);

            if (ctx.Client.GetVoiceNext().IsIncomingEnabled)
            {
                this._ssrcMap = new ConcurrentDictionary<uint, ulong>();
                this._ssrcFilemap = new ConcurrentDictionary<uint, FileStream>();
                vnc.VoiceReceived += this.OnVoiceReceived;
                vnc.UserSpeaking += this.OnUserSpeaking;
                vnc.UserJoined += this.OnUserJoined;
                vnc.UserLeft += this.OnUserLeft;
            }
        }

        [Command("leave")]
        public async Task VoiceLeaveAsync(CommandContext ctx)
        {
            var voice = ctx.Client.GetVoiceNext();
            if (voice == null)
            {
                await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
                return;
            }

            var vnc = voice.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
                return;
            }

            if (voice.IsIncomingEnabled)
            {
                vnc.UserSpeaking -= this.OnUserSpeaking;
                vnc.VoiceReceived -= this.OnVoiceReceived;

                foreach (var kvp in this._ssrcFilemap)
                    kvp.Value.Dispose();

                using (var fs = File.Create("index.txt"))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
                    foreach (var kvp in this._ssrcMap)
                        await sw.WriteLineAsync(string.Format("{0} = {1}", kvp.Key, kvp.Value)).ConfigureAwait(false);
            }

            vnc.Disconnect();
            await ctx.Message.RespondAsync("Disconnected").ConfigureAwait(false);
        }

        [Command("play")]
        public static async Task VoicePlayAsync(CommandContext ctx, params string[] filename)
        {
            var voice = ctx.Client.GetVoiceNext();
            if (voice == null)
            {
                await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
                return;
            }

            var vnc = voice.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
                return;
            }

            var snd = string.Join(" ", filename);
            if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
            {
                await ctx.Message.RespondAsync("Invalid file specified").ConfigureAwait(false);
                return;
            }

            while (vnc.IsPlaying)
            {
                await ctx.Message.RespondAsync("This connection is playing audio, waiting for end.").ConfigureAwait(false);
                await vnc.WaitForPlaybackFinishAsync().ConfigureAwait(false);
            }

            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{snd}`").ConfigureAwait(false);
            try
            {
                // borrowed from
                // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                var ffmpeg_inf = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 -",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                var ffmpeg = Process.Start(ffmpeg_inf);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var transmitStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(transmitStream).ConfigureAwait(false);
                await transmitStream.FlushAsync().ConfigureAwait(false);

                await vnc.WaitForPlaybackFinishAsync().ConfigureAwait(false);
            }
            catch (Exception ex) { exc = ex; }

            if (exc != null)
                throw exc;
        }

        [Command("playloop")]
        public async Task VoicePlayLoopAsync(CommandContext ctx, params string[] filename)
        {
            var voice = ctx.Client.GetVoiceNext();
            if (voice == null)
            {
                await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
                return;
            }

            var vnc = voice.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
                return;
            }

            if (this.AudioLoopTask != null && !this.AudioLoopCancelToken.IsCancellationRequested)
            {
                await ctx.Message.RespondAsync("Audio loop is already playing").ConfigureAwait(false);
                return;
            }

            var snd = string.Join(" ", filename);
            if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
            {
                await ctx.Message.RespondAsync("Invalid file specified").ConfigureAwait(false);
                return;
            }

            await ctx.Message.RespondAsync($"Playing `{snd}` in a loop").ConfigureAwait(false);
            this.AudioLoopCancelTokenSource = new CancellationTokenSource();
            this.AudioLoopTask = Task.Run(async () =>
            {
                var chn = ctx.Channel;
                var token = this.AudioLoopCancelToken;
                try
                {
                    // borrowed from
                    // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                    var ffmpeg_inf = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var ffmpeg = Process.Start(ffmpeg_inf);
                    var ffout = ffmpeg.StandardOutput.BaseStream;

                    var transmitStream = vnc.GetTransmitSink();
                    await ffout.CopyToAsync(transmitStream).ConfigureAwait(false);
                    await transmitStream.FlushAsync().ConfigureAwait(false);

                    await vnc.WaitForPlaybackFinishAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { await chn.SendMessageAsync($"Audio loop crashed: {ex.GetType()}: {ex.Message}").ConfigureAwait(false); }
            }, this.AudioLoopCancelToken);
        }

        [Command("playstop")]
        public async Task VoicePlayLoopStopAsync(CommandContext ctx)
        {
            var voice = ctx.Client.GetVoiceNext();
            if (voice == null)
            {
                await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
                return;
            }

            var vnc = voice.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
                return;
            }

            if (this.AudioLoopTask == null || this.AudioLoopCancelToken.IsCancellationRequested)
            {
                await ctx.Message.RespondAsync("Audio loop is already paused").ConfigureAwait(false);
                return;
            }

            this.AudioLoopCancelTokenSource.Cancel();
            await this.AudioLoopTask.ConfigureAwait(false);
            this.AudioLoopTask = null;

            await ctx.Message.RespondAsync("Audio loop stopped").ConfigureAwait(false);
        }

        [Command("playforce"), Description("Forces audio playback, regardless of whether audio is playing or not.")]
        public static async Task VoicePlayForceAsync(CommandContext ctx, params string[] filename)
        {
            var voice = ctx.Client.GetVoiceNext();
            if (voice == null)
            {
                await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
                return;
            }

            var vnc = voice.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
                return;
            }

            var snd = string.Join(" ", filename);
            if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
            {
                await ctx.Message.RespondAsync("Invalid file specified").ConfigureAwait(false);
                return;
            }

            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{snd}`").ConfigureAwait(false);
            try
            {
                // borrowed from
                // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                var ffmpeg_inf = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var ffmpeg = Process.Start(ffmpeg_inf);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var transmitStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(transmitStream).ConfigureAwait(false);
                await transmitStream.FlushAsync().ConfigureAwait(false);

                await vnc.WaitForPlaybackFinishAsync().ConfigureAwait(false);
            }
            catch (Exception ex) { exc = ex; }

            if (exc != null)
                throw exc;
        }
    }
}
