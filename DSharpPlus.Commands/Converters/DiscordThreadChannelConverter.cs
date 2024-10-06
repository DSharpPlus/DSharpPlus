using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordThreadChannelConverter : ISlashArgumentConverter<DiscordThreadChannel>, ITextArgumentConverter<DiscordThreadChannel>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Channel;
    public string ReadableName => "Discord Thread";
    public bool RequiresText => true;

    public Task<Optional<DiscordThreadChannel>> ConvertAsync(ConverterContext context)
    {
        if (context is InteractionConverterContext interactionConverterContext
            // Resolved can be null on autocomplete contexts
            && interactionConverterContext.Interaction.Data.Resolved is not null
            // Check if we can parse the channel ID (this should be guaranteed by Discord)
            && ulong.TryParse(interactionConverterContext.Argument?.RawValue, CultureInfo.InvariantCulture, out ulong channelId)
            // Check if the channel is in the resolved data
            && interactionConverterContext.Interaction.Data.Resolved.Channels.TryGetValue(channelId, out DiscordChannel? channel))
        {
            return Task.FromResult(Optional.FromValue((DiscordThreadChannel)channel));
        }

        // Threads cannot exist outside of guilds.
        if (context.Guild is null)
        {
            return Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
        }

        string? value = context.Argument?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
        }

        // Try parsing by the channel id
        if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out channelId))
        {
            // value can be a raw channel id or a channel mention. The regex will match both.
            Match match = DiscordChannelConverter.GetChannelMatchingRegex().Match(value);
            if (!match.Success || !ulong.TryParse(match.Captures[0].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out channelId))
            {
                // Attempt to find a thread channel by name, case sensitive.
                DiscordThreadChannel? namedChannel = context.Guild.Threads.Values.FirstOrDefault(channel => channel.Name.Equals(value, StringComparison.Ordinal));
                return namedChannel is not null
                    ? Task.FromResult(Optional.FromValue(namedChannel))
                    : Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
            }
        }

        return context.Guild.Threads.TryGetValue(channelId, out DiscordThreadChannel? threadChannel) && threadChannel is not null
            ? Task.FromResult(Optional.FromValue(threadChannel))
            : Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
    }
}
