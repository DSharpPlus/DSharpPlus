using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DiscordSharp.Objects
{
    public abstract class DiscordChannelBase
    {
        [JsonProperty("id")]
        public string ID { get; internal set; }
        [JsonProperty("is_private")]
        public bool Private { get; internal set; }

        internal DiscordClient Client { get; set; }

        internal DiscordChannelBase() { }

        /// <summary>
        /// Simulates typing in the specified channel. Automatically times out/stops after either:
        /// -10 Seconds
        /// -A message is sent
        /// </summary>
        /// <param name="channel"></param>
        public void SimulateTyping()
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID}" + Endpoints.Typing;
            try
            {
                WebWrapper.Post(url, DiscordClient.token, "", true);
            }
            catch (Exception ex)
            {
                Client.GetTextClientLogger.Log("Exception ocurred while simulating typing: " + ex.Message, MessageLevel.Error);
            }
        }
    }

    public enum ChannelType
    {
        Text, Voice
    }

    public class DiscordChannel : DiscordChannelBase
    {
        [JsonProperty("type")]
        public ChannelType Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        private int __bitrate;

        /// <summary>
        /// (Voice only) The channel's configured bitrate, in bps (bits per second).
        /// It's highly recommended you use this as opposed to your own bitrate.
        /// To retrieve the bitrate in kbps, divide by 1000.
        /// </summary>
        [JsonProperty("bitrate")]
        public int Bitrate
        {
            get
            {
                if (Type != ChannelType.Voice)
                    return -1;
                else
                    return __bitrate;
            }
            internal set
            {
                __bitrate = value;
            }
        }

        public List<DiscordPermissionOverride> PermissionOverrides { get; set; }

        public DiscordServer Parent { get; internal set; }

        public DiscordMessage SendMessage(string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID}" + Endpoints.Messages;
            JObject result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, JsonConvert.SerializeObject(Utils.GenerateMessage(message))));

            if (result["content"].IsNullOrEmpty())
                throw new InvalidOperationException("Request returned a blank message, you may not have permission to send messages yet!");

            DiscordMessage m = new DiscordMessage
            {
                ID = result["id"].ToString(),
                Attachments = result["attachments"].ToObject<DiscordAttachment[]>(),
                Author = this.Parent.GetMemberByKey(result["author"]["id"].ToString()),
                channel = this,
                Content = result["content"].ToString(),
                RawJson = result,
                timestamp = result["timestamp"].ToObject<DateTime>()
            };
            return m;
        }

        private void DeleteMessage(DiscordMessage message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID    }" + Endpoints.Messages + $"/{message.ID}";
            var result = JObject.Parse(WebWrapper.Delete(url, DiscordClient.token));
        }

        public DiscordChannel ShallowCopy()
        {
            DiscordChannel channel = (DiscordChannel)this.MemberwiseClone();
            return channel;
        }

        internal DiscordChannel() { }
    }

    public class DiscordPrivateChannel : DiscordChannelBase
    {
        internal string user_id { get; set; }

        public DiscordMember Recipient { get; set; }
        [JsonProperty("last_message_id")]
        private string LastMessageID { get; set; }

        internal DiscordPrivateChannel() { }
    }

    //kinda like the author
    [Obsolete]
    public class DiscordRecipient
    {
        public string username { get; set; }
        public string id { get; set; }
        internal DiscordRecipient() { }
    }

    
}
