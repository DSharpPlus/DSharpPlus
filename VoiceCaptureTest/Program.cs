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

            if(client.SendLoginRequest() != null)
            {
                Task.Run(() =>
                {
                    client.AudioPacketReceived += (sender, e) =>
                    {
                        if(capture)
                        {
                            if(e.Packet.SSRC == ssrcToCapture)
                            {
                                packets.Add(e.Packet);
                            }
                        }
                    };
                    client.UserSpeaking += (sender, e) =>
                    {
                        if (e.Speaking)
                        {
                            Console.WriteLine("Capturing audio for user " + e.UserSpeaking.Username + " (ssrc: " + e.ssrc + ")");
                            capture = true;
                            ssrcToCapture = e.ssrc;
                            packets = new List<DiscordAudioPacket>();
                        }
                        else
                        {
                            Console.WriteLine("Stopped capturing");
                            capture = false;
                            ssrcToCapture = -1;
                            justwait();
                        }
                    };
                    client.MessageReceived += (sender, e) =>
                    {
                        if(e.message.content.StartsWith("?joinvoice"))
                        {
                            string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                            if(split[1] != "")
                            {
                                DiscordChannel toJoin = e.Channel.parent.channels.Find(x => (x.name.ToLower() == split[1].ToLower()) && (x.type == "voice"));
                                if(toJoin != null)
                                {
                                    client.ConnectToVoiceChannel(toJoin);
                                }
                            }
                        }
                    };
                    client.Connected += (sender, e) =>
                    {
                        Console.WriteLine("Connected as " + e.user.Username);
                    };
                    client.Connect();
                });
            }

            Console.ReadLine();
        }

        static void justwait()
        {
            //1. do initial packet size calculation
            long totalSize = 0;
            packets.ForEach(x =>
            {
                totalSize += x.GetEncodedAudio().Length;
            });

            //2. create giant buffer.
            byte[] bigBossBuffer = new byte[totalSize];

            //3. write packets to giant buffer
            using (MemoryStream ms = new MemoryStream(bigBossBuffer))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    packets.ForEach(x => bw.Write(x.GetEncodedAudio()));
                }

                bigBossBuffer = ms.ToArray(); //just in case
            }

            //4. opus decoder yay
            var decoder = OpusDecoder.Create(480000, 1);
            int length = bigBossBuffer.Length;
            byte[] test = decoder.Decode(bigBossBuffer, length, out length);

            //5. write to a wave
            using (WaveFileWriter writer = new WaveFileWriter("lol.wav", new WaveFormat()))
            {
                writer.Write(test, 0, test.Length);
            }

            //6. free
            packets = new List<DiscordAudioPacket>();
        }
    }
}
