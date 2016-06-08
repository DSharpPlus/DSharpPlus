using DiscordSharp;
using DiscordSharp.Commands;
using DiscordSharp.Objects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace Luigibot.Modules
{
    public class Voice : IModule
    {
        private LuigibotMain MainEntry;

        public Voice(LuigibotMain luigibot)
        {
            MainEntry = luigibot;

            Name = "voice";
            Description = "Does voice.";
        }

        private void SendVoice(string file, DiscordClient client, YouTubeVideo video)
        {
            DiscordVoiceClient vc = client.GetVoiceClient();
            try
            {
                int ms = vc.VoiceConfig.FrameLengthMs;
                int channels = vc.VoiceConfig.Channels;
                int sampleRate = 48000;

                int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
                byte[] buffer = new byte[blockSize];
                var outFormat = new WaveFormat(sampleRate, 16, channels);

                vc.SetSpeaking(true);

                if(video.AudioFormat == AudioFormat.Mp3)
                {
                    using (var mp3Reader = new Mp3FileReader(video.Stream()))
                    {
                        using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 })
                        {
                            //resampler.ResamplerQuality = 60;
                            int byteCount;
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                            {
                                if (vc.Connected)
                                {
                                    vc.SendVoice(buffer);
                                }
                                else
                                    break;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Voice finished enqueuing");
                            Console.ForegroundColor = ConsoleColor.White;
                            resampler.Dispose();
                            mp3Reader.Close();
                        }
                    }
                }
                else if(video.AudioFormat == AudioFormat.Vorbis)
                {
                    using (var vorbis = new NAudio.Vorbis.VorbisWaveReader(video.Stream()))
                    {
                        using (var resampler = new MediaFoundationResampler(vorbis, outFormat) { ResamplerQuality = 60 })
                        {
                            int byteCount;
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                            {
                                if (vc.Connected)
                                {
                                    vc.SendVoice(buffer);
                                }
                                else
                                    break;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Voice finished enqueuing");
                            Console.ForegroundColor = ConsoleColor.White;
                            resampler.Dispose();
                            vorbis.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    MainEntry.owner.SendMessage("Exception during voice: `" + ex.Message + "`\n\n```" + ex.StackTrace + "\n```");
                }
                catch { }
            }
        }

        private void SendVoice(string file, DiscordClient client)
        {
            DiscordVoiceClient vc = client.GetVoiceClient();
            try
            {
                int ms = vc.VoiceConfig.FrameLengthMs;
                int channels = vc.VoiceConfig.Channels;
                int sampleRate = 48000;

                int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
                byte[] buffer = new byte[blockSize];
                var outFormat = new WaveFormat(sampleRate, 16, channels);

                vc.SetSpeaking(true);
                if (file.EndsWith(".wav"))
                {
                    using (var waveReader = new WaveFileReader(file))
                    {
                        int byteCount;
                        while ((byteCount = waveReader.Read(buffer, 0, blockSize)) > 0)
                        {
                            if (vc.Connected)
                                vc.SendVoice(buffer);
                            else
                                break;
                        }
                    }
                }
                else if (file.EndsWith(".mp3"))
                {
                    using (var mp3Reader = new MediaFoundationReader(file))
                    {
                        using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 })
                        {
                            //resampler.ResamplerQuality = 60;
                            int byteCount;
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                            {
                                if (vc.Connected)
                                {
                                    vc.SendVoice(buffer);
                                }
                                else
                                    break;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Voice finished enqueuing");
                            Console.ForegroundColor = ConsoleColor.White;
                            resampler.Dispose();
                            mp3Reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    MainEntry.owner.SendMessage("Exception during voice: `" + ex.Message + "`\n\n```" + ex.StackTrace + "\n```");
                }
                catch { }
            }
        }

        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("yt", "Streams a YouTube Video. Lewd.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args[0].Length > 0)
                {
                    string url = cmdArgs.Args[0];
                    var youtube = YouTube.Default;
                    var video = youtube.GetVideo(url);
                    if (video.AudioFormat == AudioFormat.Mp3 || video.AudioFormat == AudioFormat.Vorbis)
                    {
                        SendVoice("", manager.Client, video);
                    }
                    else
                        cmdArgs.Channel.SendMessage($"Tell Axiom not to get lazy and support other audio codecs besides MP3! (This was in {video.AudioFormat})");
                }
                else
                    cmdArgs.Channel.SendMessage($"wrong");
            }), this);
            manager.AddCommand(new CommandStub("disconnect", "Disconnects from voice", "", PermissionType.Owner, 1, cmdArgs =>
            {
                manager.Client.DisconnectFromVoice();
            }), this);
            manager.AddCommand(new CommandStub("testvoice", "Broadcasts specified file over voice.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if (File.Exists(cmdArgs.Args[0]))
                {
                    if (manager.Client.ConnectedToVoice())
                        SendVoice(cmdArgs.Args[0], manager.Client);
                    else
                        cmdArgs.Channel.SendMessage("Not connected to voice!");
                }
                else
                    cmdArgs.Channel.SendMessage("Couldn't broadcast specified file! It doesn't exist!");
            }), this);
            manager.AddCommand(new CommandStub("joinvoice", "Joins a specified voice channel", "Arg is case insensitive voice channel name to join.", PermissionType.Owner, 1, cmdArgs =>
            {
                DiscordChannel channelToJoin = cmdArgs.Channel.Parent.Channels.Find(x => x.Name.ToLower() == cmdArgs.Args[0].ToLower() && x.Type == ChannelType.Voice);
                if (channelToJoin != null)
                {
                    DiscordVoiceConfig config = new DiscordVoiceConfig
                    {
                        FrameLengthMs = 60,
                        Channels = 1,
                        OpusMode = DiscordSharp.Voice.OpusApplication.MusicOrMixed,
                        SendOnly = true
                    };

                    //waveFormat = new WaveFormat(48000, 16, config.Channels);

                    //if (!config.SendOnly)
                    //{
                    //    waveCallbackInfo = WaveCallbackInfo.FunctionCallback();
                    //    outputDevice = new WaveOut();
                    //}

                    manager.Client.ConnectToVoiceChannel(channelToJoin, config);
                    manager.Client.VoiceQueueEmpty += (sender, e) =>
                    {
                        Console.WriteLine("Queue empty.");
                    };
                }
                else
                    cmdArgs.Channel.SendMessage("Couldn't find the specified channel as a voice channel!");
            }), this);
            manager.AddCommand(new CommandStub("stop", "Stops current voice without disconnecting.", "", PermissionType.Owner, cmdArgs =>
            {
                if (manager.Client.GetVoiceClient() != null)
                {
                    manager.Client.GetVoiceClient().ClearVoiceQueue();
                }
            }), this);
        }
    }
}
