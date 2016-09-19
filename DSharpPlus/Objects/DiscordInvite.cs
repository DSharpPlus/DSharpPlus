using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DSharpPlus.Objects
{
    public class DiscordInvite
    {
        /// <summary>
        /// Invite code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; internal set; }

        /// <summary>
        /// Invites server info
        /// </summary>
        [JsonProperty("guild")]
        public DiscordInviteServer InviteServer { get; internal set; }

        /// <summary>
        /// Invites channel info
        /// </summary>
        [JsonProperty("channel")]
        public DiscordInviteChannel InviteChannel { get; internal set; }

        /// <summary>
        /// Delete this invite (REQUIRES PERMISSION)
        /// </summary>
        public void Delete()
        {
            string url = Endpoints.BaseAPI + Endpoints.Invites + $"/{Code}";
            WebWrapper.Delete(url, DiscordClient.token);
        }
    }
    /// <summary>
    /// The server the invite is coming from
    /// </summary>
    public class DiscordInviteServer
    {
        /// <summary>
        /// Server ID
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; internal set; }

        /// <summary>
        /// Server Name
        /// </summary>
        [JsonProperty("name")]
        public string name { get; internal set; }
    }
    /// <summary>
    /// The channel the invite is coming from
    /// </summary>
    public class DiscordInviteChannel
    {
        /// <summary>
        /// Channel ID
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; internal set; }

        /// <summary>
        /// Channel Name
        /// </summary>
        [JsonProperty("name")]
        public string name { get; internal set; }

        /// <summary>
        /// Channel Type
        /// </summary>
        [JsonProperty("type")]
        public ChannelType Type { get; set; }
    }
}
