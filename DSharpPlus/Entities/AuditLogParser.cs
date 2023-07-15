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

namespace DSharpPlus.Entities;

internal static class AuditLogParser
{
    internal static async Task<IEnumerable<DiscordAuditLogEntry>> ParseAuditLogToEntriesAsync
    (
        BaseDiscordClient client,
        DiscordGuild guild,
        AuditLog auditLog
    )
    {
        //Get all User
        DiscordUser[] users = auditLog.Users.ToArray();

        //Update cache
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
        DiscordWebhook[] uniqueWebhooks = auditLog.Webhooks.ToArray();

        DiscordScheduledGuildEvent[] uniqueScheduledEvents = auditLog.Events.ToArray();

        DiscordThreadChannel[] uniqueThreads = auditLog.Threads.ToArray();

        Dictionary<ulong, DiscordWebhook> webhooks = uniqueWebhooks.ToDictionary(x => x.Id);

        //update event cache and create a dictionary for it 
        Dictionary<ulong, DiscordScheduledGuildEvent> events = new();
        foreach (DiscordScheduledGuildEvent discordEvent in uniqueScheduledEvents)
        {
            guild._scheduledEvents[discordEvent.Id] = discordEvent;
        }

        events = guild._scheduledEvents.ToDictionary(x => x.Key, y => y.Value);

        Dictionary<ulong, DiscordThreadChannel> threads = new();
        if (uniqueThreads.Any())
        {
            threads = uniqueThreads.ToDictionary(xa => xa.Id, xa => xa);
        }

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

    internal static async Task<DiscordAuditLogEntry?> ParseAuditLogEntryAsync
    (
        DiscordGuild guild,
        AuditLogAction auditLogAction,
        Dictionary<ulong, DiscordMember> members = null,
        Dictionary<ulong, DiscordThreadChannel> threads = null,
        Dictionary<ulong, DiscordWebhook> webhooks = null,
        Dictionary<ulong, DiscordScheduledGuildEvent> events = null
    )
    {
        //initialize parameters if null
        members ??= new Dictionary<ulong, DiscordMember>();
        threads ??= new Dictionary<ulong, DiscordThreadChannel>();
        webhooks ??= new Dictionary<ulong, DiscordWebhook>();
        events ??= new Dictionary<ulong, DiscordScheduledGuildEvent>();

        DiscordAuditLogEntry entry = null;
        ulong ulongBefore, ulongAfter;
        int intBefore, intAfter;
        long longBefore, longAfter;
        bool boolBefore, boolAfter;
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
                            guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                                "Unknown key in emote update: {Key} - this should be reported to library developers",
                                actionChange.Key);
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

            default:
                guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                    "Unknown audit log action type: {0} - this should be reported to library developers",
                    (int)auditLogAction.ActionType);
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
                or AuditLogActionType.StickerCreate => AuditLogActionCategory.Create,
            AuditLogActionType.ChannelDelete or AuditLogActionType.EmojiDelete or AuditLogActionType.InviteDelete
                or AuditLogActionType.MessageDelete or AuditLogActionType.MessageBulkDelete
                or AuditLogActionType.OverwriteDelete or AuditLogActionType.RoleDelete
                or AuditLogActionType.WebhookDelete or AuditLogActionType.IntegrationDelete
                or AuditLogActionType.StickerDelete => AuditLogActionCategory.Delete,
            AuditLogActionType.ChannelUpdate or AuditLogActionType.EmojiUpdate or AuditLogActionType.InviteUpdate
                or AuditLogActionType.MemberRoleUpdate or AuditLogActionType.MemberUpdate
                or AuditLogActionType.OverwriteUpdate or AuditLogActionType.RoleUpdate
                or AuditLogActionType.WebhookUpdate or AuditLogActionType.IntegrationUpdate
                or AuditLogActionType.StickerUpdate => AuditLogActionCategory.Update,
            _ => AuditLogActionCategory.Other,
        };
        entry.ActionType = auditLogAction.ActionType;
        entry.Id = auditLogAction.Id;
        entry.Reason = auditLogAction.Reason;
        entry.UserResponsible = members[auditLogAction.UserId];
        entry.Discord = guild.Discord;
        return entry;
    }

    internal static DiscordAuditLogEntry ParseThreadUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction,
        Dictionary<ulong, DiscordThreadChannel> threads)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogThreadEventEntry()
        {
            Target =
                threads.TryGetValue(auditLogAction.TargetId.Value,
                    out DiscordThreadChannel? channel)
                    ? channel
                    : new DiscordThreadChannel() {Id = auditLogAction.TargetId.Value, Discord = guild.Discord},
        };

        DiscordAuditLogThreadEventEntry? threadEventEntry = entry as DiscordAuditLogThreadEventEntry;
        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    threadEventEntry.Name = new PropertyChange<string?>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;

                case "type":
                    threadEventEntry.Type = new PropertyChange<ChannelType?>
                    {
                        Before = change.OldValue != null ? (ChannelType)change.OldValueLong : null,
                        After = change.NewValue != null ? (ChannelType)change.NewValueLong : null
                    };
                    break;

                case "archived":
                    threadEventEntry.Archived = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? change.OldValueBool : null,
                        After = change.NewValue != null ? change.NewValueBool : null
                    };
                    break;

                case "auto_archive_duration":
                    threadEventEntry.AutoArchiveDuration = new PropertyChange<int?>
                    {
                        Before = change.OldValue != null ? (int)change.OldValueLong : null,
                        After = change.NewValue != null ? (int)change.NewValueLong : null
                    };
                    break;

                case "invitable":
                    threadEventEntry.Invitable = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? change.OldValueBool : null,
                        After = change.NewValue != null ? change.NewValueBool : null
                    };
                    break;

                case "locked":
                    threadEventEntry.Locked = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? change.OldValueBool : null,
                        After = change.NewValue != null ? change.NewValueBool : null
                    };
                    break;

                case "rate_limit_per_user":
                    threadEventEntry.PerUserRateLimit = new PropertyChange<int?>
                    {
                        Before = change.OldValue != null ? (int)change.OldValueLong : null,
                        After = change.NewValue != null ? (int)change.NewValueLong : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in thread update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }

    private static DiscordAuditLogEntry ParseGuildScheduledEventUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction, Dictionary<ulong, DiscordScheduledGuildEvent> events)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogGuildScheduledEventEntry()
        {
            Target =
                events.TryGetValue(auditLogAction.TargetId.Value, out DiscordScheduledGuildEvent? ta)
                    ? ta
                    : new DiscordScheduledGuildEvent() {Id = auditLogAction.TargetId.Value, Discord = guild.Discord},
        };

        ulong ulongBefore, ulongAfter;
        DiscordAuditLogGuildScheduledEventEntry? eventEntry =
            entry as DiscordAuditLogGuildScheduledEventEntry;
        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    eventEntry.Name = new PropertyChange<string?>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;
                case "channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);
                    eventEntry.Channel = new PropertyChange<DiscordChannel?>
                    {
                        Before =
                            guild.GetChannel(ulongAfter) ?? new DiscordChannel
                            {
                                Id = ulongAfter, Discord = guild.Discord, GuildId = guild.Id
                            },
                        After = guild.GetChannel(ulongBefore) ?? new DiscordChannel
                        {
                            Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "description":
                    eventEntry.Description = new PropertyChange<string?>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;

                case "entity_type":
                    eventEntry.Type = new PropertyChange<ScheduledGuildEventType?>
                    {
                        Before = change.OldValue != null
                            ? (ScheduledGuildEventType)(long)change.OldValue
                            : null,
                        After = change.NewValue != null
                            ? (ScheduledGuildEventType)(long)change.NewValue
                            : null
                    };
                    break;

                case "image_hash":
                    eventEntry.ImageHash = new PropertyChange<string?>
                    {
                        Before = (string?)change.OldValue, After = (string?)change.NewValue
                    };
                    break;

                case "location":
                    eventEntry.Location = new PropertyChange<string?>
                    {
                        Before = (string?)change.OldValue, After = (string?)change.NewValue
                    };
                    break;

                case "privacy_level":
                    eventEntry.PrivacyLevel = new PropertyChange<ScheduledGuildEventPrivacyLevel?>
                    {
                        Before =
                            change.OldValue != null
                                ? (ScheduledGuildEventPrivacyLevel)(long)change.OldValue
                                : null,
                        After = change.NewValue != null
                            ? (ScheduledGuildEventPrivacyLevel)(long)change.NewValue
                            : null
                    };
                    break;

                case "status":
                    eventEntry.Status = new PropertyChange<ScheduledGuildEventStatus?>
                    {
                        Before = change.OldValue != null
                            ? (ScheduledGuildEventStatus)(long)change.OldValue
                            : null,
                        After = change.NewValue != null
                            ? (ScheduledGuildEventStatus)(long)change.NewValue
                            : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in scheduled event update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }

    internal static async Task<DiscordAuditLogEntry> ParseGuildUpdateAsync(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogGuildEntry {Target = guild};

        ulong before, after;

        DiscordAuditLogGuildEntry guildEntry = entry as DiscordAuditLogGuildEntry;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    guildEntry.NameChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "owner_id":
                    guildEntry.OwnerChange = new PropertyChange<DiscordMember>
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
                    guildEntry.IconChange = new PropertyChange<string>
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
                    guildEntry.VerificationLevelChange = new PropertyChange<VerificationLevel>
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

                    guildEntry.AfkChannelChange = new PropertyChange<DiscordChannel>
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

                    guildEntry.EmbedChannelChange = new PropertyChange<DiscordChannel>
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
                    guildEntry.SplashChange = new PropertyChange<string>
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
                    guildEntry.NotificationSettingsChange = new PropertyChange<DefaultMessageNotifications>
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

                    guildEntry.SystemChannelChange = new PropertyChange<DiscordChannel>
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
                    guildEntry.ExplicitContentFilterChange = new PropertyChange<ExplicitContentFilter>
                    {
                        Before = (ExplicitContentFilter)(long)change.OldValue,
                        After = (ExplicitContentFilter)(long)change.NewValue
                    };
                    break;

                case "mfa_level":
                    guildEntry.MfaLevelChange = new PropertyChange<MfaLevel>
                    {
                        Before = (MfaLevel)(long)change.OldValue, After = (MfaLevel)(long)change.NewValue
                    };
                    break;

                case "region":
                    guildEntry.RegionChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in guild update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseChannelEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogChannelEntry
        {
            Target = guild.GetChannel(auditLogAction.TargetId.Value) ?? new DiscordChannel
            {
                Id = auditLogAction.TargetId.Value, Discord = guild.Discord, GuildId = guild.Id
            }
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        DiscordAuditLogChannelEntry? channelEntry = entry as DiscordAuditLogChannelEntry;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    channelEntry.NameChange = new PropertyChange<string>
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

                    channelEntry.TypeChange = new PropertyChange<ChannelType?>
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

                    channelEntry.OverwriteChange = new PropertyChange<IReadOnlyList<DiscordOverwrite>>
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
                    channelEntry.TopicChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "nsfw":
                    channelEntry.NsfwChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;

                case "bitrate":
                    channelEntry.BitrateChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)change.OldValue, After = (int?)(long?)change.NewValue
                    };
                    break;

                case "rate_limit_per_user":
                    channelEntry.PerUserRateLimitChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)change.OldValue, After = (int?)(long?)change.NewValue
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in channel update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseOverwriteEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogOverwriteEntry
        {
            Target = guild
                .GetChannel(auditLogAction.TargetId.Value)?
                .PermissionOverwrites
                .FirstOrDefault(xo => xo.Id == auditLogAction.Options.Id),
            Channel = guild.GetChannel(auditLogAction.TargetId.Value)
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        DiscordAuditLogOverwriteEntry? overwriteEntry = entry as DiscordAuditLogOverwriteEntry;
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "deny":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    overwriteEntry.DenyChange = new()
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

                    overwriteEntry.AllowChange = new PropertyChange<Permissions?>
                    {
                        Before = boolBefore ? (Permissions?)ulongBefore : null,
                        After = boolAfter ? (Permissions?)ulongAfter : null
                    };
                    break;

                case "type":
                    overwriteEntry.TypeChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "id":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    overwriteEntry.TargetIdChange = new PropertyChange<ulong?>
                    {
                        Before = boolBefore ? (ulong?)ulongBefore : null,
                        After = boolAfter ? (ulong?)ulongAfter : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in overwrite update: {Key} - this should be reported to library developers",
                        xc.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseMemberUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogMemberUpdateEntry
        {
            Target = guild._members.TryGetValue(auditLogAction.TargetId.Value, out DiscordMember? roleUpdMember)
                ? roleUpdMember
                : new DiscordMember {Id = auditLogAction.TargetId.Value, Discord = guild.Discord, _guild_id = guild.Id}
        };

        DiscordAuditLogMemberUpdateEntry? memberUpdateEntry = entry as DiscordAuditLogMemberUpdateEntry;
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "nick":
                    memberUpdateEntry.NicknameChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "deaf":
                    memberUpdateEntry.DeafenChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                case "mute":
                    memberUpdateEntry.MuteChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                case "communication_disabled_until":
                    memberUpdateEntry.TimeoutChange = new PropertyChange<DateTime?>
                    {
                        Before = xc.OldValue != null ? (DateTime)xc.OldValue : null,
                        After = xc.NewValue != null ? (DateTime)xc.NewValue : null
                    };
                    break;

                case "$add":
                    memberUpdateEntry.AddedRoles =
                        new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"])
                            .Select(guild.GetRole).ToList());
                    break;

                case "$remove":
                    memberUpdateEntry.RemovedRoles =
                        new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"])
                            .Select(guild.GetRole).ToList());
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in member update: {Key} - this should be reported to library developers", xc.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseRoleUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogRoleUpdateEntry
        {
            Target = guild.GetRole(auditLogAction.TargetId.Value) ??
                     new DiscordRole {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };
        bool boolBefore, boolAfter;
        int intBefore, intAfter;
        DiscordAuditLogRoleUpdateEntry? roleUpdateEntry = entry as DiscordAuditLogRoleUpdateEntry;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    roleUpdateEntry.NameChange = new PropertyChange<string>
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

                    roleUpdateEntry.ColorChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "permissions":
                    roleUpdateEntry.PermissionChange = new PropertyChange<Permissions?>
                    {
                        Before = change.OldValue != null ? (Permissions?)long.Parse((string)change.OldValue) : null,
                        After = change.NewValue != null ? (Permissions?)long.Parse((string)change.NewValue) : null
                    };
                    break;

                case "position":
                    roleUpdateEntry.PositionChange = new PropertyChange<int?>
                    {
                        Before = change.OldValue != null ? (int?)(long)change.OldValue : null,
                        After = change.NewValue != null ? (int?)(long)change.NewValue : null,
                    };
                    break;

                case "mentionable":
                    roleUpdateEntry.MentionableChange = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? (bool?)change.OldValue : null,
                        After = change.NewValue != null ? (bool?)change.NewValue : null
                    };
                    break;

                case "hoist":
                    roleUpdateEntry.HoistChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in role update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseInviteUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogInviteEntry();

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
        DiscordAuditLogInviteEntry? inviteEntry = entry as DiscordAuditLogInviteEntry;
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

                    inviteEntry.MaxAgeChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "code":
                    invite.Code = change.OldValueString ?? change.NewValueString;

                    inviteEntry.CodeChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "temporary":
                    inviteEntry.TemporaryChange = new PropertyChange<bool?>
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

                    inviteEntry.InviterChange = new PropertyChange<DiscordMember>
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

                    inviteEntry.ChannelChange = new PropertyChange<DiscordChannel>
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

                    DiscordChannel? channel = inviteEntry.ChannelChange.Before ?? inviteEntry.ChannelChange.After;
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

                    inviteEntry.UsesChange = new PropertyChange<int?>
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

                    inviteEntry.MaxUsesChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in invite update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        inviteEntry.Target = invite;
        return entry;
    }

    internal static DiscordAuditLogEntry ParseWebhookUpdateEntry
    (
        DiscordGuild guild,
        AuditLogAction auditLogAction,
        Dictionary<ulong, DiscordWebhook> webhooks
    )
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogWebhookEntry
        {
            Target = webhooks.TryGetValue(auditLogAction.TargetId.Value, out DiscordWebhook? webhook)
                ? webhook
                : new DiscordWebhook {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        int intBefore, intAfter;
        DiscordAuditLogWebhookEntry? webhookEntry = entry as DiscordAuditLogWebhookEntry;
        foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
        {
            switch (actionChange.Key.ToLowerInvariant())
            {
                case "name":
                    webhookEntry.NameChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;

                case "channel_id":
                    boolBefore = ulong.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    webhookEntry.ChannelChange = new PropertyChange<DiscordChannel>
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

                    webhookEntry.TypeChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "avatar_hash":
                    webhookEntry.AvatarHashChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;

                case "application_id"
                    : //Why the fuck does discord send this as a string if it's supposed to be a snowflake
                    webhookEntry.ApplicationIdChange = new PropertyChange<ulong?>
                    {
                        Before =
                            actionChange.OldValue != null ? Convert.ToUInt64(actionChange.OldValueString) : null,
                        After = actionChange.NewValue != null ? Convert.ToUInt64(actionChange.NewValueString) : null
                    };
                    break;


                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in webhook update: {Key} - this should be reported to library developers",
                        actionChange.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseStickerUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogStickerEntry
        {
            Target = guild._stickers.TryGetValue(auditLogAction.TargetId.Value, out DiscordMessageSticker? sticker)
                ? sticker
                : new DiscordMessageSticker {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };

        ulong ulongBefore, ulongAfter;
        long longBefore, longAfter;
        bool boolBefore, boolAfter;
        DiscordAuditLogStickerEntry? stickerEntry = entry as DiscordAuditLogStickerEntry;
        foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
        {
            switch (actionChange.Key.ToLowerInvariant())
            {
                case "name":
                    stickerEntry.NameChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "description":
                    stickerEntry.DescriptionChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "tags":
                    stickerEntry.TagsChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "guild_id":
                    stickerEntry.GuildIdChange = new PropertyChange<ulong?>
                    {
                        Before =
                            ulong.TryParse(actionChange.OldValueString, out ulong oldGuildId) ? oldGuildId : null,
                        After = ulong.TryParse(actionChange.NewValueString, out ulong newGuildId)
                            ? newGuildId
                            : null
                    };
                    break;
                case "available":
                    stickerEntry.AvailabilityChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)actionChange.OldValue, After = (bool?)actionChange.NewValue,
                    };
                    break;
                case "asset":
                    stickerEntry.AssetChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "id":
                    stickerEntry.IdChange = new PropertyChange<ulong?>
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
                    stickerEntry.TypeChange = new PropertyChange<StickerType?>
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
                    stickerEntry.FormatChange = new PropertyChange<StickerFormat?>
                    {
                        Before = boolBefore ? (StickerFormat?)longBefore : null,
                        After = boolAfter ? (StickerFormat?)longAfter : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in sticker update: {Key} - this should be reported to library developers",
                        actionChange.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseIntegrationUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogIntegrationEntry();

        DiscordAuditLogIntegrationEntry? integrationEntry = entry as DiscordAuditLogIntegrationEntry;
        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "enable_emoticons":
                    integrationEntry.EnableEmoticons = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;
                case "expire_behavior":
                    integrationEntry.ExpireBehavior = new PropertyChange<int?>
                    {
                        Before = (int?)change.OldValue, After = (int?)change.NewValue
                    };
                    break;
                case "expire_grace_period":
                    integrationEntry.ExpireBehavior = new PropertyChange<int?>
                    {
                        Before = (int?)change.OldValue, After = (int?)change.NewValue
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in integration update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }
}
