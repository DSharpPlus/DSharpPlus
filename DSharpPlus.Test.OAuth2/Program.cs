using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.OAuth2;
using Newtonsoft.Json;

namespace DSharpPlus.Test.OAuth2
{
    internal class Program
    {
        static void Main(string[] args)
            => Task.Run(async () =>
            {
                await MainAsync(args);
            }).Wait();

        static async Task MainAsync(string[] args)
        {
            var cfg = new OAuth2TestConfig();
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
            cfg = JsonConvert.DeserializeObject<OAuth2TestConfig>(json);

            (DiscordOAuth2Client client, DiscordTokenResponse tokens) = await DiscordOAuth2Client.FromClientCredentialsAsync(new DiscordOAuth2Config()
            {
                ClientId = cfg.ClientId,
                ClientSecret = cfg.ClientSecret,
                RedirectUri = "http://localhost",
                RefreshToken = ""
            },
            DiscordScopes.Email | DiscordScopes.Identify);

            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("The following lines contain sensitive data! Know what you screenshot or share! Blur out sensitive data!");
            Console.ResetColor();

            Console.WriteLine(tokens.RefreshToken);
            Console.WriteLine(tokens.AccessToken);
            Console.WriteLine(tokens.ExpiresIn);
            Console.WriteLine(tokens.TokenType);
            Console.WriteLine(tokens.Scopes);
        }
    }
}
