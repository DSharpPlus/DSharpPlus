using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordMessageConverter : ISlashArgumentConverter<DiscordMessage>, ITextArgumentConverter<DiscordMessage>
{
    [GeneratedRegex(@"\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    public static partial Regex GetMessageRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public string ReadableName => "Discord Message Link";
    public bool RequiresText => true;

    public async Task<Optional<DiscordMessage>> ConvertAsync(ConverterContext context)
    {
        // Check if the parameter desires a message reply
        if (context is TextConverterContext textContext
            && textContext.Parameter.Attributes.OfType<TextMessageReplyAttribute>().FirstOrDefault() is TextMessageReplyAttribute replyAttribute)
        {
            // It requested a reply and we have one available.
            if (textContext.Message.ReferencedMessage is not null)
            {
                return Optional.FromValue(textContext.Message.ReferencedMessage);
            }
            // It required a reply and we don't have one.
            else if (replyAttribute.RequiresReply)
            {
                return Optional.FromNoValue<DiscordMessage>();
            }

            // It requested a reply and we don't have one, but it's not required.
        }

        string? value = context.Argument?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Optional.FromNoValue<DiscordMessage>();
        }

        Match match = GetMessageRegex().Match(value);
        if (!match.Success
            || !ulong.TryParse(match.Groups["message"].ValueSpan, CultureInfo.InvariantCulture, out ulong messageId)
            || !match.Groups.TryGetValue("channel", out Group? channelGroup)
            || !ulong.TryParse(channelGroup.ValueSpan, CultureInfo.InvariantCulture, out ulong channelId))
        {
            // Check to see if it's just a normal message id. If it is, try setting the channel to the current channel.
            if (ulong.TryParse(value, out messageId))
            {
                channelId = context.Channel.Id;
            }
            else
            {
                // Try to see if it's Discord weird "Copy Message ID" format (channelId-messageId)
                string[] parts = value.Split('-');
                if (parts.Length != 2 || !ulong.TryParse(parts[0], out channelId) || !ulong.TryParse(parts[1], out messageId))
                {
                    return Optional.FromNoValue<DiscordMessage>();
                }
            }
        }

        DiscordChannel? channel = null;
        if (match.Groups.TryGetValue("guild", out Group? guildGroup)
            && ulong.TryParse(guildGroup.ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong guildId)
            && context.Client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
        {
            // Make sure the message belongs to the guild
            if (guild.Id != context.Guild!.Id)
            {
                return Optional.FromNoValue<DiscordMessage>();
            }
            // guildGroup is null which means the link used @me, which means DM's. At this point, we can only get the message if the DM is with the bot.
            else if (guildGroup is null && channelId == context.Client.CurrentUser.Id)
            {
                channel = context.Client.PrivateChannels.TryGetValue(context.User.Id, out DiscordDmChannel? dmChannel) ? dmChannel : null;
            }
            else if (guild.Channels.TryGetValue(channelId, out DiscordChannel? guildChannel))
            {
                channel = guildChannel;
            }
            else if (guild.Threads.TryGetValue(channelId, out DiscordThreadChannel? threadChannel))
            {
                channel = threadChannel;
            }
        }

        if (channel is null)
        {
            return Optional.FromNoValue<DiscordMessage>();
        }

        DiscordMessage? message;
        try
        {
            message = await channel.GetMessageAsync(messageId);
        }
        catch (DiscordException)
        {
            // Not logging because users can intentionally give us incorrect data to intentionally spam logs.
            return Optional.FromNoValue<DiscordMessage>();
        }

        return Optional.FromValue(message);
    }
}
