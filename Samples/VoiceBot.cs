using DSharpPlus;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot
{
    class Program
    {
        DiscordClient client;
        AudioTools audio;

        static void Main(string[] args) => new Program().Run(args);

        public void Run(string[] args)
        {
            Login();

            string input;
            while (true)
            {
                input = Console.ReadLine();

                if (input == "")
                {

                }
            }
        }

        public void Commands()
        {
            client.CommandPrefixes.Add("test.");

            client.AddCommand(DSharpPlus.Commands.DiscordCommand.Create("uinfo")
                .Do(async e =>
                {
                    string userInfo = $"```Fix{Environment.NewLine}Username: {e.Member.Username}{Environment.NewLine}Username: {e.Member.Username}{Environment.NewLine}```";

                    await e.Channel.SendMessageAsync(userInfo);
                })
                );

            client.AddCommand(DSharpPlus.Commands.DiscordCommand.Create("voice")
                .Do(async e =>
                {
                    try
                    {
                        DSharpPlus.Objects.DiscordChannel voiceChannel = client.GetChannelByID(226700492548472832);

                        client.ConnectToVoiceChannel(voiceChannel, new DiscordVoiceConfig()
                        {
                            Channels = 2,
                            FrameLengthMs = 60,
                            OpusMode = DSharpPlus.Voice.OpusApplication.MusicOrMixed,
                            SendOnly = false
                        }, false, false);

                        await e.Channel.SendMessageAsync($"Trying to connect");

                    }
                    catch (Exception ex) { Console.WriteLine($"{ex.Message} ({ex.Source}): {Environment.NewLine}{ex.StackTrace}"); }
                })
                );
            client.AddCommand(DSharpPlus.Commands.DiscordCommand.Create("play")
                .Do(async e =>
                {
                    try
                    {
                        audio.SendAudioFile(e.Message.Content.Split(new char[] { ' ' })[1]);
                        await e.Channel.SendMessageAsync($"Playing audiofile ``{e.Message.Content.Split(new char[] { ' ' })[1]}``");
                    }
                    catch (Exception ex) { Console.WriteLine($"{ex.Message} ({ex.Source}): {Environment.NewLine}{ex.StackTrace}"); }
                })
                );
        }

        /*public void MessageReceived(object sender, DiscordAudioPacketEventArgs e)
        {
            Console.WriteLine($"Received VoicePacket: By {e.FromUser.Username} in {e.Channel.Name} [Length: {e.OpusAudioLength}]");

            byte[] potential = new byte[4000];
            int decodedFrames = client.GetVoiceClient().Decoder.DecodeFrame(e.OpusAudio, 0, e.OpusAudioLength, potential);
            //client.GetVoiceClient().SendVoice(potential);
            NAudio.Wave.BufferedWaveProvider

            //client.EchoPacket(new DiscordAudioPacket(e.OpusAudio));
        }

        public void ConfigVoiceClient()
        {
            Console.WriteLine($"Waiting for vClient != null");
            while((vClient = client.GetVoiceClient()) == null) { }

            //vClient.PacketReceived += (sender, e) => Console.WriteLine($"Received packet by {e.FromUser.Username}#{e.FromUser.Discriminator} in {e.Channel.Name} [Length: {e.OpusAudioLength}]");
        }

        public void SendAudio(string fileName)
        {
            DiscordVoiceClient vc = client.GetVoiceClient();
            try
            {
                int ms = vc.VoiceConfig.FrameLengthMs;
                int channels = vc.VoiceConfig.Channels;
                int sampleRate = 48000;

                int blockSize = 48 * 2 * channels * ms;
                byte[] buffer = new byte[blockSize];
                var outFormat = new WaveFormat(sampleRate, 16, channels);

                vc.SetSpeaking(true);

                using (var wavReader = new WaveFileReader(fileName))
                using (var resampler = new MediaFoundationResampler(wavReader, outFormat))
                {
                    resampler.ResamplerQuality = 60;

                    int byteCount;
                    while((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                    {
                        if (vc.Connected)
                            vc.SendVoice(buffer);
                        else
                            break;
                    }

                    resampler.Dispose();
                    wavReader.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }*/

        public void Login()
        {
            Console.WriteLine($"Enter Bottoken or user credentials");
            string input = Console.ReadLine();
            string[] parts = input.Split(new char[] { ' ' });
            if (parts.Count() == 1)
            {
                client = new DiscordClient(parts[0], true, true);
            }
            else if (parts.Count() == 2)
            {
                client = new DiscordClient(DSharpPlus.Toolbox.Tools.getUserToken(parts[0], parts[1]), false, true);
            }
            else Login();

            client.GetTextClientLogger.EnableLogging = true;
            client.GetTextClientLogger.LogMessageReceived += (sender, e) => Console.WriteLine($"{e.message.TimeStamp} {e.message.Level.ToString()}: {e.message.Message}");

            //client.MessageReceived += (sender, e) => Console.WriteLine($"{e.Message.timestamp} {e.Author.Username}#{e.Author.Discriminator}: {e.Message.Content}");
            client.VoiceClientConnected += (sender, e) =>
            {
                Console.WriteLine($"Connected to voice");

                //bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            };
            //client.AudioPacketReceived += (sender, e) => MessageReceived(sender, e);
            client.Connected += (sender, e) => audio = new AudioTools(client);
            client.UserSpeaking += (sender, e) => Console.WriteLine($"[VOICE] {e.UserSpeaking.Username}: {e.Speaking.ToString()}");

            Console.Clear();

            Commands();

            client.SendLoginRequest();
            client.Connect();
        }
    }
}
