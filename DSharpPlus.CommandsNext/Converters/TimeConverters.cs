namespace DSharpPlus.CommandsNext.Converters;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;

public class DateTimeConverter : IArgumentConverter<DateTime>
{
    Task<Optional<DateTime>> IArgumentConverter<DateTime>.ConvertAsync(string value, CommandContext ctx) => DateTime.TryParse(value, ctx.CommandsNext.DefaultParserCulture, DateTimeStyles.None, out DateTime result)
            ? Task.FromResult(new Optional<DateTime>(result))
            : Task.FromResult(Optional.FromNoValue<DateTime>());
}

public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
{
    Task<Optional<DateTimeOffset>> IArgumentConverter<DateTimeOffset>.ConvertAsync(string value, CommandContext ctx) => DateTimeOffset.TryParse(value, ctx.CommandsNext.DefaultParserCulture, DateTimeStyles.None, out DateTimeOffset result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
}

public class TimeSpanConverter : IArgumentConverter<TimeSpan>
{
    private static Regex TimeSpanRegex { get; set; }

    static TimeSpanConverter() => TimeSpanRegex = new Regex(@"^((?<days>\d+)d\s*)?((?<hours>\d+)h\s*)?((?<minutes>\d+)m\s*)?((?<seconds>\d+)s\s*)?$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.RightToLeft | RegexOptions.Compiled);

    Task<Optional<TimeSpan>> IArgumentConverter<TimeSpan>.ConvertAsync(string value, CommandContext ctx)
    {
        if (value == "0")
        {
            return Task.FromResult(Optional.FromValue(TimeSpan.Zero));
        }

        if (int.TryParse(value, NumberStyles.Number, ctx.CommandsNext.DefaultParserCulture, out _))
        {
            return Task.FromResult(Optional.FromNoValue<TimeSpan>());
        }

        if (!ctx.Config.CaseSensitive)
        {
            value = value.ToLowerInvariant();
        }

        if (TimeSpan.TryParse(value, ctx.CommandsNext.DefaultParserCulture, out TimeSpan result))
        {
            return Task.FromResult(Optional.FromValue(result));
        }

        Match m = TimeSpanRegex.Match(value);

        int ds = m.Groups["days"].Success ? int.Parse(m.Groups["days"].Value) : 0;
        int hs = m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value) : 0;
        int ms = m.Groups["minutes"].Success ? int.Parse(m.Groups["minutes"].Value) : 0;
        int ss = m.Groups["seconds"].Success ? int.Parse(m.Groups["seconds"].Value) : 0;

        result = TimeSpan.FromSeconds((ds * 24 * 60 * 60) + (hs * 60 * 60) + (ms * 60) + ss);
        return result.TotalSeconds < 1 ? Task.FromResult(Optional.FromNoValue<TimeSpan>()) : Task.FromResult(Optional.FromValue(result));
    }
}
