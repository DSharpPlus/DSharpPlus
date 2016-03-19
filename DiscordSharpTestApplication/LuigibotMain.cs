using DiscordSharp;
using DiscordSharp.Commands;
using DiscordSharp.Objects;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
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
        //internal static readonly string OAUTH_ID = "152966395884339200";
        //internal static readonly string OAUTH_SECRET = "RFtPxLA19Lu1t3J-r4O7nNw0aaS8Wdg2";

        DiscordClient client;
        DiscordMember owner;
        CommandsManager CommandsManager;
        CancellationToken cancelToken;
        Config config;
        DateTime loginDate;
        #region Audio playback
        WaveFormat waveFormat;
        BufferedWaveProvider bufferedWaveProvider;
        WaveCallbackInfo waveCallbackInfo;
        IWavePlayer outputDevice;
        VolumeWaveProvider16 volumeProvider;
        System.Timers.Timer stutterReducingTimer;
        #endregion
        bool runningOnMono = false;
		string osString;

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
            "They don't want you to jet ski.",
            "Them doors that was always closed, I ripped the doors off, took the hinges off. And when I took the hinges off, I put the hinges on the fuckboys’ hands.",
            "Congratulations, you played yourself.",
            "Don't play yourself.",
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
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("settings.json"));
                if (config == null)
                    config = new Config();
            }
            else
                config = new Config();
            if (config.CommandPrefix.ToString().Length == 0)
                config.CommandPrefix = '?';

			runningOnMono = Type.GetType ("Mono.Runtime") != null;

			if (OSDetermination.IsOnUnix ()) 
			{
				osString = OSDetermination.GetUnixName ();
			}
			else
				osString = Environment.OSVersion.ToString ();
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
            string botToken = File.ReadAllText("bot_token_important.txt");
            client = new DiscordClient(botToken, true);
            //client = new DiscordClient();
            
            //if (!File.Exists("token_cache"))
            //{
            //    if (config.BotEmail == null || config.BotPass == null)
            //    {
            //        WriteError("Please edit settings.json with the bot's email and password. owner_id is not necessary to edit yet as this will be part of the setup after.");
            //        return;
            //    }
            //}

            //client.ClientPrivateInformation.email = config.BotEmail;
            //client.ClientPrivateInformation.password = config.BotPass;

            SetupEvents(cancelToken);
        }

        public void Cleanup()
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            var date = DateTime.Now;
            string mmddyy = $"{date.Month}-{date.Day}-{date.Year}";
            if (!Directory.Exists("logs/" + mmddyy))
                Directory.CreateDirectory("logs/" + mmddyy);

            int levels = (int)(MessageLevel.Debug & MessageLevel.Error & MessageLevel.Critical & MessageLevel.Warning);

            client.GetTextClientLogger.Save($"logs/{mmddyy}/{date.Month}-{date.Day}-{date.Year} {date.Hour}-{date.Minute}-{date.Second}.log", (MessageLevel)levels);
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
                            try
                            {
                                CommandsManager.ExecuteCommand(rawCommand, e.Channel, e.author);
                            }
                            catch(UnauthorizedAccessException ex)
                            {
                                e.Channel.SendMessage(ex.Message);
                            }
                            catch(Exception ex)
                            {
                                e.Channel.SendMessage("Exception occurred while running command:\n```" + ex.Message + "\n```");
                            }
                        }
                    }
                };
                client.VoiceClientDebugMessageReceived += (sender, e) =>
                {
                    if(e.message.Level != MessageLevel.Unecessary)
                        Console.WriteLine($"[{e.message.Level} {e.message.TimeStamp.ToString()}] {e.message.Message}");
                };
                client.VoiceClientConnected += (sender, e) =>
                {
                    owner.SlideIntoDMs($"Voice connection complete.");
                    //player = new AudioPlayer(client.GetVoiceClient().VoiceConfig);
                    //bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
                    //bufferedWaveProvider.BufferDuration = new TimeSpan(0, 0, 50);
                    //volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                    //volumeProvider.Volume = 1.1f;
                    //outputDevice.Init(volumeProvider);

                    //stutterReducingTimer = new System.Timers.Timer(500);
                    //stutterReducingTimer.Elapsed += StutterReducingTimer_Elapsed;
                    //PlayAudioAsync(cancelToken);
                };
                client.AudioPacketReceived += (sender, e) =>
                {
                    if(bufferedWaveProvider != null)
                    {
                        byte[] potential = new byte[4000];
                        int decodedFrames = client.GetVoiceClient().Decoder.DecodeFrame(e.OpusAudio, 0, e.OpusAudioLength, potential);
                        bufferedWaveProvider.AddSamples(potential, 0, decodedFrames);
                    }
                };
                client.GuildCreated += (sender, e) =>
                {
                    owner.SlideIntoDMs($"Joined server {e.server.name} ({e.server.id})");
                };
                client.SocketClosed += (sender, e) =>
                {
                    if (e.Code != 1000 && !e.WasClean)
                    {
                        WriteError($"Socket Closed! Code: {e.Code}. Reason: {e.Reason}. Clear: {e.WasClean}.");
                        Console.WriteLine("Waiting 6 seconds to reconnect..");
                        Thread.Sleep(6 * 1000);
						LetsGoAgain();
                    }
                    else
                    {
                        Console.WriteLine($"Shutting down ({e.Code}, {e.Reason}, {e.WasClean})");
                    }
                };
                client.UnknownMessageTypeReceived += (sender, e) =>
                {
                    if (!Directory.Exists("dumps"))
                        Directory.CreateDirectory("dumps");
                    string message = $"Ahoy! An unknown message type `{e.RawJson["t"].ToString()}` was discovered with the contents: \n\n";
                    message += $"```\n{e.RawJson.ToString()}\n```\nIt's been dumped to `dumps/{e.RawJson["t"].ToString()}.json` for your viewing pleasure.";

                    string filename = $"dumps{Path.DirectorySeparatorChar}{e.RawJson["t"].ToString()}.json";
                    if(!File.Exists(filename))
                    {
                        File.WriteAllText(e.RawJson.ToString(), filename);
                        owner.SlideIntoDMs(message);
                    }
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

					client.UpdateCurrentGame($"DiscordSharp {typeof(DiscordClient).Assembly.GetName().Version.ToString()}");
                };
                if(client.SendLoginRequest() != null)
                {
                    client.Connect();
                }
            }, token);
        }

		private void LetsGoAgain()
		{
			client.Dispose ();
			client = null;

			string botToken = File.ReadAllText("bot_token_important.txt");
			client = new DiscordClient(botToken, true);
			client.RequestAllUsersOnStartup = true;

			//if (!File.Exists("token_cache"))
			//{
			//    if (config.BotEmail == null || config.BotPass == null)
			//    {
			//        WriteError("Please edit settings.json with the bot's email and password. owner_id is not necessary to edit yet as this will be part of the setup after.");
			//        return;
			//    }
			//}

			client.ClientPrivateInformation.email = config.BotEmail;
			client.ClientPrivateInformation.password = config.BotPass;

			SetupEvents(cancelToken);
		}

        private void StutterReducingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(outputDevice.PlaybackState != PlaybackState.Stopped)
            {
                if(bufferedWaveProvider != null)
                {
                    var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                    if(bufferedSeconds < 0.5 && outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        outputDevice.Pause();
                    }
                    else if(bufferedSeconds > 4 && outputDevice.PlaybackState == PlaybackState.Paused)
                    {
                        outputDevice.Play();
                    }
                }
            }
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
            CommandsManager.AddCommand(new CommandStub("joinvoice", "Joins a specified voice channel", "Arg is case insensitive voice channel name to join.", PermissionType.Owner, 1, cmdArgs =>
            {
                DiscordChannel channelToJoin = cmdArgs.Channel.parent.channels.Find(x => x.Name.ToLower() == cmdArgs.Args[0].ToLower() && x.Type == ChannelType.Voice);
                if (channelToJoin != null)
                {
                    DiscordVoiceConfig config = new DiscordVoiceConfig
                    {
                        FrameLengthMs = 20,
                        Channels = 2,
                        OpusMode = Discord.Audio.Opus.OpusApplication.LowLatency,
                        SendOnly = true
                    };

                    waveFormat = new WaveFormat(48000, 16, config.Channels);

                    if (!config.SendOnly)
                    {
                        waveCallbackInfo = WaveCallbackInfo.FunctionCallback();
                        outputDevice = new WaveOut();
                    }

                    client.ConnectToVoiceChannel(channelToJoin, config);
                }
                else
                    cmdArgs.Channel.SendMessage("Couldn't find the specified channel as a voice channel!");
            }));
            CommandsManager.AddCommand(new CommandStub("disconnect", "Disconnects from voice", "", PermissionType.Owner, 1, cmdArgs =>
            {
                    client.DisconnectFromVoice();
            }));
            CommandsManager.AddCommand(new CommandStub("testvoice", "Broadcasts specified file over voice.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if (File.Exists(cmdArgs.Args[0]))
                {
                    if (client.ConnectedToVoice())
                        SendVoice(cmdArgs.Args[0]);
                    else
                        cmdArgs.Channel.SendMessage("Not connected to voice!");
                }
                else
                    cmdArgs.Channel.SendMessage("Couldn't broadcast specified file! It doesn't exist!");
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
                    char oldPrefix = config.CommandPrefix;
                    try
                    {
                        char newPrefix = cmdArgs.Args[0][0];
                        config.CommandPrefix = newPrefix;
                        cmdArgs.Channel.SendMessage($"Command prefix changed to **{config.CommandPrefix}** successfully!");
                    }
                    catch(Exception)
                    {
                        cmdArgs.Channel.SendMessage($"Unable to change prefix to `{cmdArgs.Args[0][0]}`. Falling back to `{oldPrefix}`.");
                        config.CommandPrefix = oldPrefix;
                    }
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
					if(!runningOnMono) //causes exceptions apparently >.>
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
                message += $"Owner: {owner.Username}#{owner.Discriminator}\n";
                message += $"Library: DiscordSharp {typeof(DiscordClient).Assembly.GetName().Version.ToString()}\n";
                var uptime = (DateTime.Now - loginDate);
                message += $"Uptime: {uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes.\n";
                message += "Runtime: ";

				if (runningOnMono)
                    message += "Mono\n";
                else
                    message += ".Net\n";
					
				message += $"OS: {osString}\n";
                long memUsage = GetMemoryUsage();
                if (memUsage > 0)
						message += "Memory Usage: " + (memUsage / 1024) /* / 2*/ + "mb\n";
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
					if(rng == null)
					{
						Console.WriteLine("RNG null?!");
						rng = new Random((int)DateTime.Now.Ticks);
					}
                cmdArgs.Channel.SendMessage($"***{KhaledQuotes[rng.Next(0, KhaledQuotes.Length - 1)]}***");
            }));
#endregion
        }

        private Task PlayAudioAsync(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                do
                {
                    if (bufferedWaveProvider.BufferedDuration > TimeSpan.Zero)
                    {
                        if (outputDevice != null && outputDevice.PlaybackState != PlaybackState.Playing)
                        {
                            outputDevice.Play();
                        }
                    }
                    else
                    {
                        outputDevice.Pause();
                        await Task.Delay(1).ConfigureAwait(false);
                    }
                }
                while (bufferedWaveProvider != null && !cancelToken.IsCancellationRequested);
            });
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

        private void SendVoice(string file)
        {
            DiscordVoiceClient vc = client.GetVoiceClient();
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
                        while ((byteCount = waveReader.Read(buffer, 0, blockSize)) > 0)
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
    }
}
