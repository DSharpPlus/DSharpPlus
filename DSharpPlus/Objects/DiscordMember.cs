﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Objects
{
    public enum Status
    {
        Online, Idle, Offline
    }
    public class DiscordMember
    {
        /// <summary>
        /// User's username
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; internal set; }
        /// <summary>
        /// User's ID
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; internal set; }
        /// <summary>
        /// User's Discriminator (e.g. John#1234, 1234 is discriminator)
        /// </summary>
        [JsonProperty("discriminator")]
        public string Discriminator { get; internal set; }
        /// <summary>
        /// User's Avatar
        /// </summary>
        [JsonProperty("avatar")]
        public string Avatar { get; internal set; }
        /// <summary>
        /// Wether this user has verified its email or not
        /// </summary>
        [JsonProperty("verified")]
        public bool Verified { get; internal set; }
        /// <summary>
        /// Wether this user is a bot or not
        /// </summary>
        [JsonProperty("bot")]
        public bool IsBot { get; internal set; } = false;
        /// <summary>
        /// User's join date
        /// </summary>
        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; internal set; }
        /// <summary>
        /// User's Nickname
        /// </summary>
        [JsonProperty("nick")]
        public string Nickname { get; internal set; } = "";

        /// <summary>
        /// Is user online?
        /// </summary>
        public Status Status { get; internal set; } = Status.Offline;
        /// <summary>
        /// What game this user is playing
        /// </summary>
        public string CurrentGame { get; internal set; } = null;
        /// <summary>
        /// Is this user streaming?
        /// </summary>
        public bool Streaming { get; internal set; } = false;
        /// <summary>
        /// This user's stream URL
        /// </summary>
        public string StreamURL { get; internal set; } = null;
        /// <summary>
        /// Is this user a bot developer? (manually set ID's in DiscordClient.developers)
        /// </summary>
        public bool IsDeveloper { get { return DiscordClient.developers.Contains(this.ID); } }
        /// <summary>
        /// Automatically builds the text to mention this user.
        /// </summary>
        public string Mention { get { return "<@" + this.ID + ">"; } }

        /**
        Voice only
        */
        /// <summary>
        /// Is this user muted?
        /// </summary>
        [JsonProperty("mute")]
        public bool Muted { get; internal set; } = false;
        /// <summary>
        /// Is this user deafened?
        /// </summary>
        [JsonProperty("deaf")]
        public bool Deaf { get; internal set; } = false;
        /// <summary>
        /// User's current voice channel
        /// </summary>
        public DiscordChannel CurrentVoiceChannel { get; internal set; }
        /// <summary>
        /// User's voice state
        /// </summary>
        public DiscordVoiceState VoiceState { get; internal set; }

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

        /// <summary>
        /// List of user's roles
        /// </summary>
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
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{Parent.ID}" + Endpoints.Members + $"/{ID}";
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
        /// Changes the nickname for this user, if you have permission to do so.
        /// </summary>
        /// <param name="nickname">null for no nickname.</param>
        public void ChangeNickname(string nickname)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{Parent.ID}" + Endpoints.Members + $"/{ID}";
            string payload = JsonConvert.SerializeObject
            (
                new
                {
                    nick = (nickname == null ? "" : nickname)
                }
            );
            var strResult = WebWrapper.Patch(url, DiscordClient.token, payload);
        }

        /// <summary>
        /// (Un)Mute this user, if you have permission to do so.
        /// </summary>
        /// <param name="ismuted">Wether this user is muted or not</param>
        public void UpdateMute(bool ismuted)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{Parent.ID}" + Endpoints.Members + $"/{ID}";
            string payload = JsonConvert.SerializeObject
            (
                new
                {
                    Muted = ismuted
                }
            );
            var strResult = WebWrapper.Patch(url, DiscordClient.token, payload);
        }

        /// <summary>
        /// (Un)Deafen this user, if you have permission to do so.
        /// </summary>
        /// <param name="isdeafened">Wether this user is deafened or not</param>
        public void UpdateDeaf(bool isdeafened)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{Parent.ID}" + Endpoints.Members + $"/{ID}";
            string payload = JsonConvert.SerializeObject
            (
                new
                {
                    Deaf = isdeafened
                }
            );
            var strResult = WebWrapper.Patch(url, DiscordClient.token, payload);
        }


        /// <summary>
        /// Iterates all the roles the user has checking if any of the present have the permission you pass.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <returns>True if the permission is present.</returns>
        public bool HasPermission(DiscordSpecialPermissions permission)
        {
            var result = false;
            Roles.ForEach(x =>
            {
                if (x.Permissions.HasPermission(permission))
                { result = true; return; }
            });

            return result;
        }

        /// <summary>
        /// If the user has the specified role.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool HasRole(string role)
        {
            return Roles.Select(t => t.Name).Contains(role);
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
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{Parent.ID}" + Endpoints.Bans + $"/{ID}";
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

        /// <summary>
        /// basically SendMessage. prbably intended as a joke by original dev.
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SlideIntoDMs(string message) => SendMessage(message);

        /// <summary>
        /// Sends a message to this user
        /// </summary>
        /// <param name="message">Message to send</param>
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
#if DEBUG
                if (ex.Message.Contains("400"))
                    return;
#endif
                parentclient.GetTextClientLogger.Log($"Error ocurred while sending message to user, step 1: {ex.Message}", MessageLevel.Error);
            }
        }
        private void SendActualMessage(string id, string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{id}" + Endpoints.Messages;
            DiscordMessage toSend = Utils.GenerateMessage(message, false);

            try
            {
                WebWrapper.Post(url, DiscordClient.token, JsonConvert.SerializeObject(toSend).ToString());
            }
            catch (Exception ex)
            {
                parentclient.GetTextClientLogger.Log($"Error ocurred while sending message to user, step 2: {ex.Message}", MessageLevel.Error);
            }
        }

        private bool Equals(DiscordMember obj)
        {
            return this.ID == obj.ID;
        }

        public static bool operator==(DiscordMember x, DiscordMember y)
        {
            if ((object)x == null && (object)y == null)
                return true;
            if ((object)x == null || (object)y == null)
                return false;

            return x.ID == y.ID;
        }

        public static bool operator !=(DiscordMember x, DiscordMember y)
        {
            if ((object)x == null && (object)x == null)
                return false;
            if ((object)x == null || (object)y == null)
                return true;

            return x.ID != y.ID;
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(DiscordMember))
                return this == (DiscordMember)obj;
            else
                return false;
        }
    }
}
