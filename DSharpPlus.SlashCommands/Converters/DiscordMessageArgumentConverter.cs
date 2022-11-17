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
    public class DiscordMessageSlashArgumentConverter : ISlashArgumentConverter<DiscordMessage>
    {
        private static readonly Regex _messagePathRegex = new(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        // Copied with modifications from DSharpPlus.CommandsNext.Converters.EntityConverter+DiscordMessageConverter
        public async Task<Optional<DiscordMessage>> ConvertAsync(InteractionContext interactionContext, DiscordInteractionDataOption interactionDataOption, ParameterInfo interactionMethodArgument)
        {
            if (interactionDataOption.Value is not string value || string.IsNullOrWhiteSpace(value))
                return Optional.FromNoValue<DiscordMessage>();

            // Handle <http://localhost> links, which supresses embeds.
            var messageUri = value.StartsWith("<") && value.EndsWith(">") ? value.Substring(1, value.Length - 2) : value;
            ulong messageId;
            if (Uri.TryCreate(messageUri, UriKind.Absolute, out var uri))
            {
                if (!uri.Host.EndsWith("discordapp.com") && !uri.Host.EndsWith("discord.com"))
                    return Optional.FromNoValue<DiscordMessage>();

                var uriPath = _messagePathRegex.Match(uri.AbsolutePath);
                if (!uriPath.Success
                    || !ulong.TryParse(uriPath.Groups["channel"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid)
                    || !ulong.TryParse(uriPath.Groups["message"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out messageId))
                    return Optional.FromNoValue<DiscordMessage>();

                var channel = await interactionContext.Client.GetChannelAsync(cid).ConfigureAwait(false);
                if (channel == null)
                    return Optional.FromNoValue<DiscordMessage>();

                var message = await channel.GetMessageAsync(messageId).ConfigureAwait(false);
                return message != null ? Optional.FromValue(message) : Optional.FromNoValue<DiscordMessage>();
            }

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out messageId))
            {
                var result = await interactionContext.Channel.GetMessageAsync(messageId).ConfigureAwait(false);
                return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMessage>();
            }

            return Optional.FromNoValue<DiscordMessage>();
        }
    }
}
