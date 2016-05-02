using DiscordSharp;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
using System.Diagnostics;
using Luigibot;

namespace DiscordSharpTestApplication
{
    internal class AudioPlayer
    {
        WaveCallbackInfo callbackInfo;
        WaveOut outputDevice;
        BufferedWaveProvider bufferedWaveProvider;
        DiscordVoiceConfig config;

        public AudioPlayer(DiscordVoiceConfig __config)
        {
            config = __config;
            callbackInfo = WaveCallbackInfo.FunctionCallback();
            outputDevice = new WaveOut(callbackInfo);
            bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, config.Channels));
        }

        public void EnqueueBytes(byte[] bytes)
        {
            bufferedWaveProvider.AddSamples(bytes, 0, bytes.Length);
        }

        public Task PlayAudio()
        {
            return Task.Run(() =>
            {
                outputDevice.Init(bufferedWaveProvider);
                outputDevice.Play();
            });
        }

        public void StopAudio()
        {
            outputDevice.Stop();
        }
    }

    public class Program
    {
        static DiscordClient client = new DiscordClient();
        static DiscordMember owner;
        static AudioPlayer audioPlayer;
        static WaitHandle waitHandle = new AutoResetEvent(false);
        //static LastAuth lastfmAuthentication = new LastAuth("4de0532fe30150ee7a553e160fbbe0e0", "0686c5e41f20d2dc80b64958f2df0f0c");
        static bool repeatVoice;

        static string[] KhaledQuotes = new string[]
        {
            "Always have faith. Always have hope.",
            "The key is to make it.",
            "Another one.",
            "Key to success is clean heart and clean face.",
            "Smh they get mad when you have joy.",
            "Baby, you smart. I want you to film me taking a shower.",
            "You smart! You loyal! You a genius!",
            "Give thanks to the most high.",
            "They will try to close the door on you, just open it.",
            "They don’t want you to have the No. 1 record in the country.",
            "Those that weather the storm are the great ones.",
            "The key to success is more cocoa butter.",
            "I changed... a lot.",
            "My fans expect me to be greater and keep being great.",
            "There will be road blocks but we will overcome it.",
            "They don\"t want you to jet ski.",
            "Them doors that was always closed, I ripped the doors off, took the hinges off. And when I took the hinges off, I put the hinges on the f*ckboys’ hands.",
            "Congratulations, you played yourself.",
            "Don\"t play yourself.",
            "Another one, no. Another two, drop two singles at a time.",
        };
        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static Random rng = new Random(DateTime.Now.Millisecond);

        static void WriteDebug(LogMessage m, string prefix)
        {
            if (m.Level == MessageLevel.Unecessary)
                return;
            switch (m.Level)
            {
                case MessageLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case MessageLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                case MessageLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
            Console.Write($"[{prefix}: {m.TimeStamp}:{m.TimeStamp.Millisecond}]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(m.Message + "\n");
            Console.BackgroundColor = ConsoleColor.Black;
        }

        [STAThread]
        public static void Main(string[] args)
        {            
            LuigibotMain luigibot = new LuigibotMain();
            luigibot.RunLuigibot();

            string output = "";
            while((output = Console.ReadLine()) != null)
            {
                if (output == "")
                {
                    luigibot.actuallyExit = true;
                    luigibot.Exit();
                }
            }
        }

    }
}