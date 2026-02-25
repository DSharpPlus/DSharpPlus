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

public partial class DiscordMemberConverter : ISlashArgumentConverter<DiscordMember>, ITextArgumentConverter<DiscordMember>
{
    [GeneratedRegex("""^<@!?(\d+?)>$""", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    public static partial Regex GetMemberRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.User;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Discord Server Member";

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
                // try username first
                if (context.Parameter.Attributes.Any(x => x.GetType() == typeof(DisableUsernameFuzzyMatchingAttribute)))
                {
                    if (context.Guild.Members.Values.FirstOrDefault(member => member.Username.Equals(value, StringComparison.InvariantCultureIgnoreCase)) 
                        is DiscordMember memberByUsername)
                    {
                        return Optional.FromValue(memberByUsername);
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
                        return Optional.FromValue(fuzzyDisplayNameMember);
                    }
                }
                else
                {
                    // match them all and return the highest matching member at a score of 85 or higher
                    // unfortunately, the tuple loses its names, so we can't give this readable names
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
                        return Optional.FromValue(highestScoringUsername);
                    }
                    
                    // then by display name
                    DiscordMember? highestScoringDisplayName = sortedMembers.OrderByDescending(x => x.Item3)
                        .FirstOrDefault(x => x.Item3 >= 85)
                        .Item1;
                    
                    if (highestScoringDisplayName is not null)
                    {
                        return Optional.FromValue(highestScoringDisplayName);
                    }
                }
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
