using DiscordSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordSharpTestApplication
{
    class Program
    {
        static DiscordClient client = new DiscordClient();

        static void InputThread()
        {
            bool accept = true;
            while(accept)
            {
                string input = Console.ReadLine();
                client.SendMessageToChannel(input, "general", "Super Mario Bros X");
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("DiscordSharp Tester");
            client.LoginInformation = new DiscordLoginInformation();
            Console.Write("Please enter your email: ");
            string email = Console.ReadLine();
            client.LoginInformation.email[0] = email;

            Console.Write("\nNow, your password (visible): ");
            string pass = Console.ReadLine();
            client.LoginInformation.password[0] = pass;

            Console.WriteLine("Attempting login..");
            try
            {
                Thread input = new Thread(InputThread);
                input.Start();

                client.MessageReceived += (sender, e) =>
                {
                    Console.WriteLine("[- Message from {0} in {1} on {2}: {3}", e.username, e.ChannelName, e.ServerName, e.message);
                    if (e.message.StartsWith("?status"))
                        client.SendMessageToChannel("I work ;)", e.ChannelName, e.ServerName);
                };
                client.SendLoginRequest();
                Thread tt = new Thread(client.ConnectAndReadMessages);
                tt.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
