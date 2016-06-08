using DiscordSharp;
using DiscordSharp.Commands;
using DiscordSharp.Objects;
using NLua;
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
using Luigibot.Modules;
using System.Linq;

namespace Luigibot
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

        [JsonProperty("modules")]
        public Dictionary<string, bool> ModulesDictionary { get; internal set; } //null will mean all enabled
    }

    public class LuigibotMain
    {
        private const bool UseBuiltInWebsocket = true;

        public DiscordMember owner;
        public Config config;

        private DiscordClient client;
        private CommandsManager CommandsManager;
        private CancellationToken cancelToken;
        private DateTime loginDate = DateTime.Now;
        #region Audio playback
        WaveFormat waveFormat;
        BufferedWaveProvider bufferedWaveProvider;
        WaveCallbackInfo waveCallbackInfo;
        IWavePlayer outputDevice;
        VolumeWaveProvider16 volumeProvider;
        System.Timers.Timer stutterReducingTimer;
        #endregion
        private bool runningOnMono = false;
        private const int voiceMsBuffer = 20;
        private string osString;
        public bool actuallyExit = false;
		
        string[] NaughtyWords = new string[]
        {
            "bitch", "fucking", "fuck", "cunt", "shit", "reest", "reested", "asswipe"
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
            
			osString = OSDetermination.GetUnixName();
        }

        public void RunLuigibot()
        {
            string asciiArt = System.IO.File.ReadAllText("ascii.txt");
            Console.WriteLine(asciiArt);
            Console.Title = "Luigibot - Discord";
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
            client = new DiscordClient(botToken.Trim(), true);
            client.WriteLatestReady = true;
            SetupEvents(cancelToken);
        }

        public void Cleanup()
        {
            if (client.GetTextClientLogger.EnableLogging)
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
        }

        private bool StringContainsObjectInArray(string str, string[] array)
        {
            for (int i = 0; i < array.Length; i++)
                if (str.Contains(array[i]))
                    return true;

            return false;
        }

        private Task SetupEvents(CancellationToken token)
        {
            Console.ForegroundColor = ConsoleColor.White;
            return Task.Run(() =>
            {
                client.MessageReceived += (sender, e) =>
                {
                    if (!doingInitialRun)
                    {
                        if (owner == null)
                        {
                            owner = client.GetServersList().Find(x => x.GetMemberByKey(config.OwnerID) != null).GetMemberByKey(config.OwnerID); //prays
                        }
                    }


                    if (e.Author == null)
                    {
                        string msg = $"Author had null id in message received!\nRaw JSON:\n```\n{e.RawJson}\n```\n";
                        msg += $"Args\nChannel: {e.Channel.Name}/{e.Channel.ID}\nMessage: {e.Message}";
                        try
                        {
                            owner.SlideIntoDMs(msg);
                        }
                        catch { }
                    }
                    else
                    {
                        Console.WriteLine($"[-- Message from {e.Author.Username} in #{e.Channel.Name} on {e.Channel.Parent.Name}: {e.Message.Content}");

                        if (doingInitialRun)
                        {
                            if (e.Message.Content.StartsWith("?authenticate"))
                            {
                                string[] split = e.Message.Content.Split(new char[] { ' ' }, 2);
                                if (split.Length > 1)
                                {
                                    if (codeToEnter.Trim() == split[1].Trim())
                                    {
                                        config.OwnerID = e.Author.ID;
                                        doingInitialRun = false;
                                        e.Channel.SendMessage("Authentication successful! **You are now my owner, " + e.Author.Username + ".**");
                                        CommandsManager.AddPermission(e.Author, PermissionType.Owner);
                                        owner = e.Author;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (e.Message.Content.Length > 0 && (e.Message.Content[0] == config.CommandPrefix))
                            {
                                string rawCommand = e.Message.Content.Substring(1);
                                try
                                {
                                    CommandsManager.ExecuteOnMessageCommand(rawCommand, e.Channel, e.Author);
                                }
                                catch (UnauthorizedAccessException ex)
                                {
                                    e.Channel.SendMessage(ex.Message);
                                }
                                catch(ModuleNotEnabledException x)
                                {
                                    e.Channel.SendMessage($"{x.Message}");
                                }
                                catch (Exception ex)
                                {
                                    e.Channel.SendMessage("Exception occurred while running command:\n```" + ex.Message + "\n```");
                                }
                            }
                        }
                        //Now, for fun.
                        //if(e.author == owner)
                        //{
                        //    if (StringContainsObjectInArray(e.message.content.ToLower(), NaughtyWords))
                        //    {
                        //        try
                        //        {
                        //            var msg = client.GetMessageLog()[client.GetMessageLog().Count - 1].Value;
                        //            if (msg.author == owner
                        //                && client.GetLastMessageSent(e.Channel).content != null &&
                        //                client.GetLastMessageSent(e.Channel).content != "hey now")
                        //            {
                        //                //if(msg.timestamp.AddMinutes(1) < DateTime.Now)
                        //                int timebetween = (DateTime.Now.Minute - msg.timestamp.Minute);
                        //                if ((timebetween < 1) && (timebetween > -1)) //less than one minute between his message and my vulgarity
                        //                    e.Channel.SendMessage("hey now");
                        //            }
                        //        }
                        //        catch { }
                        //    }
                        //}

                        if(e.Channel.ID == "91265608326324224") //discord-sharp on discordapi
                        {
                            if(e.Author != owner)
                            {
                                if(e.Message.Content != null && e.Message.Content.ToLower().Contains("how"))
                                {
                                    if(e.Message.Content.ToLower().Contains("bot") && e.Message.Content.ToLower().Contains("tag"))
                                    {
                                        e.Channel.SendMessage($"<#124294271900712960>");//#api-changes
                                    }
                                }
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
                    try
                    {
                        owner.SlideIntoDMs($"Voice connection complete.");
                    }
                    catch { }
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
                    if(owner == null)
                        owner = client.GetServersList().Find(x => x.GetMemberByKey(config.OwnerID) != null).GetMemberByKey(config.OwnerID);
                    Console.WriteLine($"Joined server {e.Server.Name} ({e.Server.ID})");
                    try
                    {
                        owner.SlideIntoDMs($"Joined server {e.Server.Name} ({e.Server.ID})");
                    }
                    catch { }
                };
                client.GuildAvailable += (sender, e) =>
                {
                    Console.WriteLine($"Guild {e.Server.Name} became available.");
                };
                client.SocketClosed += (sender, e) =>
                {
                    Console.Title = "Luigibot - Discord - Socket Closed..";
                    if(!actuallyExit)
                    { 
                        WriteError($"\n\nSocket Closed Unexpectedly! Code: {e.Code}. Reason: {e.Reason}. Clear: {e.WasClean}.\n\n");
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

                    string filename = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}dumps{Path.DirectorySeparatorChar}{e.RawJson["t"].ToString()}.json";
                    if(!File.Exists(filename))
                    {
                        File.WriteAllText(e.RawJson.ToString(), filename);
                        try
                        {
                            owner.SlideIntoDMs(message);
                        }
                        catch { }
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
                    Console.Title = "Luigibot - Discord - Logged in as " + e.User.Username;
                    Console.WriteLine("Connected as " + e.User.Username);
                    if (!String.IsNullOrEmpty(config.OwnerID))
                    {                    }
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
                        if (permissionsDictionary == null)
                            permissionsDictionary = new Dictionary<string, PermissionType>();
						if(permissionsDictionary.Count == 0 && owner != null)
							permissionsDictionary.Add(owner.ID, PermissionType.Owner);
								
                        CommandsManager.OverridePermissionsDictionary(permissionsDictionary);
                    }
                    SetupCommands();

                    if(config.ModulesDictionary != null)
                    {
                        CommandsManager.OverrideModulesDictionary(config.ModulesDictionary);
                    }

                    //client.UpdateCurrentGame($"DiscordSharp {typeof(DiscordClient).Assembly.GetName().Version.ToString()}");
                };
                if(client.SendLoginRequest() != null)
                {
                    client.Connect(UseBuiltInWebsocket);
                }
            }, token);
        }

        //yes, this is a bullet for my valentine reference
		private void LetsGoAgain()
		{
			client.Dispose ();
			client = null;

			string botToken = File.ReadAllText("bot_token_important.txt");
			client = new DiscordClient(botToken, true);
			client.RequestAllUsersOnStartup = true;
            
			client.ClientPrivateInformation.Email = config.BotEmail;
			client.ClientPrivateInformation.Password = config.BotPass;

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
            config.ModulesDictionary = CommandsManager.ModuleDictionaryForJson();
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(config));
			if(CommandsManager.UserRoles != null && CommandsManager.UserRoles.Count > 0)
            	File.WriteAllText("permissions.json", JsonConvert.SerializeObject(CommandsManager.UserRoles));

            client.Logout();
            client.Dispose();
            Cleanup();
            Environment.Exit(0);
        }

        private void SetupCommands()
        {
            #region Old Commands
            CommandsManager.AddCommand(new CommandStub("invite", "Makes an invite to specified server given its ID", "Pass ID douchebag.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    if (cmdArgs.Args[0].Length > 0)
                    {
                        DiscordServer server = client.GetServersList().Find(x => x.ID == cmdArgs.Args[0]);
                        DiscordChannel channel = server.Channels.Find(x => x.Name == "general");
                        cmdArgs.Channel.SendMessage(client.CreateInvite(channel));
                    }
                    else
                        cmdArgs.Channel.SendMessage("kek");
                }
                else
                {
                    cmdArgs.Channel.SendMessage("kek");
                }
            }));
            
            CommandsManager.AddCommand(new CommandStub("statusof", "`Status` test", "", PermissionType.Owner, 1, cmdArgs=>
            {
                string id = cmdArgs.Args[0].Trim(new char[] { '<', '@', '>' });
                if(!string.IsNullOrEmpty(id))
                {
                    DiscordMember member = cmdArgs.Channel.Parent.GetMemberByKey(id);
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
            CommandsManager.AddCommand(new CommandStub("serverstats", "Server stats", "help me", PermissionType.Owner, cmdArgs =>
            {
                if(cmdArgs.Channel != null && cmdArgs.Channel.Parent != null)
                {
                    DiscordServer guild = cmdArgs.Channel.Parent;
                    string msg = $"Stats for **{guild.Name}**\n```\n";
                    msg += $"{guild.Members.Count} members\n";
                    msg += $"{guild.Roles.Count} roles\n";
                    msg += $"Owned by {guild.Owner.Username}#{guild.Owner.Discriminator}\n";
                    msg += $"{guild.Region}\n```";
                    cmdArgs.Channel.SendMessage(msg);
                }
            }));
            CommandsManager.AddCommand(new CommandStub("listroles", "Lists rolls", "help me", PermissionType.Owner, cmdArgs =>
            {
                if(cmdArgs.Channel != null && cmdArgs.Channel.Parent != null)
                {
                    DiscordServer guild = cmdArgs.Channel.Parent;
                    string msg = $"Roles for **{guild.Name}**, per your request.\n```\n";
                    foreach(var role in guild.Roles)
                    {
                        msg += $"{role.Position} - {role.Name} - {role.ID} - {role.Permissions.GetRawPermissions()}\n";
                    }
                    msg += "\n```";
                    try
                    {
                        owner.SlideIntoDMs(msg);
                    }
                    catch { }
                    cmdArgs.Channel.SendMessage($"DMed to you ;)");
                }
            }));
            CommandsManager.AddCommand(new CommandStub("sendchanneltest", "`Client.SendMessageToChannel` Test", "", PermissionType.Owner, cmdArgs =>
            {
                client.SendMessageToChannel("Works!", cmdArgs.Channel);
            }));
            CommandsManager.AddCommand(new CommandStub("setplaying", "Sets the current game the bot is playing.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                client.UpdateCurrentGame(cmdArgs.Args[0], true, "http://www.google.com/");
            }));
            CommandsManager.AddCommand(new CommandStub("join", "Joins a specified server", "", PermissionType.Owner, 1, cmdArgs =>
            {
                string substring = cmdArgs.Args[0].Substring(cmdArgs.Args[0].LastIndexOf('/') + 1);
                client.AcceptInvite(substring);
            }));
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
                        if (stub.Parent != null)
                            msg += $"\nFrom module `{stub.Parent.Name}`";
                        if (stub.ID != null)
                            msg += $"\n`{stub.ID}`";
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
                message += $"WebSocket: " + (UseBuiltInWebsocket ? "`System.Net.WebSockets`" : "`WebSocketSharp`") + "\n";
                message += $"Gateway Version: {client.DiscordGatewayVersion}\n";
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
                    message += "Memory Usage: " + (memUsage / 1024) + "mb\n";
                message += "Commands: " + CommandsManager.Commands.Count + "\n";
                message += "Command Prefix: " + config.CommandPrefix + "\n";
                message += "Total Servers: " + client.GetServersList().Count + "\n";
                cmdArgs.Channel.SendMessage(message);
            }));
            CommandsManager.AddCommand(new CommandStub("moduleinfo", "Shows information about a specific module.", "", PermissionType.User, 1, cmdArgs =>
            {
                if(cmdArgs.Args.Count > 0 && cmdArgs.Args[0].Length > 0)
                {
                    foreach(var module in CommandsManager.Modules.ToList())
                    {
                        if(module.Key.Name.ToLower().Trim() == cmdArgs.Args[0].ToLower().Trim())
                        {
                            string msg = $"**About Module {module.Key.Name}**";

                            msg += $"\n{module.Key.Description}\nEnabled: {module.Value}";
                            msg += $"\nCommands: ";
                            module.Key.Commands.ForEach(x => msg += $"{x.CommandName}, ");

                            cmdArgs.Channel.SendMessage(msg);
                            break;
                        }
                    }
                }
            }));
            #endregion

            var OwnerModules = new BaseOwnerModules(this);
            OwnerModules.Install(CommandsManager);

            var funModule = new NoFunAllowedModule();
            funModule.Install(CommandsManager);

            var serverAdminModules = new ServerAdminModules(this);
            serverAdminModules.Install(CommandsManager);

            var evalModules = new EvalModules();
            evalModules.Install(CommandsManager);

            var yugiohModules = new YugiohModules();
            yugiohModules.Install(CommandsManager);

            var serverLogs = new TestServerLog(client);
            serverLogs.Install(CommandsManager);

            var voice = new Voice(this);
            voice.Install(CommandsManager);

            var holupholup = new Holup();
            holupholup.Install(CommandsManager);

            var testingModule = new TestingModule();
            testingModule.Install(CommandsManager);
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

        private void TestRheaStream()
        {
            DiscordVoiceClient vc = client.GetVoiceClient();
            try
            {
                int ms = voiceMsBuffer;
                int channels = 1;
                int sampleRate = 48000;

                int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
                byte[] buffer = new byte[blockSize];
                var outFormat = new WaveFormat(sampleRate, 16, channels);
                vc.SetSpeaking(true);
                using (var mp3Reader = new MediaFoundationReader("http://radiosidewinder.out.airtime.pro:8000/radiosidewinder_a"))
                {
                    using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 })
                    {
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
            catch(Exception ex)
            {
                owner.SendMessage("Exception during voice: `" + ex.Message + "`\n\n```" + ex.StackTrace + "\n```");
            }
        }
    }
}
