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
            client.AddCommand(DiscordCommand.Create("embed")
                .AddParameter("text")
                .Do(e =>
                {
                    DiscordEmbed embed = new DiscordEmbed()
                    {
                        Author = new DiscordEmbedAuthor()
                        {
                            Name = "Tiaqo",
                            Url = "http://platoon.pet"
                        },
                        Color = 333,
                        Description = "Description",
                        Fields = new List<DiscordEmbedField>()
                        {
                            new DiscordEmbedField()
                            {
                                Inline = false,
                                Name = "Name",
                                Value = "Value"
                            }
                        },
                        Footer = new DiscordEmbedFooter()
                        {
                            Text = "Footer"
                        },
                        Image = new DiscordEmbedImage()
                        {
                            Height = 218,
                            Width = 371,
                            Url = "https://cdn.discordapp.com/attachments/146044397861994496/232093592582094848/unknown.png"
                        },
                        Provider = new DiscordEmbedProvider()
                        {
                            Name = "Provider"
                        },
                        Timestamp = DateTime.Now,
                        Title = "Title",
                        Type = "rich",
                        Url = "https://google.com"
                    };

                    System.IO.File.WriteAllText("embed.json", JsonConvert.SerializeObject(embed));

                    System.IO.File.WriteAllText("message.json", JsonConvert.SerializeObject(e.Channel.SendMessageWithEmbeds(e.args[0], new DiscordEmbed[] { embed }), Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore } ) );
                }
                ));
            client.AddCommand(DiscordCommand.Create("create")
                .AddParameter("name")
                .Do(e =>
                {
                    Hook.Webhook hook = e.Channel.CreateWebhook(e.args[0]);
                    hook.SendSlack(new JObject()
                    {
                        { "text", "New webhook created" },
                        { "attachments", new JArray() { new JObject() { { "color", "#36a64f" }, { "pretext", hook.Token }, { "author_name", $"{hook.Name} [{hook.ID}]" }, { "author_icon", hook.User.GetAvatarURL() }, { "title", hook.Name }, { "text", hook.Token }, { "image_url", hook.Avatar } } } }
                    }, false);
                }
                ));

            client.AddCommand(DiscordCommand.Create("list")
                .Do(e =>
                {
                    Hook.Webhook fHook = null;

                    JObject message = new JObject();
                    message.Add("text", "List of Webhooks");
                    JArray attachments = new JArray();
                    foreach (Hook.Webhook hook in e.Channel.GetWebhooks())
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
