namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

public partial class DiscordMessageConverter : ISlashArgumentConverter<DiscordMessage>, ITextArgumentConverter<DiscordMessage>
{
    [GeneratedRegex(@"\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex GetMessageRegex();

    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.String;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<DiscordMessage>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context, context.As<TextConverterContext>().Argument);
    public Task<Optional<DiscordMessage>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context, context.As<SlashConverterContext>().Argument.Value.ToString());

    public static async Task<Optional<DiscordMessage>> ConvertAsync(ConverterContext context, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Optional.FromNoValue<DiscordMessage>();
        }

        Match match = GetMessageRegex().Match(value);
        if (!match.Success || !ulong.TryParse(match.Groups["message"].ValueSpan, CultureInfo.InvariantCulture, out ulong messageId))
        {
            return Optional.FromNoValue<DiscordMessage>();
        }

        if (!match.Groups.TryGetValue("channel", out Group? channelGroup) || !ulong.TryParse(channelGroup.ValueSpan, CultureInfo.InvariantCulture, out ulong channelId))
        {
            return Optional.FromNoValue<DiscordMessage>();
        }

        DiscordChannel? channel = null;
        if (match.Groups.TryGetValue("guild", out Group? guildGroup) && ulong.TryParse(guildGroup.ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong guildId) && context.Client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
        {
            // Make sure the message belongs to the guild
            if (guild.Id != context.Guild!.Id)
            {
                return Optional.FromNoValue<DiscordMessage>();
            }
            else if (guild.Channels.TryGetValue(channelId, out DiscordChannel? guildChannel))
            {
                channel = guildChannel;
            }
            // guildGroup is null which means the link used @me, which means DM's. At this point, we can only get the message if the DM is with the bot.
            else if (guildGroup is null && channelId == context.Client.CurrentUser.Id)
            {
                channel = context.Client.PrivateChannels.TryGetValue(context.User.Id, out DiscordDmChannel? dmChannel) ? dmChannel : null;
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
            message = null;
        }

        return message is not null ? Optional.FromValue(message) : Optional.FromNoValue<DiscordMessage>();
    }
}
