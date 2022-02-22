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
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Converters
{
    public class StringConverter : ISlashArgumentConverter<string>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.String;

        public Task<string> Convert(DiscordInteractionDataOption value, InteractionContext context)
            => Task.FromResult(value.Value.ToString());
    }

    public class LongConverter : ISlashArgumentConverter<long>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Integer;

        public Task<long> Convert(DiscordInteractionDataOption value, InteractionContext context)
            => Task.FromResult((long)value.Value);
    }

    public class NullableLongConverter : ISlashArgumentConverter<long?>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Integer;

        public Task<long?> Convert(DiscordInteractionDataOption value, InteractionContext context)
            => Task.FromResult((long?)value.Value);
    }

    public class BoolConverter : ISlashArgumentConverter<bool>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Boolean;
        public Task<bool> Convert(DiscordInteractionDataOption value, InteractionContext context)
            => Task.FromResult((bool)value.Value);
    }

    public class NullableBoolConverter : ISlashArgumentConverter<bool?>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Boolean;
        public Task<bool?> Convert(DiscordInteractionDataOption value, InteractionContext context)
            => Task.FromResult((bool?)value.Value);
    }

    public class DoubleConverter : ISlashArgumentConverter<double>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Number;

        public Task<double> Convert(DiscordInteractionDataOption value, InteractionContext context)
            => Task.FromResult((double)value.Value);
    }

    public class NullableDoubleConverter : ISlashArgumentConverter<double?>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Number;

        public Task<double?> Convert(DiscordInteractionDataOption value, InteractionContext context)
            => Task.FromResult((double?)value.Value);
    }

    public class UserConverter : ISlashArgumentConverter<DiscordUser>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.User;

        public async Task<DiscordUser> Convert(DiscordInteractionDataOption value, InteractionContext context)
        {
            //Checks through resolved
            if (context.Interaction.Data.Resolved.Members != null &&
                context.Interaction.Data.Resolved.Members.TryGetValue((ulong)value.Value, out var member))
                return member;
            if (context.Interaction.Data.Resolved.Users != null &&
                     context.Interaction.Data.Resolved.Users.TryGetValue((ulong)value.Value, out var user))
                return user;
            return(await context.Client.GetUserAsync((ulong)value.Value));
        }
    }

    public class ChannelConverter : ISlashArgumentConverter<DiscordChannel>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Channel;

        public Task<DiscordChannel> Convert(DiscordInteractionDataOption value, InteractionContext context)
        {
            //Checks through resolved
            if (context.Interaction.Data.Resolved.Channels != null &&
                context.Interaction.Data.Resolved.Channels.TryGetValue((ulong)value.Value, out var channel))
                return Task.FromResult(channel);
            return Task.FromResult(context.Interaction.Guild.GetChannel((ulong)value.Value));
        }
    }

    public class RoleConverter : ISlashArgumentConverter<DiscordRole>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Role;

        public Task<DiscordRole> Convert(DiscordInteractionDataOption value, InteractionContext context)
        {
            //Checks through resolved
            if (context.Interaction.Data.Resolved.Roles != null &&
                context.Interaction.Data.Resolved.Roles.TryGetValue((ulong)value.Value, out var role))
                return Task.FromResult(role);
            return Task.FromResult(context.Interaction.Guild.GetRole((ulong)value.Value));
        }
    }

    public class MentionableConverter : ISlashArgumentConverter<SnowflakeObject>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Mentionable;

        public Task<SnowflakeObject> Convert(DiscordInteractionDataOption value, InteractionContext context)
        {
            if (context.Interaction.Data.Resolved.Roles != null && context.Interaction.Data.Resolved.Roles.TryGetValue((ulong)value.Value, out var role))
                return Task.FromResult((SnowflakeObject)role);
            if (context.Interaction.Data.Resolved.Members != null && context.Interaction.Data.Resolved.Members.TryGetValue((ulong)value.Value, out var member))
                return Task.FromResult((SnowflakeObject)member);
            if (context.Interaction.Data.Resolved.Users != null && context.Interaction.Data.Resolved.Users.TryGetValue((ulong)value.Value, out var user))
                return Task.FromResult((SnowflakeObject)user);
            throw new ArgumentException("Error resolving mentionable option.");
        }
    }

    public class AttachmentConverter : ISlashArgumentConverter<DiscordAttachment>
    {
        public ApplicationCommandOptionType OptionType { get; } = ApplicationCommandOptionType.Attachment;
        public Task<DiscordAttachment> Convert(DiscordInteractionDataOption value, InteractionContext context)
        {
            if (context.Interaction.Data.Resolved.Attachments?.ContainsKey((ulong)value.Value) ?? false)
            {
                var attachment = context.Interaction.Data.Resolved.Attachments[(ulong)value.Value];
                return Task.FromResult(attachment);
            }

            throw new ArgumentException("Missing attachment in resolved data. This is an issue with Discord.");
        }
    }
}
