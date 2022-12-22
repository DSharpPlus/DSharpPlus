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

namespace DSharpPlus.Test;

[Group("voice"), Description("Provides voice commands."), Aliases("audio")]
public class TestBotVoiceCommands : BaseCommandModule
{
    private CancellationTokenSource AudioLoopCancelTokenSource { get; set; }
    private CancellationToken AudioLoopCancelToken => AudioLoopCancelTokenSource.Token;
    private Task AudioLoopTask { get; set; }

    private ConcurrentDictionary<uint, ulong> _ssrcMap;
    private ConcurrentDictionary<uint, FileStream> _ssrcFilemap;
    private async Task OnVoiceReceived(VoiceNextConnection vnc, VoiceReceiveEventArgs e)
    {
        if (!_ssrcFilemap.ContainsKey(e.SSRC))
        {
            _ssrcFilemap[e.SSRC] = File.Create($"{e.SSRC} ({e.AudioFormat.ChannelCount}).pcm");
        }

        FileStream fs = _ssrcFilemap[e.SSRC];

        // e.Client.DebugLogger.LogMessage(LogLevel.Debug, "VNEXT RX", $"{e.User?.Username ?? "Unknown user"} sent voice data. {e.AudioFormat.ChannelCount}", DateTime.Now);
        byte[] buff = e.PcmData.ToArray();
        await fs.WriteAsync(buff).ConfigureAwait(false);
        // await fs.FlushAsync().ConfigureAwait(false);
    }
    private Task OnUserSpeaking(VoiceNextConnection vnc, UserSpeakingEventArgs e)
    {
        if (_ssrcMap.ContainsKey(e.SSRC))
        {
            return Task.CompletedTask;
        }

        if (e.User == null)
        {
            return Task.CompletedTask;
        }

        _ssrcMap[e.SSRC] = e.User.Id;
        return Task.CompletedTask;
    }
    private Task OnUserJoined(VoiceNextConnection vnc, VoiceUserJoinEventArgs e)
    {
        _ssrcMap.TryAdd(e.SSRC, e.User.Id);
        return Task.CompletedTask;
    }
    private Task OnUserLeft(VoiceNextConnection vnc, VoiceUserLeaveEventArgs e)
    {
        if (_ssrcFilemap.TryRemove(e.SSRC, out FileStream? pcmFs))
        {
            pcmFs.Dispose();
        }

        _ssrcMap.TryRemove(e.SSRC, out _);
        return Task.CompletedTask;
    }

    [Command("volume"), RequireOwner]
    public async Task VolumeAsync(CommandContext ctx, double vol = 1.0)
    {
        if (vol < 0 || vol > 2.5)
        {
            throw new ArgumentOutOfRangeException(nameof(vol), "Volume needs to be between 0 and 250% inclusive.");
        }

        VoiceNextExtension voice = ctx.Client.GetVoiceNext();
        if (voice == null)
        {
            await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
            return;
        }

        VoiceNextConnection vnc = voice.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
            return;
        }

        VoiceTransmitSink transmitStream = vnc.GetTransmitSink();
        transmitStream.VolumeModifier = vol;

        await ctx.RespondAsync($"Volume set to {vol * 100:0.00}%").ConfigureAwait(false);
    }

    [Command("join")]
    public async Task VoiceJoinAsync(CommandContext ctx)
    {
        Entities.DiscordChannel? chn = ctx.Member?.VoiceState?.Channel;
        if (chn == null)
        {
            await ctx.Message.RespondAsync("Your voice channel was not found or you are not connected").ConfigureAwait(false);
            return;
        }

        VoiceNextConnection vnc = await chn.ConnectAsync().ConfigureAwait(false);
        await ctx.Message.RespondAsync($"Tryina join `{chn.Name}` ({chn.Id})").ConfigureAwait(false);

        if (ctx.Client.GetVoiceNext().IsIncomingEnabled)
        {
            _ssrcMap = new ConcurrentDictionary<uint, ulong>();
            _ssrcFilemap = new ConcurrentDictionary<uint, FileStream>();
            vnc.VoiceReceived += OnVoiceReceived;
            vnc.UserSpeaking += OnUserSpeaking;
            vnc.UserJoined += OnUserJoined;
            vnc.UserLeft += OnUserLeft;
        }
    }

    [Command("leave")]
    public async Task VoiceLeaveAsync(CommandContext ctx)
    {
        VoiceNextExtension voice = ctx.Client.GetVoiceNext();
        if (voice == null)
        {
            await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
            return;
        }

        VoiceNextConnection vnc = voice.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
            return;
        }

        if (voice.IsIncomingEnabled)
        {
            vnc.UserSpeaking -= OnUserSpeaking;
            vnc.VoiceReceived -= OnVoiceReceived;

            foreach (System.Collections.Generic.KeyValuePair<uint, FileStream> kvp in _ssrcFilemap)
            {
                kvp.Value.Dispose();
            }

            using (FileStream fs = File.Create("index.txt"))
            using (StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                foreach (System.Collections.Generic.KeyValuePair<uint, ulong> kvp in _ssrcMap)
                {
                    await sw.WriteLineAsync(string.Format("{0} = {1}", kvp.Key, kvp.Value)).ConfigureAwait(false);
                }
            }
        }

        vnc.Disconnect();
        await ctx.Message.RespondAsync("Disconnected").ConfigureAwait(false);
    }

    [Command("play")]
    public async Task VoicePlayAsync(CommandContext ctx, params string[] filename)
    {
        VoiceNextExtension voice = ctx.Client.GetVoiceNext();
        if (voice == null)
        {
            await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
            return;
        }

        VoiceNextConnection vnc = voice.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
            return;
        }

        string snd = string.Join(" ", filename);
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

            ProcessStartInfo ffmpeg_inf = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 -",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            Process? ffmpeg = Process.Start(ffmpeg_inf);
            Stream ffout = ffmpeg.StandardOutput.BaseStream;

            VoiceTransmitSink transmitStream = vnc.GetTransmitSink();
            await ffout.CopyToAsync(transmitStream).ConfigureAwait(false);
            await transmitStream.FlushAsync().ConfigureAwait(false);

            await vnc.WaitForPlaybackFinishAsync().ConfigureAwait(false);
        }
        catch (Exception ex) { exc = ex; }

        if (exc != null)
        {
            throw exc;
        }
    }

    [Command("playloop")]
    public async Task VoicePlayLoopAsync(CommandContext ctx, params string[] filename)
    {
        VoiceNextExtension voice = ctx.Client.GetVoiceNext();
        if (voice == null)
        {
            await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
            return;
        }

        VoiceNextConnection vnc = voice.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
            return;
        }

        if (AudioLoopTask != null && !AudioLoopCancelToken.IsCancellationRequested)
        {
            await ctx.Message.RespondAsync("Audio loop is already playing").ConfigureAwait(false);
            return;
        }

        string snd = string.Join(" ", filename);
        if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
        {
            await ctx.Message.RespondAsync("Invalid file specified").ConfigureAwait(false);
            return;
        }

        await ctx.Message.RespondAsync($"Playing `{snd}` in a loop").ConfigureAwait(false);
        AudioLoopCancelTokenSource = new CancellationTokenSource();
        AudioLoopTask = Task.Run(async () =>
        {
            Entities.DiscordChannel chn = ctx.Channel;
            CancellationToken token = AudioLoopCancelToken;
            try
            {
                // borrowed from
                // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                ProcessStartInfo ffmpeg_inf = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process? ffmpeg = Process.Start(ffmpeg_inf);
                Stream ffout = ffmpeg.StandardOutput.BaseStream;

                VoiceTransmitSink transmitStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(transmitStream).ConfigureAwait(false);
                await transmitStream.FlushAsync().ConfigureAwait(false);

                await vnc.WaitForPlaybackFinishAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { await chn.SendMessageAsync($"Audio loop crashed: {ex.GetType()}: {ex.Message}").ConfigureAwait(false); }
        }, AudioLoopCancelToken);
    }

    [Command("playstop")]
    public async Task VoicePlayLoopStopAsync(CommandContext ctx)
    {
        VoiceNextExtension voice = ctx.Client.GetVoiceNext();
        if (voice == null)
        {
            await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
            return;
        }

        VoiceNextConnection vnc = voice.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
            return;
        }

        if (AudioLoopTask == null || AudioLoopCancelToken.IsCancellationRequested)
        {
            await ctx.Message.RespondAsync("Audio loop is already paused").ConfigureAwait(false);
            return;
        }

        AudioLoopCancelTokenSource.Cancel();
        await AudioLoopTask.ConfigureAwait(false);
        AudioLoopTask = null;

        await ctx.Message.RespondAsync("Audio loop stopped").ConfigureAwait(false);
    }

    [Command("playforce"), Description("Forces audio playback, regardless of whether audio is playing or not.")]
    public async Task VoicePlayForceAsync(CommandContext ctx, params string[] filename)
    {
        VoiceNextExtension voice = ctx.Client.GetVoiceNext();
        if (voice == null)
        {
            await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
            return;
        }

        VoiceNextConnection vnc = voice.GetConnection(ctx.Guild);
        if (vnc == null)
        {
            await ctx.Message.RespondAsync("Voice is not connected in this guild").ConfigureAwait(false);
            return;
        }

        string snd = string.Join(" ", filename);
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

            ProcessStartInfo ffmpeg_inf = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process? ffmpeg = Process.Start(ffmpeg_inf);
            Stream ffout = ffmpeg.StandardOutput.BaseStream;

            VoiceTransmitSink transmitStream = vnc.GetTransmitSink();
            await ffout.CopyToAsync(transmitStream).ConfigureAwait(false);
            await transmitStream.FlushAsync().ConfigureAwait(false);

            await vnc.WaitForPlaybackFinishAsync().ConfigureAwait(false);
        }
        catch (Exception ex) { exc = ex; }

        if (exc != null)
        {
            throw exc;
        }
    }
}
