﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class DiscordUserConverter : IArgumentConverter<DiscordUser>
    {
        private static Regex UserRegex { get; }

        static DiscordUserConverter()
        {
#if NETSTANDARD1_3
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript);
#else
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        async Task<Optional<DiscordUser>> IArgumentConverter<DiscordUser>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await ctx.Client.GetUserAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await ctx.Client.GetUserAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Client.Guilds.Values
                .SelectMany(xkvp => xkvp.Members.Values)
                .Where(xm => (cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null));
            
            var usr = us.FirstOrDefault();
            return usr != null ? Optional.FromValue<DiscordUser>(usr) : Optional.FromNoValue<DiscordUser>();
        }
    }

    public class DiscordMemberConverter : IArgumentConverter<DiscordMember>
    {
        private static Regex UserRegex { get; }

        static DiscordMemberConverter()
        {
#if NETSTANDARD1_3
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript);
#else
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        async Task<Optional<DiscordMember>> IArgumentConverter<DiscordMember>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild == null)
                return Optional.FromNoValue<DiscordMember>();

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Guild.Members.Values
                .Where(xm => ((cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null)) 
                          || (cs ? xm.Nickname : xm.Nickname?.ToLowerInvariant()) == value);

            var mbr = us.FirstOrDefault();
            return mbr != null ? Optional.FromValue(mbr) : Optional.FromNoValue<DiscordMember>();
        }
    }

    public class DiscordChannelConverter : IArgumentConverter<DiscordChannel>
    {
        private static Regex ChannelRegex { get; }

        static DiscordChannelConverter()
        {
#if NETSTANDARD1_3
            ChannelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript);
#else
            ChannelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        async Task<Optional<DiscordChannel>> IArgumentConverter<DiscordChannel>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid))
            {
                var result = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
                return ret;
            }

            var m = ChannelRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
            {
                var result = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
                return ret;
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var chn = ctx.Guild?.Channels.Values.FirstOrDefault(xc => (cs ? xc.Name : xc.Name.ToLowerInvariant()) == value);
            return chn != null ? Optional.FromValue(chn) : Optional.FromNoValue<DiscordChannel>();
        }
    }

    public class DiscordRoleConverter : IArgumentConverter<DiscordRole>
    {
        private static Regex RoleRegex { get; }

        static DiscordRoleConverter()
        {
#if NETSTANDARD1_3
            RoleRegex = new Regex(@"^<@&(\d+?)>$", RegexOptions.ECMAScript);
#else
            RoleRegex = new Regex(@"^<@&(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        Task<Optional<DiscordRole>> IArgumentConverter<DiscordRole>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild == null)
                return Task.FromResult(Optional.FromNoValue<DiscordRole>());

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rid))
            {
                var result = ctx.Guild.GetRole(rid);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
                return Task.FromResult(ret);
            }

            var m = RoleRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out rid))
            {
                var result = ctx.Guild.GetRole(rid);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
                return Task.FromResult(ret);
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var rol = ctx.Guild.Roles.Values.FirstOrDefault(xr => (cs ? xr.Name : xr.Name.ToLowerInvariant()) == value);
            return Task.FromResult(rol != null ? Optional.FromValue(rol) : Optional.FromNoValue<DiscordRole>());
        }
    }

    public class DiscordGuildConverter : IArgumentConverter<DiscordGuild>
    {
        Task<Optional<DiscordGuild>> IArgumentConverter<DiscordGuild>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gid))
            {
                if (ctx.Client.Guilds.TryGetValue(gid, out var result))
                    return Task.FromResult(Optional.FromValue(result));
                else
                    return Task.FromResult(Optional.FromNoValue<DiscordGuild>());
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value?.ToLowerInvariant();

            var gld = ctx.Client.Guilds.Values.FirstOrDefault(xg => (cs ? xg.Name : xg.Name.ToLowerInvariant()) == value);
            return Task.FromResult(gld != null ? Optional.FromValue(gld) : Optional.FromNoValue<DiscordGuild>());
        }
    }

    public class DiscordMessageConverter : IArgumentConverter<DiscordMessage>
    {
        private static Regex MessagePathRegex { get; }

        static DiscordMessageConverter()
        {
#if NETSTANDARD1_3
            MessagePathRegex = new Regex(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$", RegexOptions.ECMAScript);
#else
            MessagePathRegex = new Regex(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        async Task<Optional<DiscordMessage>> IArgumentConverter<DiscordMessage>.ConvertAsync(string value, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Optional.FromNoValue<DiscordMessage>();

            var msguri = value.StartsWith("<") && value.EndsWith(">") ? value.Substring(1, value.Length - 2) : value;
            ulong mid;
            if (Uri.TryCreate(msguri, UriKind.Absolute, out var uri))
            {
                if (uri.Host != "discordapp.com" && uri.Host != "discord.com" && !uri.Host.EndsWith(".discordapp.com") && !uri.Host.EndsWith(".discord.com"))
                    return Optional.FromNoValue<DiscordMessage>();

                var uripath = MessagePathRegex.Match(uri.AbsolutePath);
                if (!uripath.Success 
                    || !ulong.TryParse(uripath.Groups["channel"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid)
                    || !ulong.TryParse(uripath.Groups["message"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
                    return Optional.FromNoValue<DiscordMessage>();

                var chn = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                if (chn == null)
                    return Optional.FromNoValue<DiscordMessage>();

                var msg = await chn.GetMessageAsync(mid);
                return msg != null ? Optional.FromValue(msg) : Optional.FromNoValue<DiscordMessage>();
            }

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
            {
                var result = await ctx.Channel.GetMessageAsync(mid).ConfigureAwait(false);
                return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMessage>();
            }

            return Optional.FromNoValue<DiscordMessage>();
        }
    }

    public class DiscordEmojiConverter : IArgumentConverter<DiscordEmoji>
    {
        private static Regex EmoteRegex { get; }

        static DiscordEmojiConverter()
        {
#if NETSTANDARD1_3
            EmoteRegex = new Regex(@"^<a?:([a-zA-Z0-9_]+?):(\d+?)>$", RegexOptions.ECMAScript);
#else
            EmoteRegex = new Regex(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        Task<Optional<DiscordEmoji>> IArgumentConverter<DiscordEmoji>.ConvertAsync(string value, CommandContext ctx)
        {
            if (DiscordEmoji.UnicodeEmojiList.Contains(value))
            {
                var result = DiscordEmoji.FromUnicode(ctx.Client, value);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordEmoji>();
                return Task.FromResult(ret);
            }

            var m = EmoteRegex.Match(value);
            if (m.Success)
            {
                var sid = m.Groups["id"].Value;
                var name = m.Groups["name"].Value;
                var anim = m.Groups["animated"].Success;

                if (!ulong.TryParse(sid, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    return Task.FromResult(Optional.FromNoValue<DiscordEmoji>());

                try
                {
                    var e = DiscordEmoji.FromGuildEmote(ctx.Client, id);
                    return Task.FromResult(Optional.FromValue(e));
                }
                catch (KeyNotFoundException)
                { }

                return Task.FromResult(Optional.FromValue(new DiscordEmoji
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
#if NETSTANDARD1_3
            ColorRegexHex = new Regex(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript);
            ColorRegexRgb = new Regex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript);
#else
            ColorRegexHex = new Regex(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
            ColorRegexRgb = new Regex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        Task<Optional<DiscordColor>> IArgumentConverter<DiscordColor>.ConvertAsync(string value, CommandContext ctx)
        {
            var m = ColorRegexHex.Match(value);
            if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var clr))
                return Task.FromResult(Optional.FromValue<DiscordColor>(clr));

            m = ColorRegexRgb.Match(value);
            if (m.Success)
            {
                var p1 = byte.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r);
                var p2 = byte.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var g);
                var p3 = byte.TryParse(m.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b);

                if (!(p1 && p2 && p3))
                    return Task.FromResult(Optional.FromNoValue<DiscordColor>());
                
                return Task.FromResult(Optional.FromValue(new DiscordColor(r, g, b)));
            }

            return Task.FromResult(Optional.FromNoValue<DiscordColor>());
        }
    }
}
