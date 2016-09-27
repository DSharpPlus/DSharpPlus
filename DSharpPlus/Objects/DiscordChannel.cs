using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace DSharpPlus.Objects
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

    /// <summary>
    /// A channel that is on Discord
    /// </summary>
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

        [JsonProperty("guild_id")]
        public string ServerID { get; }

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

        /// <summary>
        /// The parent discord server of a channel
        /// </summary>
        public DiscordServer Parent { get; internal set; }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="message">Your message's text.</param>
        public DiscordMessage SendMessage(string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID}" + Endpoints.Messages;
            JObject result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, JsonConvert.SerializeObject(Utils.GenerateMessage(message, false))));

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
                timestamp = result["timestamp"].ToObject<DateTime>(),
                TTS = false
            };
            return m;
        }

        public Task<DiscordMessage> SendMessageAsync(string message)
        {
            return Task.FromResult(this.SendMessage(message));
        }

        /// <summary>
        /// Sends a Text-To-Speech message
        /// </summary>
        /// <param name="message">Your message's text.</param>
        public DiscordMessage SendMessageTTS(string message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID}" + Endpoints.Messages;
            JObject result = JObject.Parse(WebWrapper.Post(url, DiscordClient.token, JsonConvert.SerializeObject(Utils.GenerateMessage(message, true))));

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
                timestamp = result["timestamp"].ToObject<DateTime>(),
                TTS = true
            };
            return m;
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="message">The message you wish to delete.</param>
        public void DeleteMessage(DiscordMessage message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID    }" + Endpoints.Messages + $"/{message.ID}";
            WebWrapper.Delete(url, DiscordClient.token);
        }

        /// <summary>
        /// Pins a message to its channel
        /// </summary>
        /// <param name="message">The message you wish to pin.</param>
        public void PinMessage(DiscordMessage message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID    }" + Endpoints.Pins + "/" + message.ID;
            WebWrapper.Put(url, DiscordClient.token);
        }

        /// <summary>
        /// Unpins a message from its channel
        /// </summary>
        /// <param name="message">The message you wish to unpin</param>
        public void UnpinMessage(DiscordMessage message)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID    }" + Endpoints.Pins + "/" + message.ID;
            var result = JObject.Parse(WebWrapper.Delete(url, DiscordClient.token));
        }

        /// <summary>
        /// Gets a list of Pinned messages
        /// </summary>
        public List<DiscordMessage> GetPinnedMessages()
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID    }" + Endpoints.Pins;
            var result = JArray.Parse(WebWrapper.Get(url, DiscordClient.token));
            List<DiscordMessage> pinnedlist = new List<DiscordMessage>();
            foreach(var child in result)
            {
                pinnedlist.Add(JsonConvert.DeserializeObject<DiscordMessage>(child.ToString()));
            }
            return pinnedlist;
        }

        /// <summary>
        /// Gets a message by ID
        /// </summary>
        /// <param name="MessageID">The ID of the message you wish to get.</param>
        public DiscordMessage GetMessage(string MessageID)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID}" + Endpoints.Messages + $"/{MessageID}";
            var result = JObject.Parse(WebWrapper.Get(url, DiscordClient.token));
            return JsonConvert.DeserializeObject<DiscordMessage>(result.ToString());
        }

        /// <summary>
        /// Returns a list of recent messages
        /// </summary>
        public List<DiscordMessage> GetMessages()
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID    }" + Endpoints.Messages;
            var result = JArray.Parse(WebWrapper.Get(url, DiscordClient.token));
            List<DiscordMessage> messagelist = new List<DiscordMessage>();
            foreach (var child in result)
            {
                messagelist.Add(JsonConvert.DeserializeObject<DiscordMessage>(child.ToString()));
            }
            return messagelist;
        }

        /// <summary>
        /// Deletes this channel
        /// </summary>
        public void Delete()
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{ID}";
            WebWrapper.Delete(url, DiscordClient.token);
        }

        public DiscordChannel ShallowCopy()
        {
            DiscordChannel channel = (DiscordChannel)this.MemberwiseClone();
            return channel;
        }

        internal DiscordChannel() { }
    }

    /// <summary>
    /// A private discord channel
    /// </summary>
    public class DiscordPrivateChannel : DiscordChannelBase
    {
        internal string user_id { get; set; }

        public DiscordMember Recipient { get; set; }
        [JsonProperty("last_message_id")]
        private string LastMessageID { get; set; }

        internal DiscordPrivateChannel() { }
    }

    /// <summary>
    /// [Deprecated] The reciepient
    /// </summary>
    [Obsolete] //kinda like the author
    public class DiscordRecipient
    {
        public string username { get; set; }
        public string id { get; set; }
        internal DiscordRecipient() { }
    }

    
}
