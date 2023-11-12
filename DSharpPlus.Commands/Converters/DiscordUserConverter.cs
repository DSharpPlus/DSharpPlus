namespace DSharpPlus.Commands.Converters;

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

public partial class DiscordUserConverter : ISlashArgumentConverter<DiscordUser>, ITextArgumentConverter<DiscordUser>
{
    [GeneratedRegex("""^<@!?(\d+?)>$""", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex _getMemberRegex();

    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.User;
    public bool RequiresText { get; init; } = true;
    private readonly ILogger<DiscordUserConverter> _logger;

    public DiscordUserConverter(ILogger<DiscordUserConverter>? logger = null) => this._logger = logger ?? NullLogger<DiscordUserConverter>.Instance;

    public async Task<Optional<DiscordUser>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
    {
        if (context.Guild is null)
        {
            return Optional.FromNoValue<DiscordUser>();
        }

        string value = context.As<TextConverterContext>().Argument;
        if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out ulong memberId))
        {
            Match match = _getMemberRegex().Match(value);
            if (!match.Success || !ulong.TryParse(match.Groups[1].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out memberId))
            {
                // Attempt to find a member by name, case sensitive.
                DiscordUser? namedMember = context.Guild.Members.Values.FirstOrDefault(member => member.DisplayName.Equals(value, StringComparison.Ordinal));
                return namedMember is not null ? Optional.FromValue(namedMember) : Optional.FromNoValue<DiscordUser>();
            }
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
            this._logger.LogError(error, "Failed to get user from client.");
        }

        return Optional.FromNoValue<DiscordUser>();
    }

    public Task<Optional<DiscordUser>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        SlashConverterContext slashContext = context.As<SlashConverterContext>();
        return Task.FromResult(Optional.FromValue(slashContext.Interaction.Data.Resolved.Users[(ulong)slashContext.Argument.Value]));
    }
}
