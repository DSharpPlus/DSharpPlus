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

        public static string ContentDeliveryNode = "https://cdn.discordapp.com";

        public static string Icons = "/icons";
        public static string Me = "/@me";
        public static string Gateway = "/gateway";
        public static string Auth = "/auth";
        public static string Login = "/login";
        public static string Channels = "/channels";
        public static string Messages = "/messages";
        public static string Users = "/users";
        public static string Guilds = "/guilds";
        public static string Invites = "/invites";
        public static string Invite = "/invite";
        public static string Roles = "/roles";
        public static string Members = "/members";
        public static string Typing = "/typing";
        public static string Avatars = "/avatars";
        public static string Bans = "/bans";
    }
}