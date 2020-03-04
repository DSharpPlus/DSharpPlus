using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class AuditLogUser
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("avatar")]
        public string AvatarHash { get; set; }
    }

    internal sealed class AuditLogWebhook
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("avatar")]
        public string AvatarHash { get; set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }
    }

    internal sealed class AuditLogActionChange
    {
        // this can be a string or an array
        [JsonProperty("old_value")]
        public object OldValue { get; set; }

        [JsonIgnore]
        public IEnumerable<JObject> OldValues 
            => (this.OldValue as JArray)?.ToObject<IEnumerable<JObject>>();

        [JsonIgnore]
        public ulong OldValueUlong 
            => (ulong)this.OldValue;

        [JsonIgnore]
        public string OldValueString 
            => (string)this.OldValue;

        // this can be a string or an array
        [JsonProperty("new_value")]
        public object NewValue { get; set; }

        [JsonIgnore]
        public IEnumerable<JObject> NewValues 
            => (this.NewValue as JArray)?.ToObject<IEnumerable<JObject>>();

        [JsonIgnore]
        public ulong NewValueUlong 
            => (ulong)this.NewValue;

        [JsonIgnore]
        public string NewValueString 
            => (string)this.NewValue;

        [JsonProperty("key")]
        public string Key { get; set; }
    }

    internal sealed class AuditLogActionOptions
    {
        [JsonProperty("type")]
        public object Type { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("message_id")]
        public ulong MessageId { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
        
        [JsonProperty("delete_member_days")]
        public int DeleteMemberDays { get; set; }

        [JsonProperty("members_removed")]
        public int MembersRemoved { get; set; }
    }

    internal sealed class AuditLogAction
    {
        [JsonProperty("target_id")]
        public ulong? TargetId { get; set; }

        [JsonProperty("user_id")]
        public ulong UserId { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("action_type")]
        public AuditLogActionType ActionType { get; set; }

        [JsonProperty("changes")]
        public IEnumerable<AuditLogActionChange> Changes { get; set; }

        [JsonProperty("options")]
        public AuditLogActionOptions Options { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    internal sealed class AuditLog
    {
        [JsonProperty("webhooks")]
        public IEnumerable<AuditLogWebhook> Webhooks { get; set; }

        [JsonProperty("users")]
        public IEnumerable<AuditLogUser> Users { get; set; }

        [JsonProperty("audit_log_entries")]
        public IEnumerable<AuditLogAction> Entries { get; set; }
    }
}