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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Converters
{
    public sealed class TimeSpanSlashArgumentConverter : ISlashArgumentConverter<TimeSpan>
    {
        public static readonly string[] TimeUnitMeasurements = new[] { "days", "hours", "minutes", "seconds" };
        public static readonly Regex TimeSpanParseRegex;

        static TimeSpanSlashArgumentConverter()
        {
#if NETSTANDARD1_3
            TimeSpanParseRegex = new Regex(@"^((?<days>\d+)d\s*)?((?<hours>\d+)h\s*)?((?<minutes>\d+)m\s*)?((?<seconds>\d+)s\s*)?$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);
#else
            TimeSpanParseRegex = new Regex(@"^((?<days>\d+)d\s*)?((?<hours>\d+)h\s*)?((?<minutes>\d+)m\s*)?((?<seconds>\d+)s\s*)?$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.RightToLeft | RegexOptions.Compiled);
#endif
        }

        public Task<Optional<TimeSpan>> ConvertAsync(InteractionContext interactionContext, DiscordInteractionDataOption interactionDataOption, ParameterInfo interactionMethodArgument)
        {
            var value = interactionDataOption.Value.ToString().Trim();
            if (value == "0")
                return Task.FromResult(Optional.FromValue(TimeSpan.Zero));

            if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional.FromValue(result));

            var m = TimeSpanParseRegex.Match(value);
            var ds = m.Groups["days"].Success ? int.Parse(m.Groups["days"].Value) : 0;
            var hs = m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value) : 0;
            var ms = m.Groups["minutes"].Success ? int.Parse(m.Groups["minutes"].Value) : 0;
            var ss = m.Groups["seconds"].Success ? int.Parse(m.Groups["seconds"].Value) : 0;

            result = TimeSpan.FromSeconds((ds * 24 * 60 * 60) + (hs * 60 * 60) + (ms * 60) + ss);
            if (result.TotalSeconds < 1)
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

            return Task.FromResult(Optional.FromValue(result));
        }
    }
}
