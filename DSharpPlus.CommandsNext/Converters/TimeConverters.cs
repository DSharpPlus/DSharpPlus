using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DSharpPlus.CommandsNext.Converters
{
    public class DateTimeConverter : IArgumentConverter<DateTime>
    {
        public bool TryConvert(string value, CommandContext ctx, out DateTime result) 
            => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
    {
        public bool TryConvert(string value, CommandContext ctx, out DateTimeOffset result) 
            => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    public class TimeSpanConverter : IArgumentConverter<TimeSpan>
    {
        private static Regex TimeSpanRegex { get; set; }

        static TimeSpanConverter()
        {
#if NETSTANDARD1_1
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript);
#else
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        public bool TryConvert(string value, CommandContext ctx, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (value == "0")
                return true;

            if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                return false;

            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result))
                return true;
            
            var gps = new string[] { "days", "hours", "minutes", "seconds" };
            var mtc = TimeSpanRegex.Match(value);
            if (!mtc.Success)
                return false;

            var d = 0;
            var h = 0;
            var m = 0;
            var s = 0;
            foreach (var gp in gps)
            {
                var gpc = mtc.Groups[gp].Value;
                if (string.IsNullOrWhiteSpace(gpc))
                    continue;

                var gpt = gpc[gpc.Length - 1];
                int.TryParse(gpc.Substring(0, gpc.Length - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val);
                switch (gpt)
                {
                    case 'd':
                        d = val;
                        break;

                    case 'h':
                        h = val;
                        break;

                    case 'm':
                        m = val;
                        break;

                    case 's':
                        s = val;
                        break;
                }
            }
            result = new TimeSpan(d, h, m, s);
            return true;
        }
    }
}
