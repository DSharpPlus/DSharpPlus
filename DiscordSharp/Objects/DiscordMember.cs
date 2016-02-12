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

namespace DiscordSharp.Objects
{
    public enum Status
    {
        Online, Idle, Offline
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

        public Status Status { get; internal set; } = Status.Offline;
        public string CurrentGame { get; internal set; } = null;

        /**
        Voice only
        */
        [JsonProperty("mute")]
        public bool Muted { get; internal set; } = false;
        [JsonProperty("deaf")]
        public bool Deaf { get; internal set; } = false;
        public DiscordChannel CurrentVoiceChannel { get; internal set; }

        internal void SetPresence(string status)
        {
            string checkAgainst = status.ToLower().Trim();
            if (checkAgainst == "online")
                Status = Status.Online;
            else if (checkAgainst == "idle")
                Status = Status.Idle;
            else
                Status = Status.Offline;
        }


        /// <summary>
        /// Applicable only for the currently signed in user.
        /// </summary>
        public string Email { get; internal set; }


        public List<DiscordRole> Roles { get; set; }

        /// <summary>
        /// The server the user belongs to.
        /// </summary>
        public DiscordServer Parent { get; internal set; }
        internal DiscordClient parentclient { get; set; }
        
        internal DiscordMember(DiscordClient parent)
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

        /// <summary>
        /// Kicks this DiscordMember from the guild that's assumed from their 
        /// parent property.
        /// </summary>
        public void Kick()
        {
            if(parentclient.Me.ID == this.ID)
                throw new InvalidOperationException("Can't kick self!");
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{Parent.id}" + Endpoints.Members + $"/{ID}";
            try
            {
                WebWrapper.Delete(url, DiscordClient.token);
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"Error during Kick\n\t{ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Testing copy method.
        /// </summary>
        /// <returns>A copied member, idk what more you expected</returns>
        internal DiscordMember Copy()
        {
            return new DiscordMember
            {
                ID = this.ID,
                Username = this.Username,
                Avatar = this.Avatar,
                Deaf = this.Deaf,
                Discriminator = this.Discriminator,
                Email = this.Email,
                Muted = this.Muted,
                Parent = this.Parent,
                parentclient = this.parentclient,
                Roles = this.Roles,
                Verified = this.Verified
            };
        }

        /// <summary>
        /// Bans this DiscordMember from the guild that's assumed from their
        /// parent property.
        /// </summary>
        /// <param name="days">The number of days the user should be banned for, or 0 for infinite.</param>
        public void Ban(int days = 0)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{Parent.id}" + Endpoints.Bans + $"/{ID}";
            if (days >= 0)
                url += $"?delete-message-days={days}";
            try
            {
                WebWrapper.Put(url, DiscordClient.token);
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"Error during Ban\n\t{ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
            }
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
