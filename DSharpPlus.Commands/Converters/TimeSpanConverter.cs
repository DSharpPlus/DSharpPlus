namespace DSharpPlus.Commands.Converters;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public partial class TimeSpanConverter : ISlashArgumentConverter<TimeSpan>, ITextArgumentConverter<TimeSpan>
{
    [GeneratedRegex("^((?<days>\\d+)d\\s*)?((?<hours>\\d+)h\\s*)?((?<minutes>\\d+)m\\s*)?((?<seconds>\\d+)s\\s*)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.RightToLeft | RegexOptions.CultureInvariant)]
    private static partial Regex _getTimeSpanRegex();

    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.String;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<TimeSpan>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context, context.Argument);
    public Task<Optional<TimeSpan>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context, context.Argument.RawValue);

    public static Task<Optional<TimeSpan>> ConvertAsync(ConverterContext context, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult(Optional.FromNoValue<TimeSpan>());
        }

        if (value == "0")
        {
            return Task.FromResult(Optional.FromValue(TimeSpan.Zero));
        }
        else if (int.TryParse(value, CultureInfo.InvariantCulture, out _))
        {
            return Task.FromResult(Optional.FromNoValue<TimeSpan>());
        }
        else if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan result))
        {
            return Task.FromResult(Optional.FromValue(result));
        }
        else
        {
            Match m = _getTimeSpanRegex().Match(value);
            int ds = m.Groups["days"].Success ? int.Parse(m.Groups["days"].Value, CultureInfo.InvariantCulture) : 0;
            int hs = m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value, CultureInfo.InvariantCulture) : 0;
            int ms = m.Groups["minutes"].Success ? int.Parse(m.Groups["minutes"].Value, CultureInfo.InvariantCulture) : 0;
            int ss = m.Groups["seconds"].Success ? int.Parse(m.Groups["seconds"].Value, CultureInfo.InvariantCulture) : 0;

            result = TimeSpan.FromSeconds((ds * 24 * 60 * 60) + (hs * 60 * 60) + (ms * 60) + ss);
            return result.TotalSeconds < 1
                ? Task.FromResult(Optional.FromNoValue<TimeSpan>())
                : Task.FromResult(Optional.FromValue(result));
        }
    }
}
