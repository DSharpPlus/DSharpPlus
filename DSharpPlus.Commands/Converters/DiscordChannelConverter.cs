using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordChannelConverter : ISlashArgumentConverter<DiscordChannel>, ITextArgumentConverter<DiscordChannel>
{
    [GeneratedRegex(@"^<#(\d+)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    public static partial Regex GetChannelMatchingRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Channel;
    public string ReadableName => "Discord Channel";
    public bool RequiresText => true;

    public async Task<Optional<DiscordChannel>> ConvertAsync(ConverterContext context)
    {
        if (context is InteractionConverterContext interactionConverterContext
            // Resolved can be null on autocomplete contexts
            && interactionConverterContext.Interaction.Data.Resolved is not null
            // Check if we can parse the channel ID (this should be guaranteed by Discord)
            && ulong.TryParse(interactionConverterContext.Argument?.RawValue, CultureInfo.InvariantCulture, out ulong channelId)
            // Check if the channel is in the resolved data
            && interactionConverterContext.Interaction.Data.Resolved.Channels.TryGetValue(channelId, out DiscordChannel? channel))
        {
            return Optional.FromValue(channel);
        }

        // If the guild is null, return.
        // We don't want to search for channels
        // in DMs or other external guilds.
        if (context.Guild is null)
        {
            return Optional.FromNoValue<DiscordChannel>();
        }

        string? channelIdString = context.Argument?.ToString();
        if (string.IsNullOrWhiteSpace(channelIdString))
        {
            return Optional.FromNoValue<DiscordChannel>();
        }

        // Attempt to parse the channel id
        if (!ulong.TryParse(channelIdString, CultureInfo.InvariantCulture, out channelId))
        {
            // Value could be a channel mention.
            Match match = GetChannelMatchingRegex().Match(channelIdString);
            if (!match.Success || !ulong.TryParse(match.Groups[1].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out channelId))
            {
                // Try searching by name
                DiscordChannel? namedChannel = context.Guild.Channels.Values.FirstOrDefault(channel => channel.Name.Equals(channelIdString, StringComparison.OrdinalIgnoreCase));
                return namedChannel is not null ? Optional.FromValue(namedChannel) : Optional.FromNoValue<DiscordChannel>();
            }
        }

        try
        {
            // Get channel async will search the guild cache for the channel
            // or thread, if it's not found, it will fetch it from the API
            return Optional.FromValue(await context.Guild.GetChannelAsync(channelId));
        }
        catch (DiscordException)
        {
            return Optional.FromNoValue<DiscordChannel>();
        }
    }
}
