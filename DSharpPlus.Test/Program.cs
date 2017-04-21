using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Test
{
    internal sealed class Program
    {
        public static void Main(string[] args) 
            => new Program().Run(args).GetAwaiter().GetResult();

        private async Task Run(string[] args)
        {
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

            var bot = new TestBot(cfg);
            await bot.RunAsync();
        }
    }
}
