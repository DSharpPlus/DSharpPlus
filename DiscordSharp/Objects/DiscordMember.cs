using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    [Obsolete]
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

        
    }

    public class DiscordMember
    {
        [JsonProperty("username")]
        public string Username { get; internal set; }
        [JsonProperty("id")]
        public string ID { get; internal set; }
        [JsonProperty("discriminator")]
        public string Discriminator { get; internal set; }
        [JsonProperty("avatar")]
        public string Avatar { get; internal set; }
        [JsonProperty("verified")]
        public bool Verified { get; internal set; }

        /**
        Voice only
        */
        [JsonProperty("mute")]
        public bool Muted { get; internal set; } = false;
        [JsonProperty("deaf")]
        public bool Deaf { get; internal set; } = false;


        /// <summary>
        /// Applicable only for the currently signed in user.
        /// </summary>
        public string Email { get; internal set; }


        //public DiscordUser user { get; set; }
        public List<DiscordRole> Roles { get; set; }

        /// <summary>
        /// The server the user belongs to.
        /// </summary>
        public DiscordServer Parent { get; internal set; }
        internal DiscordClient parentclient { get; set; }
        public DiscordMember(DiscordClient parent)
        {
            Roles = new List<DiscordRole>();
        }
        /// <summary>
        /// should only be used for Newtonsoft.Json
        /// </summary>
        internal DiscordMember() { Roles = new List<DiscordRole>(); }

        /// <summary>
        /// Gets the user's avatar.
        /// </summary>
        /// <returns>A System.Drawing.Bitmap object that is the user's avatar.</returns>
        public Bitmap GetAvatar()
        {
            using (var wc = new WebClient())
            {
                byte[] image = wc.DownloadData(GetAvatarURL().ToString());
                return new Bitmap(new MemoryStream(image));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The URL of the user's avatar.</returns>
        public Uri GetAvatarURL()
        {
            return new Uri(Endpoints.BaseAPI + Endpoints.Users + $"/{ID}" + Endpoints.Avatars + $"/{Avatar}.jpg");
        }

        public void SlideIntoDMs(string message) => SendMessage(message);

        public void SendMessage(string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Users + $"/{parentclient.Me.ID}" + Endpoints.Channels;
            string initMessage = "{\"recipient_id\":" + ID + "}";

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
                parentclient.GetTextClientLogger.Log($"Error ocurred while sending message to user, step 1: {ex.Message}", MessageLevel.Error);
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
                parentclient.GetTextClientLogger.Log($"Error ocurred while sending message to user, step 2: {ex.Message}", MessageLevel.Error);
            }
        }

    }
}
