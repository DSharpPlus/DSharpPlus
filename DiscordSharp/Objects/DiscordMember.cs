using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordUser
    {
        private DiscordClient __parent;
        public DiscordUser(DiscordClient p)
        {
            __parent = p;
        }

        public string username { get; internal set; }
        public string id { get; internal set; }
        public string discriminator { get; internal set; }
        public string avatar { get; internal set; }
        public bool verified { get; internal set; }
        public string email { get; internal set; }

        /*
        Voice only
        */
        /// <summary>
        /// Whether or not the member can speak/mic enabled in the voice channel.
        /// </summary>
        public bool mute { get; internal set; } = false;
        /// <summary>
        /// Whether or not the member can hear others in the voice channel.
        /// </summary>
        public bool deaf { get; internal set; } = false;

        public void SendMessage(string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Users + $"/{__parent.Me.user.id}" + Endpoints.Channels;
            string initMessage = "{\"recipient_id\":" + id + "}";

            try
            {
                var result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, initMessage));
                if (result != null)
                {
                    SendActualMessage(result["id"].ToString(), message);
                }
            }
            catch (Exception ex)
            {
                __parent.GetTextClientLogger.Log($"Error ocurred while sending message to user, step 1: {ex.Message}", MessageLevel.Error);
            }
        }
        private void SendActualMessage(string id, string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{id}" + Endpoints.Messages;
            DiscordMessage toSend = Utils.GenerateMessage(message);

            try
            {
                WebWrapper.Post(url, DiscordClient.token, JsonConvert.SerializeObject(toSend).ToString());
            }
            catch (Exception ex)
            {
                __parent.GetTextClientLogger.Log($"Error ocurred while sending message to user, step 2: {ex.Message}", MessageLevel.Error);
            }
        }
    }

    public class DiscordMember
    {
        public DiscordUser user { get; set; }
        public List<DiscordRole> roles { get; set; }

        public DiscordServer parent { get; internal set; }

        public DiscordMember(DiscordClient parent)
        {
            user = new DiscordUser(parent);
        }
    }
}
