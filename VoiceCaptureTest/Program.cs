using DiscordSharp;
using DiscordSharp.Commands;
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
        static DiscordClient client;
        static CommandsManager CommandManager;

        static void Main(string[] args)
        {
            client = new DiscordClient();
            if(File.Exists("credentials.txt"))
            {
                using (StreamReader sr = new StreamReader("credentials.txt"))
                {
                    client.ClientPrivateInformation.Email = sr.ReadLine();
                    client.ClientPrivateInformation.Password = sr.ReadLine();
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
                    string message = e.Message.Content;
                    if(message[0] == '?')
                    {
                        message = message.Substring(1); //to remove the command prefix
                        try
                        {
                            CommandManager.ExecuteOnMessageCommand(message, e.Channel, e.Author);
                        }
                        catch(UnauthorizedAccessException ex)
                        {
                            e.Channel.SendMessage(ex.Message);
                        }
                    }
                };
                client.Connected += (sender, e) =>
                {
                    Console.WriteLine("Connected as " + e.User.Username);
                    CommandManager = new CommandsManager(client);
                    CommandManager.AddCommand(new CommandStub("joinvoice",
                            "Joins a voice channel",
                            "Usage: ?joinvoice <channel name>", PermissionType.Owner, cmdArgs =>
                            {
                                if (cmdArgs.Args.Count < 1)
                                    return;
                                DiscordChannel toJoin = cmdArgs.Channel.Parent.Channels.Find(x => (x.Name.ToLower() == cmdArgs.Args[0].ToLower() && x.Type == ChannelType.Voice));
                                if (toJoin != null)
                                    client.ConnectToVoiceChannel(toJoin);
                            }));
                    CommandManager.AddCommand(new CommandStub("disconnect", "Disconnects from a voice channel.", "", PermissionType.Owner, cmdArgs =>
                    {
                        if (client.ConnectedToVoice())
                            client.DisconnectFromVoice();
                    }));
                    CommandManager.AddCommand(new CommandStub("khaled", "Anotha one.", "", cmdArgs =>
                    {
                        cmdArgs.Channel.SendMessage("Anotha one.\n***Bless up***");
                    }));
                    CommandManager.AddCommand(new CommandStub("test", "Tests", "", cmdArgs =>
                    {
                        cmdArgs.Channel.SendMessage("Hey whassup hello.");
                    }));

                    DiscordMember owner = client.GetServersList().Find(x => x.GetMemberByUsername("Axiom") != null).GetMemberByUsername("Axiom");
                    //CommandManager.AddPermission(owner, PermissionType.Owner);
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
                    
                    vc.SetSpeaking(true);
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
