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

public partial class DiscordRoleConverter : ISlashArgumentConverter<DiscordRole>, ITextArgumentConverter<DiscordRole>
{
    [GeneratedRegex(@"^<@&(\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex _getRoleRegex();

    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Role;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<DiscordRole>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
    {
        if (context.Guild is null)
        {
            return Task.FromResult(Optional.FromNoValue<DiscordRole>());
        }

        string value = context.As<TextConverterContext>().Argument;
        if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out ulong roleId))
        {
            // value can be a raw channel id or a channel mention. The regex will match both.
            Match match = _getRoleRegex().Match(value);
            if (!match.Success || !ulong.TryParse(match.Captures[0].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out roleId))
            {
                // Attempt to find a role by name, case sensitive.
                DiscordRole? namedRole = context.Guild.Roles.Values.FirstOrDefault(role => role.Name.Equals(value, StringComparison.Ordinal));
                return Task.FromResult(namedRole is not null ? Optional.FromValue(namedRole) : Optional.FromNoValue<DiscordRole>());
            }
        }

        return context.Guild.GetRole(roleId) is DiscordRole guildRole
            ? Task.FromResult(Optional.FromValue(guildRole))
            : Task.FromResult(Optional.FromNoValue<DiscordRole>());
    }

    public Task<Optional<DiscordRole>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        SlashConverterContext slashContext = context.As<SlashConverterContext>();
        return Task.FromResult(Optional.FromValue(slashContext.Interaction.Data.Resolved.Roles[(ulong)slashContext.Argument.Value]));
    }
}
