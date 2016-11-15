using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DSharpPlus
{
    public static class Utils
    {

        internal static string GetAPIBaseUri()
        {
            switch(DiscordClient.config.DiscordBranch)
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

        internal static string GetFormattedToken()
        {
            switch (DiscordClient.config.TokenType)
            {
                case TokenType.Bearer:
                    {
                        return $"Bearer {DiscordClient.config.Token}";
                    }
                case TokenType.Bot:
                    {
                        return $"Bot {DiscordClient.config.Token}";
                    }
                case TokenType.User:
                default:
                    {
                        return DiscordClient.config.Token;
                    }
            }
        }

        public static WebHeaderCollection GetBaseHeaders()
        {
            return new WebHeaderCollection() {
                { "Authorization", GetFormattedToken() }
            };
        }

        public static string GetUserAgent()
        {
            return $"DiscordBot (https://github.com/NaamloosDT/DSharpPlus, 1.0)";
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
            List<ulong> result = new List<ulong>();

            string pattern = @"<@(\d+)>";
            Regex regex = new Regex(pattern);

            var matches = regex.Matches(message.Content);
            foreach (var match in matches)
            {
                result.Add(ulong.Parse(match.ToString().Substring(2, match.ToString().Length - 3)));
            }

            pattern = @"<@!(\d+)>";
            regex = new Regex(pattern);

            matches = regex.Matches(message.Content);
            foreach (var match in matches)
            {
                result.Add(ulong.Parse(match.ToString().Substring(3, match.ToString().Length - 4)));
            }

            return result;
        }

        public static List<ulong> GetRoleMentions(DiscordMessage message)
        {
            List<ulong> result = new List<ulong>();

            string pattern = @"<@&(\d+)>";
            Regex regex = new Regex(pattern);

            var matches = regex.Matches(message.Content);
            foreach (var match in matches)
            {
                result.Add(ulong.Parse(match.ToString().Substring(3, match.ToString().Length - 4)));
            }

            return result;
        }

        public static List<ulong> GetChannelMentions(DiscordMessage message)
        {
            List<ulong> result = new List<ulong>();

            string pattern = @"<#(\d+)>";
            Regex regex = new Regex(pattern);

            var matches = regex.Matches(message.Content);
            foreach (var match in matches)
            {
                result.Add(ulong.Parse(match.ToString().Substring(2, match.ToString().Length - 3)));
            }

            return result;
        }
    }
}
