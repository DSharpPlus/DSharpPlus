// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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

namespace DSharpPlus.CommandsNext.Converters;

public class DateTimeConverter : IArgumentConverter<DateTime>
{
    Task<Optional<DateTime>> IArgumentConverter<DateTime>.ConvertAsync(string value, CommandContext ctx)
        => DateTime.TryParse(value, ctx.CommandsNext.DefaultParserCulture, DateTimeStyles.None, out DateTime result)
            ? Task.FromResult(new Optional<DateTime>(result))
            : Task.FromResult(Optional.FromNoValue<DateTime>());
}

public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
{
    Task<Optional<DateTimeOffset>> IArgumentConverter<DateTimeOffset>.ConvertAsync(string value, CommandContext ctx)
        => DateTimeOffset.TryParse(value, ctx.CommandsNext.DefaultParserCulture, DateTimeStyles.None, out DateTimeOffset result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
}

public class TimeSpanConverter : IArgumentConverter<TimeSpan>
{
    private static Regex TimeSpanRegex { get; set; }

    static TimeSpanConverter()
        => TimeSpanRegex = new Regex(@"^((?<days>\d+)d\s*)?((?<hours>\d+)h\s*)?((?<minutes>\d+)m\s*)?((?<seconds>\d+)s\s*)?$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.RightToLeft | RegexOptions.Compiled);

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

        Match? m = TimeSpanRegex.Match(value);

        int ds = m.Groups["days"].Success ? int.Parse(m.Groups["days"].Value) : 0;
        int hs = m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value) : 0;
        int ms = m.Groups["minutes"].Success ? int.Parse(m.Groups["minutes"].Value) : 0;
        int ss = m.Groups["seconds"].Success ? int.Parse(m.Groups["seconds"].Value) : 0;

        result = TimeSpan.FromSeconds((ds * 24 * 60 * 60) + (hs * 60 * 60) + (ms * 60) + ss);
        if (result.TotalSeconds < 1)
        {
            return Task.FromResult(Optional.FromNoValue<TimeSpan>());
        }

        return Task.FromResult(Optional.FromValue(result));
    }
}