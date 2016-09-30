using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;

namespace DSharpPlus
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordClient client = new DiscordClient("toekn", true);

            Console.WriteLine("Attempting to connect!");

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
                Console.WriteLine("CLIENT CONNECTED");@
            };
            client.MessageReceived += (sender, e) => // Channel message has been received
            {
                if (!e.Message.Author.IsBot)
                {
                    e.Channel.SendMessage(e.MessageText);
                }
            };

            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
