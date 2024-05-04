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

public partial class DiscordThreadChannelConverter : ISlashArgumentConverter<DiscordThreadChannel>, ITextArgumentConverter<DiscordThreadChannel>
{
    [GeneratedRegex(@"^<#(\d+)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex getChannelRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Channel;
    public string ReadableName => "Discord Thread";
    public bool RequiresText => true;

    public Task<Optional<DiscordThreadChannel>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs)
    {
        if (context.Guild is null)
        {
            return Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
        }

        if (!ulong.TryParse(context.Argument, CultureInfo.InvariantCulture, out ulong channelId))
        {
            // value can be a raw channel id or a channel mention. The regex will match both.
            Match match = getChannelRegex().Match(context.Argument);
            if (!match.Success || !ulong.TryParse(match.Captures[0].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out channelId))
            {
                // Attempt to find a thread channel by name, case sensitive.
                DiscordThreadChannel? namedChannel = context.Guild.Threads.Values.FirstOrDefault(channel => channel.Name.Equals(context.Argument, StringComparison.Ordinal));
                return Task.FromResult(namedChannel is not null ? Optional.FromValue(namedChannel) : Optional.FromNoValue<DiscordThreadChannel>());
            }
        }

        return context.Guild.Threads.TryGetValue(channelId, out DiscordThreadChannel? threadChannel) && threadChannel is not null
            ? Task.FromResult(Optional.FromValue(threadChannel))
            : Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>());
    }

    public Task<Optional<DiscordThreadChannel>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => context.Interaction.Data.Resolved is null
        || !ulong.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out ulong channelId)
        || !context.Interaction.Data.Resolved.Channels.TryGetValue(channelId, out DiscordChannel? channel)
            ? Task.FromResult(Optional.FromNoValue<DiscordThreadChannel>())
            : Task.FromResult(Optional.FromValue((DiscordThreadChannel)channel));
}
