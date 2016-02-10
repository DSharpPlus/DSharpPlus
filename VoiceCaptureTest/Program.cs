using DiscordSharp;
using DiscordSharp.Objects;
using FragLabs.Audio.Codecs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceCaptureTest
{
    class Program
    {
        static bool capture = false;
        static int ssrcToCapture = -1;
        static List<DiscordAudioPacket> packets;
        static DiscordClient client;

        static void Main(string[] args)
        {
            client = new DiscordClient();
            if(File.Exists("credentials.txt"))
            {
                using (StreamReader sr = new StreamReader("credentials.txt"))
                {
                    client.ClientPrivateInformation.email = sr.ReadLine();
                    client.ClientPrivateInformation.password = sr.ReadLine();
                    sr.Close();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" Please create credentials.txt with email and password for bot.");

                Console.ReadLine();
            }

            if (client.SendLoginRequest() != null)
                ClientTask(client);

            Console.ReadLine();
        }

        static Task ClientTask(DiscordClient client)
        {
            return Task.Run(() =>
            {
                client.MessageReceived += (sender, e) =>
                {
                    if (e.message.content.StartsWith("?joinvoice"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split[1] != "")
                        {
                            DiscordChannel toJoin = e.Channel.parent.channels.Find(x => (x.Name.ToLower() == split[1].ToLower()) && (x.Type == ChannelType.Voice));
                            if (toJoin != null)
                            {
                                client.ConnectToVoiceChannel(toJoin);
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?voice"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (File.Exists(split[1]))
                            DoVoice(client.GetVoiceClient(), split[1]);
                    }
                    else if(e.message.content.StartsWith("?disconnect"))
                    {
                        client.DisconnectFromVoice();
                    }
                };
                client.Connected += (sender, e) =>
                {
                    Console.WriteLine("Connected as " + e.user.Username);
                };
                client.Connect();
            });
        }
        static Task DoVoice(DiscordVoiceClient vc, string file)
        {
            return Task.Run(() =>
            {
                try
                {
                    int ms = 20;
                    int channels = 2;
                    int sampleRate = 48000;

                    int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
                    byte[] buffer = new byte[blockSize];
                    var outFormat = new WaveFormat(sampleRate, 16, channels);

                    TimestampSequenceReturn sequence = new TimestampSequenceReturn();
                    sequence.sequence = 0;
                    sequence.timestamp = 0;

                    vc.InitializeOpusEncoder(sampleRate, channels, ms, null);
                    vc.SendSpeaking(true);
                    using (var mp3Reader = new Mp3FileReader(file))
                    {
                        using (var resampler = new WaveFormatConversionStream(outFormat, mp3Reader))
                        {
                            int byteCount;
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                            {
                                if (vc.Connected)
                                {
                                    //sequence = await vc.SendSmallOpusAudioPacket(buffer, sampleRate, byteCount, sequence).ConfigureAwait(false);
                                    vc.SendVoice(buffer);
                                    //sequence = vc.SendSmallOpusAudioPacket(buffer, 48000, buffer.Length, sequence);
                                    //Task.Delay(19).Wait();
                                }
                                else
                                    break;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Voice finished enqueuing");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}
