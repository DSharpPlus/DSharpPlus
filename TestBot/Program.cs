using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using Newtonsoft.Json.Linq;
using DSharpPlus.Objects;
using Newtonsoft.Json;
using SlackMessageBuilder;
using SlackMessageBuilder.Objects;

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
                    Hook.Webhook hook = e.Channel.CreateWebhook(e.args[0]);
                    SlackMessage message = new SlackMessage();
                    message.Add(new SlackText("text", "List of Webhooks"));

                    SlackContainer attachment = new SlackContainer();
                    attachment.Add(new SlackText("color", "#36a64f"));
                    attachment.Add(new SlackText("pretext", hook.Token));
                    attachment.Add(new SlackText("author_name", $"{hook.Name} [{hook.ID}]"));
                    attachment.Add(new SlackText("author_icon", hook.User.GetAvatarURL().ToString()));
                    attachment.Add(new SlackText("title", hook.Name));
                    attachment.Add(new SlackText("text", hook.Token));
                    attachment.Add(new SlackText("image_url", hook.Avatar));

                    message.Add(new SlackContainerList("attachments", new List<SlackContainer>() { attachment }));
                    
                    hook.SendSlack(message.ToJObject());
                }
                ));

            client.AddCommand(DiscordCommand.Create("list")
                .Do(e =>
                {
                    Hook.Webhook fHook = null;

                    SlackMessage message = new SlackMessage();
                    message.Add(new SlackText("text", $"New {Formatter.Bold(Formatter.Bold("webhook"))} {Formatter.InlineCode("created")}"));
                    List<SlackContainer> attachments = new List<SlackContainer>();
                    foreach (Hook.Webhook hook in e.Channel.GetWebhooks())
                    {
                        if (fHook == null) fHook = hook;

                        SlackContainer attachment = new SlackContainer();
                        attachment.Add(new SlackText("color", "#36a64f"));
                        attachment.Add(new SlackText("pretext", hook.Token));
                        attachment.Add(new SlackText("author_name", $"{hook.Name} [{hook.ID}]"));
                        attachment.Add(new SlackText("author_icon", hook.User.GetAvatarURL().ToString()));
                        attachment.Add(new SlackText("title", hook.Name));
                        attachment.Add(new SlackText("text", hook.Token));
                        attachment.Add(new SlackText("image_url", hook.Avatar));

                        attachments.Add(attachment);
                    }
                    message.Add(new SlackContainerList("attachments", attachments));
                    fHook.SendSlack(message.ToJObject(), false);
                }
                ));
            client.AddCommand(DiscordCommand.Create("removeall")
                .Do(e =>
                {
                    foreach (Hook.Webhook hook in e.Channel.GetWebhooks())
                    {
                        hook.Delete();
                    }
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
