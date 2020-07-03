using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace DSharpPlus.Test
{
    internal sealed class Program
    {
        public static CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();
        private static CancellationToken CancelToken => CancelTokenSource.Token;
        private static List<TestBot> Shards { get; } = new List<TestBot>();

        public static void Main(string[] args)
            => MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task MainAsync(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            var cfg = new TestBotConfig();
            var json = string.Empty;
            if (!File.Exists("config.json"))
            {
                json = JsonConvert.SerializeObject(cfg);
                File.WriteAllText("config.json", json, new UTF8Encoding(false));
                Console.WriteLine("Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
                Console.ReadKey();

                return;
            }

            json = File.ReadAllText("config.json", new UTF8Encoding(false));
            cfg = JsonConvert.DeserializeObject<TestBotConfig>(json);

            var client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = cfg.Token,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,
                ShardCount = 10
            });

            client.MessageCreated += async e =>
            {
                Console.WriteLine(e.Message.Content);
            };

            await client.StartAsync();
            await client.StopAsync();

            /*
            var tskl = new List<Task>();
            for (var i = 0; i < cfg.ShardCount; i++)
            {
                var bot = new TestBot(cfg, i, ref test);
                Shards.Add(bot);
                await bot.RunAsync();
                //await Task.Delay(7500).ConfigureAwait(false);
            }
            
            await Task.WhenAll(tskl).ConfigureAwait(false);
            */
            try
            {
                await Task.Delay(-1, CancelToken).ConfigureAwait(false);
            }
            catch (Exception) { /* shush */ }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            foreach (var shard in Shards)
                shard.StopAsync().GetAwaiter().GetResult(); // it dun matter

            CancelTokenSource.Cancel();
        }

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", e.Timestamp.ToLocalTime());

            var tag = e.Application;
            if (tag.Length > 12)
                tag = tag.Substring(0, 12);
            if (tag.Length < 12)
                tag = tag.PadLeft(12, ' ');
            Console.Write("[{0}] ", tag);

            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.Write("[{0}] ", string.Concat("SHARD ", this.Discord.ShardId.ToString("00")));

            switch (e.Level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                    
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
            }
            Console.Write("[{0}] ", e.Level.ToString().PadLeft(8));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(e.Message);
        }
    }
}
