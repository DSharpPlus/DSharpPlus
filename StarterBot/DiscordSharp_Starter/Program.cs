//  If you have any questions or just want to talk, join my server!
//  https://discord.gg/0oZpaYcAjfvkDuE4
using Newtonsoft.Json.Linq;
using SharpCord;
using SharpCord.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCord_Starter
{
    class Program
    {
        public static bool isbot = true;
        //pf is you're prefix var and is loaded from the Bot_Settings.ini
        public static string pf = BotSettings.botPrefix;
        static void Main(string[] args)
        {
            //Checks to see if Bot_Settings.ini exists and if it doesnt it writes to console and ends the program.
            MakeIni.Create();
            // First of all, a DiscordClient will be created, and the email and password will be defined.
            Console.WriteLine("Defining variables");
            // Fill in token and change isbot to true if you use the API
            // Else, leave token alone and change isbot to false
            // But believe me, the API bots are nicer because of a sexy bot tag!
            //BotSettings.botToken gets the token from Bot_Settings.ini
            DiscordClient client = new DiscordClient(BotSettings.botToken, isbot);
            client.ClientPrivateInformation.Email = "email";
            client.ClientPrivateInformation.Password = "pass";

            // Then, we are going to set up our events before connecting to discord, to make sure nothing goes wrong.

            Console.WriteLine("Defining Events");
            // find that one you interested in 

            client.Connected += (sender, e) => // Client is connected to Discord
            {
                Console.WriteLine("Connected! User: " + e.User.Username);
                // If the bot is connected, this message will show.
                // Changes to client, like playing game should be called when the client is connected,
                // just to make sure nothing goes wrong.
                client.UpdateCurrentGame(BotSettings.botStatus, true, "https://github.com/NaamloosDT/DiscordSharp_Starter"); // This will display at "Playing: "
                                                                                                                            //Whoops! i messed up here. (original: Bot online!\nPress any key to close this window.)

                //if the bots username is not the same as the name set in the Bot_Settings.ini
                // this sets the bot name to be the updated name in the Bot_Settings.ini
                if (client.Me.Username != BotSettings.botName)
                {
                    DiscordUserInformation info = new DiscordUserInformation();
                    info.Username = BotSettings.botName;
                    client.ChangeClientInformation(info);
                }
                //checks to see if a avatar image exists and updates it.
                if (File.Exists("avatar.jpg"))
                    client.ChangeClientAvatarFromFile("avatar.jpg");
            };


            client.PrivateMessageReceived += (sender, e) => // Private message has been received
            {
                if (e.Message == pf + "help")
                {
                    e.Author.SendMessage("This is a private message!");
                    // Because this is a private message, the bot should send a private message back
                    // A private message does NOT have a channel
                }
                if (e.Message == pf + "update avatar")
                {
                    // Because this is a private message, the bot should send a private message back
                    // A private message does NOT have a channel
                }
                if (e.Message.StartsWith(pf +"join"))
                {
                    if (!isbot) {
                        string inviteID = e.Message.Substring(e.Message.LastIndexOf('/') + 1);
                        // Thanks to LuigiFan (Developer of DiscordSharp) for this line of code!
                        client.AcceptInvite(inviteID);
                        e.Author.SendMessage("Joined your discord server!");
                        Console.WriteLine("Got join request from " + inviteID);
                    }else
                    {
                        e.Author.SendMessage("Please use this url instead!" +
                            "https://discordapp.com/oauth2/authorize?client_id=[CLIENT_ID]&scope=bot&permissions=0");
                    }
                }
            };


            client.MessageReceived += (sender, e) => // Channel message has been received
            {
                if (e.MessageText == pf + "admin")
                {
                    bool isadmin = false;
                    List<DiscordRole> roles = e.Author.Roles;
                    foreach(DiscordRole role in roles){
                        if (role.Name.Contains("Administrator") || e.Author.HasPermission(DiscordSpecialPermissions.Administrator))
                        {
                            isadmin = true;
                            break;
                        }
                    }
                    if (isadmin)
                    {
                        e.Channel.SendMessage("Yes, you are! :D");
                    }
                    else
                    {
                        e.Channel.SendMessage("No, you aren't :c");
                    }
                }
                if (e.MessageText == pf + "help")
                {
                    e.Channel.SendMessage("This is a public message!");
                    // Because this is a public message, 
                    // the bot should send a message to the channel the message was received.
                }
                if (e.MessageText == pf + "cat")
                {
                    e.Channel.SendMessage("Meow :cat: " + randomcat());
                }
            };

            //  Below: some things that might be nice?

            //  This sends a message to every new channel on the server
            client.ChannelCreated += (sender, e) =>
                {
                    if(e.ChannelCreated.Type == ChannelType.Text)
                    {
                        e.ChannelCreated.SendMessage("Nice! a new channel has been created!");
                    }
                };

            //  When a user joins the server, send a message to them.
            client.UserAddedToServer += (sender, e) =>
                {
                    e.AddedMember.SendMessage("Welcome " + e.Guild.Name + ", please read the rules.\nEnjoy you're time here!");
                };

            //  Don't want messages to be removed? this piece of code will
            //  Keep messages for you. Remove if unused :)
            /*client.MessageDeleted += (sender, e) =>
                {
                    e.Channel.SendMessage("Removing messages has been disabled on this server!");
                    e.Channel.SendMessage("<@" + e.DeletedMessage.Author.ID + "> sent: " +e.DeletedMessage.Content.ToString());
                };*/

            

            // Now, try to connect to Discord.
            try{ 
                // Make sure that IF something goes wrong, the user will be notified.
                // The SendLoginRequest should be called after the events are defined, to prevent issues.
                Console.WriteLine("Sending login request");
                client.SendLoginRequest();
                Console.WriteLine("Connecting client in separate thread");
                // Cannot convert from 'method group' to 'ThreadStart', so i removed threading
                // Pass argument 'true' to use .Net sockets.
                client.Connect();
                 // Login request, and then connect using the discordclient i just made.
                Console.WriteLine("Client connected!");
            }catch(Exception e){
                Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            }

            // Done! your very own Discord bot is online!


            // Now to make sure the console doesnt close:
            Console.ReadKey(); // If the user presses a key, the bot will shut down.
            Environment.Exit(0); // Make sure all threads are closed.
        }

        public static string randomcat()
        {
            WebClient c = new WebClient();
            var data = c.DownloadString("http://random.cat/meow");
            //Console.WriteLine(data);

            JObject o = JObject.Parse(data);
            string response = o["file"].ToString();
            return response;
        }
    }
}
