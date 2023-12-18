using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DSharpPlus.Entities.AuditLogs;

internal static class AuditLogParser
{
    /// <summary>
    /// Parses a AuditLog to a list of AuditLogEntries
    /// </summary>
    /// <param name="guild"> <see cref="DiscordGuild"/> which is the parent of the AuditLog</param>
    /// <param name="auditLog"> <see cref="AuditLog"/> whose entries should be parsed</param>
    /// <returns>A list of <see cref="DiscordAuditLogEntry"/>. All entries which cant be parsed are dropped</returns>
    internal static async IAsyncEnumerable<DiscordAuditLogEntry> ParseAuditLogToEntriesAsync
    (
        DiscordGuild guild,
        AuditLog auditLog
    )
    {
        BaseDiscordClient client = guild.Discord;

        //Get all User
        IEnumerable<DiscordUser> users = auditLog.Users;

        //Update cache if user is not known
        foreach (DiscordUser discordUser in users)
        {
            discordUser.Discord = client;
            if (client.UserCache.ContainsKey(discordUser.Id))
            {
                continue;
            }

            client.UpdateUserCache(discordUser);
        }

        //get unique webhooks, scheduledEvents, threads
        IEnumerable<DiscordWebhook> uniqueWebhooks = auditLog.Webhooks;
        IEnumerable<DiscordScheduledGuildEvent> uniqueScheduledEvents = auditLog.Events;
        IEnumerable<DiscordThreadChannel> uniqueThreads = auditLog.Threads;
        IDictionary<ulong, DiscordWebhook> webhooks = uniqueWebhooks.ToDictionary(x => x.Id);

        //update event cache and create a dictionary for it
        foreach (DiscordScheduledGuildEvent discordEvent in uniqueScheduledEvents)
        {
            if (guild._scheduledEvents.ContainsKey(discordEvent.Id))
            {
                continue;
            }

            guild._scheduledEvents[discordEvent.Id] = discordEvent;
        }

        IDictionary<ulong, DiscordScheduledGuildEvent> events = guild._scheduledEvents;

        foreach (DiscordThreadChannel thread in uniqueThreads)
        {
            if (guild._threads.ContainsKey(thread.Id))
            {
                continue;
            }

            guild._threads[thread.Id] = thread;
        }

        IDictionary<ulong, DiscordThreadChannel> threads = guild._threads;

        IEnumerable<DiscordMember>? discordMembers = users.Select
        (
            user => 
            {
                return guild._members is not null && guild._members.TryGetValue(user.Id, out DiscordMember? member)
                    ? member
                    : new DiscordMember 
                    { 
                        Discord = guild.Discord, 
                        Id = user.Id, 
                        _guild_id = guild.Id 
                    };
            }
        );

        Dictionary<ulong, DiscordMember> members = discordMembers.ToDictionary(xm => xm.Id, xm => xm);

        IOrderedEnumerable<AuditLogAction>? auditLogActions = auditLog.Entries.OrderByDescending(xa => xa.Id);
        foreach (AuditLogAction? auditLogAction in auditLogActions)
        {
            DiscordAuditLogEntry? entry =
                await ParseAuditLogEntryAsync(guild, auditLogAction, members, threads, webhooks, events);

            if (entry is null)
            {
                continue;
            }

            yield return entry;
        }
    }

    /// <summary>
    /// Tries to parse a AuditLogAction to a DiscordAuditLogEntry
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <param name="members">A dictionary of <see cref="DiscordMember"/> which is used to inject the entities instead of passing the id</param>
    /// <param name="threads">A dictionary of <see cref="DiscordThreadChannel"/> which is used to inject the entities instead of passing the id</param>
    /// <param name="webhooks">A dictionary of <see cref="DiscordWebhook"/> which is used to inject the entities instead of passing the id</param>
    /// <param name="events">A dictionary of <see cref="DiscordScheduledGuildEvent"/> which is used to inject the entities instead of passing the id</param>
    /// <returns>Returns a <see cref="DiscordAuditLogEntry"/>. Is null if the entry can not be parsed </returns>
    /// <remarks>Will use guild cache for optional parameters if those are not present if possible</remarks>
    internal static async Task<DiscordAuditLogEntry?> ParseAuditLogEntryAsync
    (
        DiscordGuild guild,
        AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordMember>? members = null,
        IDictionary<ulong, DiscordThreadChannel>? threads = null,
        IDictionary<ulong, DiscordWebhook>? webhooks = null,
        IDictionary<ulong, DiscordScheduledGuildEvent>? events = null
    )
    {
        //initialize members if null
        members ??= guild._members;

        //initialize threads if null
        threads ??= guild._threads;

        //initialize scheduled events if null
        events ??= guild._scheduledEvents;

        webhooks ??= new Dictionary<ulong, DiscordWebhook>();

        DiscordAuditLogEntry? entry = null;
        switch (auditLogAction.ActionType)
        {
            case DiscordAuditLogActionType.GuildUpdate:
                entry = await ParseGuildUpdateAsync(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.ChannelCreate:
            case DiscordAuditLogActionType.ChannelDelete:
            case DiscordAuditLogActionType.ChannelUpdate:
                entry = ParseChannelEntry(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.OverwriteCreate:
            case DiscordAuditLogActionType.OverwriteDelete:
            case DiscordAuditLogActionType.OverwriteUpdate:
                entry = ParseOverwriteEntry(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.Kick:
                entry = new DiscordAuditLogKickEntry
                {
                    Target = members.TryGetValue(auditLogAction.TargetId!.Value, out DiscordMember? kickMember)
                        ? kickMember
                        : new DiscordMember
                        {
                            Id = auditLogAction.TargetId.Value,
                            Discord = guild.Discord,
                            _guild_id = guild.Id
                        }
                };
                break;

            case DiscordAuditLogActionType.Prune:
                entry = new DiscordAuditLogPruneEntry
                {
                    Days = auditLogAction.Options.DeleteMemberDays,
                    Toll = auditLogAction.Options.MembersRemoved
                };
                break;

            case DiscordAuditLogActionType.Ban:
            case DiscordAuditLogActionType.Unban:
                entry = new DiscordAuditLogBanEntry
                {
                    Target = members.TryGetValue(auditLogAction.TargetId!.Value, out DiscordMember? unbanMember)
                        ? unbanMember
                        : new DiscordMember
                        {
                            Id = auditLogAction.TargetId.Value,
                            Discord = guild.Discord,
                            _guild_id = guild.Id
                        }
                };
                break;

            case DiscordAuditLogActionType.MemberUpdate:
            case DiscordAuditLogActionType.MemberRoleUpdate:
                entry = ParseMemberUpdateEntry(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.RoleCreate:
            case DiscordAuditLogActionType.RoleDelete:
            case DiscordAuditLogActionType.RoleUpdate:
                entry = ParseRoleUpdateEntry(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.InviteCreate:
            case DiscordAuditLogActionType.InviteDelete:
            case DiscordAuditLogActionType.InviteUpdate:
                entry = ParseInviteUpdateEntry(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.WebhookCreate:
            case DiscordAuditLogActionType.WebhookDelete:
            case DiscordAuditLogActionType.WebhookUpdate:
                entry = ParseWebhookUpdateEntry(guild, auditLogAction, webhooks);
                break;

            case DiscordAuditLogActionType.EmojiCreate:
            case DiscordAuditLogActionType.EmojiDelete:
            case DiscordAuditLogActionType.EmojiUpdate:
                entry = new DiscordAuditLogEmojiEntry
                {
                    Target = guild._emojis.TryGetValue(auditLogAction.TargetId!.Value, out DiscordEmoji? target)
                        ? target
                        : new DiscordEmoji { Id = auditLogAction.TargetId.Value, Discord = guild.Discord }
                };

                DiscordAuditLogEmojiEntry emojiEntry = (DiscordAuditLogEmojiEntry)entry;
                foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
                {
                    switch (actionChange.Key.ToLowerInvariant())
                    {
                        case "name":
                            emojiEntry.NameChange = PropertyChange<string>.From(actionChange);
                            break;

                        default:
                            if (guild.Discord.Configuration.LogUnknownAuditlogs)
                            {
                                guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                                    "Unknown key in emote update: {Key} - this should be reported to library developers",
                                    actionChange.Key);
                            }

                            break;
                    }
                }

                break;

            case DiscordAuditLogActionType.StickerCreate:
            case DiscordAuditLogActionType.StickerDelete:
            case DiscordAuditLogActionType.StickerUpdate:
                entry = ParseStickerUpdateEntry(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.MessageDelete:
            case DiscordAuditLogActionType.MessageBulkDelete:
                {
                    entry = new DiscordAuditLogMessageEntry();

                    DiscordAuditLogMessageEntry messageEntry = (DiscordAuditLogMessageEntry)entry;

                    if (auditLogAction.Options is not null)
                    {
                        messageEntry.Channel = guild.GetChannel(auditLogAction.Options.ChannelId) ?? new DiscordChannel
                        {
                            Id = auditLogAction.Options.ChannelId,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        };
                        messageEntry.MessageCount = auditLogAction.Options.Count;
                    }

                    if (messageEntry.Channel is not null)
                    {
                        guild.Discord.UserCache.TryGetValue(auditLogAction.UserId, out DiscordUser? user);
                        messageEntry.Target = user ?? new DiscordUser 
                        { 
                            Id = auditLogAction.UserId, 
                            Discord = guild.Discord 
                        };
                    }

                    break;
                }

            case DiscordAuditLogActionType.MessagePin:
            case DiscordAuditLogActionType.MessageUnpin:
                {
                    entry = new DiscordAuditLogMessagePinEntry();

                    DiscordAuditLogMessagePinEntry messagePinEntry = (DiscordAuditLogMessagePinEntry)entry;

                    if (guild.Discord is not DiscordClient dc)
                    {
                        break;
                    }

                    if (auditLogAction.Options != null)
                    {
                        DiscordMessage? message = default;
                        dc.MessageCache?.TryGet(auditLogAction.Options.MessageId, out message);

                        messagePinEntry.Channel = guild.GetChannel(auditLogAction.Options.ChannelId) ??
                                                  new DiscordChannel
                                                  {
                                                      Id = auditLogAction.Options.ChannelId,
                                                      Discord = guild.Discord,
                                                      GuildId = guild.Id
                                                  };
                        messagePinEntry.Message = message ?? new DiscordMessage
                        {
                            Id = auditLogAction.Options.MessageId,
                            Discord = guild.Discord
                        };
                    }

                    if (auditLogAction.TargetId.HasValue)
                    {
                        dc.UserCache.TryGetValue(auditLogAction.TargetId.Value, out DiscordUser? user);
                        messagePinEntry.Target = user ?? new DiscordUser 
                        { 
                            Id = auditLogAction.TargetId.Value, 
                            Discord = guild.Discord 
                        };
                    }

                    break;
                }

            case DiscordAuditLogActionType.BotAdd:
                {
                    entry = new DiscordAuditLogBotAddEntry();

                    if (!(guild.Discord is DiscordClient dc && auditLogAction.TargetId.HasValue))
                    {
                        break;
                    }

                    dc.UserCache.TryGetValue(auditLogAction.TargetId.Value, out DiscordUser? bot);
                    (entry as DiscordAuditLogBotAddEntry)!.TargetBot = bot
                        ?? new DiscordUser
                        {
                            Id = auditLogAction.TargetId.Value,
                            Discord = guild.Discord
                        };

                    break;
                }

            case DiscordAuditLogActionType.MemberMove:
                entry = new DiscordAuditLogMemberMoveEntry();

                if (auditLogAction.Options == null)
                {
                    break;
                }

                DiscordAuditLogMemberMoveEntry? memberMoveEntry = (DiscordAuditLogMemberMoveEntry)entry;

                memberMoveEntry.UserCount = auditLogAction.Options.Count;
                memberMoveEntry.Channel = guild.GetChannel(auditLogAction.Options.ChannelId) ?? new DiscordChannel
                {
                    Id = auditLogAction.Options.ChannelId,
                    Discord = guild.Discord,
                    GuildId = guild.Id
                };
                break;

            case DiscordAuditLogActionType.MemberDisconnect:
                entry = new DiscordAuditLogMemberDisconnectEntry { UserCount = auditLogAction.Options?.Count ?? 0 };
                break;

            case DiscordAuditLogActionType.IntegrationCreate:
            case DiscordAuditLogActionType.IntegrationDelete:
            case DiscordAuditLogActionType.IntegrationUpdate:
                entry = ParseIntegrationUpdateEntry(guild, auditLogAction);
                break;

            case DiscordAuditLogActionType.GuildScheduledEventCreate:
            case DiscordAuditLogActionType.GuildScheduledEventDelete:
            case DiscordAuditLogActionType.GuildScheduledEventUpdate:
                entry = ParseGuildScheduledEventUpdateEntry(guild, auditLogAction, events);
                break;

            case DiscordAuditLogActionType.ThreadCreate:
            case DiscordAuditLogActionType.ThreadDelete:
            case DiscordAuditLogActionType.ThreadUpdate:
                entry = ParseThreadUpdateEntry(guild, auditLogAction, threads);
                break;

            case DiscordAuditLogActionType.ApplicationCommandPermissionUpdate:
                entry = new DiscordAuditLogApplicationCommandPermissionEntry();
                DiscordAuditLogApplicationCommandPermissionEntry permissionEntry =
                    (DiscordAuditLogApplicationCommandPermissionEntry)entry;

                if (auditLogAction.Options.ApplicationId == auditLogAction.TargetId)
                {
                    permissionEntry.ApplicationId = (ulong)auditLogAction.TargetId;
                    permissionEntry.ApplicationCommandId = null;
                }
                else
                {
                    permissionEntry.ApplicationId = auditLogAction.Options.ApplicationId;
                    permissionEntry.ApplicationCommandId = auditLogAction.TargetId;
                }

                permissionEntry.PermissionChanges = new List<PropertyChange<DiscordApplicationCommandPermission>>();

                foreach (AuditLogActionChange change in auditLogAction.Changes)
                {
                    DiscordApplicationCommandPermission? oldValue = ((JObject?)change
                            .OldValue)?
                        .ToDiscordObject<DiscordApplicationCommandPermission>();

                    DiscordApplicationCommandPermission? newValue = ((JObject)change
                            .NewValue)
                        .ToDiscordObject<DiscordApplicationCommandPermission>();

                    permissionEntry.PermissionChanges = permissionEntry.PermissionChanges
                        .Append(PropertyChange<DiscordApplicationCommandPermission>.From(oldValue, newValue));
                }

                break;

            case DiscordAuditLogActionType.AutoModerationBlockMessage:
            case DiscordAuditLogActionType.AutoModerationFlagToChannel:
            case DiscordAuditLogActionType.AutoModerationUserCommunicationDisabled:
                entry = new DiscordAuditLogAutoModerationExecutedEntry();

                DiscordAuditLogAutoModerationExecutedEntry autoModerationEntry =
                    (DiscordAuditLogAutoModerationExecutedEntry)entry;

                if (auditLogAction.TargetId is not null)
                {
                    autoModerationEntry.TargetUser =
                        members.TryGetValue(auditLogAction.TargetId.Value, out DiscordMember? targetMember)
                            ? targetMember
                            : new DiscordUser 
                            { 
                                Id = auditLogAction.TargetId.Value, 
                                Discord = guild.Discord 
                            };
                }

                autoModerationEntry.ResponsibleRule = auditLogAction.Options.RoleName;
                autoModerationEntry.Channel = guild.GetChannel(auditLogAction.Options.ChannelId);
                autoModerationEntry.RuleTriggerType =
                    (DiscordRuleTriggerType)int.Parse(auditLogAction.Options.AutoModerationRuleTriggerType);
                break;

            case DiscordAuditLogActionType.AutoModerationRuleCreate:
            case DiscordAuditLogActionType.AutoModerationRuleUpdate:
            case DiscordAuditLogActionType.AutoModerationRuleDelete:
                entry = ParseAutoModerationRuleUpdateEntry(guild, auditLogAction);
                break;

            default:
                if (guild.Discord.Configuration.LogUnknownAuditlogs)
                {
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown audit log action type: {type} - this should be reported to library developers",
                        (int)auditLogAction.ActionType);
                }

                break;
        }

        if (entry is null)
        {
            return null;
        }

        entry.ActionCategory = auditLogAction.ActionType switch
        {
            DiscordAuditLogActionType.ChannelCreate or DiscordAuditLogActionType.EmojiCreate or DiscordAuditLogActionType.InviteCreate
                or DiscordAuditLogActionType.OverwriteCreate or DiscordAuditLogActionType.RoleCreate
                or DiscordAuditLogActionType.WebhookCreate or DiscordAuditLogActionType.IntegrationCreate
                or DiscordAuditLogActionType.StickerCreate
                or DiscordAuditLogActionType.AutoModerationRuleCreate => DiscordAuditLogActionCategory.Create,

            DiscordAuditLogActionType.ChannelDelete or DiscordAuditLogActionType.EmojiDelete or DiscordAuditLogActionType.InviteDelete
                or DiscordAuditLogActionType.MessageDelete or DiscordAuditLogActionType.MessageBulkDelete
                or DiscordAuditLogActionType.OverwriteDelete or DiscordAuditLogActionType.RoleDelete
                or DiscordAuditLogActionType.WebhookDelete or DiscordAuditLogActionType.IntegrationDelete
                or DiscordAuditLogActionType.StickerDelete
                or DiscordAuditLogActionType.AutoModerationRuleDelete => DiscordAuditLogActionCategory.Delete,

            DiscordAuditLogActionType.ChannelUpdate or DiscordAuditLogActionType.EmojiUpdate or DiscordAuditLogActionType.InviteUpdate
                or DiscordAuditLogActionType.MemberRoleUpdate or DiscordAuditLogActionType.MemberUpdate
                or DiscordAuditLogActionType.OverwriteUpdate or DiscordAuditLogActionType.RoleUpdate
                or DiscordAuditLogActionType.WebhookUpdate or DiscordAuditLogActionType.IntegrationUpdate
                or DiscordAuditLogActionType.StickerUpdate
                or DiscordAuditLogActionType.AutoModerationRuleUpdate => DiscordAuditLogActionCategory.Update,
            _ => DiscordAuditLogActionCategory.Other,
        };
        entry.ActionType = auditLogAction.ActionType;
        entry.Id = auditLogAction.Id;
        entry.Reason = auditLogAction.Reason;
        entry.Discord = guild.Discord;

        entry.UserResponsible = members.TryGetValue(auditLogAction.UserId, out DiscordMember? member)
            ? member
            : guild.Discord.UserCache.TryGetValue(auditLogAction.UserId, out DiscordUser? discordUser)
                ? discordUser
                : new DiscordUser { Id = auditLogAction.UserId, Discord = guild.Discord };

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogAutoModerationRuleEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    private static DiscordAuditLogAutoModerationRuleEntry ParseAutoModerationRuleUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogAutoModerationRuleEntry ruleEntry = new();

        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "id":
                    ruleEntry.RuleId = PropertyChange<ulong?>.From(change);
                    break;

                case "guild_id":
                    ruleEntry.GuildId = PropertyChange<ulong?>.From(change);
                    break;

                case "name":
                    ruleEntry.Name = PropertyChange<string?>.From(change);
                    break;

                case "creator_id":
                    ruleEntry.CreatorId = PropertyChange<ulong?>.From(change);
                    break;

                case "event_type":
                    ruleEntry.EventType = PropertyChange<DiscordRuleEventType?>.From(change);
                    break;

                case "trigger_type":
                    ruleEntry.TriggerType = PropertyChange<DiscordRuleTriggerType?>.From(change);
                    break;

                case "trigger_metadata":
                    ruleEntry.TriggerMetadata = PropertyChange<DiscordRuleTriggerMetadata?>.From(change);
                    break;

                case "actions":
                    ruleEntry.Actions = PropertyChange<IEnumerable<DiscordAutoModerationAction>?>.From(change);
                    break;

                case "enabled":
                    ruleEntry.Enabled = PropertyChange<bool?>.From(change);
                    break;

                case "exempt_roles":
                    JArray oldRoleIds = (JArray)change.OldValue;
                    JArray newRoleIds = (JArray)change.NewValue;

                    IEnumerable<DiscordRole>? oldRoles = oldRoleIds?
                        .Select(x => x.ToObject<ulong>())
                        .Select(guild.GetRole);

                    IEnumerable<DiscordRole>? newRoles = newRoleIds?
                        .Select(x => x.ToObject<ulong>())
                        .Select(guild.GetRole);

                    ruleEntry.ExemptRoles =
                        PropertyChange<IEnumerable<DiscordRole>?>.From(oldRoles, newRoles);
                    break;

                case "exempt_channels":
                    JArray oldChannelIds = (JArray)change.OldValue;
                    JArray newChanelIds = (JArray)change.NewValue;

                    IEnumerable<DiscordChannel>? oldChannels = oldChannelIds?
                        .Select(x => x.ToObject<ulong>())
                        .Select(guild.GetChannel);

                    IEnumerable<DiscordChannel>? newChannels = newChanelIds?
                        .Select(x => x.ToObject<ulong>())
                        .Select(guild.GetChannel);

                    ruleEntry.ExemptChannels =
                        PropertyChange<IEnumerable<DiscordChannel>?>.From(oldChannels, newChannels);
                    break;

                case "$add_keyword_filter":
                    ruleEntry.AddedKeywords = ((JArray)change.NewValue).Cast<string>();
                    break;

                case "$remove_keyword_filter":
                    ruleEntry.RemovedKeywords = ((JArray)change.NewValue).Cast<string>();
                    break;

                case "$add_regex_patterns":
                    ruleEntry.AddedRegexPatterns = ((JArray)change.NewValue).Cast<string>();
                    break;

                case "$remove_regex_patterns":
                    ruleEntry.RemovedRegexPatterns = ((JArray)change.NewValue).Cast<string>();
                    break;

                case "$add_allow_list":
                    ruleEntry.AddedAllowList = ((JArray)change.NewValue).Cast<string>();
                    break;

                case "$remove_allow_list":
                    ruleEntry.RemovedKeywords = ((JArray)change.NewValue).Cast<string>();
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in AutoModRule update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return ruleEntry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogThreadEventEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <param name="threads">Dictionary of <see cref="DiscordThreadChannel"/> to populate entry with thread entities</param>
    /// <returns></returns>
    internal static DiscordAuditLogThreadEventEntry ParseThreadUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordThreadChannel> threads)
    {
        DiscordAuditLogThreadEventEntry entry = new()
        {
            Target =
                threads.TryGetValue(auditLogAction.TargetId!.Value,
                    out DiscordThreadChannel? channel)
                    ? channel
                    : new DiscordThreadChannel() { Id = auditLogAction.TargetId.Value, Discord = guild.Discord },
        };

        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.Name = PropertyChange<string?>.From(change);
                    break;

                case "type":
                    entry.Type = PropertyChange<ChannelType?>.From(change);
                    break;

                case "archived":
                    entry.Archived = PropertyChange<bool?>.From(change);
                    break;

                case "auto_archive_duration":
                    entry.AutoArchiveDuration = PropertyChange<int?>.From(change);
                    break;

                case "invitable":
                    entry.Invitable = PropertyChange<bool?>.From(change);
                    break;

                case "locked":
                    entry.Locked = PropertyChange<bool?>.From(change);
                    break;

                case "rate_limit_per_user":
                    entry.PerUserRateLimit = PropertyChange<int?>.From(change);
                    break;

                case "flags":
                    entry.Flags = PropertyChange<ChannelFlags?>.From(change);
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in thread update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogGuildScheduledEventEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <param name="events">Dictionary of <see cref="DiscordScheduledGuildEvent"/> to populate entry with event entities</param>
    /// <returns></returns>
    private static DiscordAuditLogGuildScheduledEventEntry ParseGuildScheduledEventUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction, IDictionary<ulong, DiscordScheduledGuildEvent> events)
    {
        DiscordAuditLogGuildScheduledEventEntry entry = new()
        {
            Target =
                events.TryGetValue(auditLogAction.TargetId!.Value, out DiscordScheduledGuildEvent? ta)
                    ? ta
                    : new DiscordScheduledGuildEvent() { Id = auditLogAction.Id, Discord = guild.Discord },
        };

        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.Name = PropertyChange<string?>.From(change);
                    break;
                case "channel_id":
                    
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulong newChannelId);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulong oldChannelId);
                    
                    entry.Channel = new PropertyChange<DiscordChannel?>
                    {
                        Before =
                            guild.GetChannel(newChannelId) ?? new DiscordChannel
                            {
                                Id = change.OldValueUlong,
                                Discord = guild.Discord,
                                GuildId = guild.Id
                            },
                        After = guild.GetChannel(newChannelId) ?? new DiscordChannel
                        {
                            Id = change.NewValueUlong,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        }
                    };
                    break;

                case "description":
                    entry.Description = PropertyChange<string?>.From(change);
                    break;

                case "entity_type":
                    entry.Type = PropertyChange<ScheduledGuildEventType?>.From(change);
                    break;

                case "image_hash":
                    entry.ImageHash = PropertyChange<string?>.From(change);
                    break;

                case "location":
                    entry.Location = PropertyChange<string?>.From(change);
                    break;

                case "privacy_level":
                    entry.PrivacyLevel = PropertyChange<ScheduledGuildEventPrivacyLevel?>.From(change);
                    break;

                case "status":
                    entry.Status = PropertyChange<ScheduledGuildEventStatus?>.From(change);
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in scheduled event update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogGuildEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static async Task<DiscordAuditLogGuildEntry> ParseGuildUpdateAsync(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogGuildEntry entry = new() { Target = guild };

        ulong before, after;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = PropertyChange<string?>.From(change);
                    break;

                case "owner_id":
                    entry.OwnerChange = new PropertyChange<DiscordMember?>
                    {
                        Before = guild._members != null && guild._members.TryGetValue(
                            change.OldValueUlong,
                            out DiscordMember? oldMember)
                            ? oldMember
                            : await guild.GetMemberAsync(change.OldValueUlong),
                        After = guild._members != null && guild._members.TryGetValue(change.NewValueUlong,
                            out DiscordMember? newMember)
                            ? newMember
                            : await guild.GetMemberAsync(change.NewValueUlong)
                    };
                    break;

                case "icon_hash":
                    entry.IconChange = new PropertyChange<string?>
                    {
                        Before = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{guild.Id}/{change.OldValueString}.webp"
                            : null,
                        After = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{guild.Id}/{change.NewValueString}.webp"
                            : null
                    };
                    break;

                case "verification_level":
                    entry.VerificationLevelChange = PropertyChange<VerificationLevel?>.From(change);
                    break;

                case "afk_channel_id":

                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    entry.AfkChannelChange = new PropertyChange<DiscordChannel?>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        }
                    };
                    break;

                case "widget_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    entry.EmbedChannelChange = new PropertyChange<DiscordChannel?>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        }
                    };
                    break;

                case "splash_hash":
                    entry.SplashChange = new PropertyChange<string?>
                    {
                        Before = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{guild.Id}/{change.OldValueString}.webp?size=2048"
                            : null,
                        After = change.NewValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{guild.Id}/{change.NewValueString}.webp?size=2048"
                            : null
                    };
                    break;

                case "default_message_notifications":
                    entry.NotificationSettingsChange = PropertyChange<DefaultMessageNotifications?>.From(change);
                    break;

                case "system_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    entry.SystemChannelChange = new PropertyChange<DiscordChannel?>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before,
                            Discord = guild.Discord,
                            GuildId = guild.Id
                        }
                    };
                    break;

                case "explicit_content_filter":
                    entry.ExplicitContentFilterChange = PropertyChange<ExplicitContentFilter?>.From(change);
                    break;

                case "mfa_level":
                    entry.MfaLevelChange = PropertyChange<MfaLevel?>.From(change);
                    break;

                case "region":
                    entry.RegionChange = PropertyChange<string?>.From(change);
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in guild update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogChannelEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static DiscordAuditLogChannelEntry ParseChannelEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogChannelEntry entry = new()
        {
            Target = guild.GetChannel(auditLogAction.TargetId!.Value) ?? new DiscordChannel
            {
                Id = auditLogAction.TargetId.Value,
                Discord = guild.Discord,
                GuildId = guild.Id
            }
        };

        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = PropertyChange<string>.From(change);
                    break;

                case "type":
                    entry.TypeChange = PropertyChange<ChannelType?>.From(change);
                    break;

                case "permission_overwrites":

                    IEnumerable<DiscordOverwrite>? olds = change.OldValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordOverwrite>())?
                        .Select(overwrite =>
                        {
                            overwrite.Discord = guild.Discord;
                            return overwrite;
                        });

                    IEnumerable<DiscordOverwrite>? news = change.NewValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordOverwrite>())?
                        .Select(overwrite =>
                        {
                            overwrite.Discord = guild.Discord;
                            return overwrite;
                        });

                    entry.OverwriteChange = new PropertyChange<IReadOnlyList<DiscordOverwrite>>
                    {
                        Before = olds != null
                            ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(olds))
                            : null,
                        After = news != null
                            ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(news))
                            : null
                    };
                    break;

                case "topic":
                    entry.TopicChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString,
                        After = change.NewValueString
                    };
                    break;

                case "nsfw":
                    entry.NsfwChange = PropertyChange<bool?>.From(change);
                    break;

                case "bitrate":
                    entry.BitrateChange = PropertyChange<int?>.From(change);
                    break;

                case "rate_limit_per_user":
                    entry.PerUserRateLimitChange = PropertyChange<int?>.From(change);
                    break;

                case "user_limit":
                    entry.UserLimit = PropertyChange<int?>.From(change);
                    break;

                case "flags":
                    entry.Flags = PropertyChange<ChannelFlags?>.From(change);
                    break;

                case "available_tags":
                    IEnumerable<DiscordForumTag>? newTags = change.NewValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordForumTag>())?
                        .Select(forumTag =>
                        {
                            forumTag.Discord = guild.Discord;
                            return forumTag;
                        });

                    IEnumerable<DiscordForumTag>? oldTags = change.OldValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordForumTag>())?
                        .Select(forumTag =>
                        {
                            forumTag.Discord = guild.Discord;
                            return forumTag;
                        });

                    entry.AvailableTags = PropertyChange<IEnumerable<DiscordForumTag>>.From(oldTags, newTags);

                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in channel update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogOverwriteEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static DiscordAuditLogOverwriteEntry ParseOverwriteEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogOverwriteEntry entry = new()
        {
            Target = guild
                .GetChannel(auditLogAction.TargetId!.Value)
                .PermissionOverwrites
                .FirstOrDefault(xo => xo.Id == auditLogAction.Options.Id),
            Channel = guild.GetChannel(auditLogAction.TargetId.Value)
        };

        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "deny":
                    entry.DeniedPermissions = PropertyChange<Permissions?>.From(change);
                    break;

                case "allow":
                    entry.AllowedPermissions = PropertyChange<Permissions?>.From(change);
                    break;

                case "type":
                    entry.Type = PropertyChange<OverwriteType>.From(change);
                    break;

                case "id":
                    entry.TargetIdChange = PropertyChange<ulong?>.From(change);
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in overwrite update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogMemberUpdateEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static DiscordAuditLogMemberUpdateEntry ParseMemberUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogMemberUpdateEntry entry = new()
        {
            Target = guild._members.TryGetValue(auditLogAction.TargetId!.Value, out DiscordMember? roleUpdMember)
                ? roleUpdMember
                : new DiscordMember { Id = auditLogAction.TargetId.Value, Discord = guild.Discord, _guild_id = guild.Id }
        };

        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "nick":
                    entry.NicknameChange = PropertyChange<string>.From(change);
                    break;

                case "deaf":
                    entry.DeafenChange = PropertyChange<bool?>.From(change);
                    break;

                case "mute":
                    entry.MuteChange = PropertyChange<bool?>.From(change);
                    break;

                case "communication_disabled_until":
                    entry.TimeoutChange = PropertyChange<DateTime?>.From(change);

                    break;

                case "$add":
                    entry.AddedRoles =
                        new ReadOnlyCollection<DiscordRole>(change.NewValues.Select(xo => (ulong)xo["id"]!)
                            .Select(guild.GetRole).ToList());
                    break;

                case "$remove":
                    entry.RemovedRoles =
                        new ReadOnlyCollection<DiscordRole>(change.NewValues.Select(xo => (ulong)xo["id"]!)
                            .Select(guild.GetRole).ToList());
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in member update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogRoleUpdateEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static DiscordAuditLogRoleUpdateEntry ParseRoleUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogRoleUpdateEntry entry = new()
        {
            Target = guild.GetRole(auditLogAction.TargetId!.Value) ??
                     new DiscordRole { Id = auditLogAction.TargetId.Value, Discord = guild.Discord }
        };

        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = PropertyChange<string>.From(change);
                    break;

                case "color":
                    entry.ColorChange = PropertyChange<int?>.From(change);
                    break;

                case "permissions":
                    entry.PermissionChange = PropertyChange<Permissions?>.From(change);
                    break;

                case "position":
                    entry.PositionChange = PropertyChange<int?>.From(change);
                    break;

                case "mentionable":
                    entry.MentionableChange = PropertyChange<bool?>.From(change);
                    break;

                case "hoist":
                    entry.HoistChange = PropertyChange<bool?>.From(change); break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in role update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogInviteEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static DiscordAuditLogInviteEntry ParseInviteUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogInviteEntry entry = new();

        DiscordInvite invite = new()
        {
            Discord = guild.Discord,
            Guild = new DiscordInviteGuild
            {
                Discord = guild.Discord,
                Id = guild.Id,
                Name = guild.Name,
                SplashHash = guild.SplashHash
            }
        };

        bool boolBefore, boolAfter;
        ulong ulongBefore, ulongAfter;
        int intBefore, intAfter;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "max_age":
                    entry.MaxAgeChange = PropertyChange<int?>.From(change);
                    break;

                case "code":
                    invite.Code = change.OldValueString ?? change.NewValueString;

                    entry.CodeChange = PropertyChange<string>.From(change);
                    break;

                case "temporary":
                    entry.TemporaryChange = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? (bool?)change.OldValue : null,
                        After = change.NewValue != null ? (bool?)change.NewValue : null
                    };
                    break;

                case "inviter_id":
                    boolBefore = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    boolAfter = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongAfter);

                    entry.InviterChange = new PropertyChange<DiscordMember>
                    {
                        Before = guild._members.TryGetValue(ulongBefore, out DiscordMember? propBeforeMember)
                            ? propBeforeMember
                            : new DiscordMember { Id = ulongBefore, Discord = guild.Discord, _guild_id = guild.Id },
                        After = guild._members.TryGetValue(ulongAfter, out DiscordMember? propAfterMember)
                            ? propAfterMember
                            : new DiscordMember { Id = ulongBefore, Discord = guild.Discord, _guild_id = guild.Id }
                    };
                    break;

                case "channel_id":
                    boolBefore = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    boolAfter = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongAfter);

                    entry.ChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = boolBefore
                            ? guild.GetChannel(ulongBefore) ??
                              new DiscordChannel { Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id }
                            : null,
                        After = boolAfter
                            ? guild.GetChannel(ulongAfter) ??
                              new DiscordChannel { Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id }
                            : null
                    };

                    DiscordChannel? channel = entry.ChannelChange.Before ?? entry.ChannelChange.After;
                    ChannelType? channelType = channel?.Type;
                    invite.Channel = new DiscordInviteChannel
                    {
                        Discord = guild.Discord,
                        Id = boolBefore ? ulongBefore : ulongAfter,
                        Name = channel?.Name ?? "",
                        Type = channelType != null ? channelType.Value : ChannelType.Unknown
                    };
                    break;

                case "uses":
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intAfter);

                    entry.UsesChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null,
                        After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "max_uses":
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intAfter);

                    entry.MaxUsesChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null,
                        After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in invite update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        entry.Target = invite;
        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogWebhookEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <param name="webhooks">Dictionary of <see cref="DiscordWebhook"/> to populate entry with webhook entities</param>
    /// <returns></returns>
    internal static DiscordAuditLogWebhookEntry ParseWebhookUpdateEntry
    (
        DiscordGuild guild,
        AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordWebhook> webhooks
    )
    {
        DiscordAuditLogWebhookEntry entry = new()
        {
            Target = webhooks.TryGetValue(auditLogAction.TargetId!.Value, out DiscordWebhook? webhook)
                ? webhook
                : new DiscordWebhook { Id = auditLogAction.TargetId.Value, Discord = guild.Discord }
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = PropertyChange<string>.From(change);
                    break;

                case "channel_id":
                    boolBefore = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    entry.ChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before =
                            boolBefore
                                ? guild.GetChannel(ulongBefore) ?? new DiscordChannel
                                {
                                    Id = ulongBefore,
                                    Discord = guild.Discord,
                                    GuildId = guild.Id
                                }
                                : null,
                        After = boolAfter
                            ? guild.GetChannel(ulongAfter) ??
                              new DiscordChannel { Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id }
                            : null
                    };
                    break;

                case "type":
                    entry.TypeChange = PropertyChange<int?>.From(change);
                    break;

                case "avatar_hash":
                    entry.AvatarHashChange = PropertyChange<string>.From(change);
                    break;

                case "application_id"
                    : //Why the fuck does discord send this as a string if it's supposed to be a snowflake
                    entry.ApplicationIdChange = PropertyChange<ulong?>.From(change);
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in webhook update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogStickerEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static DiscordAuditLogStickerEntry ParseStickerUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogStickerEntry entry = new()
        {
            Target = guild._stickers.TryGetValue(auditLogAction.TargetId!.Value, out DiscordMessageSticker? sticker)
                ? sticker
                : new DiscordMessageSticker { Id = auditLogAction.TargetId.Value, Discord = guild.Discord }
        };

        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = PropertyChange<string>.From(change);
                    break;

                case "description":
                    entry.DescriptionChange = PropertyChange<string>.From(change);
                    break;

                case "tags":
                    entry.TagsChange = PropertyChange<string>.From(change);
                    break;

                case "guild_id":
                    entry.GuildIdChange = PropertyChange<ulong?>.From(change);
                    break;

                case "available":
                    entry.AvailabilityChange = PropertyChange<bool?>.From(change);
                    break;

                case "asset":
                    entry.AssetChange = PropertyChange<string>.From(change);
                    break;

                case "id":
                    entry.IdChange = PropertyChange<ulong?>.From(change);
                    break;

                case "type":
                    entry.TypeChange = PropertyChange<StickerType?>.From(change);
                    break;

                case "format_type":
                    entry.FormatChange = PropertyChange<StickerFormat?>.From(change);
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in sticker update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogIntegrationEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <returns></returns>
    internal static DiscordAuditLogIntegrationEntry ParseIntegrationUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogIntegrationEntry entry = new();

        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "enable_emoticons":
                    entry.EnableEmoticons = PropertyChange<bool?>.From(change);
                    break;

                case "expire_behavior":
                    entry.ExpireBehavior = PropertyChange<int?>.From(change);
                    break;

                case "expire_grace_period":
                    entry.ExpireBehavior = PropertyChange<int?>.From(change);
                    break;

                case "name":
                    entry.Name = PropertyChange<string>.From(change);
                    break;

                case "type":
                    entry.Type = PropertyChange<string>.From(change);
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in integration update: {Key} - this should be reported to library developers",
                            change.Key);
                    }

                    break;
            }
        }

        return entry;
    }
}
