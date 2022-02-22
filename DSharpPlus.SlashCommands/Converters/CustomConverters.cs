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

namespace DSharpPlus.SlashCommands.Converters
{
    public class TimespanConverter : ISlashArgumentConverter<TimeSpan?>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.String;

        private static Regex TimeSpanRegex { get; set; }

        static TimespanConverter()
        {
#if NETSTANDARD1_3
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript);
#else
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        public Task<TimeSpan?> Convert(DiscordInteractionDataOption value, InteractionContext context)
        {
            if (value.Value.ToString() == "0")
                return Task.FromResult((TimeSpan?)TimeSpan.Zero);

            if (int.TryParse(value.Value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                return Task.FromResult((TimeSpan?)null);

            if (TimeSpan.TryParse(value.Value.ToString().ToLowerInvariant(), CultureInfo.InvariantCulture, out var result))
                return Task.FromResult((TimeSpan?)result);

            var gps = new string[] { "days", "hours", "minutes", "seconds" };
            var mtc = TimeSpanRegex.Match(value.Value.ToString().ToLowerInvariant());
            if (!mtc.Success)
                return Task.FromResult((TimeSpan?)null);

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
            return Task.FromResult((TimeSpan?)result);
        }
    }

    public class EmojiConverter : ISlashArgumentConverter<DiscordEmoji>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.String;

        public Task<DiscordEmoji> Convert(DiscordInteractionDataOption value, InteractionContext context)
        {
            if (DiscordEmoji.TryFromUnicode(context.Client, value.Value.ToString(), out var emoji) || DiscordEmoji.TryFromName(context.Client, value.Value.ToString(), out emoji))
                return Task.FromResult(emoji);
            throw new ArgumentException("Error parsing emoji parameter.");
        }
    }
}
