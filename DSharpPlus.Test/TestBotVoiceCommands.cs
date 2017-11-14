using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;

namespace DSharpPlus.Test
{
    [Group("voice"), Description("Provides voice commands."), Aliases("audio")]
    public class TestBotVoiceCommands
    {
        private CancellationTokenSource AudioLoopCancelTokenSource { get; set; }
        private CancellationToken AudioLoopCancelToken => this.AudioLoopCancelTokenSource.Token;
        private Task AudioLoopTask { get; set; }
        private double Volume { get; set; } = 1.0;

        private ConcurrentDictionary<uint, ulong> _ssrc_map;
        private ConcurrentDictionary<uint, FileStream> _ssrc_filemap;
        private async Task OnVoiceReceived(VoiceReceiveEventArgs e)
        {
            if (!this._ssrc_filemap.ContainsKey(e.SSRC))
                this._ssrc_filemap[e.SSRC] = File.Create($"{e.SSRC}.pcm");
            var fs = this._ssrc_filemap[e.SSRC];

            //e.Client.DebugLogger.LogMessage(LogLevel.Debug, "VNEXT RX", $"{e.User?.Username ?? "Unknown user"} sent voice data.", DateTime.Now);
            var buff = e.Voice.ToArray();
            await fs.WriteAsync(buff, 0, buff.Length).ConfigureAwait(false);
            await fs.FlushAsync().ConfigureAwait(false);
        }
        private Task OnUserSpeaking(UserSpeakingEventArgs e)
        {
            if (this._ssrc_map.ContainsKey(e.SSRC))
                return Task.CompletedTask;

            if (e.User == null)
                return Task.CompletedTask;

            this._ssrc_map[e.SSRC] = e.User.Id;
            return Task.CompletedTask;
        }

        private unsafe void RescaleVolume(byte[] data)
        {
            fixed (byte* ptr8 = data)
            {
                var ptr16 = (short*)ptr8;
                for (var i = 0; i < data.Length / 2; i++)
                    *(ptr16 + i) = (short)(*(ptr16 + i) * this.Volume);
            }
        }

        [Command("volume"), RequireOwner]
        public async Task VolumeAsync(CommandContext ctx, double vol = 1.0)
        {
            if (vol < 0 || vol > 5)
                throw new ArgumentOutOfRangeException(nameof(vol), "Volume needs to be between 0 and 500% inclusive.");

            this.Volume = vol;
            await ctx.RespondAsync($"Volume set to {(vol * 100).ToString("0.00")}%").ConfigureAwait(false);
        }

        [Command("join")]
        public async Task VoiceJoin(CommandContext ctx)
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
                this._ssrc_map = new ConcurrentDictionary<uint, ulong>();
                this._ssrc_filemap = new ConcurrentDictionary<uint, FileStream>();
                vnc.VoiceReceived += this.OnVoiceReceived;
                vnc.UserSpeaking += this.OnUserSpeaking;
            }
        }

        [Command("leave")]
        public async Task VoiceLeave(CommandContext ctx)
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

                foreach (var kvp in this._ssrc_filemap)
                    kvp.Value.Dispose();

                using (var fs = File.Create("index.txt"))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
                    foreach (var kvp in this._ssrc_map)
                        await sw.WriteLineAsync(string.Format("{0} = {1}", kvp.Key, kvp.Value)).ConfigureAwait(false);
            }

            vnc.Disconnect();
            await ctx.Message.RespondAsync("Disconnected").ConfigureAwait(false);
        }

        [Command("play")]
        public async Task VoicePlay(CommandContext ctx, params string[] filename)
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
            await vnc.SendSpeakingAsync(true).ConfigureAwait(false);
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

                using (var ms = new MemoryStream()) // if ffmpeg quits fast, that'll hold the data
                {
                    await ffout.CopyToAsync(ms).ConfigureAwait(false);
                    ms.Position = 0;

                    var buff = new byte[3840]; // buffer to hold the PCM data
                    var br = 0;
                    while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                    {
                        if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                            for (var i = br; i < buff.Length; i++)
                                buff[i] = 0;

                        if (this.Volume != 1.0)
                            this.RescaleVolume(buff);

                        await vnc.SendAsync(buff, 20).ConfigureAwait(false); // we're sending 20ms of data
                    }
                }
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false).ConfigureAwait(false);
            }

            if (exc != null)
                throw exc;
        }

        [Command("playloop")]
        public async Task VoicePlayLoop(CommandContext ctx, params string[] filename)
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
                await vnc.SendSpeakingAsync(true).ConfigureAwait(false);
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

                    using (var ms = new MemoryStream()) // this will hold our PCM data
                    {
                        await ffout.CopyToAsync(ms).ConfigureAwait(false);
                        ms.Position = 0;

                        var buff = new byte[3840]; // buffer to hold the PCM data
                        var br = 0;
                        while (true)
                        {
                            while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                            {
                                if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                                    for (var i = br; i < buff.Length; i++)
                                        buff[i] = 0;

                                await vnc.SendAsync(buff, 20).ConfigureAwait(false); // we're sending 20ms of data
                                token.ThrowIfCancellationRequested();
                            }
                            ms.Position = 0;
                            token.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { await chn.SendMessageAsync($"Audio loop crashed: {ex.GetType()}: {ex.Message}").ConfigureAwait(false); }
                finally
                {
                    await vnc.SendSpeakingAsync(false).ConfigureAwait(false);
                }
            }, this.AudioLoopCancelToken);
        }

        [Command("playstop")]
        public async Task VoicePlayLoopStop(CommandContext ctx)
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
        public async Task VoicePlayForce(CommandContext ctx, params string[] filename)
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
            await vnc.SendSpeakingAsync(true).ConfigureAwait(false);
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

                using (var ms = new MemoryStream()) // if ffmpeg quits fast, that'll hold the data
                {
                    await ffout.CopyToAsync(ms).ConfigureAwait(false);
                    ms.Position = 0;

                    var buff = new byte[3840]; // buffer to hold the PCM data
                    var br = 0;
                    while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                    {
                        if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                            for (var i = br; i < buff.Length; i++)
                                buff[i] = 0;

                        await vnc.SendAsync(buff, 20).ConfigureAwait(false); // we're sending 20ms of data
                    }
                }
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false).ConfigureAwait(false);
            }

            if (exc != null)
                throw exc;
        }
    }
}
