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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters;

public class DiscordUserConverter : IArgumentConverter<DiscordUser>
{
    private static Regex UserRegex { get; }

    static DiscordUserConverter()
        => UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

    async Task<Optional<DiscordUser>> IArgumentConverter<DiscordUser>.ConvertAsync(string value, CommandContext ctx)
    {
        if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong uid))
        {
            DiscordUser? result = await ctx.Client.GetUserAsync(uid);
            Optional<DiscordUser> ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
            return ret;
        }

        Match? m = UserRegex.Match(value);
        if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
        {
            DiscordUser? result = await ctx.Client.GetUserAsync(uid);
            Optional<DiscordUser> ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
            return ret;
        }

        bool cs = ctx.Config.CaseSensitive;

        int di = value.IndexOf('#');
        string? un = di != -1 ? value[..di] : value;
        string? dv = di != -1 ? value[(di + 1)..] : null;

        IEnumerable<DiscordMember>? us = ctx.Client.Guilds.Values
            .SelectMany(guild => guild.Members.Values).Where(xm =>
                xm.Username.Equals(un, cs ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase) &&
                ((dv != null && xm.Discriminator == dv) || dv == null));

        DiscordMember? usr = us.FirstOrDefault();
        return usr != null ? Optional.FromValue<DiscordUser>(usr) : Optional.FromNoValue<DiscordUser>();
    }
}

public class DiscordMemberConverter : IArgumentConverter<DiscordMember>
{
    private static Regex UserRegex { get; }

    static DiscordMemberConverter()
        => UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

    async Task<Optional<DiscordMember>> IArgumentConverter<DiscordMember>.ConvertAsync(string value, CommandContext ctx)
    {
        if (ctx.Guild == null)
        {
            return Optional.FromNoValue<DiscordMember>();
        }

        if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong uid))
        {
            DiscordMember? result = await ctx.Guild.GetMemberAsync(uid);
            Optional<DiscordMember> ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
            return ret;
        }

        Match? m = UserRegex.Match(value);
        if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
        {
            DiscordMember? result = await ctx.Guild.GetMemberAsync(uid);
            Optional<DiscordMember> ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
            return ret;
        }

        IReadOnlyList<DiscordMember>? searchResult = await ctx.Guild.SearchMembersAsync(value);
        if (searchResult.Any())
        {
            return Optional.FromValue(searchResult.First());
        }

        bool cs = ctx.Config.CaseSensitive;

        int di = value.IndexOf('#');
        string? un = di != -1 ? value[..di] : value;
        string? dv = di != -1 ? value[(di + 1)..] : null;

        StringComparison comparison = cs ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        IEnumerable<DiscordMember>? us = ctx.Guild.Members.Values.Where(xm =>
            (xm.Username.Equals(un, comparison) &&
             ((dv != null && xm.Discriminator == dv) || dv == null)) || value.Equals(xm.Nickname, comparison));

        DiscordMember? mbr = us.FirstOrDefault();
        return mbr != null ? Optional.FromValue(mbr) : Optional.FromNoValue<DiscordMember>();
    }
}

public class DiscordChannelConverter : IArgumentConverter<DiscordChannel>
{
    private static Regex ChannelRegex { get; }

    static DiscordChannelConverter()
        => ChannelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

    async Task<Optional<DiscordChannel>> IArgumentConverter<DiscordChannel>.ConvertAsync(string value, CommandContext ctx)
    {
        if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong cid))
        {
            DiscordChannel? result = await ctx.Client.GetChannelAsync(cid);
            return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
        }

        Match? m = ChannelRegex.Match(value);
        if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
        {
            DiscordChannel? result = await ctx.Client.GetChannelAsync(cid);
            return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
        }

        bool cs = ctx.Config.CaseSensitive;

        StringComparison comparison = cs ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        DiscordChannel? chn = ctx.Guild?.Channels.Values.FirstOrDefault(xc => xc.Name.Equals(value, comparison)) ??
                              ctx.Guild?.Threads.Values.FirstOrDefault(xThread => xThread.Name.Equals(value, comparison));

        return chn != null ? Optional.FromValue(chn) : Optional.FromNoValue<DiscordChannel>();
    }
}

public class DiscordThreadChannelConverter : IArgumentConverter<DiscordThreadChannel>
{
    private static Regex ThreadRegex { get; }

    static DiscordThreadChannelConverter()
        => ThreadRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

    Task<Optional<DiscordThreadChannel>> IArgumentConverter<DiscordThreadChannel>.ConvertAsync(string value, CommandContext ctx)
    {
        if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong threadId))
        {
            DiscordThreadChannel? result = ctx.Client.InternalGetCachedThread(threadId);
            return Task.FromResult(result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordThreadChannel>());
        }

        Match? m = ThreadRegex.Match(value);
        if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out threadId))
        {
            DiscordThreadChannel? result = ctx.Client.InternalGetCachedThread(threadId);
            return Task.FromResult(result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordThreadChannel>());
        }

        bool cs = ctx.Config.CaseSensitive;

        DiscordThreadChannel? thread = ctx.Guild?.Threads.Values.FirstOrDefault(xt =>
            xt.Name.Equals(value, cs ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase));

        return Task.FromResult(thread != null ? Optional.FromValue(thread) : Optional.FromNoValue<DiscordThreadChannel>());
    }
}

public class DiscordRoleConverter : IArgumentConverter<DiscordRole>
{
    private static Regex RoleRegex { get; }

    static DiscordRoleConverter()
        => RoleRegex = new Regex(@"^<@&(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

    Task<Optional<DiscordRole>> IArgumentConverter<DiscordRole>.ConvertAsync(string value, CommandContext ctx)
    {
        if (ctx.Guild == null)
        {
            return Task.FromResult(Optional.FromNoValue<DiscordRole>());
        }

        if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong rid))
        {
            DiscordRole? result = ctx.Guild.GetRole(rid);
            Optional<DiscordRole> ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
            return Task.FromResult(ret);
        }

        Match? m = RoleRegex.Match(value);
        if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out rid))
        {
            DiscordRole? result = ctx.Guild.GetRole(rid);
            Optional<DiscordRole> ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
            return Task.FromResult(ret);
        }

        bool cs = ctx.Config.CaseSensitive;

        DiscordRole? rol = ctx.Guild.Roles.Values.FirstOrDefault(xr =>
            xr.Name.Equals(value, cs ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase));
        return Task.FromResult(rol != null ? Optional.FromValue(rol) : Optional.FromNoValue<DiscordRole>());
    }
}

public class DiscordGuildConverter : IArgumentConverter<DiscordGuild>
{
    Task<Optional<DiscordGuild>> IArgumentConverter<DiscordGuild>.ConvertAsync(string value, CommandContext ctx)
    {
        if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong gid))
        {
            return ctx.Client.Guilds.TryGetValue(gid, out DiscordGuild? result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<DiscordGuild>());
        }

        bool cs = ctx.Config.CaseSensitive;

        DiscordGuild? gld = ctx.Client.Guilds.Values.FirstOrDefault(xg =>
            xg.Name.Equals(value, cs ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase));
        return Task.FromResult(gld != null ? Optional.FromValue(gld) : Optional.FromNoValue<DiscordGuild>());
    }
}

public class DiscordMessageConverter : IArgumentConverter<DiscordMessage>
{
    private static Regex MessagePathRegex { get; }

    static DiscordMessageConverter()
        => MessagePathRegex = new Regex(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

    async Task<Optional<DiscordMessage>> IArgumentConverter<DiscordMessage>.ConvertAsync(string value, CommandContext ctx)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Optional.FromNoValue<DiscordMessage>();
        }

        string? msguri = value.StartsWith("<") && value.EndsWith(">") ? value.Substring(1, value.Length - 2) : value;
        ulong mid;
        if (Uri.TryCreate(msguri, UriKind.Absolute, out Uri? uri))
        {
            if (uri.Host != "discordapp.com" && uri.Host != "discord.com" && !uri.Host.EndsWith(".discordapp.com") && !uri.Host.EndsWith(".discord.com"))
            {
                return Optional.FromNoValue<DiscordMessage>();
            }

            Match? uripath = MessagePathRegex.Match(uri.AbsolutePath);
            if (!uripath.Success
                || !ulong.TryParse(uripath.Groups["channel"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong cid)
                || !ulong.TryParse(uripath.Groups["message"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
            {
                return Optional.FromNoValue<DiscordMessage>();
            }

            DiscordChannel? chn = await ctx.Client.GetChannelAsync(cid);
            if (chn == null)
            {
                return Optional.FromNoValue<DiscordMessage>();
            }

            DiscordMessage? msg = await chn.GetMessageAsync(mid);
            return msg != null ? Optional.FromValue(msg) : Optional.FromNoValue<DiscordMessage>();
        }

        if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
        {
            DiscordMessage? result = await ctx.Channel.GetMessageAsync(mid);
            return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMessage>();
        }

        return Optional.FromNoValue<DiscordMessage>();
    }
}

public class DiscordEmojiConverter : IArgumentConverter<DiscordEmoji>
{
    private static Regex EmoteRegex { get; }

    static DiscordEmojiConverter()
        => EmoteRegex = new Regex(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

    Task<Optional<DiscordEmoji>> IArgumentConverter<DiscordEmoji>.ConvertAsync(string value, CommandContext ctx)
    {
        if (DiscordEmoji.TryFromUnicode(ctx.Client, value, out DiscordEmoji? emoji))
        {
            Optional<DiscordEmoji> ret = Optional.FromValue(emoji);
            return Task.FromResult(ret);
        }

        Match? m = EmoteRegex.Match(value);
        if (m.Success)
        {
            string? sid = m.Groups["id"].Value;
            string? name = m.Groups["name"].Value;
            bool anim = m.Groups["animated"].Success;

            if (!ulong.TryParse(sid, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong id))
            {
                return Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
            }

            return DiscordEmoji.TryFromGuildEmote(ctx.Client, id, out emoji)
                ? Task.FromResult(Optional.FromValue(emoji))
                : Task.FromResult(Optional.FromValue(new DiscordEmoji
                {
                    Discord = ctx.Client,
                    Id = id,
                    Name = name,
                    IsAnimated = anim,
                    RequiresColons = true,
                    IsManaged = false
                }));
        }

        return Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
    }
}

public class DiscordColorConverter : IArgumentConverter<DiscordColor>
{
    private static Regex ColorRegexHex { get; }
    private static Regex ColorRegexRgb { get; }

    static DiscordColorConverter()
    {
        ColorRegexHex = new Regex(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
        ColorRegexRgb = new Regex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
    }

    Task<Optional<DiscordColor>> IArgumentConverter<DiscordColor>.ConvertAsync(string value, CommandContext ctx)
    {
        Match? m = ColorRegexHex.Match(value);
        if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int clr))
        {
            return Task.FromResult(Optional.FromValue<DiscordColor>(clr));
        }

        m = ColorRegexRgb.Match(value);
        if (m.Success)
        {
            bool p1 = byte.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte r);
            bool p2 = byte.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte g);
            bool p3 = byte.TryParse(m.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte b);

            return !(p1 && p2 && p3)
                ? Task.FromResult(Optional.FromNoValue<DiscordColor>())
                : Task.FromResult(Optional.FromValue(new DiscordColor(r, g, b)));
        }

        return Task.FromResult(Optional.FromNoValue<DiscordColor>());
    }
}
