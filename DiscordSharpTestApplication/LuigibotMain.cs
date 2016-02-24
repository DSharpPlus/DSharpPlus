using DiscordSharp;
using DiscordSharp.Commands;
using DiscordSharp.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YugiohPrices;

namespace DiscordSharpTestApplication
{
    public class Config
    {
        [JsonProperty("owner_id")]
        public string OwnerID { get; internal set; }

        [JsonProperty("bot_email")]
        public string BotEmail { get; internal set; }

        [JsonProperty("bot_pass")]
        public string BotPass { get; internal set; }

        [JsonProperty("command_prefix")]
        public char CommandPrefix { get; internal set; } = '?';
    }

    public class LuigibotMain
    {
        DiscordClient client;
        DiscordMember owner;
        CommandsManager CommandsManager;
        CancellationToken cancelToken;
        Config config;
        DateTime loginDate;

        Random rng = new Random((int)DateTime.Now.Ticks);
        string[] KhaledQuotes = new string[]
        {
            "Always have faith. Always have hope.",
            "The key is to make it.",
            "Another one.",
            "Key to success is clean heart and clean face.",
            "Smh they get mad when you have joy.",
            "Baby, you smart. I want you to film me taking a shower.",
            "You smart! You loyal! You a genius!",
            "Give thanks to the most high.",
            "They will try to close the door on you, just open it.",
            "They don’t want you to have the No. 1 record in the country.",
            "Those that weather the storm are the great ones.",
            "The key to success is more cocoa butter.",
            "I changed... a lot.",
            "My fans expect me to be greater and keep being great.",
            "There will be road blocks but we will overcome it.",
            "They don\"t want you to jet ski.",
            "Them doors that was always closed, I ripped the doors off, took the hinges off. And when I took the hinges off, I put the hinges on the f*ckboys’ hands.",
            "Congratulations, you played yourself.",
            "Don\"t play yourself.",
            "Another one, no. Another two, drop two singles at a time.",
        };

#region Initial Run
        bool doingInitialRun = false;
        string codeToEnter = "";
#endregion

        public LuigibotMain()
        {
            cancelToken = new CancellationToken();
            if (File.Exists("settings.json"))
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("settings.json"));
            else
                config = new Config();
            if (config.CommandPrefix.ToString().Length == 0)
                config.CommandPrefix = '?';
        }

        public void RunLuigibot()
        {
            string asciiArt = System.IO.File.ReadAllText("ascii.txt");
            Console.WriteLine(asciiArt);
            DoLogin();
        }

        private void WriteError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
        }

        private void WriteWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
        }

        public void DoLogin()
        {
            client = new DiscordClient();
            client.RequestAllUsersOnStartup = true;

            if (!File.Exists("token_cache"))
            {
                if (config.BotEmail == null || config.BotPass == null)
                {
                    WriteError("Please edit settings.json with the bot's email and password. owner_id is not necessary to edit yet as this will be part of the setup after.");
                    return;
                }
            }

            client.ClientPrivateInformation.email = config.BotEmail;
            client.ClientPrivateInformation.password = config.BotPass;

            SetupEvents(cancelToken);
        }

        public void Cleanup()
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            var date = DateTime.Now;
            client.GetTextClientLogger.Save($"logs/{date.Month}-{date.Day}-{date.Year} {date.Hour}-{date.Minute}-{date.Second}.log");
        }

        private Task SetupEvents(CancellationToken token)
        {
            Console.ForegroundColor = ConsoleColor.White;
            return Task.Run(() =>
            {
                client.MessageReceived += (sender, e) =>
                {
                    Console.WriteLine($"[-- Message from {e.author.Username} in #{e.Channel.Name} on {e.Channel.parent.name}: {e.message.content}");

                    if(doingInitialRun)
                    {
                        if(e.message.content.StartsWith("?authenticate"))
                        {
                            string[] split = e.message.content.Split(new char[] { ' ' }, 2);
                            if(split.Length > 1)
                            {
                                if(codeToEnter.Trim() == split[1].Trim())
                                {
                                    config.OwnerID = e.author.ID;
                                    doingInitialRun = false;
                                    e.Channel.SendMessage("Authentication successful! **You are now my owner, " + e.author.Username + ".**");
                                    CommandsManager.AddPermission(e.author, PermissionType.Owner);
                                    owner = e.author;
                                }
                            }
                        }
                    }
                    else
                    {
                        if(e.message.content.Length > 0 && (e.message.content[0] == config.CommandPrefix))
                        {
                            string rawCommand = e.message.content.Substring(1);
                            //try
                            //{
                                CommandsManager.ExecuteCommand(rawCommand, e.Channel, e.author);
                            //}
                            //catch(UnauthorizedAccessException ex)
                            //{
                                //e.Channel.SendMessage(ex.Message);
                            //}
                            //c/atch(Exception ex)
                            //{
                                //e.Channel.SendMessage("Exception occurred while running command:\n```" + ex.Message + "\n```");
                            //}
                        }
                    }
                };
                client.GuildCreated += (sender, e) =>
                {
                    owner.SlideIntoDMs($"Joined server {e.server.name} ({e.server.id})");
                };
                client.SocketClosed += (sender, e) =>
                {
                    WriteError($"Socket Closed! Code: {e.Code}. Reason: {e.Reason}. Clear: {e.WasClean}.");
                    Console.WriteLine("Waiting 6 seconds to reconnect..");
                    Thread.Sleep(6 * 1000);
                    client.Connect();
                };
                client.TextClientDebugMessageReceived += (sender, e) =>
                {
                    if (e.message.Level == MessageLevel.Error || e.message.Level == MessageLevel.Critical)
                    {
                        WriteError($"(Logger Error) {e.message.Message}");
                        try
                        {
                            owner.SlideIntoDMs($"Bot error ocurred: ({e.message.Level.ToString()})```\n{e.message.Message}\n```");
                        }
                        catch { }
                    }
                    if (e.message.Level == MessageLevel.Warning)
                        WriteWarning($"(Logger Warning) {e.message.Message}");
                };
                client.Connected += (sender, e) =>
                {
                    Console.WriteLine("Connected as " + e.user.Username);
                    loginDate = DateTime.Now;

                    if(!String.IsNullOrEmpty(config.OwnerID))
                        owner = client.GetServersList().Find(x => x.members.Find(y => y.ID == config.OwnerID) != null).members.Find(x => x.ID == config.OwnerID);
                    else
                    {
                        doingInitialRun = true;
                        RandomCodeGenerator rcg = new RandomCodeGenerator();
                        codeToEnter = rcg.GenerateRandomCode();

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Important: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\tPlease authenticate yourself as owner by typing the following into any Discord server you and the bot are in: ");
                        Console.WriteLine($"\t{config.CommandPrefix}authenticate " + codeToEnter);
                    }
                    CommandsManager = new CommandsManager(client);
                    if (File.Exists("permissions.json"))
                    {
                        var permissionsDictionary = JsonConvert.DeserializeObject<Dictionary<string, PermissionType>>(File.ReadAllText("permissions.json"));
                        CommandsManager.OverridePermissionsDictionary(permissionsDictionary);
                    }
                    SetupCommands();
                };
                if(client.SendLoginRequest() != null)
                {
                    client.Connect();
                }
            }, token);
        }

        public void Exit()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(config));
            File.WriteAllText("permissions.json", JsonConvert.SerializeObject(CommandsManager.UserRoles));

            client.Logout();
            client.Dispose();
            Cleanup();
            Environment.Exit(0);
        }

        private void SetupCommands()
        {
#region Owner only
            CommandsManager.AddCommand(new CommandStub("selfdestruct", "Shuts the bot down.", "", PermissionType.Owner, cmdArgs=>
            {
                Exit();
            }));
            CommandsManager.AddCommand(new CommandStub("statusof", "`Status` test", "", PermissionType.Owner, 1, cmdArgs=>
            {
                string id = cmdArgs.Args[0].Trim(new char[] { '<', '@', '>' });
                if(!string.IsNullOrEmpty(id))
                {
                    DiscordMember member = cmdArgs.Channel.parent.members.Find(x => x.ID == id);
                    if (member != null)
                    {
                        string msg = $"Status of `{member.Username}`\n{member.Status}";
                        if (!string.IsNullOrEmpty(member.CurrentGame))
                        {
                            msg += $"\nPlaying: *{member.CurrentGame}*";
                        }
                        cmdArgs.Channel.SendMessage(msg);
                    }
                }
            }));
            CommandsManager.AddCommand(new CommandStub("changepic", "Changes the bot's guild pic test.", "Test", PermissionType.Owner, 1, cmdArgs =>
            {
                Regex linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                string rawString = $"{cmdArgs.Args[0]}";
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
            }));
            CommandsManager.AddCommand(new CommandStub("sendchanneltest", "`Client.SendMessageToChannel` Test", "", PermissionType.Owner, cmdArgs =>
            {
                client.SendMessageToChannel("Works!", cmdArgs.Channel);
            }));
            CommandsManager.AddCommand(new CommandStub("setplaying", "Sets the current game the bot is playing.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                client.UpdateCurrentGame(cmdArgs.Args[0]);
            }));
            CommandsManager.AddCommand(new CommandStub("join", "Joins a specified server", "", PermissionType.Owner, 1, cmdArgs =>
            {
                string substring = cmdArgs.Args[0].Substring(cmdArgs.Args[0].LastIndexOf('/') + 1);
                client.AcceptInvite(substring);
            }));
            CommandsManager.AddCommand(new CommandStub("changeprefix", "Changes the command prefix to a specified character.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    char newPrefix = cmdArgs.Args[0][0];
                    config.CommandPrefix = newPrefix;
                    cmdArgs.Channel.SendMessage($"Command prefix changed to **{config.CommandPrefix}** successfully!");
                }
                else
                    cmdArgs.Channel.SendMessage("What prefix?");
            }));
            CommandsManager.AddCommand(new CommandStub("giveperm", "Gives the perm to the specified user (bot scope)", "", PermissionType.Owner, 2, e =>
            {
                //giveperm Admin <@2309208509852>
                if (e.Args.Count > 1)
                {
                    string permString = e.Args[0];
                    PermissionType type = PermissionType.User;
                    switch(permString.ToLower())
                    {
                        case "admin":
                            type = PermissionType.Admin;
                            break;
                        case "mod":
                            type = PermissionType.Mod;
                            break;
                        case "none":
                            type = PermissionType.None;
                            break;
                        case "user":
                            type = PermissionType.User;
                            break;
                    }
                    string id = e.Args[1].Trim(new char[] { '<', '@', '>' });
                    CommandsManager.AddPermission(id, type);
                    e.Channel.SendMessage($"Given permission {type.ToString().Substring(type.ToString().IndexOf('.') + 1)} to <@{id}>!");
                }
            }));
            CommandsManager.AddCommand(new CommandStub("prune", "Prune test", "", PermissionType.Owner, 1, cmdArgs =>
            {
                int messageCount = 0;
                if(int.TryParse(cmdArgs.Args[0], out messageCount))
                {
                    var messagesToPrune = client.GetMessageHistory(cmdArgs.Channel, messageCount);
                    foreach(var msg in messagesToPrune)
                    {
                        client.DeleteMessage(msg);
                        Thread.Sleep(100);
                    }
                    cmdArgs.Channel.SendMessage($"Attempted pruning of {messageCount} messages.");
                }
            }));
#endregion
#region Admin
            CommandsManager.AddCommand(new CommandStub("eval", "Evaluates real-time C# code. Be careful with this", 
                "Evaluates C# code that is dynamically compiled.\n\nThe following namespaces are available for use:\n * DiscordSharp\n * System.Threading\n * DiscordSharp.Objects\n\n\nMake sure your function returns a string value.\nYou can reference the DiscordSharp client by using `discordClient`.", PermissionType.Admin, 1, e =>
            {
                string whatToEval = e.Args[0];
                if (whatToEval.StartsWith("`") && whatToEval.EndsWith("`"))
                    whatToEval = whatToEval.Trim('`');
                try
                {
                    var eval = EvalProvider.CreateEvalMethod<DiscordClient, string>(whatToEval, new string[] { "DiscordSharp", "System.Threading", "DiscordSharp.Objects" }, new string[] { "DiscordSharp.dll" });
                    string res = "";
                    Thread.Sleep(1000);
                    Thread executionThread = null;
                    Task evalTask = new Task(() =>
                    {
                        executionThread = Thread.CurrentThread;
                        if(eval != null)
                            res = eval(client);
                        else
                        {
                            string errors = "Errors While Compiling: \n";
                            if (EvalProvider.errors != null)
                            {
                                if (EvalProvider.errors.Count > 0)
                                {
                                    foreach (var error in EvalProvider.errors)
                                    {
                                        errors += $"{error.ToString()}\n\n";
                                    }
                                }
                                e.Channel.SendMessage($"```\n{errors}\n```");
                            }
                            else
                                e.Channel.SendMessage("Errors!");
                        }

                    });
                    evalTask.Start();
                    evalTask.Wait(10 * 1000);
                    if(executionThread != null)
                        executionThread.Abort();
                    if (res == null || res == "")
                        e.Channel.SendMessage("Terminated after 10 second timeout.");
                    else
                        e.Channel.SendMessage($"**Result**\n```\n{res}\n```");
                }
                catch(Exception ex)
                {
                    string errors = "Errors While Compiling: \n";
                    if (EvalProvider.errors != null)
                    {
                        if (EvalProvider.errors.Count > 0)
                        {
                            foreach (var error in EvalProvider.errors)
                            {
                                errors += $"{error.ToString()}\n\n";
                            }
                        }
                        else
                            errors += ex.Message;
                        e.Channel.SendMessage($"```\n{errors}\n```");
                    }
                    else
                        e.Channel.SendMessage("Errors!");
                }
            }));
#endregion
#region Anyone, but limited to server mods
            CommandsManager.AddCommand(new CommandStub("gtfo", "Makes the bot leave the server", "", PermissionType.User, cmdArgs =>
            {
                bool canExecute = false;
                foreach (var roll in cmdArgs.Author.Roles)
                    if (roll.permissions.HasPermission(DiscordSpecialPermissions.ManageServer))
                        canExecute = true;
                if (canExecute)
                    client.LeaveServer(cmdArgs.Channel.parent);
                else
                    cmdArgs.Channel.SendMessage("You don't have the proper permissions to do this! You need the ManagerServer permission.");
            }));
#endregion
#region Literally anyone
            CommandsManager.AddCommand(new CommandStub("cmdinfo", "Displays help for a command.", "Help", PermissionType.User, 2, e =>
            {
                if (!String.IsNullOrEmpty(e.Args[0]))
                {
                    ICommand stub = CommandsManager.Commands.Find(x => x.CommandName == e.Args[0]);
                    if (stub != null)
                    {
                        string msg = "**Help for " + stub.CommandName + "**";
                        msg += $"\n{stub.Description}";
                        if (!String.IsNullOrEmpty(stub.HelpTag))
                            msg += $"\n\n{stub.HelpTag}";
                        e.Channel.SendMessage(msg);
                    }
                    else
                    {
                        e.Channel.SendMessage("What command?");
                    }
                }
                else
                    e.Channel.SendMessage("What command?");
            }));
            CommandsManager.AddCommand(new CommandStub("about", "Shows bot information", "", cmdArgs =>
            {
                string message = "**About Luigibot**\n";
                message += "Owner: " + owner.Username + "\n";
                message += $"Library: DiscordSharp {typeof(DiscordClient).Assembly.GetName().Version.ToString()}\n";
                message += "Uptime: " + (DateTime.Now - loginDate).ToString() + "\n";
                message += "Compiled Under: ";
                if (Mono())
                    message += "Mono\n";
                else
                    message += ".Net\n";
                long memUsage = GetMemoryUsage();
                if (memUsage > 0)
                    message += "Memory Usage: " + (memUsage / 1024) / 2 + "mb\n";
                message += "Commands: " + CommandsManager.Commands.Count + "\n";
                message += "Command Prefix: " + config.CommandPrefix + "\n";
                message += "Total Servers: " + client.GetServersList().Count + "\n";
                cmdArgs.Channel.SendMessage(message);
            }));
            CommandsManager.AddCommand(new CommandStub("ygo", "Retrieves information for a Yu-Gi-Oh card from the YugiohPrices database.", 
                "Card names are (unfortunately) case sensitive.\n\n**Valid:** Dark Magician\n**Invalid: **dark magician", PermissionType.User, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    YugiohPricesSearcher searcher = new YugiohPricesSearcher();
                    try
                    {
                        var card = searcher.GetCardByName(cmdArgs.Args[0]).Result;
                        if (card.Name != "<NULL CARD>")
                        {
                            card.CardImage.Save("ygotemp.png");
                            string message = $"**{card.Name}**";
                            if (card.Type == CardType.Monster)
                                message += $" Level: {card.Level} Attribute: {card.Attribute}\n";
                            else
                                message += "\n";
                            message += $"**Description:** {card.Description}";
                            if (card.Type == CardType.Monster)
                                message += $"\n**Type:** {card.MonsterType}\n**ATK/DEF:** {card.Attack}/{card.Defense}";

                            client.AttachFile(cmdArgs.Channel, message, "ygotemp.png");
                        }
                        else
                            cmdArgs.Channel.SendMessage("Couldn't find that specified card!");
                    }
                    catch(HttpRequestException ex)
                    {
                        cmdArgs.Channel.SendMessage("Couldn't find that specified card! (" + ex.Message + ")");
                    }
                    
                }
            }));
            CommandsManager.AddCommand(new CommandStub("khaled", "Anotha one.", "", cmdArgs =>
            {
                cmdArgs.Channel.SendMessage($"***{KhaledQuotes[rng.Next(0, KhaledQuotes.Length - 1)]}***");
            }));
#endregion
        }

        private long GetMemoryUsage()
        {
            Process luigibotProcess = Process.GetCurrentProcess();
            if (luigibotProcess == null)
                return 0;
            else
            {
                return (luigibotProcess.WorkingSet64 / 1024);
            }
        }

        public bool Mono()
        {
#if __MONOCS__
            return true;
#endif
            return false;
        }
    }
}
