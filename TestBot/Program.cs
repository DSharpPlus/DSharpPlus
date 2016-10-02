using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordClient client = new DiscordClient("token", true, true);
            client.EnableVerboseLogging = true;
            client.GetTextClientLogger.EnableLogging = true;
            client.GetTextClientLogger.LogMessageReceived += (sender, e) => Console.WriteLine($"{e.message.TimeStamp} -- {e.message.Message}");

            Console.WriteLine("Attempting to connect!");
            client.CommandPrefixes.Add("testbot.");
            client.AddCommand(DiscordCommand.Create("create")
                .AddParameter("name")
                .Do(e =>
                {
                    e.Channel.CreateWebhook(e.args[0]);
                }
                ));

            client.AddCommand(DiscordCommand.Create("list")
                .Do(e =>
                {
                    Webhooks.Webhook fHook = null;

                    JObject message = new JObject();
                    message.Add("text", "List of Webhooks");
                    JArray attachments = new JArray();
                    foreach (Webhooks.Webhook hook in e.Channel.GetWebhooks())
                    {
                        if (fHook == null) fHook = hook;

                        attachments.Add(new JObject()
                        {
                            { "color", "#36a64f" },
                            { "pretext", hook.Token },
                            { "author_name", $"{hook.Name} [{hook.ID}]" },
                            { "author_icon", hook.User.GetAvatarURL() },
                            { "title", hook.Name },
                            { "text", hook.Token },
                            { "image_url", hook.Avatar }
                        });
                    }
                    message.Add("attachments", attachments);
                    fHook.SendSlack(message, false);
                }
                ));

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
                Console.WriteLine("CLIENT CONNECTED");
            };
            client.MessageReceived += (sender, e) => // Channel message has been received
            {
                if (e.MessageText == "tiaqoy0 is the most awesome person in the world") // fixed* -> love you afro <3
                {
                    e.Channel.SendMessage(e.MessageText);
                }
            };

            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
