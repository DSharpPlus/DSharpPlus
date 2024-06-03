using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordUserConverter : ISlashArgumentConverter<DiscordUser>, ITextArgumentConverter<DiscordUser>
{
    [GeneratedRegex("""^<@!?(\d+?)>$""", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex getMemberRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.User;
    public string ReadableName => "Discord User";
    public bool RequiresText => true;

    private readonly ILogger<DiscordUserConverter> logger;

    public DiscordUserConverter(ILogger<DiscordUserConverter>? logger = null) => this.logger = logger ?? NullLogger<DiscordUserConverter>.Instance;

    public async Task<Optional<DiscordUser>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs)
    {
        if (context.Guild is null)
        {
            return Optional.FromNoValue<DiscordUser>();
        }

        if (!ulong.TryParse(context.Argument, CultureInfo.InvariantCulture, out ulong memberId))
        {
            Match match = getMemberRegex().Match(context.Argument);
            if (!match.Success || !ulong.TryParse(match.Groups[1].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out memberId))
            {
                // Attempt to find a member by name, case sensitive.
                DiscordUser? namedMember = context.Guild.Members.Values.FirstOrDefault(member => member.DisplayName.Equals(context.Argument, StringComparison.Ordinal));
                return namedMember is not null ? Optional.FromValue(namedMember) : Optional.FromNoValue<DiscordUser>();
            }
        }

        // Search the guild first
        if (context.Guild.Members.TryGetValue(memberId, out DiscordMember? member))
        {
            return Optional.FromValue<DiscordUser>(member);
        }

        // If we didn't find the user in the guild, try to get the user from the API.
        try
        {
            DiscordUser? possiblyCachedUser = await context.Client.GetUserAsync(memberId);
            if (possiblyCachedUser is not null)
            {
                return Optional.FromValue(possiblyCachedUser);
            }
        }
        catch (DiscordException error)
        {
            this.logger.LogError(error, "Failed to get user from client.");
        }

        return Optional.FromNoValue<DiscordUser>();
    }

    public Task<Optional<DiscordUser>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => context.Interaction.Data.Resolved is null
        || !ulong.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out ulong memberId)
        || !context.Interaction.Data.Resolved.Users.TryGetValue(memberId, out DiscordUser? user)
            ? Task.FromResult(Optional.FromNoValue<DiscordUser>())
            : Task.FromResult(Optional.FromValue(user));
}
