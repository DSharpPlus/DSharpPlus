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

            // If no time unit is specified, assume seconds (100 = 100 seconds)
            if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            {
                return Task.FromResult(Optional.FromValue(TimeSpan.FromSeconds(number)));
            }

            // Try to parse the timespan using the TryParse method (00:01:40 = 1 minutes and 40 seconds, 100 seconds)
            if (TimeSpan.TryParse(value.ToLowerInvariant(), CultureInfo.InvariantCulture, out var result))
            {
                return Task.FromResult(Optional.FromValue(result));
            }

            // Regex the shit outta it (1m40s = 1 minute and 40 seconds, 100s = 100 seconds)
            var matches = TimeSpanParseRegex.Match(value);
            if (!matches.Success)
            {
                // We couldn't infer the unit of measurement, TimeSpan couldn't parse the input and we couldn't regex it.
                // Assume it's junk input and return false.
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());
            }

            var days = 0;
            var hours = 0;
            var minutes = 0;
            var seconds = 0;
            foreach (var timeMeasurement in TimeUnitMeasurements)
            {
                var groupCapture = matches.Groups[timeMeasurement].Value?.Trim();
                if (string.IsNullOrEmpty(groupCapture))
                    continue;

                var currentTimeMeasurement = groupCapture[groupCapture.Length - 1];
                if (!int.TryParse(groupCapture.Substring(0, groupCapture.Length - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var currentValue))
                    continue;

                switch (currentTimeMeasurement)
                {
                    case 'M': // Months
                        days += currentValue * 30;
                        break;
                    case 'w': // Weeks
                        days += currentValue * 7;
                        break;
                    case 'd':
                        days = currentValue;
                        break;
                    case 'h':
                        hours = currentValue;
                        break;
                    case 'm':
                        minutes = currentValue;
                        break;
                    case 's':
                        seconds = currentValue;
                        break;
                    default:
                        // Unknown unit of measurement, do not continue.
                        result = default;
                        return Task.FromResult(Optional.FromNoValue<TimeSpan>());
                }
            }

            result = new TimeSpan(days, hours, minutes, seconds);
            return Task.FromResult(Optional.FromValue(result));
        }
    }
}
