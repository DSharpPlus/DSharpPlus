using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordChannelConverter : ISlashArgumentConverter<DiscordChannel>, ITextArgumentConverter<DiscordChannel>
{
    [GeneratedRegex(@"^<#(\d+)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex getChannelRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Channel;
    public string ReadableName => "Discord Channel";
    public bool RequiresText => true;

    public Task<Optional<DiscordChannel>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs)
    {
        // Attempt to parse the channel id
        if (!ulong.TryParse(context.Argument, CultureInfo.InvariantCulture, out ulong channelId))
        {
            // Value could be a channel mention.
            Match match = getChannelRegex().Match(context.Argument);
            if (!match.Success || !ulong.TryParse(match.Groups[1].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out channelId))
            {
                // Attempt to find a channel by name, case insensitive.
                DiscordChannel? namedChannel = context.Guild!.Channels.Values.FirstOrDefault(channel => channel.Name.Equals(context.Argument, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(namedChannel is not null ? Optional.FromValue(namedChannel) : Optional.FromNoValue<DiscordChannel>());
            }
        }

        if (context.Guild is null)
        {
            return channelId == context.User.Id && context.Client.PrivateChannels.TryGetValue(channelId, out DiscordDmChannel? dmChannel)
                ? Task.FromResult(Optional.FromValue<DiscordChannel>(dmChannel))
                : Task.FromResult(Optional.FromNoValue<DiscordChannel>());
        }
        else if (context.Guild.Channels.TryGetValue(channelId, out DiscordChannel? guildChannel))
        {
            return Task.FromResult(Optional.FromValue(guildChannel));
        }
        else if (context.Guild.Threads.TryGetValue(channelId, out DiscordThreadChannel? threadChannel))
        {
            return Task.FromResult(Optional.FromValue<DiscordChannel>(threadChannel));
        }

        return Task.FromResult(Optional.FromNoValue<DiscordChannel>());
    }

    public Task<Optional<DiscordChannel>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => context.Interaction.Data.Resolved is null
        || !ulong.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out ulong channelId)
        || !context.Interaction.Data.Resolved.Channels.TryGetValue(channelId, out DiscordChannel? channel)
            ? Task.FromResult(Optional.FromNoValue<DiscordChannel>())
            : Task.FromResult(Optional.FromValue(channel));
}
