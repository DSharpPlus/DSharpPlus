using System;

namespace DSharpPlus.CommandsNext.Converters
{
    public class DateTimeConverter : IArgumentConverter<DateTime>
    {
        public bool TryConvert(string value, CommandContext ctx, out DateTime result) =>
            DateTime.TryParse(value, out result);
    }

    public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
    {
        public bool TryConvert(string value, CommandContext ctx, out DateTimeOffset result) =>
            DateTimeOffset.TryParse(value, out result);
    }

    public class TimeSpanConverter : IArgumentConverter<TimeSpan>
    {
        public bool TryConvert(string value, CommandContext ctx, out TimeSpan result) =>
            TimeSpan.TryParse(value, out result);
    }
}
