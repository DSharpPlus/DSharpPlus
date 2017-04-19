﻿using System.Linq;
using System.Text.RegularExpressions;

namespace DSharpPlus.CommandsNext.Converters
{
    public class DiscordUserConverter : IArgumentConverter<DiscordUser>
    {
        private static Regex UserRegex { get; set; }

        static DiscordUserConverter()
        {
            UserRegex = new Regex(@"<@\!?(\d+?)>");
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordUser result)
        {
            if (ulong.TryParse(value, out var uid))
            {
                result = ctx.Client.GetUser(uid).GetAwaiter().GetResult();
                return true;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, out uid))
            {
                result = ctx.Client.GetUser(uid).GetAwaiter().GetResult();
                return true;
            }

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Client.Guilds
                .SelectMany(xkvp => xkvp.Value.Members)
                .Where(xm => xm.User.Username == un && ((dv != null && xm.User.Discriminator.ToString("0000") == dv) || true));

            if (us.Any())
            {
                result = us.First().User;
                return true;
            }

            result = null;
            return false;
        }
    }

    public class DiscordMemberConverter : IArgumentConverter<DiscordMember>
    {
        private static Regex UserRegex { get; set; }

        static DiscordMemberConverter()
        {
            UserRegex = new Regex(@"<@\!?(\d+?)>");
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordMember result)
        {
            if (ulong.TryParse(value, out var uid))
            {
                result = ctx.Guild.GetMember(uid).GetAwaiter().GetResult();
                return true;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, out uid))
            {
                result = ctx.Guild.GetMember(uid).GetAwaiter().GetResult();
                return true;
            }

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Guild.Members
                .Where(xm => xm.User.Username == un && ((dv != null && xm.User.Discriminator.ToString("0000") == dv) || true));

            if (us.Any())
            {
                result = us.First();
                return true;
            }

            result = null;
            return false;
        }
    }

    public class DiscordChannelConverter : IArgumentConverter<DiscordChannel>
    {
        private static Regex ChannelRegex { get; set; }

        static DiscordChannelConverter()
        {
            ChannelRegex = new Regex(@"<#(\d+)>");
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordChannel result)
        {
            if (ulong.TryParse(value, out var cid))
            {
                result = ctx.Guild.Channels.FirstOrDefault(xc => xc.ID == cid);
                return true;
            }

            var m = ChannelRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, out cid))
            {
                result = ctx.Guild.Channels.FirstOrDefault(xc => xc.ID == cid);
                return true;
            }

            var chn = ctx.Guild.Channels.FirstOrDefault(xc => xc.Name == value);
            result = chn;
            return true;
        }
    }

    public class DiscordRoleConverter : IArgumentConverter<DiscordRole>
    {
        private static Regex RoleRegex { get; set; }

        static DiscordRoleConverter()
        {
            RoleRegex = new Regex(@"<@&(\d+?)>");
        }

        public bool TryConvert(string value, CommandContext ctx, out DiscordRole result)
        {
            if (ulong.TryParse(value, out var rid))
            {
                result = ctx.Guild.Roles.FirstOrDefault(xr => xr.ID == rid);
                return true;
            }

            var m = RoleRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, out rid))
            {
                result = ctx.Guild.Roles.FirstOrDefault(xr => xr.ID == rid);
                return true;
            }

            var rl = ctx.Guild.Roles.FirstOrDefault(xr => xr.Name == value);
            result = rl;
            return true;
        }
    }

    public class DiscordGuildConverter : IArgumentConverter<DiscordGuild>
    {
        public bool TryConvert(string value, CommandContext ctx, out DiscordGuild result)
        {
            if (ulong.TryParse(value, out var gid))
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
}
