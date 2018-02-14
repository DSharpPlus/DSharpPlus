using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.Net;

namespace DSharpPlus
{
    /// <summary>
    /// Various Discord-related utilities.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Gets the version of the library
        /// </summary>
        private static string VersionHeader { get; set; }
        private static Dictionary<Permissions, string> PermissionStrings { get; set; }

        static Utilities()
        {
            PermissionStrings = new Dictionary<Permissions, string>();
            var t = typeof(Permissions);
            var ti = t.GetTypeInfo();
            var vals = Enum.GetValues(t).Cast<Permissions>();

            foreach (var xv in vals)
            {
                var xsv = xv.ToString();
                var xmv = ti.DeclaredMembers.FirstOrDefault(xm => xm.Name == xsv);
                var xav = xmv.GetCustomAttribute<PermissionStringAttribute>();

                PermissionStrings[xv] = xav.String;
            }

            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var vs = "";
            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                vs = iv.InformationalVersion;
            else
            {
                var v = a.GetName().Version;
                vs = v.ToString(3);
            }

            VersionHeader = string.Concat("DiscordBot (https://github.com/NaamloosDT/DSharpPlus, v", vs, ")");
        }

        internal static int CalculateIntegrity(int ping, DateTimeOffset timestamp, int heartbeat_interval)
        {

            Random r = new Random();
            return r.Next(ping, int.MaxValue);
        }

        internal static string GetApiBaseUri() 
            => Endpoints.BASE_URI;

        internal static string GetFormattedToken(BaseDiscordClient client)
        {
            return GetFormattedToken(client.Configuration);
        }

        internal static string GetFormattedToken(DiscordConfiguration config)
        { 
            switch (config.TokenType)
            {
                case TokenType.Bearer:
                    {
                        return $"Bearer {config.Token}";
                    }
                case TokenType.Bot:
                    {
                        return $"Bot {config.Token}";
                    }
                default:
                    {
                        return config.Token;
                    }
            }
        }

        internal static Dictionary<string, string> GetBaseHeaders()
            => new Dictionary<string, string>();

        internal static string GetUserAgent()
            => VersionHeader;

        internal static bool ContainsUserMentions(string message)
        {
            string pattern = @"<@(\d+)>";
            Regex regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsNicknameMentions(string message)
        {
            string pattern = @"<@!(\d+)>";
            Regex regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsChannelMentions(string message)
        {
            string pattern = @"<#(\d+)>";
            Regex regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsRoleMentions(string message)
        {
            string pattern = @"<@&(\d+)>";
            Regex regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static bool ContainsEmojis(string message)
        {
            string pattern = @"<:(.*):(\d+)>";
            Regex regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(message);
        }

        internal static IEnumerable<ulong> GetUserMentions(DiscordMessage message)
        {
            var regex = new Regex(@"<@!?(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        internal static IEnumerable<ulong> GetRoleMentions(DiscordMessage message)
        {
            var regex = new Regex(@"<@&(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        internal static IEnumerable<ulong> GetChannelMentions(DiscordMessage message)
        {
            var regex = new Regex(@"<#(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        internal static IEnumerable<ulong> GetEmojis(DiscordMessage message)
        {
            var regex = new Regex(@"<:([a-zA-Z0-9_]+):(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Helper method to create a <see cref="DateTimeOffset"/> from Unix time seconds for targets that do not support this natively.
        /// </summary>
        /// <param name="unixTime">Unix time seconds to convert.</param>
        /// <param name="shouldThrow">Whether the method should throw on failure. Defaults to true.</param>
        /// <returns>Calculated <see cref="DateTimeOffset"/>.</returns>
        public static DateTimeOffset GetDateTimeOffset(long unixTime, bool shouldThrow = true)
        {
            try
            {
#if !(NETSTANDARD1_1 || NET45)
                return DateTimeOffset.FromUnixTimeSeconds(unixTime);
#else
                // below constant taken from 
                // https://github.com/dotnet/coreclr/blob/cdb827b6cf72bdb8b4d0dbdaec160c32de7c185f/src/mscorlib/shared/System/DateTimeOffset.cs#L40
                var ticks = unixTime * TimeSpan.TicksPerSecond + 621_355_968_000_000_000;
                return new DateTimeOffset(ticks, TimeSpan.Zero);
#endif
            }
            catch (Exception)
            {
                if (shouldThrow)
                    throw;

                return DateTimeOffset.MinValue;
            }
        }

        /// <summary>
        /// Helper method to create a <see cref="DateTimeOffset"/> from Unix time milliseconds for targets that do not support this natively.
        /// </summary>
        /// <param name="unixTime">Unix time milliseconds to convert.</param>
        /// <param name="shouldThrow">Whether the method should throw on failure. Defaults to true.</param>
        /// <returns>Calculated <see cref="DateTimeOffset"/>.</returns>
        public static DateTimeOffset GetDateTimeOffsetFromMilliseconds(long unixTime, bool shouldThrow = true)
        {
            try
            {
#if !(NETSTANDARD1_1 || NET45)
                return DateTimeOffset.FromUnixTimeMilliseconds(unixTime);
#else
                // below constant taken from 
                // https://github.com/dotnet/coreclr/blob/cdb827b6cf72bdb8b4d0dbdaec160c32de7c185f/src/mscorlib/shared/System/DateTimeOffset.cs#L40
                var ticks = unixTime * TimeSpan.TicksPerMillisecond + 621_355_968_000_000_000;
                return new DateTimeOffset(ticks, TimeSpan.Zero);
#endif
            }
            catch (Exception)
            {
                if (shouldThrow)
                    throw;

                return DateTimeOffset.MinValue;
            }
        }

        /// <summary>
        /// Helper method to calculate Unix time seconsd from a <see cref="DateTimeOffset"/> for targets that do not support this natively.
        /// </summary>
        /// <param name="dto"><see cref="DateTimeOffset"/> to calculate Unix time for.</param>
        /// <returns>Calculated Unix time.</returns>
        public static long GetUnixTime(DateTimeOffset dto)
        {
#if !(NETSTANDARD1_1 || NET45)
            return dto.ToUnixTimeMilliseconds();
#else
            // below constant taken from 
            // https://github.com/dotnet/coreclr/blob/cdb827b6cf72bdb8b4d0dbdaec160c32de7c185f/src/mscorlib/shared/System/DateTimeOffset.cs#L40
            var millis = dto.Ticks / TimeSpan.TicksPerMillisecond;
            return millis - 62_135_596_800_000;
#endif
        }
        
        /// <summary>
        /// Converts this <see cref="Permissions"/> into human-readable format.
        /// </summary>
        /// <param name="perm">Permissions enumeration to convert.</param>
        /// <returns>Human-readable permissions.</returns>
        public static string ToPermissionString(this Permissions perm)
        {
            if (perm == Permissions.None)
                return PermissionStrings[perm];

            perm &= PermissionMethods.FULL_PERMS;

            var strs = PermissionStrings
                .Where(xkvp => xkvp.Key != Permissions.None && (perm & xkvp.Key) == xkvp.Key)
                .Select(xkvp => xkvp.Value);

            return string.Join(", ", strs.OrderBy(xs => xs));
        }

        /// <summary>
        /// Checks whether this string contains given characters.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <param name="characters">Characters to check for.</param>
        /// <returns>Whether the string contained these characters.</returns>
        public static bool Contains(this string str, params char[] characters)
        {
            foreach (var xc in str)
                if (characters.Contains(xc))
                    return true;

            return false;
        }
    }
}
