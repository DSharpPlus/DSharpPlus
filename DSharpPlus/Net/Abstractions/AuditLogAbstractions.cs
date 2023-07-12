// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Abstractions;

internal sealed class AuditLogActionChange
{
    // this can be a string or an array
    [JsonProperty("old_value")]
    public object OldValue { get; set; }

    [JsonIgnore]
    public IEnumerable<JObject> OldValues
        => (this.OldValue as JArray)?.ToDiscordObject<IEnumerable<JObject>>();

    [JsonIgnore]
    public ulong OldValueUlong
        => (ulong)this.OldValue;

    [JsonIgnore]
    public string OldValueString
        => (string)this.OldValue;

    [JsonIgnore]
    public bool OldValueBool
        => (bool)this.OldValue;

    [JsonIgnore]
    public long OldValueLong
        => (long)this.OldValue;


    // this can be a string or an array
    [JsonProperty("new_value")]
    public object NewValue { get; set; }

    [JsonIgnore]
    public IEnumerable<JObject> NewValues
        => (this.NewValue as JArray)?.ToDiscordObject<IEnumerable<JObject>>();

    [JsonIgnore]
    public ulong NewValueUlong
        => (ulong)this.NewValue;

    [JsonIgnore]
    public string NewValueString
        => (string)this.NewValue;

    [JsonIgnore]
    public bool NewValueBool
        => (bool)this.NewValue;

    [JsonIgnore]
    public long NewValueLong
        => (long)this.NewValue;

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
    [JsonProperty("application_commands")]
    private IEnumerable<DiscordApplicationCommand> SlashCommands { get; set; }

    [JsonProperty("audit_log_entries")]
    public IEnumerable<AuditLogAction> Entries { get; set; }
        
    [JsonProperty("auto_moderation_rules")]
    private IEnumerable<DiscordAutoModerationRule> AutoModerationRules { get; set; }
        
    [JsonProperty("guild_scheduled_events")]
    public IEnumerable<DiscordScheduledGuildEvent> Events { get; set; }
        
    [JsonProperty("integrations")]
    public IEnumerable<DiscordIntegration> Integrations { get; set; }
        
    [JsonProperty("threads")]
    public IEnumerable<DiscordThreadChannel> Threads { get; set; }
        
    [JsonProperty("users")]
    public IEnumerable<DiscordUser> Users { get; set; }

    [JsonProperty("webhooks")]
    public IEnumerable<DiscordWebhook> Webhooks { get; set; }
}
