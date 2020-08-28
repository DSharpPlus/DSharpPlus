using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

            var tskl = new List<Task>();
            for (var i = 0; i < cfg.ShardCount; i++)
            {
                var bot = new TestBot(cfg, i);
                Shards.Add(bot);
                tskl.Add(bot.RunAsync());
                await Task.Delay(7500).ConfigureAwait(false);
            }
            
            await Task.WhenAll(tskl).ConfigureAwait(false);

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
    }
}
