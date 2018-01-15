﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class DateTimeConverter : IArgumentConverter<DateTime>
    {
        public Task<Optional<DateTime>> ConvertAsync(string value, CommandContext ctx)
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return Task.FromResult(new Optional<DateTime>(result));

            return Task.FromResult(Optional<DateTime>.FromNoValue());
        }
    }

    public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
    {
        public Task<Optional<DateTimeOffset>> ConvertAsync(string value, CommandContext ctx)
        {
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return Task.FromResult(Optional<DateTimeOffset>.FromValue(result));

            return Task.FromResult(Optional<DateTimeOffset>.FromNoValue());
        }
    }

    public class TimeSpanConverter : IArgumentConverter<TimeSpan>
    {
        private static Regex TimeSpanRegex { get; set; }

        static TimeSpanConverter()
        {
#if NETSTANDARD1_1 || NETSTANDARD1_3
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript);
#else
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        public Task<Optional<TimeSpan>> ConvertAsync(string value, CommandContext ctx)
        {
            if (value == "0")
                return Task.FromResult(Optional<TimeSpan>.FromValue(TimeSpan.Zero));

            if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                return Task.FromResult(Optional<TimeSpan>.FromNoValue());

            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional<TimeSpan>.FromValue(result));

            var gps = new string[] { "days", "hours", "minutes", "seconds" };
            var mtc = TimeSpanRegex.Match(value);
            if (!mtc.Success)
                return Task.FromResult(Optional<TimeSpan>.FromNoValue());

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
            return Task.FromResult(Optional<TimeSpan>.FromValue(result));
        }
    }
}
