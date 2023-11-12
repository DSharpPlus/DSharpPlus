namespace DSharpPlus.CommandAll.Converters;

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public partial class DiscordThreadChannelConverter : ISlashArgumentConverter<DiscordThreadChannel>, ITextArgumentConverter<DiscordThreadChannel>
{
    [GeneratedRegex(@"^<#(\d+)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex _getChannelRegex();

    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Channel;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<DiscordThreadChannel>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
    {
        if (context.Guild is null)
        {
            return Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
        }

        string value = context.As<TextConverterContext>().Argument;
        if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out ulong channelId))
        {
            // value can be a raw channel id or a channel mention. The regex will match both.
            Match match = _getChannelRegex().Match(value);
            if (!match.Success || !ulong.TryParse(match.Captures[0].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out channelId))
            {
                // Attempt to find a thread channel by name, case sensitive.
                DiscordThreadChannel? namedChannel = context.Guild.Threads.Values.FirstOrDefault(channel => channel.Name.Equals(value, StringComparison.Ordinal));
                return Task.FromResult(namedChannel is not null ? Optional.FromValue(namedChannel) : Optional.FromNoValue<DiscordThreadChannel>());
            }
        }

        return context.Guild.Threads.TryGetValue(channelId, out DiscordThreadChannel? threadChannel) && threadChannel is not null
            ? Task.FromResult(Optional.FromValue(threadChannel))
            : Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
    }

    public Task<Optional<DiscordThreadChannel>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        SlashConverterContext slashContext = context.As<SlashConverterContext>();
        return Task.FromResult(Optional.FromValue((DiscordThreadChannel)slashContext.Interaction.Data.Resolved.Channels[(ulong)slashContext.Argument.Value]));
    }
}
