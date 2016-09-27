using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Objects;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Toolbox
{
    public static class Tools
    {
        /// <summary>
        /// Takes a DiscordServer and provides a list of DiscordMembers with admin permissions.
        /// </summary>
        /// <param name="server">The discord server to sample from</param>
        /// <returns></returns>
        public static List<DiscordMember> AdminList(DiscordServer server)
        {
            return server.membersAsList.Where(t => t.HasPermission(DiscordSpecialPermissions.Administrator)).ToList();
        }
        /// <summary>
        /// Returns a list of owners as DiscordMember objects meaning you will get a list with all owners of the servers the client is connected to.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static List<DiscordMember> Owners(DiscordClient client)
        {
            return client.GetServersList().Select(t => t.Owner).ToList();
        }

        public static string GetWord(this string s, int i)
        {
            return s.Split(' ')[i];
        }

        public static string[] GetWordsPastFirst(this string s)
        {
            List<string> words = new List<string>();
            for(int i = 1; i < s.WordCount(); i++)
            {
                words.Add(s.GetWord(i));
            }
            return words.ToArray();
        }

        public static string getUserToken(string Email, string Password)
        {
            string url = Endpoints.BaseAPI + Endpoints.Auth + Endpoints.Login;
            string content = $"{{\"email\":\"{Email}\", \"password\":\"{Password}\"}}";
            try
            {
                JObject result = JObject.Parse(WebWrapper.Post(url, content));
                return result["token"].ToString().Trim();
            }
            catch
            {
                throw new Exception("Either your Email or Password is wrong, or something else fcked up!");
            }
        }

        public static int WordCount(this string s)
        {
            return s.Split(' ').Count();
        }
    }
}
