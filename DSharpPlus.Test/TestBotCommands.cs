using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Test
{
    public sealed class TestBotCommands
    {
        public async Task Test(CommandEventArgs e) =>
            await e.Channel.SendMessage("u w0t m8");

        public async Task Testerino(CommandEventArgs e)
        {
            await e.Discord.SendMessage(e.Channel, "ill bash ur head in i sweak on me fkin mum");
            await e.Discord.SendMessage(e.Message.ChannelID, $@"```
Servername: {e.Guild.Name}
Serverowner: {e.Guild.OwnerID}
```");
        }

        public async Task Kill(CommandEventArgs e)
        {
            await e.Channel.SendMessage("kthxbai 👋");
            e.Discord.Dispose();
            await Task.Delay(-1);
        }

        public async Task Restart(CommandEventArgs e) =>
            await e.Discord.Reconnect();

        public async Task PurgeChannel(CommandEventArgs e)
        {
            var ids = (await e.Channel.GetMessages(before: e.Message.ID, limit: 50))
                .Select(y => y.ID)
                .ToList();
            await e.Channel.BulkDeleteMessages(ids);
            await e.Message.Respond($"Removed `{ids.Count}` messages");
        }

        public async Task Presence(CommandEventArgs e) =>
            await e.Message.Respond(e.Author.Username);

        public async Task Guild(CommandEventArgs e)
        {
            var roles = e.Guild.Roles.Select(xr => xr.Mention);
            var overs = e.Channel.PermissionOverwrites.Select(xo => string.Concat("Principal: ", xo.ID, " (", xo.Type, "), Allow: ", (ulong)xo.Allow, "; Deny: ", (ulong)xo.Deny));

            var embed = new DiscordEmbed
            {
                Title = "Guild info",
                Description = "ye boiii!",
                Type = "rich",
                Color = 0x007FFF,
                Fields = new List<DiscordEmbedField>
                {
                    new DiscordEmbedField
                    {
                        Inline = false,
                        Name = "Roles",
                        Value = string.Join("\n", roles)
                    },
                    new DiscordEmbedField
                    {
                        Inline = false,
                        Name = string.Concat("Overrides for ", e.Channel.Mention),
                        Value = string.Join("\n", overs)
                    }
                }
            };

            await e.Message.Respond("", embed: embed);
        }

        public async Task Embed(CommandEventArgs e)
        {
            List<DiscordEmbedField> fields = new List<DiscordEmbedField>();
            fields.Add(new DiscordEmbedField()
            {
                Name = "This is a field",
                Value = "it works :p",
                Inline = false
            });
            fields.Add(new DiscordEmbedField()
            {
                Name = "Multiple fields",
                Value = "cool",
                Inline = false
            });

            DiscordEmbed embed = new DiscordEmbed
            {
                Title = "Testing embed",
                Description = "It works!",
                Type = "rich",
                Url = "https://github.com/NaamloosDT/DSharpPlus",
                Color = 8257469,
                Fields = fields,
                Author = new DiscordEmbedAuthor()
                {
                    Name = "DSharpPlus team",
                    IconUrl = "https://raw.githubusercontent.com/NaamloosDT/DSharpPlus/master/logo_smaller.png",
                    Url = "https://github.com/NaamloosDT/DSharpPlus"
                },
                Footer = new DiscordEmbedFooter()
                {
                    Text = "I am a footer"
                },
                Image = new DiscordEmbedImage()
                {
                    Url = "https://raw.githubusercontent.com/NaamloosDT/DSharpPlus/master/logo_smaller.png",
                    Height = 50,
                    Width = 50,
                },
                Thumbnail = new DiscordEmbedThumbnail()
                {
                    Url = "https://raw.githubusercontent.com/NaamloosDT/DSharpPlus/master/logo_smaller.png",
                    Height = 10,
                    Width = 10
                }
            };
            await e.Message.Respond("testing embed:", embed: embed);
        }

        public async Task AppInfo(CommandEventArgs e)
        {
            var app = await e.Discord.GetCurrentApp();
            var usrn = app.Owner.Username
                .Replace(@"\", @"\\")
                .Replace(@"*", @"\*")
                .Replace(@"_", @"\_")
                .Replace(@"~", @"\~")
                .Replace(@"`", @"\`");

            var embed = new DiscordEmbed
            {
                Title = "Application info",
                Color = 0x007FFF,
                Fields = new List<DiscordEmbedField>()
            };
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Name", Value = app.Name });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Description", Value = string.Concat("```\n", app.Description, "\n```") });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "ID", Value = app.ID.ToString() });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Created", Value = app.CreationDate.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Owner", Value = $"{usrn}#{app.Owner.Discriminator.ToString("0000")} ({app.Owner.ID})" });

            await e.Message.Respond("", embed: embed);
        }

        public async Task Mention(CommandEventArgs e) =>
            await e.Message.Respond($"User {e.Author.Mention} invoked this from {e.Channel.Mention}");

        public async Task ModifyMe(CommandEventArgs e) =>
            await e.Discord.ModifyMember(e.Guild.ID, e.Author.ID, "Tests D#+ instead of going outside");

        public async Task VoiceJoin(CommandEventArgs e) =>
            await Task.Yield();

        public async Task VoiceLeave(CommandEventArgs e) =>
            await Task.Yield();

        public async Task VoicePlay(CommandEventArgs e) =>
            await Task.Yield();
    }
}
