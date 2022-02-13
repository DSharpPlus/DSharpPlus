// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class DateTimeConverter : IArgumentConverter<DateTime>
    {
        Task<Optional<DateTime>> IArgumentConverter<DateTime>.ConvertAsync(string value, CommandContext ctx)
        {
            return DateTime.TryParse(value, ctx.CommandsNext.DefaultParserCulture, DateTimeStyles.None, out var result)
                ? Task.FromResult(new Optional<DateTime>(result))
                : Task.FromResult(Optional.FromNoValue<DateTime>());
        }
    }

    public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
    {
        Task<Optional<DateTimeOffset>> IArgumentConverter<DateTimeOffset>.ConvertAsync(string value, CommandContext ctx)
        {
            return DateTimeOffset.TryParse(value, ctx.CommandsNext.DefaultParserCulture, DateTimeStyles.None, out var result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
        }
    }

    public class TimeSpanConverter : IArgumentConverter<TimeSpan>
    {
        private static Regex TimeSpanRegex { get; set; }

        static TimeSpanConverter()
        {
#if NETSTANDARD1_3
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript);
#else
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        Task<Optional<TimeSpan>> IArgumentConverter<TimeSpan>.ConvertAsync(string value, CommandContext ctx)
        {
            if (value == "0")
                return Task.FromResult(Optional.FromValue(TimeSpan.Zero));

            if (int.TryParse(value, NumberStyles.Number, ctx.CommandsNext.DefaultParserCulture, out _))
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (TimeSpan.TryParse(value, ctx.CommandsNext.DefaultParserCulture, out var result))
                return Task.FromResult(Optional.FromValue(result));

            var gps = new string[] { "days", "hours", "minutes", "seconds" };
            var mtc = TimeSpanRegex.Match(value);
            if (!mtc.Success)
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

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
                int.TryParse(gpc.Substring(0, gpc.Length - 1), NumberStyles.Integer, ctx.CommandsNext.DefaultParserCulture, out var val);
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
            return Task.FromResult(Optional.FromValue(result));
        }
    }
}
