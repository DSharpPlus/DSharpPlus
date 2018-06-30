using Newtonsoft.Json;
using System;

namespace DSharpPlus.Entities
{
    public class DiscordReadState : PropertyChangedBase
    {
        private int _mentionCount;
        private ulong _lastMessageId;
        private DateTimeOffset _lastPinTimestamp;

        [JsonIgnore]
        internal DiscordClient Discord { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; internal set; }

        [JsonIgnore]
        public bool Unread
        {
            get
            {
                var channel = Discord?.InternalGetCachedChannel(Id);

                if (channel?.Type == ChannelType.Text)
                {
                    return channel.LastMessageId != 0 ? channel.LastMessageId > _lastMessageId : false;
                }
                else
                {
                    return false;
                }
            }
        }

        [JsonProperty("mention_count")]
        public int MentionCount { get => _mentionCount; internal set => OnPropertySet(ref _mentionCount, value); }

        [JsonProperty("last_message_id")]
        public ulong LastMessageId { get => _lastMessageId; internal set { OnPropertySet(ref _lastMessageId, value); InvokePropertyChanged(nameof(Unread)); } }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset LastPinTimestamp { get => _lastPinTimestamp; internal set => OnPropertySet(ref _lastPinTimestamp, value); }
    }
}