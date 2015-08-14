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
                client.SendMessageToChannel(input, "testing", "Discord API");
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
                    else if (e.message.StartsWith("?whereami"))
                    {
                        DiscordServer server = client.ServerFromID(e.ServerID);
                        string owner = "";
                        foreach (var member in server.members)
                            if (member.user.id == server.owner_id)
                                owner = member.user.username;
                        string whereami = String.Format("I am currently in *#{0}* ({1}) on server *{2}* ({3}) owned by {4}.", e.ChannelName, e.ChannelID, e.ServerName, e.ServerID, owner);
                        client.SendMessageToChannel(whereami, e.ChannelName, e.ServerName);
                    }
                    else if (e.message.StartsWith("?quoththeraven"))
                        client.SendMessageToChannel("nevermore", e.ChannelName, e.ServerName);
                    else if (e.message.StartsWith("?quote"))
                        client.SendMessageToChannel("Luigibot does what Reta don't.", e.ChannelName, e.ServerName);
                    else if (e.message.StartsWith("?selfdestruct"))
                    {
                        if (e.username == "Axiom")
                            client.SendMessageToChannel("riparoni and cheese", e.ChannelName, e.ServerName);

                        Environment.Exit(0);
                    }
                };
                client.Connected += (sender, e) =>
                {
                    Console.WriteLine("Connected! User: " + e.username);
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
