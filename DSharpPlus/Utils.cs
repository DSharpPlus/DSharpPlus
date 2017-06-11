using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DSharpPlus
{
    public static class Utils
    {
        /// <summary>
        /// Gets the version of the library
        /// </summary>
        public static Version LibraryVersion { get; private set; }
        private static string VersionHeader { get; set; }

        static Utils()
        {
            var a = typeof(Utils).GetTypeInfo().Assembly;
            var n = a.GetName();
            LibraryVersion = n.Version;
            VersionHeader = string.Concat("DiscordBot (https://github.com/NaamloosDT/DSharpPlus, ", n.Version.ToString(2) , ")");
        }

        internal static int CalculateIntegrity(int ping, DateTimeOffset timestamp, int heartbeat_interval)
        {
            Random r = new Random();
            return r.Next(ping, int.MaxValue);
        }

        internal static string GetApiBaseUri(DiscordClient client)
        {
            return GetApiBaseUri(client.config);
        }
        
        internal static string GetApiBaseUri(DiscordConfig config)
        { 
            switch(config.DiscordBranch)
            {
                case Branch.Canary:
                    return Endpoints.CanaryBaseUri;
                case Branch.PTB:
                    return Endpoints.PTBBaseUri;
                case Branch.Stable:
                    return Endpoints.StableBaseUri;
                default:
                    throw new NotSupportedException("");
            }
        }

        internal static string GetFormattedToken(DiscordClient client)
        {
            return GetFormattedToken(client.config);
        }

        internal static string GetFormattedToken(DiscordConfig config)
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

        public static Dictionary<string, string> GetBaseHeaders()
        {
            return new Dictionary<string, string>();
        }

        public static string GetUserAgent()
        {
            return VersionHeader;
        }

        public static bool ContainsUserMentions(string message)
        {
            string pattern = @"<@(\d+)>";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(message);
        }

        public static bool ContainsNicknameMentions(string message)
        {
            string pattern = @"<@!(\d+)>";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(message);
        }

        public static bool ContainsChannelMentions(string message)
        {
            string pattern = @"<#(\d+)>";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(message);
        }

        public static bool ContainsRoleMentions(string message)
        {
            string pattern = @"<@&(\d+)>";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(message);
        }

        public static bool ContainsEmojis(string message)
        {
            string pattern = @"<:(.*):(\d+)>";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(message);
        }

        public static List<ulong> GetUserMentions(DiscordMessage message)
        {
            var result = new List<ulong>();
            
            var regex = new Regex(@"<@!?(\d+)>");
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                result.Add(ulong.Parse(match.Groups[1].Value));

            return result;
        }

        public static List<ulong> GetRoleMentions(DiscordMessage message)
        {
            var result = new List<ulong>();
            
            var regex = new Regex(@"<@&(\d+)>");
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                result.Add(ulong.Parse(match.Groups[1].Value));

            return result;
        }

        public static List<ulong> GetChannelMentions(DiscordMessage message)
        {
            var result = new List<ulong>();

            var regex = new Regex(@"<#(\d+)>");
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                result.Add(ulong.Parse(match.Groups[1].Value));

            return result;
        }

        public static List<ulong> GetEmojis(DiscordMessage message)
        {
            var result = new List<ulong>();
            
            var regex = new Regex(@"<:([a-zA-Z0-9_]+):(\d+)>");
            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
                result.Add(ulong.Parse(match.Groups[2].Value));

            return result;
        }

        public static DateTimeOffset GetTimestamp(long unixtime)
        {
#if !(NETSTANDARD1_1 || NET45)
            return DateTimeOffset.FromUnixTimeSeconds(unixtime);
#else
            // below constant taken from 
            // https://github.com/dotnet/coreclr/blob/cdb827b6cf72bdb8b4d0dbdaec160c32de7c185f/src/mscorlib/shared/System/DateTimeOffset.cs#L40
            var ticks = unixtime * TimeSpan.TicksPerSecond + 621_355_968_000_000_000;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
#endif
        }
    }
}
