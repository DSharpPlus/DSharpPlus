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

public partial class DiscordUserConverter
    : ISlashArgumentConverter<DiscordUser>,
        ITextArgumentConverter<DiscordUser>
{
    [GeneratedRegex("""^<@!?(\d+?)>$""", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex getMemberRegex();

    public DiscordApplicationCommandOptionType ParameterType =>
        DiscordApplicationCommandOptionType.User;
    public string ReadableName => "Discord User";
    public bool RequiresText => true;

    public async Task<Optional<DiscordUser>> ConvertAsync(ConverterContext context)
    {
        if (
            context is InteractionConverterContext interactionContext
            // Resolved can be null on autocomplete contexts
            && interactionContext.Interaction.Data.Resolved is not null
            // Check if we can parse the member ID (this should be guaranteed by Discord)
            && ulong.TryParse(
                interactionContext.Argument?.RawValue,
                CultureInfo.InvariantCulture,
                out ulong memberId
            )
            // Check if the member is in the resolved data
            && interactionContext.Interaction.Data.Resolved.Users.TryGetValue(
                memberId,
                out DiscordUser? user
            )
        )
        {
            return Optional.FromValue(user);
        }

        string? value = context.Argument?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Optional.FromNoValue<DiscordUser>();
        }

        // Try parsing by the member id
        if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out memberId))
        {
            // Try parsing through a member mention
            Match match = getMemberRegex().Match(value);
            if (
                !match.Success
                || !ulong.TryParse(
                    match.Groups[1].ValueSpan,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out memberId
                )
            )
            {
                // If this is invoked in a guild, try to get the member first.
                if (
                    context.Guild is not null
                    && context.Guild.Members.Values.FirstOrDefault(member =>
                        member.DisplayName.Equals(value, StringComparison.Ordinal)
                    )
                        is DiscordMember namedMember
                )
                {
                    // Attempt to find a member by name, case sensitive.
                    return Optional.FromValue<DiscordUser>(namedMember);
                }

                // An invalid user id was passed and we couldn't find a member by name.
                return Optional.FromNoValue<DiscordUser>();
            }
        }

        // Search the guild cache first. We want to allow the dev to
        // try casting to a member for the most amount of information available.
        if (
            context.Guild is not null
            && context.Guild.Members.TryGetValue(memberId, out DiscordMember? member)
        )
        {
            return Optional.FromValue<DiscordUser>(member);
        }

        // If we didn't find the user in the guild, try to get the user from the API.
        try
        {
            return Optional.FromValue(await context.Client.GetUserAsync(memberId));
        }
        catch (DiscordException)
        {
            return Optional.FromNoValue<DiscordUser>();
        }
    }
}
