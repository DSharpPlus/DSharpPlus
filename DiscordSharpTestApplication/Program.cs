using DiscordSharp;
using DiscordSharp.Objects;
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
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;

namespace DiscordSharpTestApplication
{
    class Program
    {
        static DiscordClient client = new DiscordClient();
        static DiscordMember owner;
        static WaitHandle waitHandle = new AutoResetEvent(false);
        static LastAuth lastfmAuthentication = new LastAuth("4de0532fe30150ee7a553e160fbbe0e0", "0686c5e41f20d2dc80b64958f2df0f0c");
        static bool repeatVoice;

        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static Random rng = new Random(DateTime.Now.Millisecond);

        static void WriteDebug(LogMessage m, string prefix)
        {
            if (m.Level == MessageLevel.Unecessary)
                return;
            switch (m.Level)
            {
                case MessageLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case MessageLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                case MessageLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
            Console.Write($"[{prefix}: {m.TimeStamp}:{m.TimeStamp.Millisecond}]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(m.Message + "\n");

            Console.BackgroundColor = ConsoleColor.Black;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                quitEvent.Set();
                e.Cancel = true;
            };

            Console.WriteLine("\t\t\tDiscordSharp Tester");
            client.ClientPrivateInformation = new DiscordUserInformation();
#if DEBUG
            client.WriteLatestReady = true;
#endif

            if (File.Exists("credentials.txt"))
            {
                using (StreamReader sr = new StreamReader("credentials.txt"))
                {
                    client.ClientPrivateInformation.email = sr.ReadLine();
                    client.ClientPrivateInformation.password = sr.ReadLine();
                }
            }
            else
            {
                Console.Write("Please enter your email: ");
                string email = Console.ReadLine();
                client.ClientPrivateInformation.email = email;

                Console.Write("Now, your password (visible): ");
                string pass = Console.ReadLine();
                client.ClientPrivateInformation.password = pass;
            }
            client.RequestAllUsersOnStartup = true;

            Console.WriteLine("Attempting login..");

            var worker = new Thread(() =>
            {
                client.VoiceClientConnected += (sender, e) =>
                {
                    owner.SlideIntoDMs("Voice client connection complete.");
                };
                client.TextClientDebugMessageReceived += (sender, e) =>
                {
                    if (e.message.Level == MessageLevel.Error || e.message.Level == MessageLevel.Error || e.message.Level == MessageLevel.Warning)
                    {
                        WriteDebug(e.message, "Text Client");
                        if (client.GetServersList() != null)
                        {
                            DiscordMember onwer = client.GetServersList().Find(x => x.members.Find(y => y.Username == "Axiom") != null).members.Find(x => x.Username == "Axiom");
                            if (onwer != null)
                                if (!e.message.Message.Contains("Setting current game"))
                                    onwer.SendMessage($"**LOGGING**\n```\n[Text Client: {e.message.Level}]: {e.message.Message}\n```");
                        }
                    }
                };
                client.VoiceClientDebugMessageReceived += (sender, e) =>
                {
                    WriteDebug(e.message, "Voice Debug");
                };
                client.RoleDeleted += (sender, e) =>
                {
                    Console.WriteLine($"Role '{e.DeletedRole.name}' deleted.");
                };
                client.UnknownMessageTypeReceived += (sender, e) =>
                {
                    using (var sw = new StreamWriter(e.RawJson["t"].ToString() + ".txt"))
                    {
                        sw.WriteLine(e.RawJson);
                    }
                    client.SendMessageToUser("Heya, a new message type, '" + e.RawJson["t"].ToString() + "', has popped up!", client.GetServersList().Find(x => x.members.Find(y => y.Username == "Axiom") != null).members.Find(x => x.Username == "Axiom"));
                };
                client.AudioPacketReceived += (sender, e) =>
                {
                    //client.EchoPacket(e.Packet);
                };
                client.VoiceStateUpdate += (sender, e) =>
                {
                    Console.WriteLine("***Voice State Update*** User: " + e.user.Username);
                };
                client.GuildCreated += (sender, e) =>
                {
                    Console.WriteLine("Server Created: " + e.server.name);
                };
                client.MessageEdited += (sender, e) =>
                {
                    Console.WriteLine(e.message);
                    //if (e.author.user.username == "Axiom")
                    //    client.SendMessageToChannel("What the fuck, <@" + e.author.user.id + "> you can't event type your message right. (\"" + e.MessageEdited.content + "\")", e.Channel);
                };
                client.ChannelCreated += (sender, e) =>
                {
                    var parentServer = client.GetServersList().Find(x => x.channels.Find(y => y.ID == e.ChannelCreated.ID) != null);
                    if (parentServer != null)
                        Console.WriteLine("Channel {0} created in {1}!", e.ChannelCreated.Name, parentServer.name);
                };
                client.PrivateChannelDeleted += (sender, e) =>
                {
                    Console.WriteLine("Private channel deleted with " + e.PrivateChannelDeleted.recipient.Username);
                };
                client.PrivateChannelCreated += (sender, e) =>
                {
                    Console.WriteLine("Private channel started with {0}", e.ChannelCreated.recipient.Username);
                };
                client.PrivateMessageReceived += (sender, e) =>
                {
                    client.SendMessageToUser("Pong!", e.author);
                };
                client.MentionReceived += (sender, e) =>
                {
                    string rawMessage = e.message.content;
                    string whatToSend = $"I received a mention from @{e.author.Username} in #{e.Channel.Name} in {e.Channel.parent.name}. It said: \n```\n{rawMessage}\n```";
                    if (rawMessage.Contains($"{client.Me.ID}"))
                        whatToSend += $"Where `<@{client.Me.ID}>` is my user being mentioned.";
                    DiscordMember owner = client.GetServersList().Find(x => x.members.Find(y => y.Username == "Axiom") != null).members.Find(x => x.Username == "Axiom");
                    client.SendMessageToUser(whatToSend, owner);
                };
                client.MessageDeleted += (sender, e) =>
                {
                    if (e.DeletedMessage != null)
                        Console.WriteLine($"Message {e.DeletedMessage.content} deleted");
                    else
                        Console.WriteLine("Message deleted, message was null.");
                };
                client.MessageReceived += (sender, e) =>
                {
                    DiscordServer fromServer = client.GetServersList().Find(x => x.channels.Find(y => y.ID == e.Channel.ID) != null);

                    Console.WriteLine("[- Message from {0} in {1} on {2}: {3}", e.author.Username, e.Channel.Name, fromServer.name, e.message.content);

                    if (e.message.content.StartsWith("?typemonkey"))
                    {
                        client.SimulateTyping(e.Channel);
                    }
                    else if (e.message.content.StartsWith("?unban"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            DiscordMember meInServer = e.Channel.parent.members.Find(x => x.ID == client.Me.ID);
                            bool hasPermission = false;
                            foreach (var role in meInServer.Roles)
                            {
                                if (role.permissions.HasPermission(DiscordSpecialPermissions.BanMembers) || role.permissions.HasPermission(DiscordSpecialPermissions.ManageServer))
                                {
                                    hasPermission = true;
                                    break;
                                }
                            }
                            if (hasPermission)
                                client.RemoveBan(e.Channel.parent, split[1]);
                            else
                                e.Channel.SendMessage("No permission to do so!");

                        }
                    }
                    else if (e.message.content.StartsWith("?repeatvoice"))
                    {
                        repeatVoice = !repeatVoice;
                        e.Channel.SendMessage("Repeat: " + repeatVoice);
                    }
                    else if (e.message.content.StartsWith("?getbans"))
                    {
                        var banList = client.GetBans(e.Channel.parent);
                        if (banList == null)
                        {
                            e.Channel.SendMessage("No permission!");
                            return;
                        }
                        if (banList.Count > 0)
                        {
                            string msg = $"**{banList.Count}** bans in this server.";
                            msg += "\n```";
                            for (int i = 0; i < banList.Count; i++)
                            {
                                if (i > 5)
                                    break;
                                msg += $"\n* {banList[i].Username} ({banList[i].ID})";
                            }
                            msg += "\n```";
                            e.Channel.SendMessage(msg);
                        }
                        else
                        {
                            e.Channel.SendMessage("No bans in this server!");
                        }
                    }
                    else if (e.message.content.StartsWith("?ban"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            //<@09380598340598>
                            string id = split[1].Trim(new char[] { '<', '@', '>' });
                            DiscordMember toBan = e.Channel.parent.members.Find(x => x.ID == id);
                            if (toBan != null)
                            {
                                if (client.BanMember(toBan) != null)
                                    e.Channel.SendMessage($"**{toBan.Username}** will be missed.");
                                else
                                    e.Channel.SendMessage("No permission!");
                            }
                            else
                                e.Channel.SendMessage("Ban who?");
                        }
                        else
                            e.Channel.SendMessage("Ban who?");
                    }
                    else if (e.message.content.StartsWith("?stop"))
                    {
                        if (client.ConnectedToVoice())
                        {
                            client.GetVoiceClient().ClearVoiceQueue();
                        }
                    }
                    else if (e.message.content.StartsWith("?editlast"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            DiscordMessage toEdit = client.GetLastMessageSent(e.Channel);
                            if (toEdit != null)
                            {
                                client.EditMessage(toEdit.id, split[1], e.Channel);
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?testjoinvoice"))
                    {
                        if (e.author.Username != "Axiom")
                            return;
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            DiscordChannel voiceToJoin = e.Channel.parent.channels.Find(x => x.Name.ToLower() == split[1].ToLower() && x.Type == ChannelType.Voice);
                            if (voiceToJoin != null)
                                client.ConnectToVoiceChannel(voiceToJoin);
                        }
                    }
                    else if (e.message.content.StartsWith("?disconnect"))
                    {
                        client.DisconnectFromVoice();
                    }
                    else if (e.message.content.StartsWith("?newguild"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            DiscordServer created = client.CreateGuild(split[1]);
                            DiscordChannel channel = created.channels.Find(x => x.Type == ChannelType.Text);
                            client.ChangeChannelTopic("Created with DiscordSharp test bot", channel);

                            client.SendMessageToChannel($"Join: {client.MakeInviteURLFromCode(client.CreateInvite(channel))}", e.Channel);
                        }
                    }
                    else if (e.message.content.StartsWith("?notify"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                    }
                    else if (e.message.content.StartsWith("?channeltype"))
                    {
                        e.Channel.SendMessage(e.Channel.Type.ToString());
                    }
                    else if (e.message.content.StartsWith("?whereami"))
                    {
                        DiscordServer server = client.GetServersList().Find(x => x.channels.Find(y => y.ID == e.Channel.ID) != null);
                        string owner = "";
                        foreach (var member in server.members)
                            if (member.ID == server.owner.ID)
                                owner = member.Username;
                        string whereami = String.Format("I am currently in *#{0}* ({1}) on server *{2}* ({3}) owned by @{4}. The channel's topic is: {5}", e.Channel.Name, e.Channel.ID, server.name, server.id, owner, e.Channel.Topic);
                        client.SendMessageToChannel(whereami, e.Channel);
                    }
                    else if (e.message.content.StartsWith("?makeroll"))
                    {
                        DiscordMember me = e.Channel.parent.members.Find(x => x.ID == client.Me.ID);
                        DiscordServer inServer = e.Channel.parent;

                        foreach (var role in me.Roles)
                        {
                            if (role.permissions.HasPermission(DiscordSpecialPermissions.ManageRoles))
                            {
                                DiscordRole madeRole = client.CreateRole(inServer);
                                DiscordRole newRole = madeRole.Copy();
                                newRole.name = "DiscordSharp Test Roll";
                                newRole.color = new DiscordSharp.Color("0xFF0000");
                                newRole.permissions.SetPermission(DiscordSpecialPermissions.ManageRoles);

                                client.EditRole(inServer, newRole);
                                client.SendMessageToChannel("Created test roll successfully?", e.Channel);
                                return;
                            }
                        }
                        client.SendMessageToChannel("Can't create role: no permission.", e.Channel);
                    }
                    else if (e.message.content.StartsWith("?getroles"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        //?getroles <@10394803598>
                        if (split.Length > 1)
                        {
                            Regex r = new Regex(@"(?<=<)([^>]+)(?=>)");
                            string userToGetRoles = r.Match(split[1]).Value;
                            userToGetRoles = userToGetRoles.Trim('@'); //lol
                            DiscordMember foundMember = e.Channel.parent.members.Find(x => x.ID == userToGetRoles);
                            if (foundMember != null)
                            {
                                string whatToSend = "Found roles for user **" + foundMember.Username + "**:\n```";
                                for (int i = 0; i < foundMember.Roles.Count; i++)
                                {
                                    if (i > 0)
                                        whatToSend += "\n";
                                    whatToSend += $"* {foundMember.Roles[i].name}: id={foundMember.Roles[i].id} color=0x{foundMember.Roles[i].color.ToString()}, (R: {foundMember.Roles[i].color.R} G: {foundMember.Roles[i].color.G} B: {foundMember.Roles[i].color.B}) permissions={foundMember.Roles[i].permissions.GetRawPermissions()}";

                                    string tempPermissions = "";
                                    foreach (var permissions in foundMember.Roles[i].permissions.GetAllPermissions())
                                    {
                                        tempPermissions += " " + permissions.ToString();
                                    }
                                    whatToSend += "\n\n  Friendly Permissions: " + tempPermissions;
                                }
                                whatToSend += "\n```";
                                client.SendMessageToChannel(whatToSend, e.Channel);
                            }
                            else
                            {
                                client.SendMessageToChannel("User with id `" + userToGetRoles + "` not found.", e.Channel);
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?test_game"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 0)
                        {
                            client.UpdateCurrentGame(split[1]);
                        }
                    }
                    else if (e.message.content.StartsWith("?gtfo"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            DiscordServer curServer = client.GetServersList().Find(x => x.channels.Find(y => y.ID == split[1]) != null);
                            if (curServer != null)
                            {
                                client.SendMessageToChannel("Leaving server " + curServer.name, e.Channel);
                                client.LeaveServer(curServer.id);
                            }
                        }
                        else
                        {
                            DiscordServer curServer = client.GetServersList().Find(x => x.channels.Find(y => y.ID == e.Channel.ID) != null);
                            if (curServer != null)
                            {
                                //client.SendMessageToChannel("Bye!", e.Channel);
                                client.LeaveServer(e.Channel.parent.id);
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?everyone"))
                    {
                        //DiscordServer server = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                        //string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        //if (split.Length > 1)
                        //{
                        //    string message = "";
                        //    foreach (var user in server.members)
                        //    {
                        //        if (user.ID == client.Me.ID)
                        //            continue;
                        //        if (user.user.username == "Blank")
                        //            continue;
                        //        message += "@" + user.user.username + " ";
                        //    }
                        //    message += ": " + split[1];
                        //    client.SendMessageToChannel(message, e.Channel);
                        //}
                    }
                    else if (e.message.content.StartsWith("?lastfm"))
                    {
#if __MONOCS__
                        client.SendMessageToChannel("Sorry, not on Mono :(", e.Channel);
#else
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            using (var lllfclient = new LastfmClient(lastfmAuthentication))
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
                    else if (e.message.content.StartsWith("?assignrole"))
                    {
                        DiscordServer server = e.Channel.parent;
                        DiscordMember me = server.members.Find(x => x.ID == client.Me.ID);

                        bool hasPermission = false;
                        me.Roles.ForEach(r =>
                        {
                            if (r.permissions.HasPermission(DiscordSpecialPermissions.ManageRoles))
                                hasPermission = true;
                        });
                        if (hasPermission)
                        {
                            string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                            if (split.Length > 0)
                            {
                                DiscordRole toAssign = server.roles.Find(x => x.name.ToLower().Trim() == split[1].ToLower().Trim());
                                if (toAssign != null)
                                {
                                    client.AssignRoleToMember(server, toAssign, server.members.Find(x => x.Username == "Axiom"));
                                }
                                else
                                {
                                    client.SendMessageToChannel($"Role '{split[1]}' not found!", e.Channel);
                                }
                            }
                        }
                        else
                        {
                            client.SendMessageToChannel("Cannot assign role: no permission.", e.Channel);
                        }
                    }
                    else if (e.message.content.StartsWith("?rename"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 0)
                        {
                            //client.ChangeBotUsername(split[1]);
                            DiscordUserInformation newUserInfo = client.ClientPrivateInformation;
                            newUserInfo.username = split[1].ToString();
                            client.ChangeClientInformation(newUserInfo);
                        }
                    }
                    else if (e.message.content.StartsWith("?changepic"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 0)
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
                                            client.ChangeClientAvatar(new Bitmap(image));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?changeguildpic"))
                    {
                        DiscordServer current = e.Channel.parent;
                        DiscordMember me = current.members.Find(x => x.ID == client.Me.ID);
                        foreach (var role in me.Roles)
                        {
                            if (role.permissions.HasPermission(DiscordSpecialPermissions.ManageServer))
                            {
                                string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                                if (split.Length > 0)
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
                                                    client.ChangeGuildIcon(new Bitmap(image), current);
                                                }
                                            }
                                        }
                                    }
                                }
                                return;
                            }
                        }
                        client.SendMessageToChannel("Unable to change pic: No permission.", e.Channel);
                    }
                    else if (e.message.content.StartsWith("?servericon"))
                    {
                        if (e.Channel.parent.IconURL != null)
                        {
                            e.Channel.SendMessage(e.Channel.parent.IconURL);
                        }
                    }
                    else if (e.message.content.StartsWith("?whois"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            string justID = split[1].Trim(new char[] { '<', '>', '@' });
                            DiscordMember memberToCheck = e.Channel.parent.members.Find(x => x.ID == justID.Trim());
                            if (memberToCheck != null)
                            {
                                string msg = $"User info for {memberToCheck.Username}\n```\nID: {memberToCheck.ID}\nStatus: {memberToCheck.Status}\n";
                                if (memberToCheck.CurrentGame != null)
                                    msg += $"\nCurrent Game: {memberToCheck.CurrentGame}";
                                msg += "\n```";
                                e.Channel.SendMessage(msg);
                            }
                            else
                            {
                                e.Channel.SendMessage("Couldn't find user with ID `" + justID + "`!");
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?deletelast"))
                    {
                        //client.DeleteMessage(client.GetLastMessageSent(e.Channel).id);

                    }
                    else if (e.message.content.StartsWith("?testdmdelete"))
                    {
                        var msg = client.SendMessageToUser("lmao you suck", client.GetServersList()[0].members.Find(x => x.Username == "Axiom"));
                        client.DeleteMessage(msg);
                    }
                    else if (e.message.content.StartsWith("?prune"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 0)
                        {
                            client.DeleteMultipleMessagesInChannel(e.Channel, int.Parse(split[1]));
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
                        if (e.author.Username == "Axiom")
                            client.SendMessageToChannel("restaroni in pepparoni", e.Channel);
                        Environment.Exit(0);
                    }
                    else if (e.message.content.Contains("?checkchannelperm"))
                    {
                        DiscordChannel channel = e.Channel;
                        string toSend = $"Channel Permission Overrides for #{channel.Name}\n\n```";
                        foreach (var over in channel.PermissionOverrides)
                        {
                            toSend += $"* Type: {over.type}\n";
                            if (over.type == DiscordPermissionOverride.OverrideType.member)
                                toSend += $"  Member: {over.id} ({channel.parent.members.Find(x => x.ID == over.id).Username})\n";
                            else
                                toSend += $"  Role: {over.id} ({channel.parent.roles.Find(x => x.id == over.id).name})\n";
                            toSend += $" Allowed: {over.GetAllowedRawPermissions()}\n";
                            toSend += $" Friendly:";
                            foreach (var allowed in over.GetAllAllowedPermissions())
                            {
                                toSend += " " + allowed.ToString();
                            }
                            toSend += $"\n Denied: {over.GetDeniedRawPermissions()}\n";
                            toSend += $" Friendly:";
                            foreach (var denied in over.GetAllDeniedPermissions())
                            {
                                toSend += " " + denied.ToString();
                            }
                            toSend += "\n\n";
                        }
                        toSend += "```";
                        client.SendMessageToChannel(toSend, channel);
                    }
                    else if (e.message.content.StartsWith("?createchannel"))
                    {
                        if (e.author.Username == "Axiom")
                        {
                            string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                            if (split.Length > 0)
                            {
                                client.CreateChannel(client.GetServerChannelIsIn(e.Channel), split[1], false);
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?deletechannel"))
                    {
                        if (e.author.Username == "Axiom")
                        {
                            client.DeleteChannel(e.Channel);
                        }
                    }
                    else if (e.message.content.StartsWith("?changetopic"))
                    {
                        if (e.author.Username == "Axiom")
                        {
                            string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                            if (split.Length > 0)
                                client.ChangeChannelTopic(split[1], e.Channel);
                        }
                    }
                    else if (e.message.content.StartsWith("?join"))
                    {
                        if (e.author.Username == "Axiom")
                        {
                            string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                            if (split.Length > 0)
                            {
                                string substring = split[1].Substring(split[1].LastIndexOf('/') + 1);
                                //client.SendMessageToChannel(substring, e.Channel);
                                client.AcceptInvite(substring);
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?testvoice"))
                    {
                        bool played = false;
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            if (split[1].Trim() == "list")
                            {
                                string msg = $"Available Songs\n```";
                                foreach (var file in Directory.GetFiles(Environment.CurrentDirectory))
                                {
                                    if (file.EndsWith(".mp3"))
                                        msg += "* " + Path.GetFileName(file.ToString()) + "\n";
                                }
                                msg += $"\n```";
                                e.Channel.SendMessage(msg);
                            }
                            else if (split[1].Trim() == "ran")
                            {
                                if (!client.ConnectedToVoice())
                                    return;
                                List<string> availableSongs = new List<string>();
                                foreach (var file in Directory.GetFiles(Environment.CurrentDirectory))
                                    if (file.EndsWith(".mp3"))
                                        availableSongs.Add(file);

                                string fileToPlay = availableSongs[rng.Next(availableSongs.Count - 1)];
                                e.Channel.SendMessage($"Playing `{Path.GetFileName(fileToPlay)}`");
                                played = true;
                                VoiceStuffs(client.GetVoiceClient(), fileToPlay);
                            }
                            else if (File.Exists(split[1]))
                            {
                                if (!client.ConnectedToVoice())
                                    return;
                                played = true;
                                VoiceStuffs(client.GetVoiceClient(), split[1]);
                            }
                        }
                        if (played)
                        {
                            client.VoiceQueueEmpty += (sx, ex) =>
                            {
                                if (!client.ConnectedToVoice())
                                    return;
                                List<string> availableSongs = new List<string>();
                                foreach (var file in Directory.GetFiles(Environment.CurrentDirectory))
                                    if (file.EndsWith(".mp3"))
                                        availableSongs.Add(file);

                                string fileToPlay = availableSongs[rng.Next(availableSongs.Count - 1)];
                                client.GetServersList().Find(x => x.name == "Axiom's Test Server").channels.Find(x => x.Name == "testing" && x.Type == ChannelType.Text).SendMessage($"Playing `{Path.GetFileName(fileToPlay)}`");
                                VoiceStuffs(client.GetVoiceClient(), fileToPlay);
                            };
                        }
                    }
                    else if (e.message.content.StartsWith("?eval"))
                    {
                        if (e.author.Username == "Axiom" || e.author.Username == "Uniquoooo")
                        {
                            string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                            if (split.Length > 1)
                            {
                                string whatToEval = split[1];
                                if (whatToEval.StartsWith("`") && whatToEval.EndsWith("`"))
                                    whatToEval = whatToEval.Trim('`');
                                try
                                {
                                    //var eval = EvalProvider.CreateEvalMethod<string, string>($"return \"Arg: \" + arg;");
                                    var eval = EvalProvider.CreateEvalMethod<DiscordClient, string>(whatToEval, new string[] { "DiscordSharp", "System.Threading", "DiscordSharp.Objects" }, new string[] { "DiscordSharp.dll" });
                                    string res = eval(client);
                                    Console.WriteLine(res);
                                    e.Channel.SendMessage("Result: `" + res + "`");
                                }
                                catch (Exception)
                                {
                                    string errors = "";
                                    if (EvalProvider.errors != null)
                                    {
                                        foreach (var error in EvalProvider.errors)
                                        {
                                            errors += $"{error.ToString()}\n\n";
                                        }
                                        e.Channel.SendMessage($"```\n{errors}\n```");
                                    }
                                    else
                                        e.Channel.SendMessage("Errors!");
                                    
                                }
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?statusof"))
                    {
                        string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                        if(split.Length > 1)
                        {
                            string id = split[1].Trim(new char[] { '<', '@', '>' });
                            DiscordMember member = e.Channel.parent.members.Find(x => x.ID == id);
                            if(member != null)
                            {
                                string msg = $"Status of **{member.Username}**";
                                msg += $"\n```\nStatus: " + member.Status.ToString();
                                if (member.CurrentGame != null)
                                    msg += "\nPlaying: " + member.CurrentGame;
                                msg += "\n```";
                                e.Channel.SendMessage(msg);
                            }
                        }
                    }
                    else if (e.message.content.StartsWith("?playing"))
                    {
                        DiscordMember member = e.Channel.parent.members.Find(x => x.Username == "Axiom");
                        using (var lllfclient = new LastfmClient(lastfmAuthentication))
                        {
                            try
                            {
                                client.SimulateTyping(e.Channel);
                                var recentScrobbles = lllfclient.User.GetRecentScrobbles("mrmiketheripper", null, 1, 1);
                                LastTrack lastTrack = recentScrobbles.Result.Content[0];
                                if (lastTrack.TimePlayed != null) //means the track is still playing
                                {
                                    var localTime = lastTrack.TimePlayed.Value.DateTime.ToLocalTime();
                                    if (DateTime.Now.Subtract(localTime) > (lastTrack.Duration == null ? new TimeSpan(0, 10, 0) : lastTrack.Duration))
                                        e.Channel.SendMessage($"<@{member.ID}> last played: **{lastTrack.Name}** by *{lastTrack.ArtistName}*. It was scrobbled at: {localTime} EST (-5).");
                                    else
                                        e.Channel.SendMessage($"<@{member.ID}> is now playing: **{lastTrack.Name}** by *{lastTrack.ArtistName}*.");
                                }
                                else
                                    e.Channel.SendMessage($"<@{member.ID}> is now playing: **{lastTrack.Name}** by *{lastTrack.ArtistName}*.");

                                if (e.message.content.Contains("art"))
                                {
                                    client.SimulateTyping(e.Channel);
                                    using (WebClient wc = new WebClient())
                                    {
                                        Image downloaded = Image.FromStream(new MemoryStream(wc.DownloadData(lastTrack.Images.Medium.AbsoluteUri)));
                                        if (downloaded != null)
                                        {
                                            downloaded.Save("temp.png");
                                            client.AttachFile(e.Channel, "", "temp.png");
                                            File.Delete("temp.png");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string whatToSend = $"Couldn't get Last.fm recent scrobbles for you! Exception:\n```{ex.Message}\n{ex.StackTrace}\n```\n";
                                client.SendMessageToUser(whatToSend, client.GetServersList().Find(x => x.members.Find(y => y.Username == "Axiom") != null).members.Find(x => x.Username == "Axiom"));
                            }
                        }
                    }
                };

                client.Connected += (sender, e) =>
                {
                    Console.WriteLine("Connected! User: " + e.user.Username);
                    using (var sw = new StreamWriter("credentials.txt"))
                    {
                        sw.WriteLine(client.ClientPrivateInformation.email);
                        sw.WriteLine(client.ClientPrivateInformation.password);
                        sw.Flush();
                    }
                    using (var lllfclient = new LastfmClient(lastfmAuthentication))
                    {
                        try
                        {
                            var recentScrobbles = lllfclient.User.GetRecentScrobbles("mrmiketheripper", null, 1, 1);
                            LastTrack lastTrack = recentScrobbles.Result.Content[0];
                            string newGame = $"{lastTrack.Name} by {lastTrack.ArtistName}";
                            if (newGame != client.GetCurrentGame)
                                client.UpdateCurrentGame(newGame);
                        }
                        catch (Exception ex)
                        {
                            string whatToSend = $"Couldn't get Last.fm recent scrobbles for you! Exception:\n```{ex.Message}\n{ex.StackTrace}\n```\n";
                            client.SendMessageToUser(whatToSend, client.GetServersList().Find(x => x.members.Find(y => y.Username == "Axiom") != null).members.Find(x => x.Username == "Axiom"));
                        }
                    }

                    owner = client.GetServersList().Find(x => x.members.Find(y => y.Username == "Axiom") != null).members.Find(x => x.Username == "Axiom");
                };
                client.SocketClosed += (sender, e) =>
                {
                    Console.WriteLine("Closed ({0}): {1}", e.Code, e.Reason);
                };
                //ConnectStuff();
                //while (true) ;
                if (client.SendLoginRequest() != null)
                {
                    Console.WriteLine("Logged in!");

                    client.Connect();
                    Console.WriteLine($"Connected to {client.CurrentGatewayURL}");
                    client.UpdateCurrentGame("development testing");


                }
            });
            worker.Start();

            System.Timers.Timer lastfmUpdateTimer = new System.Timers.Timer(25 * 1000); //check last.fm every 25 seconds
            lastfmUpdateTimer.Elapsed += (sender, e) =>
            {
                using (var lllfclient = new LastfmClient(lastfmAuthentication))
                {
                    try
                    {
                        var recentScrobbles = lllfclient.User.GetRecentScrobbles("mrmiketheripper", null, 1, 1);
                        LastTrack lastTrack = recentScrobbles.Result.Content[0];
                        if (lastTrack.TimePlayed != null)
                        {
                            var localTime = lastTrack.TimePlayed.Value.DateTime.ToLocalTime();
                            if (DateTime.Now.Subtract(localTime) > new TimeSpan(0, 15, 0))
                            {
                                if (client.GetCurrentGame != "")
                                    if (client.GetCurrentGame == null)
                                        client.UpdateCurrentGame("");
                            }
                        }
                        else
                        {
                            string newGame = $"{lastTrack.Name} by {lastTrack.ArtistName}";
                            if (newGame != client.GetCurrentGame)
                                client.UpdateCurrentGame(newGame);
                        }

                    }
                    catch (Exception ex)
                    {
                        string whatToSend = $"Couldn't get Last.fm recent scrobbles for you! Exception:\n```{ex.Message}\n{ex.StackTrace}\n```\n";
                        client.SendMessageToUser(whatToSend, client.GetServersList().Find(x => x.members.Find(y => y.Username == "Axiom") != null).members.Find(x => x.Username == "Axiom"));
                    }
                }
            };
            lastfmUpdateTimer.Start();

            InputCheck();
            //System.Windows.Forms.Application.Run();
            /**
            Since this is a test app and people actually do read my code...
            I'd like to say one thing:
            If you're not going to be doing any input accepting **INSIDE OF THE CONSOLE** you don't have to setup 
            a seperate thread for the client and accept input on main. This is why I've only commented out
            the System.Windows.Forms.Application.Run(); part instead of removing it.

            If you're not accepting input, you can just run client on the main thread and run that 
            System.Windows.Forms.Application.Run(); snippet and this will keep the app alive, with no CPU 
            hit!
            */

            if (client.GetTextClientLogger.LogCount > 0)
            {
                client.GetTextClientLogger.Save($"log-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year} {DateTime.Now.Hour} {DateTime.Now.Minute}.log");
                Console.WriteLine("Wrote log.");
            }
            if (client.GetLastVoiceClientLogger != null && client.GetLastVoiceClientLogger.LogCount > 0)
            {
                client.GetLastVoiceClientLogger.Save($"voicelog-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year} {DateTime.Now.Hour} {DateTime.Now.Minute}.log");
                Console.WriteLine("Wrote voice log");
            }

            quitEvent.WaitOne();
            client.Dispose();
        }

        private static Task InputCheck()
        {
            return Task.Run(() => 
            {
                string input;
                do
                {
                    input = Console.ReadLine();
                    if (input == null)
                        break;
                    if (input.Contains("?changepass"))
                    {
                        Console.Write("Please enter the new password (will be visible): ");
                        string newPass = Console.ReadLine();
                        DiscordUserInformation i = client.ClientPrivateInformation.Copy();
                        i.password = newPass;
                        client.ChangeClientInformation(i);

                        Console.WriteLine("Password changed!");
                    }
                    else if (input.Contains("?uploadtest"))
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Title = "Select file to attach";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            DiscordChannel c = client.GetServersList().Find(x => x.name.Contains("Discord API")).channels.Find(x => x.Name.Contains("dotnet_discord-net"));
                            client.AttachFile(c, "Test", ofd.FileName);
                        }
                    }
                    else if (input.Contains("?logout"))
                    {
                        client.Logout();
                        break;
                    }
                    else if (input.Contains("?idle"))
                    {
                        client.UpdateBotStatus(true);
                    }
                    else if (input.Contains("?online"))
                    {
                        client.UpdateBotStatus(false);
                    }
                    else if (input.Contains("?testvoice"))
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Title = "Select file to attach";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            DiscordVoiceClient vc = client.GetVoiceClient();
                            if (vc != null)
                            {
                                VoiceStuffs(vc, ofd.FileName);
                            }
                        }
                    }
                    else if (input.Contains("?stopvoice"))
                    {
                        client.GetVoiceClient().ClearVoiceQueue();
                    }
                    else if (input.Contains("?game"))
                    {
                        string[] split = input.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            try
                            {
                                client.UpdateCurrentGame(split[1]);
                            }
                            catch (Exception ex)
                            { Console.WriteLine($"Error changing game: {ex.Message}"); }
                        }
                        else
                        {
                            client.UpdateCurrentGame("");
                        }
                    }
                    else if (input.Contains("?lastmsgid"))
                    {
                        DiscordMessage msg = client.GetLastMessageSent();
                        Console.WriteLine("--Last Message Sent--");

                        DiscordChannel channel = Convert.ChangeType(msg.Channel(), typeof(DiscordChannel));
                        Console.WriteLine($"  ID: {msg.id}\n  Channel: {channel.Name}\n  Content: {msg.content}");
                    }
                } while (!string.IsNullOrWhiteSpace(input));
            });
        }

        private static void VoiceStuffs(DiscordVoiceClient vc, string file)
        {
            try
            {
                int ms = 60;
                int channels = 1;
                int sampleRate = 48000;

                int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
                byte[] buffer = new byte[blockSize];
                var outFormat = new WaveFormat(sampleRate, 16, channels);
                
                vc.SetSpeaking(true);
                if (file.EndsWith(".wav"))
                {
                    using (var waveReader = new WaveFileReader(file))
                    {
                        int byteCount;
                        while((byteCount = waveReader.Read(buffer, 0, blockSize)) > 0)
                        {
                            if (vc.Connected)
                                vc.SendVoice(buffer);
                            else
                                break;
                        }
                    }
                }
                else if (file.EndsWith(".mp3"))
                {
                    using (var mp3Reader = new MediaFoundationReader(file))
                    {
                        using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 })
                        {
                            //resampler.ResamplerQuality = 60;
                            int byteCount;
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                            {
                                if (vc.Connected)
                                {
                                    vc.SendVoice(buffer);
                                }
                                else
                                    break;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Voice finished enqueuing");
                            Console.ForegroundColor = ConsoleColor.White;
                            resampler.Dispose();
                            mp3Reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                owner.SendMessage("Exception during voice: `" + ex.Message + "`\n\n```" + ex.StackTrace + "\n```");
            }
        }

        //private static void VoiceStuffs(DiscordVoiceClient vc, string file)
        //{
        //    try
        //        {
        //            int ms = 20;
        //            int channels = 2;
        //            int sampleRate = 48000;

        //            int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
        //            byte[] buffer = new byte[blockSize];
        //            var outFormat = new WaveFormat(sampleRate, 16, channels);

        //            TimestampSequenceReturn sequence = new TimestampSequenceReturn();
        //            sequence.sequence = 0;
        //            sequence.timestamp = 0;

        //            vc.InitializeOpusEncoder(sampleRate, channels, ms, null);
        //            vc.SendSpeaking(true);
        //            using (var mp3Reader = new Mp3FileReader(file))
        //            {
        //                using (var resampler = new WaveFormatConversionStream(outFormat, mp3Reader))
        //                {
        //                    int byteCount;
        //                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
        //                    {
        //                    if (vc.Connected)
        //                    {
        //                        //sequence = await vc.SendSmallOpusAudioPacket(buffer, sampleRate, byteCount, sequence).ConfigureAwait(false);
        //                        vc.SendVoice(buffer);
        //                        //sequence = vc.SendSmallOpusAudioPacket(buffer, 48000, buffer.Length, sequence);
        //                        //Task.Delay(19).Wait();
        //                    }
        //                    else
        //                        break;
        //                    }
        //                Console.ForegroundColor = ConsoleColor.Yellow;
        //                Console.WriteLine("Voice finished enqueuing");
        //                Console.ForegroundColor = ConsoleColor.White;
        //            }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            owner.SendMessage("Exception during voice: `" + ex.Message + "`\n\n```" + ex.StackTrace + "\n```");
        //        }
        //}

        private static async void ConnectStuff()
        {
            if (await client.SendLoginRequestAsync() != null)
            {
                Console.WriteLine("Logged in..async!");
                client.Connect();
                client.UpdateCurrentGame("");
            }
        }
    }
}
