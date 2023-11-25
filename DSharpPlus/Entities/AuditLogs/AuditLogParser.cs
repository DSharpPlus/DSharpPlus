using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Caching;
using DSharpPlus.Enums;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Entities.AuditLogs;

internal class AuditLogParser
{
    private BaseDiscordClient _client;
    private DiscordGuild? _guild;
    private ulong _guildId;
    
    internal AuditLogParser(BaseDiscordClient client, ulong guildId, DiscordGuild? guild = null)
    {
        _client = client;
        _guildId = guildId;
        _guild = guild;
    }
    
    /// <summary>
    /// Parses a AuditLog to a list of AuditLogEntries
    /// </summary>
    /// <param name="auditLog"> <see cref="AuditLog"/> whose entries should be parsed</param>
    /// <returns>A list of <see cref="DiscordAuditLogEntry"/>. All entries which cant be parsed are dropped</returns>
    internal async IAsyncEnumerable<DiscordAuditLogEntry> ParseAuditLogToEntriesAsync
    (
        AuditLog auditLog
    )
    {
        _guild ??= await _client.Cache.TryGetGuildAsync(this._guildId);
        
        IEnumerable<DiscordUser> users = auditLog.Users.ToList();
        IEnumerable<DiscordWebhook> uniqueWebhooks = auditLog.Webhooks;
        IEnumerable<DiscordScheduledGuildEvent> uniqueScheduledEvents = auditLog.Events;
        IEnumerable<DiscordThreadChannel> uniqueThreads = auditLog.Threads; 
        
        List<DiscordMember>? discordMembers = new List<DiscordMember>();
        foreach (DiscordUser user in users)
        {
            DiscordMember? cachedMember = await this._client.Cache.TryGetMemberAsync(user.Id, _guildId);

            if (cachedMember is null && _guild is not null)
            {
                cachedMember = _guild._members.TryGetValue(user.Id, out cachedMember) ? cachedMember : null;
            }
            
            cachedMember ??= new DiscordMember
            {
                Id = user.Id,
                Discord = this._client,
                _guild_id = _guild.Id
            };
            
            discordMembers.Add(cachedMember);
        }
        
        IDictionary<ulong, DiscordScheduledGuildEvent>? events = uniqueScheduledEvents.ToDictionary(x => x.Id);
        IDictionary<ulong, DiscordThreadChannel>? threads = uniqueThreads.ToDictionary(x => x.Id);
        IDictionary<ulong, DiscordWebhook> webhooks = uniqueWebhooks.ToDictionary(x => x.Id);
        IDictionary<ulong, DiscordMember>? members = discordMembers.ToDictionary(x => x.Id);
        
        IOrderedEnumerable<AuditLogAction>? auditLogActions = auditLog.Entries.OrderByDescending(xa => xa.Id);
        foreach (AuditLogAction? auditLogAction in auditLogActions)
        {
            DiscordAuditLogEntry? entry =
                await ParseAuditLogEntryAsync(auditLogAction, members, threads, webhooks, events);

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
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    /// <param name="members">A dictionary of <see cref="DiscordMember"/> which is used to inject the entities instead of passing the id</param>
    /// <param name="threads">A dictionary of <see cref="DiscordThreadChannel"/> which is used to inject the entities instead of passing the id</param>
    /// <param name="webhooks">A dictionary of <see cref="DiscordWebhook"/> which is used to inject the entities instead of passing the id</param>
    /// <param name="events">A dictionary of <see cref="DiscordScheduledGuildEvent"/> which is used to inject the entities instead of passing the id</param>
    /// <returns>Returns a <see cref="DiscordAuditLogEntry"/>. Is null if the entry can not be parsed </returns>
    /// <remarks>Will use guild cache for optional parameters if those are not present if possible</remarks>
    internal async Task<DiscordAuditLogEntry?> ParseAuditLogEntryAsync
    (
        AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordMember>? members = null,
        IDictionary<ulong, DiscordThreadChannel>? threads = null,
        IDictionary<ulong, DiscordWebhook>? webhooks = null,
        IDictionary<ulong, DiscordScheduledGuildEvent>? events = null
    )
    {
        //initialize members if null
        if (members is null && _guild is not null)
        {
            members = _guild._members;
        }

        //initialize threads if null
        if (threads is null && _guild is not null)
        {
            threads = _guild._threads;
        }

        //initialize scheduled events if null
        if (events is null && _guild is not null)
        {
            events = _guild._scheduledEvents;
        }

        members ??= new Dictionary<ulong, DiscordMember>();
        threads ??= new Dictionary<ulong, DiscordThreadChannel>();
        events ??= new Dictionary<ulong, DiscordScheduledGuildEvent>();
        webhooks ??= new Dictionary<ulong, DiscordWebhook>();

        ulong channelId, targetId;
        DiscordChannel? channel = null;
        DiscordMember? member = null;
        DiscordUser? user = null;
        
        DiscordAuditLogEntry? entry = null;
        switch (auditLogAction.ActionType)
        {
            case AuditLogActionType.GuildUpdate:
                entry = await ParseGuildUpdateAsync(auditLogAction);
                break;

            case AuditLogActionType.ChannelCreate:
            case AuditLogActionType.ChannelDelete:
            case AuditLogActionType.ChannelUpdate:
                entry = await ParseChannelEntryAsync(auditLogAction);
                break;

            case AuditLogActionType.OverwriteCreate:
            case AuditLogActionType.OverwriteDelete:
            case AuditLogActionType.OverwriteUpdate:
                entry = await ParseOverwriteEntry(auditLogAction);
                break;

            case AuditLogActionType.Kick:
                ulong memberId = auditLogAction.TargetId.Value;
                DiscordMember cachedMember = await this._client.Cache.TryGetMemberAsync(memberId, this._guildId);
                
                entry = new DiscordAuditLogKickEntry
                {
                    Target = new CachedEntity<ulong, DiscordMember>(memberId, cachedMember)
                };
                break;

            case AuditLogActionType.Prune:
                entry = new DiscordAuditLogPruneEntry
                {
                    Days = auditLogAction.Options.DeleteMemberDays,
                    Toll = auditLogAction.Options.MembersRemoved
                };
                break;

            case AuditLogActionType.Ban:
            case AuditLogActionType.Unban:
                memberId = auditLogAction.TargetId.Value;
                cachedMember = await this._client.Cache.TryGetMemberAsync(memberId, this._guildId);
                
                entry = new DiscordAuditLogBanEntry
                {
                    Target = new CachedEntity<ulong, DiscordMember>(memberId, cachedMember)
                };
                break;

            case AuditLogActionType.MemberUpdate:
            case AuditLogActionType.MemberRoleUpdate:
                entry = await ParseMemberUpdateEntry(auditLogAction);
                break;

            case AuditLogActionType.RoleCreate:
            case AuditLogActionType.RoleDelete:
            case AuditLogActionType.RoleUpdate:
                entry = ParseRoleUpdateEntry(auditLogAction);
                break;

            case AuditLogActionType.InviteCreate:
            case AuditLogActionType.InviteDelete:
            case AuditLogActionType.InviteUpdate:
                entry = await ParseInviteUpdateEntry(auditLogAction);
                break;

            case AuditLogActionType.WebhookCreate:
            case AuditLogActionType.WebhookDelete:
            case AuditLogActionType.WebhookUpdate:
                entry = await ParseWebhookUpdateEntry(auditLogAction, webhooks);
                break;

            case AuditLogActionType.EmojiCreate:
            case AuditLogActionType.EmojiDelete:
            case AuditLogActionType.EmojiUpdate:
                entry = new DiscordAuditLogEmojiEntry
                {
                    Target = this._guild._emojis.TryGetValue(auditLogAction.TargetId.Value, out DiscordEmoji? target)
                        ? target
                        : new DiscordEmoji { Id = auditLogAction.TargetId.Value, Discord = this._client }
                };

                DiscordAuditLogEmojiEntry? emojiEntry = entry as DiscordAuditLogEmojiEntry;
                foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
                {
                    switch (actionChange.Key.ToLowerInvariant())
                    {
                        case "name":
                            emojiEntry.NameChange = PropertyChange<string>.From(actionChange);
                            break;

                        default:
                            if (this._client.Configuration.LogUnknownAuditlogs)
                            {
                                this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                                    "Unknown key in emote update: {Key}",
                                    actionChange.Key);
                            }
                            break;
                    }
                }

                break;

            case AuditLogActionType.StickerCreate:
            case AuditLogActionType.StickerDelete:
            case AuditLogActionType.StickerUpdate:
                entry = ParseStickerUpdateEntry(auditLogAction);
                break;

            case AuditLogActionType.MessageDelete:
            case AuditLogActionType.MessageBulkDelete:
                {
                    entry = new DiscordAuditLogMessageEntry();

                    DiscordAuditLogMessageEntry? messageEntry = entry as DiscordAuditLogMessageEntry;

                    if (auditLogAction.Options != null)
                    {
                        channelId = auditLogAction.Options.ChannelId;
                        channel = await this._client.Cache.TryGetChannelAsync(channelId);
                        messageEntry.Channel = new CachedEntity<ulong, DiscordChannel>(channelId, channel);
                        messageEntry.MessageCount = auditLogAction.Options.Count;
                    }

                    if (messageEntry.Channel != null)
                    {
                        ulong userId = auditLogAction.TargetId.Value;
                        user = await this._client.Cache.TryGetMemberAsync(userId, this._guildId);
                        if (user is null)
                        {
                            if (this._guild?._members.TryGetValue(userId, out var guildMember) ?? false)
                            {
                                user = guildMember;
                            }
                        }
                        if (user is null)
                        {
                            user = await this._client.Cache.TryGetUserAsync(userId);
                        }
                        messageEntry.Target = new CachedEntity<ulong, DiscordUser>(userId, member);
                    }

                    break;
                }

            case AuditLogActionType.MessagePin:
            case AuditLogActionType.MessageUnpin:
                {
                    entry = new DiscordAuditLogMessagePinEntry();

                    DiscordAuditLogMessagePinEntry? messagePinEntry = entry as DiscordAuditLogMessagePinEntry;

                    if (this._client is not DiscordClient dc)
                    {
                        break;
                    }

                    if (auditLogAction.Options != null)
                    {
                        channelId = auditLogAction.Options.ChannelId;
                        ulong messageId = auditLogAction.Options.MessageId;
                        
                        DiscordMessage? message = null;
                        message = await this._client.Cache.TryGetMessageAsync(messageId);
                        messagePinEntry.Message = new CachedEntity<ulong, DiscordMessage>(messageId, message);
                        
                        channel = await this._client.Cache.TryGetChannelAsync(channelId);
                        messagePinEntry.Channel = new CachedEntity<ulong, DiscordChannel>(channelId, channel);
                        
                    }

                    if (auditLogAction.TargetId.HasValue)
                    {
                        targetId = auditLogAction.TargetId.Value;
                        user = await this._client.Cache.TryGetMemberAsync(targetId, this._guildId);
                        if (user is null)
                        {
                            if (this._guild?._members.TryGetValue(targetId, out var guildMember) ?? false)
                            {
                                user = guildMember;
                            }
                        }
                        if (user is null)
                        {
                            user = await this._client.Cache.TryGetUserAsync(targetId);
                        }
                        messagePinEntry.Target = new CachedEntity<ulong, DiscordUser>(targetId, user);
                    }

                    break;
                }

            case AuditLogActionType.BotAdd:
                {
                    DiscordAuditLogBotAddEntry botAddEntry = new DiscordAuditLogBotAddEntry();

                    if (!(this._client is DiscordClient dc && auditLogAction.TargetId.HasValue))
                    {
                        break;
                    }

                    ulong botId = auditLogAction.TargetId.Value;
                    DiscordUser? bot = await this._client.Cache.TryGetMemberAsync(botId, this._guildId);
                    if (bot is null)
                    {
                        if (this._guild?._members.TryGetValue(botId, out var botMember) ?? false)
                        {
                            bot = botMember;
                        }
                    }
                    if (bot is null)
                    {
                        bot = await this._client.Cache.TryGetUserAsync(botId);
                    }
                    botAddEntry.TargetBot = new CachedEntity<ulong, DiscordUser>(botId, bot);
                    entry = botAddEntry;
                    break;
                }

            case AuditLogActionType.MemberMove:
                entry = new DiscordAuditLogMemberMoveEntry();

                if (auditLogAction.Options == null)
                {
                    break;
                }

                DiscordAuditLogMemberMoveEntry? memberMoveEntry = entry as DiscordAuditLogMemberMoveEntry;
                channelId = auditLogAction.Options.ChannelId;
                DiscordChannel? movedInChannel = await this._client.Cache.TryGetChannelAsync(channelId);
                memberMoveEntry.Channel = new CachedEntity<ulong, DiscordChannel>(channelId, movedInChannel);
                memberMoveEntry.UserCount = auditLogAction.Options.Count;
                break;
            
            case AuditLogActionType.MemberDisconnect:
                entry = new DiscordAuditLogMemberDisconnectEntry { UserCount = auditLogAction.Options?.Count ?? 0 };
                break;

            case AuditLogActionType.IntegrationCreate:
            case AuditLogActionType.IntegrationDelete:
            case AuditLogActionType.IntegrationUpdate:
                entry = ParseIntegrationUpdateEntry(auditLogAction);
                break;

            case AuditLogActionType.GuildScheduledEventCreate:
            case AuditLogActionType.GuildScheduledEventDelete:
            case AuditLogActionType.GuildScheduledEventUpdate:
                entry = await ParseGuildScheduledEventUpdateEntry(auditLogAction, events);
                break;

            case AuditLogActionType.ThreadCreate:
            case AuditLogActionType.ThreadDelete:
            case AuditLogActionType.ThreadUpdate:
                entry = ParseThreadUpdateEntry(auditLogAction, threads);
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

                permissionEntry.PermissionChanges = new List<PropertyChange<DiscordApplicationCommandPermission>>();

                foreach (AuditLogActionChange change in auditLogAction.Changes)
                {
                    DiscordApplicationCommandPermission? oldValue = ((JObject?)change
                            .OldValue)?
                        .ToDiscordObject<DiscordApplicationCommandPermission>();

                    DiscordApplicationCommandPermission? newValue = ((JObject)change
                            .NewValue)?
                        .ToDiscordObject<DiscordApplicationCommandPermission>();

                    permissionEntry.PermissionChanges
                        .Append(PropertyChange<DiscordApplicationCommandPermission>.From(oldValue, newValue));
                }

                break;

            case AuditLogActionType.AutoModerationBlockMessage:
            case AuditLogActionType.AutoModerationFlagToChannel:
            case AuditLogActionType.AutoModerationUserCommunicationDisabled:
                entry = new DiscordAuditLogAutoModerationExecutedEntry();
                
                targetId = auditLogAction.TargetId.Value;
                channelId = auditLogAction.Options.ChannelId;

                DiscordAuditLogAutoModerationExecutedEntry autoModerationEntry =
                    entry as DiscordAuditLogAutoModerationExecutedEntry;
                
                member = await this._client.Cache.TryGetMemberAsync(targetId, this._guildId);
                if (member is null)
                {
                    if (this._guild?._members.TryGetValue(targetId, out var guildMember) ?? false)
                    {
                        member = guildMember;
                    }
                }
                autoModerationEntry.TargetUser = new CachedEntity<ulong, DiscordUser>(targetId, member);
                
                DiscordChannel? modActionChanel = await this._client.Cache.TryGetChannelAsync(channelId);
                if (modActionChanel is null)
                {
                    modActionChanel = this._guild?.GetChannel(channelId);
                }
                autoModerationEntry.Channel = new CachedEntity<ulong, DiscordChannel>(channelId, modActionChanel);
                    

                autoModerationEntry.ResponsibleRule = auditLogAction.Options.AutoModerationRuleName;
                autoModerationEntry.RuleTriggerType =
                    (RuleTriggerType)int.Parse(auditLogAction.Options.AutoModerationRuleTriggerType);
                break;

            case AuditLogActionType.AutoModerationRuleCreate:
            case AuditLogActionType.AutoModerationRuleUpdate:
            case AuditLogActionType.AutoModerationRuleDelete:
                entry = await ParseAutoModerationRuleUpdateEntry(auditLogAction);
                break;

            default:
                if (this._client.Configuration.LogUnknownAuditlogs)
                {
                    this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown audit log action type: {0}",
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
        entry.Discord = this._client;
        entry.Guild = new CachedEntity<ulong, DiscordGuild>(this._guildId, this._guild);
        
        DiscordUser? responsibleMember = await this._client.Cache.TryGetMemberAsync(auditLogAction.UserId, this._guildId);
        if (responsibleMember is null)
        {
            if (this._guild?._members.TryGetValue(auditLogAction.UserId, out var guildMember) ?? false)
            {
                responsibleMember = guildMember;
            }
        }
        if (responsibleMember is null)
        {
            responsibleMember = await this._client.Cache.TryGetUserAsync(auditLogAction.UserId);
        }
        entry.UserResponsible = new CachedEntity<ulong, DiscordUser>(auditLogAction.UserId, responsibleMember);
        
        return entry;
    }

    /// <summary>
    /// Parses a <see cref="AuditLogAction"/> to a <see cref="DiscordAuditLogAutoModerationRuleEntry"/>
    /// </summary>
    /// <param name="guild"><see cref="DiscordGuild"/> which is the parent of the entry</param>
    /// <param name="auditLogAction"><see cref="AuditLogAction"/> which should be parsed</param>
    private async Task<DiscordAuditLogAutoModerationRuleEntry> ParseAutoModerationRuleUpdateEntry(AuditLogAction auditLogAction)
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
                    ruleEntry.EventType = PropertyChange<RuleEventType?>.From(change);
                    break;

                case "trigger_type":
                    ruleEntry.TriggerType = PropertyChange<RuleTriggerType?>.From(change);
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

                    IEnumerable<CachedEntity<ulong,DiscordRole>> oldRoles = oldRoleIds?
                        .Select(x => x.ToObject<ulong>())
                        .Select(x => new CachedEntity<ulong, DiscordRole>(x, this._guild.GetRole(x)));

                    IEnumerable<CachedEntity<ulong,DiscordRole>> newRoles = newRoleIds?
                        .Select(x => x.ToObject<ulong>())
                        .Select(x => new CachedEntity<ulong, DiscordRole>(x, this._guild.GetRole(x)));

                    ruleEntry.ExemptRoles =
                        PropertyChange<IEnumerable<CachedEntity<ulong,DiscordRole>>>.From(oldRoles, newRoles);
                    break;

                case "exempt_channels":
                    JArray oldChannelIds = (JArray)change.OldValue;
                    JArray newChanelIds = (JArray)change.NewValue;

                    IEnumerable<ulong> oldChannelsIds = oldChannelIds?
                        .Select(x => x.ToObject<ulong>())
                        .ToList();
                    
                    IEnumerable<ulong> newChannelsIds = newChanelIds?
                        .Select(x => x.ToObject<ulong>())
                        .ToList();

                    List<CachedEntity<ulong, DiscordChannel>> oldChannels = new();
                    List<CachedEntity<ulong, DiscordChannel>> newChannels = new();
                    
                    foreach (ulong oldChannelId in oldChannelsIds)
                    {
                        DiscordChannel? channel = await this._client.Cache.TryGetChannelAsync(oldChannelId);
                        if (channel is null)
                        {
                            channel = this._guild?.GetChannel(oldChannelId);
                        }
                        oldChannels.Add(new CachedEntity<ulong, DiscordChannel>(oldChannelId, channel));
                    }
                    
                    foreach (ulong newChannelId in newChannelsIds)
                    {
                        DiscordChannel? channel = await this._client.Cache.TryGetChannelAsync(newChannelId);
                        if (channel is null)
                        {
                            channel = this._guild?.GetChannel(newChannelId);
                        }
                        newChannels.Add(new CachedEntity<ulong, DiscordChannel>(newChannelId, channel));
                    }
                    
                    ruleEntry.ExemptChannels =
                        PropertyChange<IEnumerable<CachedEntity<ulong, DiscordChannel>>>.From(oldChannels, newChannels);
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
                    if (_client.Configuration.LogUnknownAuditlogs)
                    {
                        _client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in AutoModRule update: {Key}",
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
    internal DiscordAuditLogThreadEventEntry ParseThreadUpdateEntry
    (
        AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordThreadChannel> threads
    )
    {
        DiscordAuditLogThreadEventEntry entry = new()
        {
            Target =
                threads.TryGetValue(auditLogAction.TargetId.Value,
                    out DiscordThreadChannel? channel)
                    ? channel
                    : new DiscordThreadChannel() { Id = auditLogAction.TargetId.Value, Discord = this._client },
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in thread update: {Key}",
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
    private async Task<DiscordAuditLogGuildScheduledEventEntry> ParseGuildScheduledEventUpdateEntry
    (
        AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordScheduledGuildEvent> events
    )
    {
        DiscordScheduledGuildEvent? target = null;
        if (events.TryGetValue(auditLogAction.TargetId.Value, out DiscordScheduledGuildEvent? t))
        {
            target = t;
        }
        
        DiscordAuditLogGuildScheduledEventEntry entry = new()
        {
            Target = new CachedEntity<ulong, DiscordScheduledGuildEvent>(auditLogAction.TargetId.Value, target)
        };

        foreach (AuditLogActionChange change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.Name = PropertyChange<string?>.From(change);
                    break;
                case "channel_id":
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulong oldChannelId);
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulong newChannelId);
                    
                    DiscordChannel? oldChannel = null;
                    CachedEntity<ulong, DiscordChannel>? cachedOldChannel = null;
                    if (change.OldValue is not null)
                    {
                        oldChannel = await this._client.Cache.TryGetChannelAsync(oldChannelId);
                        if (oldChannel is null)
                        {
                            this._guild?.Channels.TryGetValue(oldChannelId, out oldChannel);
                        }
                        cachedOldChannel = new CachedEntity<ulong, DiscordChannel>(oldChannelId, oldChannel);
                    }
                    
                    DiscordChannel? newChannel = null;
                    CachedEntity<ulong, DiscordChannel>? cachedNewChannel = null;
                    if (change.NewValue is not null)
                    {
                        newChannel = await this._client.Cache.TryGetChannelAsync(newChannelId);
                        if (newChannel is null)
                        {
                            this._guild?.Channels.TryGetValue(newChannelId, out newChannel);
                        }
                        cachedNewChannel = new CachedEntity<ulong, DiscordChannel>(newChannelId, newChannel);
                    }
                    
                    entry.Channel = new PropertyChange<CachedEntity<ulong,DiscordChannel>?>
                    {
                        Before = cachedOldChannel,
                        After = cachedNewChannel
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in scheduled event update: {Key}",
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
    internal async Task<DiscordAuditLogGuildEntry> ParseGuildUpdateAsync(AuditLogAction auditLogAction)
    {
        this._guild ??= await this._client.Cache.TryGetGuildAsync(this._guildId);
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new CachedEntity<ulong, DiscordGuild>(_guildId, _guild);
        
        
        DiscordAuditLogGuildEntry entry = new() { Target = cachedGuild};

        ulong before, after;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = PropertyChange<string?>.From(change);
                    break;

                case "owner_id":
                    
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);
                    
                    DiscordMember? memberBefore = null;
                    CachedEntity<ulong, DiscordMember>? cachedMemberBefore = null;
                    if (change.OldValue is not null)
                    {
                        memberBefore = await this._client.Cache.TryGetMemberAsync(before, this._guildId);
                        if (memberBefore is null)
                        {
                            this._guild?._members.TryGetValue(before, out memberBefore);
                        }

                        cachedMemberBefore = new CachedEntity<ulong, DiscordMember>(before, memberBefore);
                    }
                    
                    DiscordMember? memberAfter = null;
                    CachedEntity<ulong, DiscordMember>? cachedMemberAfter = null;
                    if (change.NewValue is not null)
                    {
                        memberAfter = await this._client.Cache.TryGetMemberAsync(after, this._guildId);
                        if (memberAfter is null)
                        {
                            this._guild?._members.TryGetValue(after, out memberAfter);
                        }
                        
                        cachedMemberAfter = new CachedEntity<ulong, DiscordMember>(after, memberAfter);
                    }
                    
                    entry.OwnerChange = new PropertyChange<CachedEntity<ulong, DiscordMember>?>
                    {
                        Before = cachedMemberBefore,
                        After = cachedMemberAfter
                    };
                    break;

                case "icon_hash":
                    entry.IconChange = new PropertyChange<string?>
                    {
                        Before = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{this._guildId}/{change.OldValueString}.webp"
                            : null,
                        After = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{this._guildId}/{change.NewValueString}.webp"
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
                    
                    DiscordChannel? channelBefore = null;
                    CachedEntity<ulong, DiscordChannel>? cachedChannelBefore = null;
                    if (change.OldValue is not null)
                    {
                        channelBefore = await this._client.Cache.TryGetChannelAsync(before);
                        if (channelBefore is null)
                        {
                            this._guild?.Channels.TryGetValue(before, out channelBefore);
                        }
                        
                        cachedChannelBefore = new CachedEntity<ulong, DiscordChannel>(before, channelBefore);
                    }
                    
                    DiscordChannel? channelAfter = null;
                    CachedEntity<ulong, DiscordChannel>? cachedChannelAfter = null;
                    if (change.NewValue is not null)
                    {
                        channelAfter = await this._client.Cache.TryGetChannelAsync(after);
                        if (channelAfter is null)
                        {
                            this._guild?.Channels.TryGetValue(after, out channelAfter);
                        }
                        
                        cachedChannelAfter = new CachedEntity<ulong, DiscordChannel>(after, channelAfter);
                    }
                    
                    entry.AfkChannelChange = new PropertyChange<CachedEntity<ulong, DiscordChannel>?>
                    {
                        Before = cachedChannelBefore,
                        After = cachedChannelAfter
                    };
                    break;

                case "widget_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);
                    
                    channelBefore = null;
                    cachedChannelBefore = null;
                    if (change.OldValue is not null)
                    {
                        channelBefore = await this._client.Cache.TryGetChannelAsync(before);
                        if (channelBefore is null)
                        {
                            this._guild?.Channels.TryGetValue(before, out channelBefore);
                        }
                        
                        cachedChannelBefore = new CachedEntity<ulong, DiscordChannel>(before, channelBefore);
                    }
                    
                    channelAfter = null;
                    cachedChannelAfter = null;
                    if (change.NewValue is not null)
                    {
                        channelAfter = await this._client.Cache.TryGetChannelAsync(after);
                        if (channelAfter is null)
                        {
                            this._guild?.Channels.TryGetValue(after, out channelAfter);
                        }
                        
                        cachedChannelAfter = new CachedEntity<ulong, DiscordChannel>(after, channelAfter);
                    }
                    
                    entry.EmbedChannelChange = new PropertyChange<CachedEntity<ulong, DiscordChannel?>?>
                    {
                        Before = cachedChannelBefore,
                        After = cachedChannelAfter
                    };
                    break;

                case "splash_hash":
                    entry.SplashChange = new PropertyChange<string?>
                    {
                        Before = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{this._guildId}/{change.OldValueString}.webp?size=2048"
                            : null,
                        After = change.NewValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{this._guildId}/{change.NewValueString}.webp?size=2048"
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
                    
                    channelBefore = null;
                    cachedChannelBefore = null;
                    if (change.OldValue is not null)
                    {
                        channelBefore = await this._client.Cache.TryGetChannelAsync(before);
                        if (channelBefore is null)
                        {
                            this._guild?.Channels.TryGetValue(before, out channelBefore);
                        }
                        
                        cachedChannelBefore = new CachedEntity<ulong, DiscordChannel>(before, channelBefore);
                    }
                    
                    channelAfter = null;
                    cachedChannelAfter = null;
                    if (change.NewValue is not null)
                    {
                        channelAfter = await this._client.Cache.TryGetChannelAsync(after);
                        if (channelAfter is null)
                        {
                            this._guild?.Channels.TryGetValue(after, out channelAfter);
                        }
                        
                        cachedChannelAfter = new CachedEntity<ulong, DiscordChannel>(after, channelAfter);
                    }

                    entry.SystemChannelChange = new PropertyChange<CachedEntity<ulong, DiscordChannel?>?>
                    {
                        Before = cachedChannelBefore,
                        After = cachedChannelAfter
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in guild update: {Key}",
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
    internal async Task<DiscordAuditLogChannelEntry> ParseChannelEntryAsync(AuditLogAction auditLogAction)
    {
        ulong channelId = auditLogAction.TargetId.Value;
        DiscordChannel? channel = await this._client.Cache.TryGetChannelAsync(channelId);
        CachedEntity<ulong, DiscordChannel> cachedChannel = new CachedEntity<ulong, DiscordChannel>(channelId, channel);
        
        DiscordAuditLogChannelEntry entry = new()
        {
            Target = cachedChannel
        };

        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    entry.NameChange = PropertyChange<string?>.From(change);
                    break;

                case "type":
                    entry.TypeChange = PropertyChange<ChannelType?>.From(change);
                    break;

                case "permission_overwrites":

                    IEnumerable<DiscordOverwrite>? olds = change.OldValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordOverwrite>())?
                        .Select(overwrite =>
                        {
                            overwrite.Discord = this._client;
                            return overwrite;
                        });

                    IEnumerable<DiscordOverwrite>? news = change.NewValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordOverwrite>())?
                        .Select(overwrite =>
                        {
                            overwrite.Discord = this._client;
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
                            forumTag.Discord = this._client;
                            return forumTag;
                        });

                    IEnumerable<DiscordForumTag>? oldTags = change.OldValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordForumTag>())?
                        .Select(forumTag =>
                        {
                            forumTag.Discord = this._client;
                            return forumTag;
                        });

                    entry.AvailableTags = PropertyChange<IEnumerable<DiscordForumTag>>.From(oldTags, newTags);

                    break;

                default:
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in channel update: {Key}",
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
    internal async Task<DiscordAuditLogOverwriteEntry> ParseOverwriteEntry(AuditLogAction auditLogAction)
    {
        ulong overwriteId = auditLogAction.Options.Id;
        ulong channelId = auditLogAction.TargetId.Value;
        
        this._guild ??= await this._client.Cache.TryGetGuildAsync(this._guildId);
        
        DiscordChannel? channel = await this._client.Cache.TryGetChannelAsync(channelId);
        if (channel is null)
        {
            this._guild?.Channels.TryGetValue(channelId, out channel);
        }
        
        DiscordOverwrite? overwrite = channel?
            .PermissionOverwrites
            .FirstOrDefault(xo => xo.Id == overwriteId);
        
        if (overwrite is null)
        {
            overwrite = this._guild?.GetChannel(channelId)?._permissionOverwrites.FirstOrDefault(xo => xo.Id == overwriteId);;
        }
        
        CachedEntity<ulong, DiscordOverwrite> cachedOverwrite = new CachedEntity<ulong, DiscordOverwrite>(overwriteId, overwrite);
        CachedEntity<ulong, DiscordChannel> cachedChannel = new CachedEntity<ulong, DiscordChannel>(channelId, channel);
        
        
        DiscordAuditLogOverwriteEntry entry = new()
        {
            Target = cachedOverwrite,
            Channel = cachedChannel
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in overwrite update: {Key}",
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
    internal async Task<DiscordAuditLogMemberUpdateEntry> ParseMemberUpdateEntry(AuditLogAction auditLogAction)
    {
        ulong memberId = auditLogAction.TargetId.Value;
        DiscordMember? member = await this._client.Cache.TryGetMemberAsync(memberId, this._guildId);
        if (member is null)
        {
            this._guild?._members.TryGetValue(memberId, out member);
        }
        CachedEntity<ulong, DiscordMember> cachedMember = new (memberId, member);
        
        DiscordAuditLogMemberUpdateEntry entry = new()
        {
            Target = cachedMember
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
                    List<CachedEntity<ulong, DiscordRole>> addedRoles =
                        change.NewValues?
                            .Select(xo => (ulong)xo["id"])
                            .Select(x => new CachedEntity<ulong, DiscordRole>(x, this._guild?.GetRole(x)))
                            .ToList();
                    
                    entry.AddedRoles =
                        new ReadOnlyCollection<CachedEntity<ulong, DiscordRole>>(addedRoles);
                    break;

                case "$remove":
                    List<CachedEntity<ulong, DiscordRole>> removedRoles =
                        change.NewValues?
                            .Select(xo => (ulong)xo["id"])
                            .Select(x => new CachedEntity<ulong, DiscordRole>(x, this._guild?.GetRole(x)))
                            .ToList();
                    
                    entry.RemovedRoles =
                        new ReadOnlyCollection<CachedEntity<ulong, DiscordRole>>(removedRoles);
                    break;

                default:
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in member update: {Key}",
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
    internal DiscordAuditLogRoleUpdateEntry ParseRoleUpdateEntry(AuditLogAction auditLogAction)
    {
        ulong roleId = auditLogAction.TargetId.Value;
        DiscordRole? discordRole = this._guild?.GetRole(roleId);
        
        DiscordAuditLogRoleUpdateEntry entry = new()
        {
            Target = new(roleId, discordRole)
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in role update: {Key}",
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
    internal async Task<DiscordAuditLogInviteEntry> ParseInviteUpdateEntry(AuditLogAction auditLogAction)
    {
        DiscordAuditLogInviteEntry entry = new();

        DiscordInvite invite = new()
        {
            Discord = this._client,
            Guild = new DiscordInviteGuild
            {
                Discord = this._client,
                Id = this._guildId,
                Name = this._guild?.Name,
                SplashHash = _guild?.SplashHash
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
                        Before = change.OldValue is not null ? (bool?)change.OldValue : null,
                        After = change.NewValue is not null ? (bool?)change.NewValue : null
                    };
                    break;

                case "inviter_id":
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongAfter);
                    
                    DiscordUser? memberBefore = null;
                    CachedEntity<ulong, DiscordUser>? cachedMemberBefore = null;
                    if (change.OldValue is not null)
                    {
                        memberBefore = await this._client.Cache.TryGetMemberAsync(ulongBefore, this._guildId);
                        if (memberBefore is null)
                        {
                            if (this._guild?._members.TryGetValue(ulongBefore, out var guildMember) ?? false)
                            {
                                memberBefore = guildMember;
                            }
                        }
                        if (memberBefore is null)
                        {
                            memberBefore = await this._client.Cache.TryGetUserAsync(ulongBefore);
                        }
                        cachedMemberBefore = new CachedEntity<ulong, DiscordUser>(ulongBefore, memberBefore);
                    }
                    
                    DiscordUser? memberAfter = null;
                    CachedEntity<ulong, DiscordUser>? cachedMemberAfter = null;
                    if (change.NewValue is not null)
                    {
                        memberAfter = await this._client.Cache.TryGetMemberAsync(ulongAfter, this._guildId);
                        if (memberAfter is null)
                        {
                            if (this._guild?._members.TryGetValue(ulongAfter, out var guildMember) ?? false)
                            {
                                memberAfter = guildMember;
                            }
                        }
                        if (memberAfter is null)
                        {
                            memberAfter = await this._client.Cache.TryGetUserAsync(ulongAfter);
                        }
                        cachedMemberAfter = new CachedEntity<ulong, DiscordUser>(ulongAfter, memberAfter);
                    }

                    entry.InviterChange = new PropertyChange<CachedEntity<ulong, DiscordUser>?>
                    {
                        Before = cachedMemberBefore,
                        After = cachedMemberAfter
                    };
                    break;

                case "channel_id":
                    boolBefore = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    boolAfter = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongAfter);
                    
                    DiscordChannel? channelBefore = null;
                    CachedEntity<ulong, DiscordChannel>? cachedChannelBefore = null;
                    if (change.OldValue is not null)
                    {
                        channelBefore = await this._client.Cache.TryGetChannelAsync(ulongBefore);
                        if (channelBefore is null)
                        {
                            this._guild?.Channels.TryGetValue(ulongBefore, out channelBefore);
                        }
                        cachedChannelBefore = new CachedEntity<ulong, DiscordChannel>(ulongBefore, channelBefore);
                    }
                    
                    DiscordChannel? channelAfter = null;
                    CachedEntity<ulong, DiscordChannel>? cachedChannelAfter = null;
                    if (change.NewValue is not null)
                    {
                        channelAfter = await this._client.Cache.TryGetChannelAsync(ulongAfter);
                        if (channelAfter is null)
                        {
                            this._guild?.Channels.TryGetValue(ulongAfter, out channelAfter);
                        }
                        cachedChannelAfter = new CachedEntity<ulong, DiscordChannel>(ulongAfter, channelAfter);
                    }

                    entry.ChannelChange = new PropertyChange<CachedEntity<ulong, DiscordChannel>?>
                    {
                        Before = cachedChannelBefore,
                        After = cachedChannelAfter
                    };

                    DiscordChannel? channel = null;
                    ulong? channelId = null;
                    entry.ChannelChange?.Before?.TryGetCachedValue(out channel);
                    channelId = channel?.Id;
                    if (channel is null)
                    {
                        entry.ChannelChange?.After?.TryGetCachedValue(out channel);
                        channelId = channel?.Id;
                    }
                    
                    ChannelType? channelType = channel?.Type;
                    invite.Channel = new DiscordInviteChannel
                    {
                        Discord = this._client,
                        Id = channel?.Id ?? 0,
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in invite update: {Key}",
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
    internal async Task<DiscordAuditLogWebhookEntry> ParseWebhookUpdateEntry
    (
        AuditLogAction auditLogAction,
        IDictionary<ulong, DiscordWebhook> webhooks
    )
    {
        DiscordAuditLogWebhookEntry entry = new()
        {
            Target = webhooks.TryGetValue(auditLogAction.TargetId.Value, out DiscordWebhook? webhook)
                ? webhook
                : new DiscordWebhook { Id = auditLogAction.TargetId.Value, Discord = this._client }
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
                    
                    DiscordChannel? channelBefore = null;
                    CachedEntity<ulong, DiscordChannel>? cachedChannelBefore = null;
                    if (change.OldValue is not null)
                    {
                        channelBefore = await this._client.Cache.TryGetChannelAsync(ulongBefore);
                        cachedChannelBefore = new CachedEntity<ulong, DiscordChannel>(ulongBefore, channelBefore);
                    }
                    
                    DiscordChannel? channelAfter = null;
                    CachedEntity<ulong, DiscordChannel>? cachedChannelAfter = null;
                    if (change.NewValue is not null)
                    {
                        channelAfter = await this._client.Cache.TryGetChannelAsync(ulongAfter);
                        cachedChannelAfter = new CachedEntity<ulong, DiscordChannel>(ulongAfter, channelAfter);
                    }

                    entry.ChannelChange = new PropertyChange<CachedEntity<ulong,DiscordChannel>?>
                    {
                        Before = cachedChannelBefore,
                        After = cachedChannelAfter
                    };
                    break;

                case "type":
                    entry.TypeChange = PropertyChange<int?>.From(change);
                    break;

                case "avatar_hash":
                    entry.AvatarHashChange = PropertyChange<string>.From(change);
                    break;

                case "application_id":
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);
                    
                    ulong? applicationIdBefore = null;
                    if (change.OldValue is not null)
                    {
                        applicationIdBefore = ulongBefore;
                    }
                    
                    ulong? applicationIdAfter = null;
                    if (change.NewValue is not null)
                    {
                        applicationIdAfter = ulongAfter;
                    }
                    
                    entry.ApplicationIdChange = PropertyChange<ulong?>.From(applicationIdBefore, applicationIdAfter);
                    break;

                default:
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in webhook update: {Key}",
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
    internal DiscordAuditLogStickerEntry ParseStickerUpdateEntry(AuditLogAction auditLogAction)
    {
        ulong targetId = auditLogAction.TargetId.Value;
        DiscordMessageSticker? sticker = this._guild?._stickers.TryGetValue(targetId, out sticker) ?? false ? sticker : null;
        DiscordAuditLogStickerEntry entry = new()
        {
            Target = new CachedEntity<ulong, DiscordMessageSticker>(targetId, sticker)
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in sticker update: {Key}",
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
    internal DiscordAuditLogIntegrationEntry ParseIntegrationUpdateEntry(AuditLogAction auditLogAction)
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
                    if (this._client.Configuration.LogUnknownAuditlogs)
                    {
                        this._client.Logger.LogWarning(LoggerEvents.AuditLog,
                            "Unknown key in integration update: {Key}",
                            change.Key);
                    }
                    break;
            }
        }

        return entry;
    }
}
