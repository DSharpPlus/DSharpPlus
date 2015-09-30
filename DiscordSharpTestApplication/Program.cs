using DiscordSharp;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordSharpTestApplication
{
    class Program
    {
        static DiscordClient client = new DiscordClient();

        public static void Main(string[] args)
        {
            Console.WriteLine("DiscordSharp Tester");
            client.ClientPrivateInformation = new DiscordUserInformation();
            Console.Write("Please enter your email: ");
            string email = Console.ReadLine();
            client.ClientPrivateInformation.email = email;

            Console.Write("Now, your password (visible): ");
            string pass = Console.ReadLine();
            client.ClientPrivateInformation.password = pass;

            Console.WriteLine("Attempting login..");

            client.DebugMessageReceived += (sender, e) =>
            {
                client.SendMessageToUser("[DEBUG MESSAGE]: " + e.message, client.GetServersList().Find(x => x.members.Find(y => y.user.username == "Axiom") != null).members.Find(x => x.user.username == "Axiom"));
            };
            client.UnknownMessageTypeReceived += (sender, e) =>
            {
                using (var sw = new StreamWriter(e.RawJson["t"].ToString() + ".txt"))
                {
                    sw.WriteLine(e.RawJson);
                }
                client.SendMessageToUser("Heya, a new message type, '" + e.RawJson["t"].ToString() + "', has popped up!", client.GetServersList().Find(x=>x.members.Find(y=>y.user.username == "Axiom") != null).members.Find(x=>x.user.username == "Axiom"));
            };
            client.VoiceStateUpdate += (sender, e) =>
            {
                Console.WriteLine("***Voice State Update*** User: " + e.user.user.username);
            };
            client.GuildCreated += (sender, e) =>
            {
                Console.WriteLine("Server Created: " + e.server.name);
            };
            client.MessageEdited += (sender, e) =>
            {
                if (e.author.user.username == "Axiom")
                    client.SendMessageToChannel("What the fuck, <@" + e.author.user.id + "> you can't event type your message right. (\"" + e.MessageEdited.content + "\")", e.Channel);
            };
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
            client.PrivateMessageReceived += (sender, e) =>
            {
                client.SendMessageToUser("Pong!", e.author);
            };
            client.MentionReceived += (sender, e) =>
            {
                if (e.author.user.id != client.Me.user.id)
                    client.SendMessageToChannel("Heya, @" + e.author.user.username, e.Channel);
            };
            client.MessageReceived += (sender, e) =>
            {
                DiscordServer fromServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                Console.WriteLine("[- Message from {0} in {1} on {2}: {3}", e.author.user.username, e.Channel.name, fromServer.name, e.message.content);
                if (e.message.content.StartsWith("?status"))
                    client.SendMessageToChannel("I work ;)", e.Channel);
                else if (e.message.content.StartsWith("?notify"))
                {
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                }
                else if (e.message.content.StartsWith("?whereami"))
                {
                    DiscordServer server = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                    string owner = "";
                    foreach (var member in server.members)
                        if (member.user.id == server.owner_id)
                            owner = member.user.username;
                    string whereami = String.Format("I am currently in *#{0}* ({1}) on server *{2}* ({3}) owned by {4}. The channel's topic is: {5}", e.Channel.name, e.Channel.id, server.name, server.id, owner, e.Channel.topic);
                    client.SendMessageToChannel(whereami, e.Channel);
                }
                else if(e.message.content.StartsWith("?test_game"))
                {
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    if(split.Length > 0)
                    {
                        client.UpdateCurrentGame(int.Parse(split[1]));
                    }
                }
                else if(e.message.content.StartsWith("?gtfo"))
                {
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    if(split.Length > 1)
                    {
                        DiscordServer curServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == split[1]) != null);
                        if (curServer != null)
                        {
                            client.SendMessageToChannel("Leaving server " + curServer.name, e.Channel);
                            client.LeaveServer(curServer.id);
                        }
                    }
                    else
                    {
                        DiscordServer curServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                        client.SendMessageToChannel("Bye!", e.Channel);
                        client.LeaveServer(curServer.id);
                    }
                }
                else if (e.message.content.StartsWith("?everyone"))
                {
                    DiscordServer server = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    if (split.Length > 1)
                    {
                        string message = "";
                        foreach (var user in server.members)
                        {
                            if (user.user.id == client.Me.user.id)
                                continue;
                            if (user.user.username == "Blank")
                                continue;
                            message += "@" + user.user.username + " ";
                        }
                        message += ": " + split[1];
                        client.SendMessageToChannel(message, e.Channel);
                    }
                }
                else if (e.message.content.StartsWith("?lastfm"))
                {
#if __MONOCS__
                        client.SendMessageToChannel("Sorry, not on Mono :(", e.Channel);
#else
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    if (split.Length > 1)
                    {
                        using (var lllfclient = new LastfmClient("4de0532fe30150ee7a553e160fbbe0e0", "0686c5e41f20d2dc80b64958f2df0f0c", null, null))
                        {
                            try
                            {
                                
                                var recentScrobbles = lllfclient.User.GetRecentScrobbles(split[1], null, 1, 1);
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
#endif
                }
                else if(e.message.content.StartsWith("?rename"))
                {
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    if(split.Length > 0)
                    {
                        //client.ChangeBotUsername(split[1]);
                        DiscordUserInformation newUserInfo = client.ClientPrivateInformation;
                        newUserInfo.username = split[1].ToString();
                        client.ChangeBotInformation(newUserInfo);
                    }
                }
                else if(e.message.content.StartsWith("?changepic"))
                {
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    if(split.Length > 0)
                    {
                        Regex linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        string rawString = $"{split[1]}";
                        if (linkParser.Matches(rawString).Count > 0)
                        {
                            string url = linkParser.Matches(rawString)[0].ToString();
                            using (WebClient wc = new WebClient())
                            {
                                byte[] data = wc.DownloadData(url);
                                using (MemoryStream mem = new MemoryStream(data))
                                {
                                    using (var image = System.Drawing.Image.FromStream(mem))
                                    {
                                        client.ChangeBotPicture(new Bitmap(image));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (e.message.content.StartsWith("?whois"))
                {
                    //?whois <@01393408>
                    Regex r = new Regex("\\d+");
                    Match m = r.Match(e.message.content);
                    Console.WriteLine("WHOIS INVOKED ON: " + m.Value);
                    var foundServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                    if (foundServer != null)
                    {
                        var foundMember = foundServer.members.Find(x => x.user.id == m.Value);
                        client.SendMessageToChannel(string.Format("<@{0}>: {1}, {2}", foundMember.user.id, foundMember.user.id, foundMember.user.username), e.Channel);
                    }
                }
                else if(e.message.content.StartsWith("?prune"))
                {
                    string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    if(split.Length > 0)
                    {
                        if(split[1].Trim() == "all")
                        {
                            int messagesDeleted = client.DeleteAllMessages();
                            if(split.Length > 1 && split[2] != "nonotice")
                                client.SendMessageToChannel(messagesDeleted + " messages deleted across all channels.", e.Channel);
                        }
                        else if(split[1].Trim() == "here")
                        {
                            int messagesDeleted = client.DeleteAllMessagesInChannel(e.Channel);
                            if (split.Length > 1 && split[2] != "nonotice")
                                client.SendMessageToChannel(messagesDeleted + " messages deleted in channel '" + e.Channel.name + "'.", e.Channel);
                        }
                        else
                        {
                            DiscordChannel channelToPrune = client.GetChannelByName(split[1].Trim());
                            if(channelToPrune != null)
                            {
                                int messagesDeleted = client.DeleteAllMessagesInChannel(channelToPrune);
                                if (split.Length > 1 && split[2] != "nonotice")
                                    client.SendMessageToChannel(messagesDeleted + " messages deleted in channel '" + channelToPrune.name + "'.", e.Channel);
                            }
                        }
                    }
                    else
                    {
                        client.SendMessageToChannel("Prune what?", e.Channel);
                    }
                }
                else if (e.message.content.StartsWith("?quoththeraven"))
                    client.SendMessageToChannel("nevermore", e.Channel);
                else if (e.message.content.StartsWith("?quote"))
                    client.SendMessageToChannel("Luigibot does what Reta don't.", e.Channel);
                else if (e.message.content.StartsWith("?selfdestruct"))
                {
                    if (e.author.user.username == "Axiom")
                        client.SendMessageToChannel("restaroni in pepparoni", e.Channel);
                    Environment.Exit(0);
                }
                else if(e.message.content.StartsWith("?changetopic"))
                {
                    if(e.author.user.username == "Axiom")
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 0)
                            client.ChangeChannelTopic(split[1], e.Channel);
                    }
                }
                else if(e.message.content.StartsWith("?join"))
                {
                    if(e.author.user.username == "Axiom")
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if(split.Length > 0)
                        {
                            string substring = split[1].Substring(split[1].LastIndexOf('/') + 1);
                            //client.SendMessageToChannel(substring, e.Channel);
                            client.AcceptInvite(substring);
                        }
                    }
                }
            };
            client.Connected += (sender, e) =>
            {
                Console.WriteLine("Connected! User: " + e.user.user.username);
            };
            client.SocketClosed += (sender, e) =>
            {
                Console.WriteLine("Closed ({0}): {1}", e.Code, e.Reason);
            };
            ConnectStuff();
            while (true) ;
        }

        private static async void ConnectStuff()
        {
            if(await client.SendLoginRequestAsync() != null)
            {
                Console.WriteLine("Logged in..async!");
                client.ConnectAndReadMessages();
                client.UpdateCurrentGame(256);
            }
        }
    }
}
