using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.CommandAll.Converters
{
    public partial class DiscordMemberConverter : ISlashArgumentConverter<DiscordMember>, ITextArgumentConverter<DiscordMember>
    {
        [GeneratedRegex(@"^<@\!?(\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
        private static partial Regex _getMemberRegex();

        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.User;
        public bool RequiresText { get; init; } = true;
        private readonly ILogger<DiscordMemberConverter> _logger;

        public DiscordMemberConverter(ILogger<DiscordMemberConverter>? logger = null) => _logger = logger ?? NullLogger<DiscordMemberConverter>.Instance;

        public async Task<Optional<DiscordMember>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
        {
            if (context.Guild is null)
            {
                return Optional.FromNoValue<DiscordMember>();
            }

            string value = context.As<TextConverterContext>().CurrentTextArgument;
            if (!ulong.TryParse(value, CultureInfo.InvariantCulture, out ulong memberId))
            {
                Match match = _getMemberRegex().Match(value);
                if (!match.Success || !ulong.TryParse(match.Captures[0].ValueSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out memberId))
                {
                    // Attempt to find a member by name, case sensitive.
                    DiscordMember? namedMember = context.Guild.Members.Values.FirstOrDefault(member => member.DisplayName.Equals(value, StringComparison.Ordinal));
                    return namedMember is not null ? Optional.FromValue(namedMember) : Optional.FromNoValue<DiscordMember>();
                }
            }

            try
            {
                DiscordMember? possiblyCachedMember = await context.Guild.GetMemberAsync(memberId);
                return possiblyCachedMember is not null ? Optional.FromValue(possiblyCachedMember) : Optional.FromNoValue<DiscordMember>();
            }
            catch (DiscordException error)
            {
                _logger.LogError(error, "Failed to get member from guild.");
                return Optional.FromNoValue<DiscordMember>();
            }
        }

        public Task<Optional<DiscordMember>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            SlashConverterContext slashContext = context.As<SlashConverterContext>();
            return Task.FromResult(Optional.FromValue(slashContext.Interaction.Data.Resolved.Members[(ulong)slashContext.CurrentOption.Value]));
        }
    }
}
