using DiscordSharp;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
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
                //client.SendMessageToChannel(input, "testing", "Discord API");
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

                client.ChannelCreated += (sender, e) =>
                {
                        var parentServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.ChannelCreated.id) != null);
                        if (parentServer != null)
                            Console.WriteLine("Channel {0} created in {1}!", e.ChannelCreated.name, parentServer.name);
                };
                client.PrivateChannelCreated += (sender, e) =>
                {
                    Console.WriteLine("Private channel started with {0}", e.ChannelCreated.recipient.username);
                };

                client.MessageReceived += (sender, e) =>
                {
                    DiscordServer fromServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                        Console.WriteLine("[- Message from {0} in {1} on {2}: {3}", e.username, e.Channel.name, fromServer.name, e.message);
                        if (e.message.StartsWith("?status"))
                            client.SendMessageToChannel("I work ;)", e.Channel);
                        else if (e.message.StartsWith("?whereami"))
                        {
                            DiscordServer server = client.ServerFromID(client.GetServersList().Find(x=>x.channels.Find(y=>y.id == e.Channel.id) != null).id);
                            string owner = "";
                            foreach (var member in server.members)
                                if (member.user.id == server.owner_id)
                                    owner = member.user.username;
                            string whereami = String.Format("I am currently in *#{0}* ({1}) on server *{2}* ({3}) owned by {4}.", e.Channel.name, e.Channel.id, server.name, server.id, owner);
                            client.SendMessageToChannel(whereami, e.Channel);
                        }
                        else if (e.message.StartsWith("?lastfm"))
                        {
                            string[] split = e.message.Split(new char[] { ' ' }, 2);
                            if (split.Length > 1)
                            {
                                using (var lllfclient = new LastfmClient("4de0532fe30150ee7a553e160fbbe0e0", "0686c5e41f20d2dc80b64958f2df0f0c", null, null))
                                {
                                    try
                                    {
                                        var recentScrobbles = lllfclient.User.GetRecentScrobbles(split[1], null, 0, 1);
                                        LastTrack lastTrack = recentScrobbles.Result.Content[0];
                                        client.SendMessageToChannel(string.Format("*{0}* last listened to _{1}_ by _{2}_", split[1], lastTrack.Name, lastTrack.ArtistName), e.Channel);
                                    }
                                    catch
                                    {
                                        client.SendMessageToChannel(string.Format("User _*{0}*_ not found!", split[1]), e.Channel);
                                    }
                                }
                            }
                            else
                                client.SendMessageToChannel("Who??", e.Channel);
                        }
                        else if (e.message.StartsWith("?quoththeraven"))
                            client.SendMessageToChannel("nevermore", e.Channel);
                        else if (e.message.StartsWith("?quote"))
                            client.SendMessageToChannel("Luigibot does what Reta don't.", e.Channel);
                        else if (e.message.StartsWith("?selfdestruct"))
                        {
                            if (e.username == "Axiom")
                                client.SendMessageToChannel("riparoni and cheese", e.Channel);

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
