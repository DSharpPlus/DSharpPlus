using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Raffinert.FuzzySharp;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordUserConverter : ISlashArgumentConverter<DiscordUser>, ITextArgumentConverter<DiscordUser>
{
    [GeneratedRegex("""^<@!?(\d+?)>$""", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    public static partial Regex GetMemberRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.User;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Discord User";

    public async Task<Optional<DiscordUser>> ConvertAsync(ConverterContext context)
    {
        if (context is InteractionConverterContext interactionContext
            // Resolved can be null on autocomplete contexts
            && interactionContext.Interaction.Data.Resolved is not null
            // Check if we can parse the member ID (this should be guaranteed by Discord)
            && ulong.TryParse(interactionContext.Argument?.RawValue, CultureInfo.InvariantCulture, out ulong memberId)
            // Check if the member is in the resolved data
            && interactionContext.Interaction.Data.Resolved.Users.TryGetValue(memberId, out DiscordUser? user))
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
            Match match = GetMemberRegex().Match(value);
            if (!match.Success || !ulong.TryParse(match.Groups[1].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out memberId))
            {
                // If this is invoked in a guild, try to get the member first.
                if (context.Guild is not null)
                {
                    // try username first
                    if (context.Parameter.Attributes.Any(x => x.GetType() == typeof(DisableUsernameFuzzyMatchingAttribute)))
                    {
                        if (context.Guild.Members.Values.FirstOrDefault(member => member.Username.Equals(value, StringComparison.InvariantCultureIgnoreCase)) 
                            is DiscordMember memberByUsername)
                        {
                            return Optional.FromValue<DiscordUser>(memberByUsername);
                        }
                        
                        // then try display name
                        DiscordMember? fuzzyDisplayNameMember = context.Guild.Members.Values.Select(x => new
                            {
                                Member = x, 
                                Ratio = Fuzz.Ratio(x.DisplayName, value)
                            })
                            .OrderByDescending(x => x.Ratio)
                            .FirstOrDefault(x => x.Ratio >= 85)
                            ?.Member;

                        if (fuzzyDisplayNameMember is not null)
                        {
                            return Optional.FromValue<DiscordUser>(fuzzyDisplayNameMember);
                        }
                    }
                    else
                    {
                        // match them all and return the highest matching member at a score of 85 or higher
                        // unfortunately, that tuple loses its names, but its okay, still helps with readability here
                        IEnumerable<(DiscordMember?, int, int)> sortedMembers = context.Guild.Members.Values.Select(x => 
                        (
                            Item1: x,
                            Item2: Fuzz.Ratio(x.Username, value),
                            Item3: Fuzz.Ratio(x.DisplayName, value)
                        ));

                        DiscordMember? highestScoringUsername = sortedMembers.OrderByDescending(x => x.Item2)
                            .FirstOrDefault(x => x.Item2 >= 85)
                            .Item1;

                        if (highestScoringUsername is not null)
                        {
                            return Optional.FromValue<DiscordUser>(highestScoringUsername);
                        }
                        
                        // then by display name
                        DiscordMember? highestScoringDisplayName = sortedMembers.OrderByDescending(x => x.Item3)
                            .FirstOrDefault(x => x.Item3 >= 85)
                            .Item1;

                        if (highestScoringDisplayName is not null)
                        {
                            return Optional.FromValue<DiscordUser>(highestScoringDisplayName);
                        }
                    }
                }

                // An invalid user id was passed and we couldn't find a member by name.
                return Optional.FromNoValue<DiscordUser>();
            }
        }

        // Search the guild cache first. We want to allow the dev to
        // try casting to a member for the most amount of information available.
        if (context.Guild is not null && context.Guild.Members.TryGetValue(memberId, out DiscordMember? member))
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
