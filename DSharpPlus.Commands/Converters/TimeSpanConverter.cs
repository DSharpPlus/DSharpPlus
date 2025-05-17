using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public partial class TimeSpanConverter : ISlashArgumentConverter<TimeSpan>, ITextArgumentConverter<TimeSpan>
{
    [GeneratedRegex(
        @"^((?<years>\d+)y\s*)?((?<months>\d+)mo\s*)?((?<weeks>\d+)w\s*)?((?<days>\d+)d\s*)?((?<hours>\d+)h\s*)?((?<minutes>\d+)m\s*)?((?<seconds>\d+)s\s*)?((?<milliseconds>\d+)ms\s*)?((?<microseconds>\d+)(µs|us)\s*)?((?<nanoseconds>\d+)ns\s*)?$",
        RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.RightToLeft | RegexOptions.CultureInvariant
    )]
    private static partial Regex GetTimeSpanRegex();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Duration";

    public Task<Optional<TimeSpan>> ConvertAsync(ConverterContext context)
    {
        string? value = context.Argument?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult(Optional.FromNoValue<TimeSpan>());
        }
        else if (value == "0")
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
            Match match = GetTimeSpanRegex().Match(value);
            if (!match.Success)
            {
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());
            }

            int years = match.Groups["years"].Success ? int.Parse(match.Groups["years"].Value, CultureInfo.InvariantCulture) : 0;
            int months = match.Groups["months"].Success ? int.Parse(match.Groups["months"].Value, CultureInfo.InvariantCulture) : 0;
            int weeks = match.Groups["weeks"].Success ? int.Parse(match.Groups["weeks"].Value, CultureInfo.InvariantCulture) : 0;
            int days = match.Groups["days"].Success ? int.Parse(match.Groups["days"].Value, CultureInfo.InvariantCulture) : 0;
            int hours = match.Groups["hours"].Success ? int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture) : 0;
            int minutes = match.Groups["minutes"].Success ? int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture) : 0;
            int seconds = match.Groups["seconds"].Success ? int.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture) : 0;
            int milliseconds = match.Groups["milliseconds"].Success ? int.Parse(match.Groups["milliseconds"].Value, CultureInfo.InvariantCulture) : 0;
            int microseconds = match.Groups["microseconds"].Success ? int.Parse(match.Groups["microseconds"].Value, CultureInfo.InvariantCulture) : 0;
            int nanoseconds = match.Groups["nanoseconds"].Success ? int.Parse(match.Groups["nanoseconds"].Value, CultureInfo.InvariantCulture) : 0;
            result = new TimeSpan(
                ticks: (years * TimeSpan.TicksPerDay * 365)
                    + (months * TimeSpan.TicksPerDay * 30)
                    + (weeks * TimeSpan.TicksPerDay * 7)
                    + (days * TimeSpan.TicksPerDay)
                    + (hours * TimeSpan.TicksPerHour)
                    + (minutes * TimeSpan.TicksPerMinute)
                    + (seconds * TimeSpan.TicksPerSecond)
                    + (milliseconds * TimeSpan.TicksPerMillisecond)
                    + (microseconds * TimeSpan.TicksPerMicrosecond)
                    + (nanoseconds * TimeSpan.NanosecondsPerTick)
            );

            return Task.FromResult(Optional.FromValue(result));
        }
    }
}
