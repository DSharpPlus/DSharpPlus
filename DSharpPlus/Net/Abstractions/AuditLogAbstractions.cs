using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    internal sealed class AuditLogActionChange
    {
        [JsonProperty("old_value")]
        public string OldValue { get; set; }

        // this can be a string or an array
        [JsonProperty("new_value")]
        public object NewValue { get; set; }

        [JsonIgnore]
        public IEnumerable<AuditLogActionChangeNewValue> NewValues => (this.NewValue as JArray).ToObject<IEnumerable<AuditLogActionChangeNewValue>>();

        [JsonIgnore]
        public ulong NewValueNumeric => (ulong)this.NewValue;

        [JsonIgnore]
        public string NewValueString => (string)this.NewValue;

        [JsonProperty("key")]
        public string Key { get; set; }
    }

    internal sealed class AuditLogActionChangeNewValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }
    }

    internal sealed class AuditLogActionOptions
    {
        [JsonProperty("type")]
        public object Type { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }
    }

    internal sealed class AuditLogAction
    {
        [JsonProperty("target_id")]
        public ulong TargetId { get; set; }

        [JsonProperty("user_id")]
        public ulong UserId { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("action_type")]
        public int ActionType { get; set; }

        [JsonProperty("changes")]
        public IEnumerable<AuditLogActionChange> Changes { get; set; }

        [JsonProperty("options")]
        public AuditLogActionOptions Options { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
