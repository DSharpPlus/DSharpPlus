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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Enums;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Entities;

internal static class AuditLogParser
{
    /// <summary>
    /// Parses a AuditLog to a list of AuditLogEntries
    /// </summary>
    /// <param name="guild"> <see cref="DiscordGuild"/> which is the parent of the AuditLog</param>
    /// <param name="auditLog"> <see cref="AuditLog"/> whose entries should be parsed</param>
    /// <returns>A list of <see cref="DiscordAuditLogEntry"/>. All entries which cant be parsed are dropped</returns>
    internal static async Task<IEnumerable<DiscordAuditLogEntry>> ParseAuditLogToEntriesAsync
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


        IEnumerable<DiscordMember>? discordMembers = users
            .Select(xau => guild._members != null && guild._members.TryGetValue(xau.Id, out DiscordMember? member)
                ? member
                : new DiscordMember {Discord = guild.Discord, Id = xau.Id, _guild_id = guild.Id});

        Dictionary<ulong, DiscordMember>? members = discordMembers.ToDictionary(xm => xm.Id, xm => xm);

        IOrderedEnumerable<AuditLogAction>? auditLogActions = auditLog.Entries.OrderByDescending(xa => xa.Id);
        List<DiscordAuditLogEntry>? entries = new();
        foreach (AuditLogAction? auditLogAction in auditLogActions)
        {
            DiscordAuditLogEntry? entry =
                await ParseAuditLogEntryAsync(guild, auditLogAction, members, threads, webhooks, events);

            if (entry is null)
            {
                continue;
            }

            entries.Add(entry);
        }

        return new ReadOnlyCollection<DiscordAuditLogEntry>(entries);
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
    /// <remarks>Will use guild cache if possible for optional parameters if those are not present</remarks>
    internal static async Task<DiscordAuditLogEntry?> ParseAuditLogEntryAsync
    (
        DiscordGuild guild,
        AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordMember> members = null,
        IDictionary<ulong, DiscordThreadChannel> threads = null,
        IDictionary<ulong, DiscordWebhook> webhooks = null,
        IDictionary<ulong, DiscordScheduledGuildEvent> events = null
    )
    {
        //initialize members if null
        if (members is null)
        {
            members = guild._members;
        }

        //initialize threads if null
        if (threads is null)
        {
            threads = guild._threads;
        }

        //initialize scheduled events if null
        if (events is null)
        {
            events = guild._scheduledEvents;
        }

        webhooks ??= new Dictionary<ulong, DiscordWebhook>();

        DiscordAuditLogEntry? entry = null;
        switch (auditLogAction.ActionType)
        {
            case AuditLogActionType.GuildUpdate:
                entry = await ParseGuildUpdateAsync(guild, auditLogAction);
                break;

            case AuditLogActionType.ChannelCreate:
            case AuditLogActionType.ChannelDelete:
            case AuditLogActionType.ChannelUpdate:
                entry = ParseChannelEntry(guild, auditLogAction);
                break;

            case AuditLogActionType.OverwriteCreate:
            case AuditLogActionType.OverwriteDelete:
            case AuditLogActionType.OverwriteUpdate:
                entry = ParseOverwriteEntry(guild, auditLogAction);
                break;

            case AuditLogActionType.Kick:
                entry = new DiscordAuditLogKickEntry
                {
                    Target = members.TryGetValue(auditLogAction.TargetId.Value, out DiscordMember? kickMember)
                        ? kickMember
                        : new DiscordMember
                        {
                            Id = auditLogAction.TargetId.Value, Discord = guild.Discord, _guild_id = guild.Id
                        }
                };
                break;

            case AuditLogActionType.Prune:
                entry = new DiscordAuditLogPruneEntry
                {
                    Days = auditLogAction.Options.DeleteMemberDays, Toll = auditLogAction.Options.MembersRemoved
                };
                break;

            case AuditLogActionType.Ban:
            case AuditLogActionType.Unban:
                entry = new DiscordAuditLogBanEntry
                {
                    Target = members.TryGetValue(auditLogAction.TargetId.Value, out DiscordMember? unbanMember)
                        ? unbanMember
                        : new DiscordMember
                        {
                            Id = auditLogAction.TargetId.Value, Discord = guild.Discord, _guild_id = guild.Id
                        }
                };
                break;

            case AuditLogActionType.MemberUpdate:
            case AuditLogActionType.MemberRoleUpdate:
                entry = ParseMemberUpdateEntry(guild, auditLogAction);
                break;

            case AuditLogActionType.RoleCreate:
            case AuditLogActionType.RoleDelete:
            case AuditLogActionType.RoleUpdate:
                entry = ParseRoleUpdateEntry(guild, auditLogAction);
                break;

            case AuditLogActionType.InviteCreate:
            case AuditLogActionType.InviteDelete:
            case AuditLogActionType.InviteUpdate:
                entry = ParseInviteUpdateEntry(guild, auditLogAction);
                break;

            case AuditLogActionType.WebhookCreate:
            case AuditLogActionType.WebhookDelete:
            case AuditLogActionType.WebhookUpdate:
                entry = ParseWebhookUpdateEntry(guild, auditLogAction, webhooks);
                break;

            case AuditLogActionType.EmojiCreate:
            case AuditLogActionType.EmojiDelete:
            case AuditLogActionType.EmojiUpdate:
                entry = new DiscordAuditLogEmojiEntry
                {
                    Target = guild._emojis.TryGetValue(auditLogAction.TargetId.Value, out DiscordEmoji? target)
                        ? target
                        : new DiscordEmoji {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
                };

                DiscordAuditLogEmojiEntry? emojiEntry = entry as DiscordAuditLogEmojiEntry;
                foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
                {
                    switch (actionChange.Key.ToLowerInvariant())
                    {
                        case "name":
                            emojiEntry.NameChange = new PropertyChange<string>
                            {
                                Before = actionChange.OldValueString, After = actionChange.NewValueString
                            };
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

            case AuditLogActionType.StickerCreate:
            case AuditLogActionType.StickerDelete:
            case AuditLogActionType.StickerUpdate:
                entry = ParseStickerUpdateEntry(guild, auditLogAction);
                break;

            case AuditLogActionType.MessageDelete:
            case AuditLogActionType.MessageBulkDelete:
            {
                entry = new DiscordAuditLogMessageEntry();

                DiscordAuditLogMessageEntry? messageEntry = entry as DiscordAuditLogMessageEntry;

                if (auditLogAction.Options != null)
                {
                    messageEntry.Channel = guild.GetChannel(auditLogAction.Options.ChannelId) ?? new DiscordChannel
                    {
                        Id = auditLogAction.Options.ChannelId, Discord = guild.Discord, GuildId = guild.Id
                    };
                    messageEntry.MessageCount = auditLogAction.Options.Count;
                }

                if (messageEntry.Channel != null)
                {
                    messageEntry.Target = guild.Discord is DiscordClient dc
                                          && dc.MessageCache != null
                                          && dc.MessageCache.TryGet(auditLogAction.TargetId.Value,
                                              out DiscordMessage? msg)
                        ? msg
                        : new DiscordMessage {Discord = guild.Discord, Id = auditLogAction.TargetId.Value};
                }

                break;
            }

            case AuditLogActionType.MessagePin:
            case AuditLogActionType.MessageUnpin:
            {
                entry = new DiscordAuditLogMessagePinEntry();

                DiscordAuditLogMessagePinEntry? messagePinEntry = entry as DiscordAuditLogMessagePinEntry;

                if (guild.Discord is not DiscordClient dc)
                {
                    break;
                }

                if (auditLogAction.Options != null)
                {
                    DiscordMessage message = default;
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
                        Id = auditLogAction.Options.MessageId, Discord = guild.Discord
                    };
                }

                if (auditLogAction.TargetId.HasValue)
                {
                    dc.UserCache.TryGetValue(auditLogAction.TargetId.Value, out DiscordUser? user);
                    messagePinEntry.Target = user ?? new DiscordUser {Id = user.Id, Discord = guild.Discord};
                }

                break;
            }

            case AuditLogActionType.BotAdd:
            {
                entry = new DiscordAuditLogBotAddEntry();

                if (!(guild.Discord is DiscordClient dc && auditLogAction.TargetId.HasValue))
                {
                    break;
                }

                dc.UserCache.TryGetValue(auditLogAction.TargetId.Value, out DiscordUser? bot);
                (entry as DiscordAuditLogBotAddEntry).TargetBot = bot ??
                                                                  new DiscordUser
                                                                  {
                                                                      Id = auditLogAction.TargetId.Value,
                                                                      Discord = guild.Discord
                                                                  };

                break;
            }

            case AuditLogActionType.MemberMove:
                entry = new DiscordAuditLogMemberMoveEntry();

                if (auditLogAction.Options == null)
                {
                    break;
                }

                DiscordAuditLogMemberMoveEntry? memberMoveEntry = entry as DiscordAuditLogMemberMoveEntry;

                memberMoveEntry.UserCount = auditLogAction.Options.Count;
                memberMoveEntry.Channel = guild.GetChannel(auditLogAction.Options.ChannelId) ?? new DiscordChannel
                {
                    Id = auditLogAction.Options.ChannelId, Discord = guild.Discord, GuildId = guild.Id
                };
                break;

            case AuditLogActionType.MemberDisconnect:
                entry = new DiscordAuditLogMemberDisconnectEntry {UserCount = auditLogAction.Options?.Count ?? 0};
                break;

            case AuditLogActionType.IntegrationCreate:
            case AuditLogActionType.IntegrationDelete:
            case AuditLogActionType.IntegrationUpdate:
                entry = ParseIntegrationUpdateEntry(guild, auditLogAction);
                break;

            case AuditLogActionType.GuildScheduledEventCreate:
            case AuditLogActionType.GuildScheduledEventDelete:
            case AuditLogActionType.GuildScheduledEventUpdate:
                entry = ParseGuildScheduledEventUpdateEntry(guild, auditLogAction, events);
                break;

            case AuditLogActionType.ThreadCreate:
            case AuditLogActionType.ThreadDelete:
            case AuditLogActionType.ThreadUpdate:
                entry = ParseThreadUpdateEntry(guild, auditLogAction, threads);
                break;

            case AuditLogActionType.ApplicationCommandPermissionUpdate:
                entry = new DiscordAuditLogApplicationCommandPermissionEntry();
                DiscordAuditLogApplicationCommandPermissionEntry permissionEntry =
                    entry as DiscordAuditLogApplicationCommandPermissionEntry;

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

                foreach (AuditLogActionChange change in auditLogAction.Changes)
                {
                    DiscordApplicationCommandPermission? oldValue = ((JObject?)change
                            .OldValue)?
                        .ToDiscordObject<DiscordApplicationCommandPermission>();

                    DiscordApplicationCommandPermission? newValue = ((JObject)change
                            .NewValue)?
                        .ToDiscordObject<DiscordApplicationCommandPermission>();

                    permissionEntry.PermissionChanges.Append(new PropertyChange<DiscordApplicationCommandPermission>
                    {
                        Before = oldValue != null ? oldValue : null, After = newValue != null ? newValue : null
                    });
                }

                break;

            case AuditLogActionType.AutoModerationBlockMessage:
            case AuditLogActionType.AutoModerationFlagToChannel:
            case AuditLogActionType.AutoModerationUserCommunicationDisabled:
                entry = new DiscordAuditLogAutoModerationExecutedEntry();

                DiscordAuditLogAutoModerationExecutedEntry autoModerationEntry =
                    entry as DiscordAuditLogAutoModerationExecutedEntry;

                autoModerationEntry.TargetUser =
                    members.TryGetValue(auditLogAction.TargetId.Value, out DiscordMember? targetMember)
                        ? targetMember
                        : new DiscordUser {Id = auditLogAction.TargetId.Value, Discord = guild.Discord};

                autoModerationEntry.ResponsibleRule = auditLogAction.Options.RoleName;
                autoModerationEntry.Channel = guild.GetChannel(auditLogAction.Options.ChannelId);
                autoModerationEntry.RuleTriggerType =
                    (RuleTriggerType)int.Parse(auditLogAction.Options.AutoModerationRuleTriggerType);
                break;

            case AuditLogActionType.AutoModerationRuleCreate:
            case AuditLogActionType.AutoModerationRuleUpdate:
            case AuditLogActionType.AutoModerationRuleDelete:
                entry = ParseAutoModerationRuleUpdateEntry(guild, auditLogAction);
                break;

            default:
                if (guild.Discord.Configuration.LogUnknownAuditlogs)
                {
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown audit log action type: {0} - this should be reported to library developers",
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
            AuditLogActionType.ChannelCreate or AuditLogActionType.EmojiCreate or AuditLogActionType.InviteCreate
                or AuditLogActionType.OverwriteCreate or AuditLogActionType.RoleCreate
                or AuditLogActionType.WebhookCreate or AuditLogActionType.IntegrationCreate
                or AuditLogActionType.StickerCreate
                or AuditLogActionType.AutoModerationRuleCreate => AuditLogActionCategory.Create,

            AuditLogActionType.ChannelDelete or AuditLogActionType.EmojiDelete or AuditLogActionType.InviteDelete
                or AuditLogActionType.MessageDelete or AuditLogActionType.MessageBulkDelete
                or AuditLogActionType.OverwriteDelete or AuditLogActionType.RoleDelete
                or AuditLogActionType.WebhookDelete or AuditLogActionType.IntegrationDelete
                or AuditLogActionType.StickerDelete
                or AuditLogActionType.AutoModerationRuleDelete => AuditLogActionCategory.Delete,

            AuditLogActionType.ChannelUpdate or AuditLogActionType.EmojiUpdate or AuditLogActionType.InviteUpdate
                or AuditLogActionType.MemberRoleUpdate or AuditLogActionType.MemberUpdate
                or AuditLogActionType.OverwriteUpdate or AuditLogActionType.RoleUpdate
                or AuditLogActionType.WebhookUpdate or AuditLogActionType.IntegrationUpdate
                or AuditLogActionType.StickerUpdate
                or AuditLogActionType.AutoModerationRuleUpdate => AuditLogActionCategory.Update,
            _ => AuditLogActionCategory.Other,
        };
        entry.ActionType = auditLogAction.ActionType;
        entry.Id = auditLogAction.Id;
        entry.Reason = auditLogAction.Reason;
        entry.Discord = guild.Discord;

        DiscordMember member;
        DiscordUser discordUser;
        if (members.TryGetValue(auditLogAction.UserId, out member))
        {
            entry.UserResponsible = member;
        }
        else if (guild.Discord.UserCache.TryGetValue(auditLogAction.UserId, out discordUser))
        {
            entry.UserResponsible = discordUser;
        }
        else
        {
            entry.UserResponsible = new DiscordUser {Id = auditLogAction.UserId, Discord = guild.Discord};
        }

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
                    ruleEntry.RuleId = new PropertyChange<ulong?>
                    {
                        Before = change.OldValue != null ? change.OldValueUlong : null,
                        After = change.NewValue != null ? change.NewValueUlong : null
                    };
                    break;

                case "guild_id":
                    ruleEntry.GuildId = new PropertyChange<ulong?>
                    {
                        Before = change.OldValue != null ? change.OldValueUlong : null,
                        After = change.NewValue != null ? change.NewValueUlong : null
                    };
                    break;

                case "name":
                    ruleEntry.Name = new PropertyChange<string?>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;

                case "creator_id":
                    ruleEntry.CreatorId = new PropertyChange<ulong?>
                    {
                        Before = change.OldValue != null ? change.OldValueUlong : null,
                        After = change.NewValue != null ? change.NewValueUlong : null
                    };
                    break;

                case "event_type":
                    ruleEntry.EventType = new PropertyChange<RuleEventType?>
                    {
                        Before = change.OldValue != null ? (RuleEventType)int.Parse(change.OldValueString) : null,
                        After = change.NewValue != null ? (RuleEventType)int.Parse(change.NewValueString) : null
                    };
                    break;

                case "trigger_type":
                    ruleEntry.TriggerType = new PropertyChange<RuleTriggerType?>
                    {
                        Before = change.OldValue != null ? (RuleTriggerType)int.Parse(change.OldValueString) : null,
                        After = change.NewValue != null ? (RuleTriggerType)int.Parse(change.NewValueString) : null
                    };
                    break;

                case "trigger_metadata":
                    ruleEntry.TriggerMetadata = new PropertyChange<DiscordRuleTriggerMetadata?>
                    {
                        Before =
                            change.OldValue != null
                                ? ((JObject)change.OldValue).ToDiscordObject<DiscordRuleTriggerMetadata>()
                                : null,
                        After = change.NewValue != null
                            ? ((JObject)change.NewValue).ToDiscordObject<DiscordRuleTriggerMetadata>()
                            : null
                    };
                    break;

                case "actions":
                    ruleEntry.Actions = new PropertyChange<IEnumerable<DiscordAutoModerationAction>>
                    {
                        Before =
                            change.OldValue != null
                                ? change.OldValues.Select(x => x.ToDiscordObject<DiscordAutoModerationAction>())
                                : null,
                        After = change.NewValue != null
                            ? change.NewValues.Select(x => x.ToDiscordObject<DiscordAutoModerationAction>())
                            : null
                    };
                    break;

                case "enabled":
                    ruleEntry.Enabled = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? bool.Parse(change.OldValueString) : null,
                        After = change.NewValue != null ? bool.Parse(change.NewValueString) : null
                    };
                    break;

                case "exempt_roles":
                    ruleEntry.ExemptRoles = new PropertyChange<IEnumerable<DiscordRole>>
                    {
                        Before =
                            change.OldValue != null
                                ? change.OldValues.Select(xo => (ulong)xo["id"]).Select(guild.GetRole)
                                : null,
                        After = change.NewValue != null
                            ? change.NewValues.Select(xo => (ulong)xo["id"]).Select(guild.GetRole)
                            : null
                    };
                    break;

                case "exempt_channels":
                    ruleEntry.ExemptChannels = new PropertyChange<IEnumerable<DiscordChannel>>
                    {
                        Before =
                            change.OldValue != null
                                ? change.OldValues.Select(xo => (ulong)xo["id"]).Select(guild.GetChannel)
                                : null,
                        After = change.NewValue != null
                            ? change.NewValues.Select(xo => (ulong)xo["id"]).Select(guild.GetChannel)
                            : null
                    };
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
                threads.TryGetValue(auditLogAction.TargetId.Value,
                    out DiscordThreadChannel? channel)
                    ? channel
                    : new DiscordThreadChannel() {Id = auditLogAction.TargetId.Value, Discord = guild.Discord},
        };
        
        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.Name = new PropertyChange<string?>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;

                case "type":
                    entry.Type = new PropertyChange<ChannelType?>
                    {
                        Before = change.OldValue != null ? (ChannelType)change.OldValueLong : null,
                        After = change.NewValue != null ? (ChannelType)change.NewValueLong : null
                    };
                    break;

                case "archived":
                    entry.Archived = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? change.OldValueBool : null,
                        After = change.NewValue != null ? change.NewValueBool : null
                    };
                    break;

                case "auto_archive_duration":
                    entry.AutoArchiveDuration = new PropertyChange<int?>
                    {
                        Before = change.OldValue != null ? (int)change.OldValueLong : null,
                        After = change.NewValue != null ? (int)change.NewValueLong : null
                    };
                    break;

                case "invitable":
                    entry.Invitable = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? change.OldValueBool : null,
                        After = change.NewValue != null ? change.NewValueBool : null
                    };
                    break;

                case "locked":
                    entry.Locked = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? change.OldValueBool : null,
                        After = change.NewValue != null ? change.NewValueBool : null
                    };
                    break;

                case "rate_limit_per_user":
                    entry.PerUserRateLimit = new PropertyChange<int?>
                    {
                        Before = change.OldValue != null ? (int)change.OldValueLong : null,
                        After = change.NewValue != null ? (int)change.NewValueLong : null
                    };
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
                events.TryGetValue(auditLogAction.TargetId.Value, out DiscordScheduledGuildEvent? ta)
                    ? ta
                    : new DiscordScheduledGuildEvent() {Id = auditLogAction.TargetId.Value, Discord = guild.Discord},
        };
        
        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.Name = new PropertyChange<string?>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;
                case "channel_id":
                    entry.Channel = new PropertyChange<DiscordChannel?>
                    {
                        Before =
                            guild.GetChannel(change.OldValueUlong) ?? new DiscordChannel
                            {
                                Id = change.OldValueUlong, Discord = guild.Discord, GuildId = guild.Id
                            },
                        After = guild.GetChannel(change.NewValueUlong) ?? new DiscordChannel
                        {
                            Id = change.NewValueUlong, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "description":
                    entry.Description = new PropertyChange<string?>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;

                case "entity_type":
                    entry.Type = new PropertyChange<ScheduledGuildEventType?>
                    {
                        Before = change.OldValue != null
                            ? (ScheduledGuildEventType)change.OldValueLong
                            : null,
                        After = change.NewValue != null
                            ? (ScheduledGuildEventType)change.NewValueLong
                            : null
                    };
                    break;

                case "image_hash":
                    entry.ImageHash = new PropertyChange<string?>
                    {
                        Before = (string?)change.OldValue, After = (string?)change.NewValue
                    };
                    break;

                case "location":
                    entry.Location = new PropertyChange<string?>
                    {
                        Before = (string?)change.OldValue, After = (string?)change.NewValue
                    };
                    break;

                case "privacy_level":
                    entry.PrivacyLevel = new PropertyChange<ScheduledGuildEventPrivacyLevel?>
                    {
                        Before =
                            change.OldValue != null
                                ? (ScheduledGuildEventPrivacyLevel)change.OldValueLong
                                : null,
                        After = change.NewValue != null
                            ? (ScheduledGuildEventPrivacyLevel)change.NewValueLong
                            : null
                    };
                    break;

                case "status":
                    entry.Status = new PropertyChange<ScheduledGuildEventStatus?>
                    {
                        Before = change.OldValue != null
                            ? (ScheduledGuildEventStatus)change.OldValueLong
                            : null,
                        After = change.NewValue != null
                            ? (ScheduledGuildEventStatus)change.NewValueLong
                            : null
                    };
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
        DiscordAuditLogGuildEntry entry = new() {Target = guild};

        ulong before, after;
        
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "owner_id":
                    entry.OwnerChange = new PropertyChange<DiscordMember>
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
                    entry.IconChange = new PropertyChange<string>
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
                    entry.VerificationLevelChange = new PropertyChange<VerificationLevel>
                    {
                        Before = (VerificationLevel)(long)change.OldValue,
                        After = (VerificationLevel)(long)change.NewValue
                    };
                    break;

                case "afk_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    entry.AfkChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "widget_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    entry.EmbedChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "splash_hash":
                    entry.SplashChange = new PropertyChange<string>
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
                    entry.NotificationSettingsChange = new PropertyChange<DefaultMessageNotifications>
                    {
                        Before = (DefaultMessageNotifications)(long)change.OldValue,
                        After = (DefaultMessageNotifications)(long)change.NewValue
                    };
                    break;

                case "system_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    entry.SystemChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "explicit_content_filter":
                    entry.ExplicitContentFilterChange = new PropertyChange<ExplicitContentFilter>
                    {
                        Before = (ExplicitContentFilter)change.OldValueLong,
                        After = (ExplicitContentFilter)change.NewValueLong
                    };
                    break;

                case "mfa_level":
                    entry.MfaLevelChange = new PropertyChange<MfaLevel>
                    {
                        Before = (MfaLevel)change.OldValueLong, After = (MfaLevel)change.NewValueLong
                    };
                    break;

                case "region":
                    entry.RegionChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
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
            Target = guild.GetChannel(auditLogAction.TargetId.Value) ?? new DiscordChannel
            {
                Id = auditLogAction.TargetId.Value, Discord = guild.Discord, GuildId = guild.Id
            }
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = new PropertyChange<string>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;

                case "type":
                    boolBefore = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    entry.TypeChange = new PropertyChange<ChannelType?>
                    {
                        Before = boolBefore ? (ChannelType?)ulongBefore : null,
                        After = boolAfter ? (ChannelType?)ulongAfter : null
                    };
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
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "nsfw":
                    entry.NsfwChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;

                case "bitrate":
                    entry.BitrateChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)change.OldValue, After = (int?)(long?)change.NewValue
                    };
                    break;

                case "rate_limit_per_user":
                    entry.PerUserRateLimitChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)change.OldValue, After = (int?)(long?)change.NewValue
                    };
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
                .GetChannel(auditLogAction.TargetId.Value)?
                .PermissionOverwrites
                .FirstOrDefault(xo => xo.Id == auditLogAction.Options.Id),
            Channel = guild.GetChannel(auditLogAction.TargetId.Value)
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "deny":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    entry.DenyChange = new()
                    {
                        Before = boolBefore ? (Permissions?)ulongBefore : null,
                        After = boolAfter ? (Permissions?)ulongAfter : null
                    };
                    break;

                case "allow":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    entry.AllowChange = new PropertyChange<Permissions?>
                    {
                        Before = boolBefore ? (Permissions?)ulongBefore : null,
                        After = boolAfter ? (Permissions?)ulongAfter : null
                    };
                    break;

                case "type":
                    entry.TypeChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "id":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    entry.TargetIdChange = new PropertyChange<ulong?>
                    {
                        Before = boolBefore ? (ulong?)ulongBefore : null,
                        After = boolAfter ? (ulong?)ulongAfter : null
                    };
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in overwrite update: {Key} - this should be reported to library developers",
                            xc.Key);
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
            Target = guild._members.TryGetValue(auditLogAction.TargetId.Value, out DiscordMember? roleUpdMember)
                ? roleUpdMember
                : new DiscordMember {Id = auditLogAction.TargetId.Value, Discord = guild.Discord, _guild_id = guild.Id}
        };
        
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "nick":
                    entry.NicknameChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "deaf":
                    entry.DeafenChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                case "mute":
                    entry.MuteChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                case "communication_disabled_until":
                    entry.TimeoutChange = new PropertyChange<DateTime?>
                    {
                        Before = xc.OldValue != null ? (DateTime)xc.OldValue : null,
                        After = xc.NewValue != null ? (DateTime)xc.NewValue : null
                    };
                    break;

                case "$add":
                    entry.AddedRoles =
                        new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"])
                            .Select(guild.GetRole).ToList());
                    break;

                case "$remove":
                    entry.RemovedRoles =
                        new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"])
                            .Select(guild.GetRole).ToList());
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in member update: {Key} - this should be reported to library developers",
                            xc.Key);
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
            Target = guild.GetRole(auditLogAction.TargetId.Value) ??
                     new DiscordRole {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };
        
        bool boolBefore, boolAfter;
        int intBefore, intAfter;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "color":
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intAfter);

                    entry.ColorChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "permissions":
                    entry.PermissionChange = new PropertyChange<Permissions?>
                    {
                        Before = change.OldValue != null ? (Permissions?)long.Parse((string)change.OldValue) : null,
                        After = change.NewValue != null ? (Permissions?)long.Parse((string)change.NewValue) : null
                    };
                    break;

                case "position":
                    entry.PositionChange = new PropertyChange<int?>
                    {
                        Before = change.OldValue != null ? (int?)(long)change.OldValue : null,
                        After = change.NewValue != null ? (int?)(long)change.NewValue : null,
                    };
                    break;

                case "mentionable":
                    entry.MentionableChange = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? (bool?)change.OldValue : null,
                        After = change.NewValue != null ? (bool?)change.NewValue : null
                    };
                    break;

                case "hoist":
                    entry.HoistChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;

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
                Discord = guild.Discord, Id = guild.Id, Name = guild.Name, SplashHash = guild.SplashHash
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
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out intAfter);

                    entry.MaxAgeChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "code":
                    invite.Code = change.OldValueString ?? change.NewValueString;

                    entry.CodeChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
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
                            : new DiscordMember {Id = ulongBefore, Discord = guild.Discord, _guild_id = guild.Id},
                        After = guild._members.TryGetValue(ulongAfter, out DiscordMember? propAfterMember)
                            ? propAfterMember
                            : new DiscordMember {Id = ulongBefore, Discord = guild.Discord, _guild_id = guild.Id}
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
                              new DiscordChannel {Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id}
                            : null,
                        After = boolAfter
                            ? guild.GetChannel(ulongAfter) ??
                              new DiscordChannel {Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id}
                            : null
                    };

                    DiscordChannel? channel = entry.ChannelChange.Before ?? entry.ChannelChange.After;
                    ChannelType? channelType = channel?.Type;
                    invite.Channel = new DiscordInviteChannel
                    {
                        Discord = guild.Discord,
                        Id = boolBefore ? ulongBefore : ulongAfter,
                        Name = channel?.Name,
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
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
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
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
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
            Target = webhooks.TryGetValue(auditLogAction.TargetId.Value, out DiscordWebhook? webhook)
                ? webhook
                : new DiscordWebhook {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        int intBefore, intAfter;
        foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
        {
            switch (actionChange.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;

                case "channel_id":
                    boolBefore = ulong.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    entry.ChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before =
                            boolBefore
                                ? guild.GetChannel(ulongBefore) ?? new DiscordChannel
                                {
                                    Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id
                                }
                                : null,
                        After = boolAfter
                            ? guild.GetChannel(ulongAfter) ??
                              new DiscordChannel {Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id}
                            : null
                    };
                    break;

                case "type": // ???
                    boolBefore = int.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out intBefore);
                    boolAfter = int.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out intAfter);

                    entry.TypeChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "avatar_hash":
                    entry.AvatarHashChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;

                case "application_id"
                    : //Why the fuck does discord send this as a string if it's supposed to be a snowflake
                    entry.ApplicationIdChange = new PropertyChange<ulong?>
                    {
                        Before =
                            actionChange.OldValue != null ? Convert.ToUInt64(actionChange.OldValueString) : null,
                        After = actionChange.NewValue != null ? Convert.ToUInt64(actionChange.NewValueString) : null
                    };
                    break;


                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in webhook update: {Key} - this should be reported to library developers",
                            actionChange.Key);
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
            Target = guild._stickers.TryGetValue(auditLogAction.TargetId.Value, out DiscordMessageSticker? sticker)
                ? sticker
                : new DiscordMessageSticker {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };

        ulong ulongBefore, ulongAfter;
        long longBefore, longAfter;
        bool boolBefore, boolAfter;
        foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
        {
            switch (actionChange.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "description":
                    entry.DescriptionChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "tags":
                    entry.TagsChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "guild_id":
                    entry.GuildIdChange = new PropertyChange<ulong?>
                    {
                        Before =
                            ulong.TryParse(actionChange.OldValueString, out ulong oldGuildId) ? oldGuildId : null,
                        After = ulong.TryParse(actionChange.NewValueString, out ulong newGuildId)
                            ? newGuildId
                            : null
                    };
                    break;
                case "available":
                    entry.AvailabilityChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)actionChange.OldValue, After = (bool?)actionChange.NewValue,
                    };
                    break;
                case "asset":
                    entry.AssetChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "id":
                    entry.IdChange = new PropertyChange<ulong?>
                    {
                        Before = ulong.TryParse(actionChange.OldValueString, out ulong oid) ? oid : null,
                        After = ulong.TryParse(actionChange.NewValueString, out ulong nid) ? nid : null
                    };
                    break;
                case "type":
                    boolBefore = long.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longBefore);
                    boolAfter = long.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longAfter);
                    entry.TypeChange = new PropertyChange<StickerType?>
                    {
                        Before = boolBefore ? (StickerType?)longBefore : null,
                        After = boolAfter ? (StickerType?)longAfter : null
                    };
                    break;
                case "format_type":
                    boolBefore = long.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longBefore);
                    boolAfter = long.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longAfter);
                    entry.FormatChange = new PropertyChange<StickerFormat?>
                    {
                        Before = boolBefore ? (StickerFormat?)longBefore : null,
                        After = boolAfter ? (StickerFormat?)longAfter : null
                    };
                    break;

                default:
                    if (guild.Discord.Configuration.LogUnknownAuditlogs)
                    {
                        guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in sticker update: {Key} - this should be reported to library developers",
                            actionChange.Key);
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
                    entry.EnableEmoticons = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;
                case "expire_behavior":
                    entry.ExpireBehavior = new PropertyChange<int?>
                    {
                        Before = (int?)change.OldValue, After = (int?)change.NewValue
                    };
                    break;
                case "expire_grace_period":
                    entry.ExpireBehavior = new PropertyChange<int?>
                    {
                        Before = (int?)change.OldValue, After = (int?)change.NewValue
                    };
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
