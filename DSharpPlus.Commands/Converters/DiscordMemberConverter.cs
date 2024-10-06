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

public partial class DiscordMemberConverter : ISlashArgumentConverter<DiscordMember>, ITextArgumentConverter<DiscordMember>
{
    [GeneratedRegex("""^<@!?(\d+?)>$""", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    public static partial Regex GetMemberRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.User;
    public string ReadableName => "Discord Server Member";
    public bool RequiresText => true;

    public async Task<Optional<DiscordMember>> ConvertAsync(ConverterContext context)
    {
        if (context is InteractionConverterContext interactionConverterContext
            // Resolved can be null on autocomplete contexts
            && interactionConverterContext.Interaction.Data.Resolved is not null
            // Check if we can parse the member ID (this should be guaranteed by Discord)
            && ulong.TryParse(interactionConverterContext.Argument?.RawValue, CultureInfo.InvariantCulture, out ulong memberId)
            // Check if the member is in the resolved data
            && interactionConverterContext.Interaction.Data.Resolved.Members.TryGetValue(memberId, out DiscordMember? member))
        {
            return Optional.FromValue(member);
        }

        // How the fuck are we gonna get a member from a null guild.
        if (context.Guild is null)
        {
            return Optional.FromNoValue<DiscordMember>();
        }

        string? value = context.Argument?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Optional.FromNoValue<DiscordMember>();
        }

        // Try parsing by the member id
        if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out memberId))
        {
            // Try parsing through a member mention
            Match match = GetMemberRegex().Match(value);
            if (!match.Success || !ulong.TryParse(match.Groups[1].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out memberId))
            {
                // Try to find a member by name, case sensitive.
                DiscordMember? namedMember = context.Guild.Members.Values.FirstOrDefault(member => member.DisplayName.Equals(value, StringComparison.Ordinal));
                return namedMember is not null
                    ? Optional.FromValue(namedMember)
                    : Optional.FromNoValue<DiscordMember>();
            }
        }

        try
        {
            // GetMemberAsync will search the member cache first, then fetch the member from the API if not found.
            member = await context.Guild.GetMemberAsync(memberId);
            return member is not null
                ? Optional.FromValue(member)
                : Optional.FromNoValue<DiscordMember>();
        }
        catch (DiscordException)
        {
            // Not logging because users can intentionally give us incorrect data to intentionally spam logs.
            return Optional.FromNoValue<DiscordMember>();
        }
    }
}
