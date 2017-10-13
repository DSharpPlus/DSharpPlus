using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Test
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var cfg = new TestBotConfig();
            string json;
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
                tskl.Add(bot.RunAsync());
                await Task.Delay(7500);
            }
            
            await Task.WhenAll(tskl);

            await Task.Delay(-1);
        }
    }
}
