using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class DiscordUserConverter : IArgumentConverter<DiscordUser>
    {
        private static Regex UserRegex { get; }

        static DiscordUserConverter()
        {
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript);
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordUser result)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                result = ctx.Client.GetUserAsync(uid).GetAwaiter().GetResult();
                return true;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                result = ctx.Client.GetUserAsync(uid).GetAwaiter().GetResult();
                return true;
            }

            value = value.ToLowerInvariant();
            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Client.Guilds
                .SelectMany(xkvp => xkvp.Value.Members)
                .Where(xm => xm.Username.ToLowerInvariant() == un && ((dv != null && xm.Discriminator == dv) || dv == null));
            
            result = us.FirstOrDefault();
            return result != null;
        }
    }

    public class DiscordMemberConverter : IArgumentConverter<DiscordMember>
    {
        private static Regex UserRegex { get; }

        static DiscordMemberConverter()
        {
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript);
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordMember result)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                result = ctx.Guild.GetMemberAsync(uid).GetAwaiter().GetResult();
                return true;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                result = ctx.Guild.GetMemberAsync(uid).GetAwaiter().GetResult();
                return true;
            }

            value = value.ToLowerInvariant();
            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Guild.Members
                .Where(xm => (xm.Username.ToLowerInvariant() == un && ((dv != null && xm.Discriminator == dv) || dv == null)) || xm.Nickname?.ToLowerInvariant() == value);

            result = us.FirstOrDefault();
            return result != null;
        }
    }

    public class DiscordChannelConverter : IArgumentConverter<DiscordChannel>
    {
        private static Regex ChannelRegex { get; }

        static DiscordChannelConverter()
        {
            ChannelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript);
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordChannel result)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid))
            {
                result = ctx.Guild.Channels.FirstOrDefault(xc => xc.Id == cid);
                return true;
            }

            var m = ChannelRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
            {
                result = ctx.Guild.Channels.FirstOrDefault(xc => xc.Id == cid);
                return true;
            }

            var chn = ctx.Guild.Channels.FirstOrDefault(xc => xc.Name == value);
            result = chn;
            return result != null;
        }
    }

    public class DiscordRoleConverter : IArgumentConverter<DiscordRole>
    {
        private static Regex RoleRegex { get; }

        static DiscordRoleConverter()
        {
            RoleRegex = new Regex(@"^<@&(\d+?)>$", RegexOptions.ECMAScript);
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordRole result)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rid))
            {
                result = ctx.Guild.Roles.FirstOrDefault(xr => xr.Id == rid);
                return true;
            }

            var m = RoleRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out rid))
            {
                result = ctx.Guild.Roles.FirstOrDefault(xr => xr.Id == rid);
                return true;
            }

            var rl = ctx.Guild.Roles.FirstOrDefault(xr => xr.Name == value);
            result = rl;
            return result != null;
        }
    }

    public class DiscordGuildConverter : IArgumentConverter<DiscordGuild>
    {
        public bool TryConvert(string value, CommandContext ctx, out DiscordGuild result)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gid))
            {
                return ctx.Client.Guilds.TryGetValue(gid, out result);
            }

            if (ctx.Client.Guilds.Any(xg => xg.Value.Name == value))
            {
                result = ctx.Client.Guilds.First(xg => xg.Value.Name == value).Value;
                return true;
            }

            result = null;
            return false;
        }
    }

    public class DiscordMessageConverter : IArgumentConverter<DiscordMessage>
    {
        public bool TryConvert(string value, CommandContext ctx, out DiscordMessage result)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mid))
            {
                result = ctx.Channel.GetMessageAsync(mid).GetAwaiter().GetResult();
                return true;
            }

            result = null;
            return false;
        }
    }

    public class DiscordEmojiConverter : IArgumentConverter<DiscordEmoji>
    {
        private static Regex EmoteRegex { get; }

        static DiscordEmojiConverter()
        {
            EmoteRegex = new Regex(@"^<:([a-zA-Z0-9_]+?):(\d+?)>$", RegexOptions.ECMAScript);
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordEmoji result)
        {
            if (DiscordEmoji.UnicodeEmojiList.Contains(value))
            {
                result = DiscordEmoji.FromUnicode(ctx.Client, value);
                return true;
            }

            var m = EmoteRegex.Match(value);
            if (m.Success)
            {
                var id = ulong.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                result = DiscordEmoji.FromGuildEmote(ctx.Client, id);
                return true;
            }

            result = null;
            return false;
        }
    }

    public class DiscordColorConverter : IArgumentConverter<DiscordColor>
    {
        private static Regex ColorRegexHex { get; }
        private static Regex ColorRegexRgb { get; }

        static DiscordColorConverter()
        {
            ColorRegexHex = new Regex(@"^#?([a-fA-F0-9]{6})$");
            ColorRegexRgb = new Regex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$");
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordColor result)
        {
            result = default;

            var m = ColorRegexHex.Match(value);
            if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var clr))
            {
                result = new DiscordColor(clr);
                return true;
            }

            m = ColorRegexRgb.Match(value);
            if (m.Success)
            {
                var p1 = byte.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r);
                var p2 = byte.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var g);
                var p3 = byte.TryParse(m.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b);

                if (!(p1 && p2 && p3))
                    return false;

                result = new DiscordColor(r, g, b);
                return true;
            }
            
            return false;
        }
    }
}
