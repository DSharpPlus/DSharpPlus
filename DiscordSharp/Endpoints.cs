using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    /// <summary>
    /// The class that contains the end points needed by DiscordSharp.
    /// </summary>
    public static class Endpoints
    {
        public static string BaseAPI = "https://discordapp.com/api";
        public static string Gateway = "/gateway";
        public static string Auth = "/auth";
        public static string Login = "/login";
        public static string Channels = "/channels";
        public static string Messages = "/messages";
        public static string Users = "/users";
        public static string Guilds = "/guilds";
        public static string Invites = "/invites";
    }
}