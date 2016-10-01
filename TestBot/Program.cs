using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;

namespace DSharpPlus
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordClient client = new DiscordClient("MTk3MDcyNjMwMjY2ODU1NDI1.Cr4tag.43eJye2qX_C4ByW4bqvmXMHY2KQ", true);

            Console.WriteLine("Attempting to connect!");
            client.CommandPrefixes.Add(";kaori");
            client.AddCommand(DiscordCommand.Create("testbot")
                .AddParameter("words")
                .Do(async e =>
                {
                    await e.Channel.SendMessageAsync(e.GetArg(1));
                }
                ));

            try
            {
                client.SendLoginRequest();
                client.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            client.Connected += (sender, e) =>
            {
                Console.WriteLine("CLIENT CONNECTED");
            };
            client.MessageReceived += (sender, e) => // Channel message has been received
            {
                if (e.MessageText == "afroraydude is the most awesome person in the world")
                {
                    e.Channel.SendMessage(e.MessageText);
                }
            };

            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
