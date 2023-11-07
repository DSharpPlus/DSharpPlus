using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Caching;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DSharpPlus;

public sealed partial class DiscordClient
{
    #region Private Fields

    private string _sessionId;
    private bool _guildDownloadCompleted = false;
    private List<ulong> _readyGuilds = new();
    private int _readyGuildCount = 0;

    #endregion

    #region Dispatch Handler

    internal async Task HandleDispatchAsync(GatewayPayload payload)
    {
        if (payload.Data is not JObject dat)
        {
            this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Invalid payload body (this message is probably safe to ignore); opcode: {Op} event: {Event}; payload: {Payload}", payload.OpCode, payload.EventName, payload.Data);
            return;
        }

        DiscordChannel chn;
        DiscordThreadChannel thread;
        ulong gid;
        ulong cid;
        TransportUser usr = default;
        TransportMember mbr = default;
        TransportUser refUsr = default;
        TransportMember refMbr = default;
        JToken rawMbr = default;
        JToken? rawRefMsg = dat["referenced_message"];
        JArray rawMembers = default;
        JArray rawPresences = default;

        switch (payload.EventName.ToLowerInvariant())
        {
            #region Gateway Status

            case "ready":
                JArray? glds = (JArray)dat["guilds"];
                JArray? dmcs = (JArray)dat["private_channels"];

                dat.Remove("guilds");
                dat.Remove("private_channels");
                
                await this.OnReadyEventAsync(dat.ToDiscordObject<ReadyPayload>(), glds, dmcs);
                break;

            case "resumed":
                await this.OnResumedAsync();
                break;

            #endregion

            #region Channel

            case "channel_create":
                chn = dat.ToDiscordObject<DiscordChannel>();
                await this.OnChannelCreateEventAsync(chn);
                break;

            case "channel_update":
                await this.OnChannelUpdateEventAsync(dat.ToDiscordObject<DiscordChannel>());
                break;

            case "channel_delete":
                bool isPrivate = dat["is_private"]?.ToObject<bool>() ?? false;

                chn = isPrivate ? dat.ToDiscordObject<DiscordDmChannel>() : dat.ToDiscordObject<DiscordChannel>();
                await this.OnChannelDeleteEventAsync(chn);
                break;

            case "channel_pins_update":
                cid = (ulong)dat["channel_id"];
                string? ts = (string)dat["last_pin_timestamp"];
                await this.OnChannelPinsUpdateAsync((ulong?)dat["guild_id"], cid, ts != null ? DateTimeOffset.Parse(ts, CultureInfo.InvariantCulture) : default(DateTimeOffset?));
                break;

            #endregion

            #region Scheduled Guild Events

            case "guild_scheduled_event_create":
                DiscordScheduledGuildEvent cevt = dat.ToDiscordObject<DiscordScheduledGuildEvent>();
                await this.OnScheduledGuildEventCreateEventAsync(cevt);
                break;
            case "guild_scheduled_event_delete":
                DiscordScheduledGuildEvent devt = dat.ToDiscordObject<DiscordScheduledGuildEvent>();
                await this.OnScheduledGuildEventDeleteEventAsync(devt);
                break;
            case "guild_scheduled_event_update":
                DiscordScheduledGuildEvent uevt = dat.ToDiscordObject<DiscordScheduledGuildEvent>();
                await this.OnScheduledGuildEventUpdateEventAsync(uevt);
                break;
            case "guild_scheduled_event_user_add":
                gid = (ulong)dat["guild_id"];
                ulong uid = (ulong)dat["user_id"];
                ulong eid = (ulong)dat["guild_scheduled_event_id"];
                await this.OnScheduledGuildEventUserAddEventAsync(gid, eid, uid);
                break;
            case "guild_scheduled_event_user_remove":
                gid = (ulong)dat["guild_id"];
                uid = (ulong)dat["user_id"];
                eid = (ulong)dat["guild_scheduled_event_id"];
                await this.OnScheduledGuildEventUserRemoveEventAsync(gid, eid, uid);
                break;
            #endregion

            #region Guild

            case "guild_create":

                rawMembers = (JArray)dat["members"];
                rawPresences = (JArray)dat["presences"];
                dat.Remove("members");
                dat.Remove("presences");

                await this.OnGuildCreateEventAsync(dat.ToDiscordObject<DiscordGuild>(), rawMembers, rawPresences.ToDiscordObject<IEnumerable<DiscordPresence>>());
                break;

            case "guild_update":

                rawMembers = (JArray)dat["members"];
                dat.Remove("members");

                await this.OnGuildUpdateEventAsync(dat.ToDiscordObject<DiscordGuild>(), rawMembers);
                break;

            case "guild_delete":

                rawMembers = (JArray)dat["members"];
                dat.Remove("members");

                await this.OnGuildDeleteEventAsync(dat.ToDiscordObject<DiscordGuild>(), rawMembers);
                break;

            case "guild_emojis_update":
                gid = (ulong)dat["guild_id"];
                IEnumerable<DiscordEmoji> ems = dat["emojis"].ToDiscordObject<IEnumerable<DiscordEmoji>>();
                await this.OnGuildEmojisUpdateEventAsync(gid, ems);
                break;

            case "guild_integrations_update":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildIntegrationsUpdateEventAsync(gid);
                break;

            //TODO: Fix this event to use the new caching system
            case "guild_audit_log_entry_create":
                gid = (ulong)dat["guild_id"];
                AuditLogAction auditLogAction = dat.ToDiscordObject<AuditLogAction>();
                await this.OnGuildAuditLogEntryCreateEventAsync(gid, auditLogAction);
                break;

            #endregion

            #region Guild Ban

            case "guild_ban_add":
                usr = dat["user"].ToDiscordObject<TransportUser>();
                gid = (ulong)dat["guild_id"];
                await this.OnGuildBanAddEventAsync(usr, gid);
                break;

            case "guild_ban_remove":
                usr = dat["user"].ToDiscordObject<TransportUser>();
                gid = (ulong)dat["guild_id"];
                await this.OnGuildBanRemoveEventAsync(usr, gid);
                break;

            #endregion

            #region Guild Member

            case "guild_member_add":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildMemberAddEventAsync(dat.ToDiscordObject<TransportMember>(), gid);
                break;

            case "guild_member_remove":
                gid = (ulong)dat["guild_id"];
                usr = dat["user"].ToDiscordObject<TransportUser>();
                
                await this.OnGuildMemberRemoveEventAsync(usr, gid);
                break;

            case "guild_member_update":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildMemberUpdateEventAsync(dat.ToDiscordObject<TransportMember>(), gid);
                break;

            case "guild_members_chunk":
                await this.OnGuildMembersChunkEventAsync(dat);
                break;

            #endregion

            #region Guild Role

            case "guild_role_create":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildRoleCreateEventAsync(dat["role"].ToDiscordObject<DiscordRole>(), gid);
                break;

            case "guild_role_update":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildRoleUpdateEventAsync(dat["role"].ToDiscordObject<DiscordRole>(), gid);
                break;

            case "guild_role_delete":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], gid);
                break;

            #endregion

            #region Invite

            case "invite_create":
                gid = (ulong)dat["guild_id"];
                cid = (ulong)dat["channel_id"];
                await this.OnInviteCreateEventAsync(cid, gid, dat.ToDiscordObject<DiscordInvite>());
                break;

            case "invite_delete":
                gid = (ulong)dat["guild_id"];
                cid = (ulong)dat["channel_id"];
                await this.OnInviteDeleteEventAsync(cid, gid, dat);
                break;

            #endregion

            #region Message

            case "message_create":
                rawMbr = dat["member"];

                if (rawMbr != null)
                {
                    mbr = rawMbr.ToDiscordObject<TransportMember>();
                }

                if (rawRefMsg != null && rawRefMsg.HasValues)
                {
                    if (rawRefMsg.SelectToken("author") != null)
                    {
                        refUsr = rawRefMsg.SelectToken("author").ToDiscordObject<TransportUser>();
                    }

                    if (rawRefMsg.SelectToken("member") != null)
                    {
                        refMbr = rawRefMsg.SelectToken("member").ToDiscordObject<TransportMember>();
                    }
                }

                TransportUser author = dat["author"].ToDiscordObject<TransportUser>();
                dat.Remove("author");
                dat.Remove("member");

                await this.OnMessageCreateEventAsync(dat.ToDiscordObject<DiscordMessage>(), author, mbr, refUsr, refMbr);
                break;

            case "message_update":
                rawMbr = dat["member"];

                if (rawMbr != null)
                {
                    mbr = rawMbr.ToDiscordObject<TransportMember>();
                }

                if (rawRefMsg != null && rawRefMsg.HasValues)
                {
                    if (rawRefMsg.SelectToken("author") != null)
                    {
                        refUsr = rawRefMsg.SelectToken("author").ToDiscordObject<TransportUser>();
                    }

                    if (rawRefMsg.SelectToken("member") != null)
                    {
                        refMbr = rawRefMsg.SelectToken("member").ToDiscordObject<TransportMember>();
                    }
                }

                await this.OnMessageUpdateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"]?.ToDiscordObject<TransportUser>(), mbr, refUsr, refMbr);
                break;

            // delete event does *not* include message object
            case "message_delete":
                await this.OnMessageDeleteEventAsync((ulong)dat["id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]);
                break;

            case "message_delete_bulk":
                await this.OnMessageBulkDeleteEventAsync(dat["ids"].ToDiscordObject<ulong[]>(), (ulong)dat["channel_id"], (ulong?)dat["guild_id"]);
                break;

            #endregion

            #region Message Reaction

            case "message_reaction_add":
                rawMbr = dat["member"];

                if (rawMbr != null)
                {
                    mbr = rawMbr.ToDiscordObject<TransportMember>();
                }

                await this.OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], mbr, dat["emoji"].ToDiscordObject<DiscordEmoji>());
                break;

            case "message_reaction_remove":
                await this.OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], dat["emoji"].ToDiscordObject<DiscordEmoji>());
                break;

            case "message_reaction_remove_all":
                await this.OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]);
                break;

            case "message_reaction_remove_emoji":
                await this.OnMessageReactionRemoveEmojiAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong)dat["guild_id"], dat["emoji"]);
                break;

            #endregion

            #region User/Presence Update

            case "presence_update":
                // Presences are a mess. I'm not touching this. ~Velvet
                await this.OnPresenceUpdateEventAsync(dat, (JObject)dat["user"]);
                break;

            case "user_settings_update":
                await this.OnUserSettingsUpdateEventAsync(dat.ToDiscordObject<TransportUser>());
                break;

            case "user_update":
                await this.OnUserUpdateEventAsync(dat.ToDiscordObject<TransportUser>());
                break;

            #endregion

            #region Voice

            case "voice_state_update":
                await this.OnVoiceStateUpdateEventAsync(dat);
                break;

            case "voice_server_update":
                gid = (ulong)dat["guild_id"];
                await this.OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], gid);
                break;

            #endregion

            #region Thread

            case "thread_create":
                thread = dat.ToDiscordObject<DiscordThreadChannel>();
                await this.OnThreadCreateEventAsync(thread, thread.IsNew);
                break;

            case "thread_update":
                thread = dat.ToDiscordObject<DiscordThreadChannel>();
                await this.OnThreadUpdateEventAsync(thread);
                break;

            case "thread_delete":
                thread = dat.ToDiscordObject<DiscordThreadChannel>();
                await this.OnThreadDeleteEventAsync(thread);
                break;

            case "thread_list_sync":
                gid = (ulong)dat["guild_id"]; //get guild
                await this.OnThreadListSyncEventAsync(gid, dat["channel_ids"].ToDiscordObject<IReadOnlyList<ulong>>(), dat["threads"].ToDiscordObject<IReadOnlyList<DiscordThreadChannel>>(), dat["members"].ToDiscordObject<IReadOnlyList<DiscordThreadChannelMember>>());
                break;

            case "thread_member_update":
                await this.OnThreadMemberUpdateEventAsync(dat.ToDiscordObject<DiscordThreadChannelMember>());
                break;

            case "thread_members_update":
                gid = (ulong)dat["guild_id"];
                await this.OnThreadMembersUpdateEventAsync(gid, (ulong)dat["id"], dat["added_members"]?.ToDiscordObject<IReadOnlyList<DiscordThreadChannelMember>>(), dat["removed_member_ids"]?.ToDiscordObject<IReadOnlyList<ulong?>>(), (int)dat["member_count"]);
                break;

            #endregion

            #region Interaction/Integration/Application

            case "interaction_create":

                rawMbr = dat["member"];

                if (rawMbr != null)
                {
                    mbr = dat["member"].ToDiscordObject<TransportMember>();
                    usr = mbr.User;
                }
                else
                {
                    usr = dat["user"].ToDiscordObject<TransportUser>();
                }

                // Re: Removing re-serialized data: This one is probably fine?
                // The user on the object is marked with [JsonIgnore].

                cid = (ulong)dat["channel_id"];
                await this.OnInteractionCreateAsync((ulong?)dat["guild_id"], cid, usr, mbr, dat.ToDiscordObject<DiscordInteraction>());
                break;


            case "integration_create":
                await this.OnIntegrationCreateAsync(dat.ToDiscordObject<DiscordIntegration>(), (ulong)dat["guild_id"]);
                break;

            case "integration_update":
                await this.OnIntegrationUpdateAsync(dat.ToDiscordObject<DiscordIntegration>(), (ulong)dat["guild_id"]);
                break;

            case "integration_delete":
                await this.OnIntegrationDeleteAsync((ulong)dat["id"], (ulong)dat["guild_id"], (ulong?)dat["application_id"]);
                break;

            case "application_command_permissions_update":
                await this.OnApplicationCommandPermissionsUpdateAsync(dat);
                break;
            #endregion

            #region Stage Instance

            case "stage_instance_create":
                await this.OnStageInstanceCreateAsync(dat.ToDiscordObject<DiscordStageInstance>());
                break;

            case "stage_instance_update":
                await this.OnStageInstanceUpdateAsync(dat.ToDiscordObject<DiscordStageInstance>());
                break;

            case "stage_instance_delete":
                await this.OnStageInstanceDeleteAsync(dat.ToDiscordObject<DiscordStageInstance>());
                break;

            #endregion

            #region Misc

            case "gift_code_update": //Not supposed to be dispatched to bots
                break;

            case "embedded_activity_update": //Not supposed to be dispatched to bots
                break;

            case "typing_start":
                cid = (ulong)dat["channel_id"];
                rawMbr = dat["member"];

                if (rawMbr != null)
                {
                    mbr = rawMbr.ToDiscordObject<TransportMember>();
                }

                await this.OnTypingStartEventAsync((ulong)dat["user_id"], cid, (ulong?)dat["guild_id"], Utilities.GetDateTimeOffset((long)dat["timestamp"]), mbr);
                break;

            case "webhooks_update":
                gid = (ulong)dat["guild_id"];
                cid = (ulong)dat["channel_id"];
                await this.OnWebhooksUpdateAsync(cid, gid);
                break;

            case "guild_stickers_update":
                IEnumerable<DiscordMessageSticker> strs = dat["stickers"].ToDiscordObject<IEnumerable<DiscordMessageSticker>>();
                await this.OnStickersUpdatedAsync(strs, dat);
                break;

            default:
                await this.OnUnknownEventAsync(payload);
                if (this.Configuration.LogUnknownEvents)
                {
                    this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown event: {EventName}\npayload: {@Payload}", payload.EventName, payload.Data);
                }

                break;

            #endregion

            #region AutoModeration
            case "auto_moderation_rule_create":
                await this.OnAutoModerationRuleCreateAsync(dat.ToDiscordObject<DiscordAutoModerationRule>());
                break;

            case "auto_moderation_rule_update":
                await this.OnAutoModerationRuleUpdatedAsync(dat.ToDiscordObject<DiscordAutoModerationRule>());
                break;

            case "auto_moderation_rule_delete":
                await this.OnAutoModerationRuleDeletedAsync(dat.ToDiscordObject<DiscordAutoModerationRule>());
                break;

            case "auto_moderation_action_execution":
                await this.OnAutoModerationRuleExecutedAsync(dat.ToDiscordObject<DiscordAutoModerationActionExecution>());
                break;
                #endregion
        }
    }

    #endregion

    #region Events

    #region Gateway

    internal async Task OnReadyEventAsync(ReadyPayload ready, JArray rawGuilds, JArray rawDmChannels)
    {
        TransportUser rawCurrentUser = ready.CurrentUser;
        this.CurrentUser.Username = rawCurrentUser.Username;
        this.CurrentUser.Discriminator = rawCurrentUser.Discriminator;
        this.CurrentUser.AvatarHash = rawCurrentUser.AvatarHash;
        this.CurrentUser.MfaEnabled = rawCurrentUser.MfaEnabled;
        this.CurrentUser.Verified = rawCurrentUser.Verified;
        this.CurrentUser.IsBot = rawCurrentUser.IsBot;

        this.GatewayVersion = ready.GatewayVersion;
        this._sessionId = ready.SessionId;
        Dictionary<ulong, JObject> rawGuildIndex = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);
        this._readyGuilds = rawGuildIndex.Keys.ToList();
        this._readyGuildCount = rawGuildIndex.Count;
        
        foreach (JToken rawChannel in rawDmChannels)
        {
            DiscordDmChannel channel = rawChannel.ToDiscordObject<DiscordDmChannel>();
            channel.Discord = this;

            IEnumerable<TransportUser> rawRecipients = rawChannel["recipients"].ToDiscordObject<IEnumerable<TransportUser>>();
            List<DiscordUser> recipients = new();
            foreach (TransportUser rawRecipient in rawRecipients)
            {
                DiscordUser recipientDiscordUser = new(rawRecipient) { Discord = this };
                await this.Cache.AddUserAsync(recipientDiscordUser);

                recipients.Add(recipientDiscordUser);
            }
            channel.Recipients = recipients;

            await this.Cache.AddChannelAsync(channel);
        }

        this._guilds.Clear();

        IEnumerable<DiscordGuild> guilds = rawGuilds.ToDiscordObject<IEnumerable<DiscordGuild>>();
        foreach (DiscordGuild guild in guilds)
        {
            this._guilds.Add(guild.Id);
            guild.Discord = this;
            guild._channels ??= new ConcurrentDictionary<ulong, DiscordChannel>();
            guild._threads ??= new ConcurrentDictionary<ulong, DiscordThreadChannel>();

            foreach (DiscordChannel channel in guild.Channels.Values)
            {
                channel.GuildId = guild.Id;
                channel.Discord = this;
                foreach (DiscordOverwrite xo in channel._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = channel.Id;
                }
            }
            foreach (DiscordThreadChannel threadChannel in guild.Threads.Values)
            {
                threadChannel.GuildId = guild.Id;
                threadChannel.Discord = this;
            }

            guild._roles ??= new ConcurrentDictionary<ulong, DiscordRole>();

            foreach (DiscordRole xr in guild.Roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            JObject rawGuild = rawGuildIndex[guild.Id];
            JArray? rawMembers = (JArray)rawGuild["members"];

            guild._members?.Clear();
            guild._members ??= new ConcurrentDictionary<ulong, DiscordMember>();

            if (rawMembers != null)
            {
                foreach (JToken rawMember in rawMembers)
                {
                    TransportMember transportMember = rawMember.ToDiscordObject<TransportMember>();

                    DiscordUser newUser = new(transportMember.User) { Discord = this };
                    await this.Cache.AddUserAsync(newUser);

                    guild._members[transportMember.User.Id] = new DiscordMember(transportMember) { Discord = this, _guild_id = guild.Id };
                }
            }

            guild._emojis ??= new ConcurrentDictionary<ulong, DiscordEmoji>();

            foreach (DiscordEmoji xe in guild.Emojis.Values)
            {
                xe.Discord = this;
            }

            guild._voiceStates ??= new ConcurrentDictionary<ulong, DiscordVoiceState>();

            foreach (DiscordVoiceState xvs in guild.VoiceStates.Values)
            {
                xvs.Discord = this;
            }

            await this.Cache.AddGuildAsync(guild);
            
        }

        await this._ready.InvokeAsync(this, new SessionReadyEventArgs());
    }

    internal Task OnResumedAsync()
    {
        this.Logger.LogInformation(LoggerEvents.SessionUpdate, "Session resumed");
        return this._resumed.InvokeAsync(this, new SessionReadyEventArgs());
    }

    #endregion

    #region Channel

    internal async Task OnChannelCreateEventAsync(DiscordChannel channel)
    {
        channel.Discord = this;
        foreach (DiscordOverwrite discordOverwrite in channel._permissionOverwrites)
        {
            discordOverwrite.Discord = this;
            discordOverwrite._channel_id = channel.Id;
        }

        CachedEntity<ulong, DiscordGuild>? cachedChannel = null;
        if (channel.GuildId.HasValue)
        {
            DiscordGuild? guild = await this.Cache.TryGetGuildAsync(channel.GuildId.Value);
            if (guild is not null)
            {
                guild._channels[channel.Id] = channel;
            }
            cachedChannel = new CachedEntity<ulong, DiscordGuild>(channel.Id, guild);
        }
        
        await this._channelCreated.InvokeAsync(this, new ChannelCreateEventArgs { Channel = channel, Guild = cachedChannel });
    }

    internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
    {
        if (channel is null)
        {
            return;
        }
        channel.Discord = this;
        
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(channel.GuildId.Value);
        if (guild is not null)
        {
            guild._channels[channel.Id] = channel;
        }

        DiscordChannel? newChannel = await this.Cache.TryGetChannelAsync(channel.Id);
        DiscordChannel? oldChannel = null;

        if (newChannel is not null)
        {
            oldChannel = new DiscordChannel
            {
                Bitrate = newChannel.Bitrate,
                Discord = this,
                GuildId = newChannel.GuildId,
                Id = newChannel.Id,
                LastMessageId = newChannel.LastMessageId,
                Name = newChannel.Name,
                _permissionOverwrites = new List<DiscordOverwrite>(newChannel._permissionOverwrites),
                Position = newChannel.Position,
                Topic = newChannel.Topic,
                Type = newChannel.Type,
                UserLimit = newChannel.UserLimit,
                ParentId = newChannel.ParentId,
                IsNSFW = newChannel.IsNSFW,
                PerUserRateLimit = newChannel.PerUserRateLimit,
                RtcRegionId = newChannel.RtcRegionId,
                QualityMode = newChannel.QualityMode
            };

            newChannel.Bitrate = channel.Bitrate;
            newChannel.Name = channel.Name;
            newChannel.Position = channel.Position;
            newChannel.Topic = channel.Topic;
            newChannel.UserLimit = channel.UserLimit;
            newChannel.ParentId = channel.ParentId;
            newChannel.IsNSFW = channel.IsNSFW;
            newChannel.PerUserRateLimit = channel.PerUserRateLimit;
            newChannel.Type = channel.Type;
            newChannel.RtcRegionId = channel.RtcRegionId;
            newChannel.QualityMode = channel.QualityMode;

            newChannel._permissionOverwrites.Clear();

            foreach (DiscordOverwrite overwrite in channel._permissionOverwrites)
            {
                overwrite.Discord = this;
                overwrite._channel_id = channel.Id;
            }

            newChannel._permissionOverwrites.AddRange(channel._permissionOverwrites);
        }
        else
        {
            newChannel = channel;
        }

        await this.Cache.AddChannelAsync(newChannel);

        await this._channelUpdated.InvokeAsync(this, new ChannelUpdateEventArgs { ChannelAfter = newChannel, Guild = guild, ChannelBefore = oldChannel });
    }
    
    internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
    {
        if (channel is null) //TODO why is this check here?
        {
            return;
        }
        channel.Discord = this;
        
        if (channel.Type is ChannelType.Group || channel.Type is ChannelType.Private)
        {
            DiscordDmChannel? dmChannel = channel as DiscordDmChannel;
            
            DiscordChannel? cachedChannel = await this.Cache.TryGetChannelAsync(channel.Id);
            if (cachedChannel is not null && cachedChannel is DiscordDmChannel cachedDmChannel)
            {
                dmChannel = cachedDmChannel;
            }

            await this.Cache.RemoveChannelAsync(channel.Id);
            await this._dmChannelDeleted.InvokeAsync(this, new DmChannelDeleteEventArgs { Channel = dmChannel });
        }
        else
        {
            DiscordGuild? guild = await this.Cache.TryGetGuildAsync(channel.GuildId.Value);

            if (guild is not null)
            {
                if (guild._channels.TryRemove(channel.Id, out DiscordChannel? cachedChannel))
                {
                    channel = cachedChannel;
                }
            }
            else
            {
                DiscordChannel? cachedChannel = await this.Cache.TryGetChannelAsync(channel.Id);
                if (cachedChannel is not null)
                {
                    channel = cachedChannel;
                }
            }
            
            await this.Cache.RemoveChannelAsync(channel.Id);
            await this._channelDeleted.InvokeAsync(this, new ChannelDeleteEventArgs { Channel = channel, Guild = guild });
        }
    }

    internal async Task OnChannelPinsUpdateAsync(ulong? guildId, ulong channelId, DateTimeOffset? lastPinTimestamp)
    {
        CachedEntity<ulong, DiscordGuild>? cachedGuild = null; 
        if (guildId.HasValue)
        {
            DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId.Value);
            cachedGuild = new CachedEntity<ulong, DiscordGuild>(guild.Id, guild);
        }
        
        DiscordChannel? channel = await this.Cache.TryGetChannelAsync(channelId);
        if (channel is null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = ChannelType.Private,
                Recipients = Array.Empty<DiscordUser>()
            };

            DiscordDmChannel chn = (DiscordDmChannel)channel;
            await this.Cache.AddChannelAsync(chn);
            //TODO track private channel ids on client
            //this._privateChannels[channelId] = chn;
        }
        CachedEntity<ulong, DiscordChannel> cachedChannel = new(channel.Id, channel);
            
        ChannelPinsUpdateEventArgs ea = new()
        {
            Guild = cachedGuild,
            Channel = cachedChannel,
            LastPinTimestamp = lastPinTimestamp
        };
        await this._channelPinsUpdated.InvokeAsync(this, ea);
    }

    #endregion

    #region Scheduled Guild Events

    private async Task OnScheduledGuildEventCreateEventAsync(DiscordScheduledGuildEvent evt)
    {
        evt.Discord = this;

        if (evt.Creator is not null)
        {
            evt.Creator.Discord = this;
            
            //Add user to cache if its not already there
            await this.Cache.AddIfNotPresentAsync(evt.Creator, evt.Creator.GetCacheKey());
        }
        
        //Add event to cached guild if guild is cached
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(evt.GuildId);
        if (guild is not null)
        {
            guild._scheduledEvents[evt.Id] = evt;
        }
        
        await this._scheduledGuildEventCreated.InvokeAsync(this, new ScheduledGuildEventCreateEventArgs { Event = evt });
    }

    private async Task OnScheduledGuildEventDeleteEventAsync(DiscordScheduledGuildEvent evt)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(evt.GuildId);
        if (guild is not null) 
        {
            guild._scheduledEvents.TryRemove(evt.Id, out _);
        }
        evt.Discord = this;

        if (evt.Creator is not null)
        {
            evt.Creator.Discord = this;
            await this.Cache.AddIfNotPresentAsync(evt.Creator, evt.Creator.GetCacheKey());
        }

        await this._scheduledGuildEventDeleted.InvokeAsync(this, new ScheduledGuildEventDeleteEventArgs { Event = evt });
    }

    private async Task OnScheduledGuildEventUpdateEventAsync(DiscordScheduledGuildEvent evt)
    {
        evt.Discord = this;

        if (evt.Creator is not null)
        {
            evt.Creator.Discord = this;
            await this.Cache.AddIfNotPresentAsync(evt.Creator, evt.Creator.GetCacheKey());
        }

        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(evt.GuildId);
        DiscordScheduledGuildEvent? oldEvt = null;
        if (guild is not null)
        {
            guild._scheduledEvents.TryGetValue(evt.GuildId, out oldEvt);
            guild._scheduledEvents[evt.Id] = evt;
        }

        if (evt.Status is ScheduledGuildEventStatus.Completed)
        {
            await this._scheduledGuildEventCompleted.InvokeAsync(this, new ScheduledGuildEventCompletedEventArgs() { Event = evt });
        }
        else
        {
            await this._scheduledGuildEventUpdated.InvokeAsync(this, new ScheduledGuildEventUpdateEventArgs() { EventBefore = oldEvt, EventAfter = evt });
        }
    }

    private async Task OnScheduledGuildEventUserAddEventAsync(ulong guildId, ulong eventId, ulong userId)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordScheduledGuildEvent? evt = guild?._scheduledEvents.GetOrAdd(eventId, new DiscordScheduledGuildEvent()
        {
            Id = eventId,
            GuildId = guildId,
            Guild = guild,
            Discord = this,
        });
 
        if (guild is not null && evt?.UserCount is not null)
        {
            evt.UserCount++;
        }
        
        DiscordUser? cachedUser = await this.Cache.TryGetUserAsync(userId);
        if (cachedUser is null)
        {
            if (guild is not null && guild.Members.TryGetValue(userId, out DiscordMember? mbr))
            {
                cachedUser = mbr;
            }
            else
            {
                cachedUser = new DiscordUser() { Id = userId, Discord = this };
                await this.Cache.AddUserAsync(cachedUser);
            }
        }

        DiscordUser user = cachedUser;

        await this._scheduledGuildEventUserAdded.InvokeAsync(this, new ScheduledGuildEventUserAddEventArgs() { Event = evt, User = user });
    }

    private async Task OnScheduledGuildEventUserRemoveEventAsync(ulong guildId, ulong eventId, ulong userId)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordScheduledGuildEvent? evt = guild?._scheduledEvents.GetOrAdd(eventId, new DiscordScheduledGuildEvent()
        {
            Id = eventId,
            GuildId = guildId,
            Guild = guild,
            Discord = this
        });

        if (guild is not null && evt?.UserCount is not null)
        {
            if (evt!.UserCount > 0)
            {
                evt.UserCount--;
            }
        }
        
        DiscordUser? cachedUser = await this.Cache.TryGetUserAsync(userId);
        if (cachedUser is null)
        {
            if (guild is not null && guild.Members.TryGetValue(userId, out DiscordMember? mbr))
            {
                cachedUser = mbr;
            }
            else
            {
                cachedUser = new DiscordUser() { Id = userId, Discord = this };
            }
        }

        DiscordUser user = cachedUser;
        await this._scheduledGuildEventUserRemoved.InvokeAsync(this, new ScheduledGuildEventUserRemoveEventArgs() { Event = evt, User = user });
    }

    #endregion

    #region Guild

    internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray rawMembers, IEnumerable<DiscordPresence> presences)
    {
        if (presences is not null)
        {
            foreach (DiscordPresence presence in presences)
            {
                presence.Discord = this;
                presence.GuildId = guild.Id;
                presence.Activity = new DiscordActivity(presence.RawActivity);
                if (presence.RawActivities is not null)
                {
                    presence._internalActivities = new DiscordActivity[presence.RawActivities.Length];
                    for (int i = 0; i < presence.RawActivities.Length; i++)
                    {
                        presence._internalActivities[i] = new DiscordActivity(presence.RawActivities[i]);
                    }
                }
                this._presences[presence.User.Id] = presence;
            }
        }

        
        DiscordGuild? foundGuild = await this.Cache.TryGetGuildAsync(guild.Id);
        bool exists = foundGuild is not null;
        
        
        guild.Discord = this;
        guild.IsUnavailable = false;
        DiscordGuild eventGuild = guild;

        if (foundGuild is not null)
        {
            guild = foundGuild;
        }

        guild._channels ??= new ConcurrentDictionary<ulong, DiscordChannel>();
        guild._threads ??= new ConcurrentDictionary<ulong, DiscordThreadChannel>();
        guild._roles ??= new ConcurrentDictionary<ulong, DiscordRole>();
        guild._emojis ??= new ConcurrentDictionary<ulong, DiscordEmoji>();
        guild._stickers ??= new ConcurrentDictionary<ulong, DiscordMessageSticker>();
        guild._voiceStates ??= new ConcurrentDictionary<ulong, DiscordVoiceState>();
        guild._members ??= new ConcurrentDictionary<ulong, DiscordMember>();
        guild._stageInstances ??= new ConcurrentDictionary<ulong, DiscordStageInstance>();
        guild._scheduledEvents ??= new ConcurrentDictionary<ulong, DiscordScheduledGuildEvent>();

        this.UpdateCachedGuildAsync(eventGuild, rawMembers);

        guild.JoinedAt = eventGuild.JoinedAt;
        guild.IsLarge = eventGuild.IsLarge;
        guild.MemberCount = Math.Max(eventGuild.MemberCount, guild._members.Count);
        guild.IsUnavailable = eventGuild.IsUnavailable;
        guild.PremiumSubscriptionCount = eventGuild.PremiumSubscriptionCount;
        guild.PremiumTier = eventGuild.PremiumTier;
        guild.Banner = eventGuild.Banner;
        guild.VanityUrlCode = eventGuild.VanityUrlCode;
        guild.Description = eventGuild.Description;
        guild.IsNSFW = eventGuild.IsNSFW;


        foreach (KeyValuePair<ulong, DiscordVoiceState> voicestate in eventGuild._voiceStates ??= new())
        {
            guild._voiceStates[voicestate.Key] = voicestate.Value;
        }

        foreach (DiscordScheduledGuildEvent discordEvent in guild._scheduledEvents.Values)
        {
            discordEvent.Discord = this;

            if (discordEvent.Creator is not null)
            {
                discordEvent.Creator.Discord = this;
            }
        }

        foreach (DiscordChannel channel in guild._channels.Values)
        {
            channel.GuildId = guild.Id;
            channel.Discord = this;
            foreach (DiscordOverwrite xo in channel._permissionOverwrites)
            {
                xo.Discord = this;
                xo._channel_id = channel.Id;
            }
        }
        foreach (DiscordThreadChannel threadChannel in guild._threads.Values)
        {
            threadChannel.GuildId = guild.Id;
            threadChannel.Discord = this;
        }
        foreach (DiscordEmoji emoji in guild._emojis.Values)
        {
            emoji.Discord = this;
        }

        foreach (DiscordMessageSticker sticker in guild._stickers.Values)
        {
            sticker.Discord = this;
        }

        foreach (DiscordVoiceState voiceState in guild._voiceStates.Values)
        {
            voiceState.Discord = this;
        }

        foreach (DiscordRole role in guild._roles.Values)
        {
            role.Discord = this;
            role._guild_id = guild.Id;
        }

        foreach (DiscordStageInstance instance in guild._stageInstances.Values)
        {
            instance.Discord = this;
        }

        bool guildDownloadCompleted = Volatile.Read(ref this._guildDownloadCompleted);
        
        if (this._readyGuilds.Contains(guild.Id) && this._readyGuildCount > 0)
        {
            this._readyGuildCount--;
        }

        bool guildDownloadComplete = this._readyGuildCount == 0;
        Volatile.Write(ref this._guildDownloadCompleted, guildDownloadComplete);

        if (exists)
        {
            await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild });
        }
        else
        {
            await this._guildCreated.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild });
        }

        if (guildDownloadComplete && !guildDownloadCompleted)
        {
            List<DiscordGuild> guilds = new();
            
            foreach (ulong guildId in this._readyGuilds)
            {
                DiscordGuild? cachedGuild = await this.Cache.TryGetGuildAsync(guildId);
                if (cachedGuild is not null)
                {
                    guilds.Add(cachedGuild);
                }
            }
            
            IReadOnlyDictionary<ulong, DiscordGuild> guildsReadOnly = guilds.ToDictionary(x => x.Id, x => x);
            
            await this._guildDownloadCompletedEv.InvokeAsync(this, new GuildDownloadCompletedEventArgs(guildsReadOnly));
        }
    }

    internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
    {
        DiscordGuild? oldGuild = await this.Cache.TryGetGuildAsync(guild.Id);

        if (oldGuild is null)
        {
            await this.Cache.AddGuildAsync(guild);
            oldGuild = null;
        }
        else
        {
            DiscordGuild discordGuild = oldGuild;

            oldGuild = new DiscordGuild
            {
                Discord = discordGuild.Discord,
                Name = discordGuild.Name,
                _afkChannelId = discordGuild._afkChannelId,
                AfkTimeout = discordGuild.AfkTimeout,
                DefaultMessageNotifications = discordGuild.DefaultMessageNotifications,
                ExplicitContentFilter = discordGuild.ExplicitContentFilter,
                Features = discordGuild.Features,
                IconHash = discordGuild.IconHash,
                Id = discordGuild.Id,
                IsLarge = discordGuild.IsLarge,
                _isSynced = discordGuild._isSynced,
                IsUnavailable = discordGuild.IsUnavailable,
                JoinedAt = discordGuild.JoinedAt,
                MemberCount = discordGuild.MemberCount,
                MaxMembers = discordGuild.MaxMembers,
                MaxPresences = discordGuild.MaxPresences,
                ApproximateMemberCount = discordGuild.ApproximateMemberCount,
                ApproximatePresenceCount = discordGuild.ApproximatePresenceCount,
                MaxVideoChannelUsers = discordGuild.MaxVideoChannelUsers,
                DiscoverySplashHash = discordGuild.DiscoverySplashHash,
                PreferredLocale = discordGuild.PreferredLocale,
                MfaLevel = discordGuild.MfaLevel,
                OwnerId = discordGuild.OwnerId,
                SplashHash = discordGuild.SplashHash,
                _systemChannelId = discordGuild._systemChannelId,
                SystemChannelFlags = discordGuild.SystemChannelFlags,
                WidgetEnabled = discordGuild.WidgetEnabled,
                _widgetChannelId = discordGuild._widgetChannelId,
                VerificationLevel = discordGuild.VerificationLevel,
                _rulesChannelId = discordGuild._rulesChannelId,
                _publicUpdatesChannelId = discordGuild._publicUpdatesChannelId,
                _voiceRegionId = discordGuild._voiceRegionId,
                PremiumProgressBarEnabled = discordGuild.PremiumProgressBarEnabled,
                IsNSFW = discordGuild.IsNSFW,
                _channels = new ConcurrentDictionary<ulong, DiscordChannel>(),
                _threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>(),
                _emojis = new ConcurrentDictionary<ulong, DiscordEmoji>(),
                _members = new ConcurrentDictionary<ulong, DiscordMember>(),
                _roles = new ConcurrentDictionary<ulong, DiscordRole>(),
                _voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>()
            };

            foreach (KeyValuePair<ulong, DiscordChannel> keyedChannel in discordGuild._channels ??= new())
            {
                oldGuild._channels[keyedChannel.Key] = keyedChannel.Value;
            }

            foreach (KeyValuePair<ulong, DiscordThreadChannel> keyedThreadChannel in discordGuild._threads ??= new())
            {
                oldGuild._threads[keyedThreadChannel.Key] = keyedThreadChannel.Value;
            }

            foreach (KeyValuePair<ulong, DiscordEmoji> keyedEmoji in discordGuild._emojis ??= new())
            {
                oldGuild._emojis[keyedEmoji.Key] = keyedEmoji.Value;
            }

            foreach (KeyValuePair<ulong, DiscordRole> keyedRole in discordGuild._roles ??= new())
            {
                oldGuild._roles[keyedRole.Key] = keyedRole.Value;
            }
            //new ConcurrentDictionary<ulong, DiscordVoiceState>()
            foreach (KeyValuePair<ulong, DiscordVoiceState> keyedVoicestate in discordGuild._voiceStates ??= new())
            {
                oldGuild._voiceStates[keyedVoicestate.Key] = keyedVoicestate.Value;
            }

            foreach (KeyValuePair<ulong, DiscordMember> keyedMember in discordGuild._members ??= new())
            {
                oldGuild._members[keyedMember.Key] = keyedMember.Value;
            }
        }

        guild.Discord = this;
        guild.IsUnavailable = false;
        DiscordGuild eventGuild = guild;
        guild = (await this.Cache.TryGetGuildAsync(guild.Id))!;

        if (guild._channels is null)
        {
            guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
        }

        if (guild._threads is null)
        {
            guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
        }

        if (guild._roles is null)
        {
            guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
        }

        if (guild._emojis is null)
        {
            guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
        }

        if (guild._voiceStates is null)
        {
            guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
        }

        if (guild._members is null)
        {
            guild._members = new ConcurrentDictionary<ulong, DiscordMember>();
        }

        this.UpdateCachedGuildAsync(eventGuild, rawMembers);

        foreach (DiscordChannel channel in guild._channels.Values)
        {
            channel.GuildId = guild.Id;
            channel.Discord = this;
            foreach (DiscordOverwrite channelOverwrite in channel._permissionOverwrites)
            {
                channelOverwrite.Discord = this;
                channelOverwrite._channel_id = channel.Id;
            }
        }
        foreach (DiscordThreadChannel threadChannel in guild._threads.Values)
        {
            threadChannel.GuildId = guild.Id;
            threadChannel.Discord = this;
        }
        foreach (DiscordEmoji emoji in guild._emojis.Values)
        {
            emoji.Discord = this;
        }

        foreach (DiscordVoiceState discordVoiceState in guild._voiceStates.Values)
        {
            discordVoiceState.Discord = this;
        }

        foreach (DiscordRole discordRole in guild._roles.Values)
        {
            discordRole.Discord = this;
            discordRole._guild_id = guild.Id;
        }

        await this._guildUpdated.InvokeAsync(this, new GuildUpdateEventArgs { GuildBefore = oldGuild, GuildAfter = guild });
    }

    internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray rawMembers)
    {
        DiscordGuild? cachedGuild = await this.Cache.TryGetGuildAsync(guild.Id);
        if (guild.IsUnavailable)
        {
            if (cachedGuild is not null)
            {
                cachedGuild.IsUnavailable = true;
            }
            
            await this._guildUnavailable.InvokeAsync(this, new GuildDeleteEventArgs { Guild = guild, Unavailable = true });
        }
        else
        {
            cachedGuild ??= guild;

            await this._guildDeleted.InvokeAsync(this, new GuildDeleteEventArgs { Guild = cachedGuild });
        }
    }
    
    internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
    {
        List<DiscordEmoji> newEmojisList = newEmojis.ToList();
        foreach (DiscordEmoji emoji in newEmojisList)
        {
            emoji.Discord = this;
        }

        ConcurrentDictionary<ulong, DiscordEmoji>? oldEmojis = null;
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        if (guild is not null)
        {
            oldEmojis = new ConcurrentDictionary<ulong, DiscordEmoji>(guild._emojis);
            guild._emojis.Clear();

            foreach (DiscordEmoji emoji in newEmojisList)
            {
                guild._emojis[emoji.Id] = emoji;
            }
        }
        

        GuildEmojisUpdateEventArgs ea = new()
        {
            Guild = guild,
            EmojisAfter = newEmojisList.ToDictionary(x => x.Id),
            EmojisBefore = oldEmojis
        };
        await this._guildEmojisUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildIntegrationsUpdateEventAsync(ulong guildId)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        GuildIntegrationsUpdateEventArgs ea = new()
        {
            Guild = guild,
            GuildId = guildId
        };
        await this._guildIntegrationsUpdated.InvokeAsync(this, ea);
    }

    private async Task OnGuildAuditLogEntryCreateEventAsync(ulong guildId, AuditLogAction auditLogEntry)
    {
        //TODO update auditlogParser
        AuditLogParser.ParseAuditLogEntryAsync();
        
        GuildAuditLogCreatedEventArgs ea = new()
        {
            Guild = guildId,
            AuditLogEntry = auditLogEntry
        };
        await _guildAuditLogCreated.InvokeAsync(this, ea);
    }

    #endregion

    #region Guild Ban

    internal async Task OnGuildBanAddEventAsync(TransportUser user, ulong guildId)
    {
        DiscordUser usr = new(user) { Discord = this };
        await this.Cache.AddIfNotPresentAsync(usr, usr.GetCacheKey());
        
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordMember? member = await this.Cache.TryGetMemberAsync(guildId, user.Id);

        if (guild is not null)
        {
            if (guild.Members.TryGetValue(user.Id, out DiscordMember? mbr))
            {
                member ??= mbr;
            }
        }

        member ??= new DiscordMember(usr) { Discord = this, _guild_id = guildId };

        CachedEntity<ulong, DiscordMember> cachedMember = new(user.Id, member);
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        
        GuildBanAddEventArgs ea = new()
        {
            Guild = cachedGuild,
            GuildId = guildId,
            Member = cachedMember
        };
        await this._guildBanAdded.InvokeAsync(this, ea);
    }

    internal async Task OnGuildBanRemoveEventAsync(TransportUser user, ulong guildId)
    {
        DiscordUser usr = new(user) { Discord = this };
        await this.Cache.AddIfNotPresentAsync(usr, usr.GetCacheKey());
        
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordMember? member = await this.Cache.TryGetMemberAsync(guildId, user.Id);

        if (guild is not null)
        {
            if (guild.Members.TryGetValue(user.Id, out DiscordMember? mbr))
            {
                member ??= mbr;
            }
        }

        member ??= new DiscordMember(usr) { Discord = this, _guild_id = guildId };
        
        CachedEntity<ulong, DiscordMember> cachedMember = new(user.Id, member);
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        
        GuildBanRemoveEventArgs ea = new()
        {
            Guild = cachedGuild,
            Member = cachedMember
        };
        await this._guildBanRemoved.InvokeAsync(this, ea);
    }

    #endregion

    #region Guild Member

    internal async Task OnGuildMemberAddEventAsync(TransportMember member, ulong guildId)
    {
        DiscordUser usr = new(member.User) { Discord = this };
        await this.Cache.AddIfNotPresentAsync(usr, usr.GetCacheKey());

        DiscordMember mbr = new(member)
        {
            Discord = this,
            _guild_id = guildId
        };
        await this.Cache.AddIfNotPresentAsync(mbr, mbr.GetCacheKey());
        
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        if(guild is not null) 
        {
            guild._members[mbr.Id] = mbr;
            guild.MemberCount++;
        }
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        
        GuildMemberAddEventArgs ea = new()
        {
            Guild = cachedGuild,
            Member = mbr
        };
        await this._guildMemberAdded.InvokeAsync(this, ea);
    }

    internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, ulong guildId)
    {
        DiscordUser usr = new(user);
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordMember? member = await this.Cache.TryGetMemberAsync(guildId, user.Id);

        if (guild is not null)
        {
            if (guild._members.TryRemove(user.Id, out DiscordMember? mbr))
            {
                member ??= mbr;
            }
            guild.MemberCount--;
        }
        member ??= new DiscordMember(new TransportMember() { User = user }) { Discord = this, _guild_id = guildId };
        
        await this.Cache.AddIfNotPresentAsync(usr, usr.GetCacheKey());
        await this.Cache.RemoveMemberAsync(user.Id, guildId);
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        CachedEntity<ulong, DiscordMember> cachedMember = new(user.Id, member);

        GuildMemberRemoveEventArgs ea = new()
        {
            Guild = cachedGuild,
            Member = cachedMember
        };
        await this._guildMemberRemoved.InvokeAsync(this, ea);
    }

    internal async Task OnGuildMemberUpdateEventAsync(TransportMember transportMember, ulong guildId)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId); 
        DiscordMember? member = await this.Cache.TryGetMemberAsync(guildId, transportMember.User.Id);
        
        DiscordUser userAfter = new(transportMember.User) { Discord = this };
        await this.Cache.AddIfNotPresentAsync(userAfter, userAfter.GetCacheKey());

        DiscordMember memberAfter = new(transportMember) { Discord = this, _guild_id = guildId };

        if (guild is not null)
        {
            if (guild.Members.TryGetValue(transportMember.User.Id, out DiscordMember? memberBefore))
            {
                member ??= memberBefore;
            }
            guild._members.AddOrUpdate(transportMember.User.Id, memberAfter, (_, _) => memberAfter);
        }
        
        member ??= new DiscordMember(new TransportMember() { User = transportMember.User }) { Discord = this, _guild_id = guildId };
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);

        GuildMemberUpdateEventArgs ea = new()
        {
            Guild = cachedGuild,
            MemberAfter = memberAfter,
            MemberBefore = member,
        };

        await this._guildMemberUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildMembersChunkEventAsync(JObject dat)
    {
        ulong guildId = (ulong)dat["guild_id"];
        int chunkIndex = (int)dat["chunk_index"];
        int chunkCount = (int)dat["chunk_count"];
        string? nonce = (string)dat["nonce"];
        
        DiscordGuild? cachedGuild = await this.Cache.TryGetGuildAsync(guildId);

        HashSet<DiscordMember> discordMembers = new();
        HashSet<DiscordPresence> discordPresences = new();

        TransportMember[] members = dat["members"].ToDiscordObject<TransportMember[]>();

        int memCount = members.Count();
        for (int i = 0; i < memCount; i++)
        {
            DiscordMember member = new(members[i]) { Discord = this, _guild_id = guildId };

            DiscordUser newUser = new DiscordUser(members[i].User) { Discord = this };
            
            await this.Cache.AddIfNotPresentAsync(newUser, newUser.GetCacheKey());

            if (cachedGuild is not null)
            {
                cachedGuild._members[member.Id] = member;
            }
            discordMembers.Add(member);
        }

        if (cachedGuild is not null)
        {
            cachedGuild.MemberCount = cachedGuild._members.Count;
        }
        
        CachedEntity<ulong, DiscordGuild> cachedGuildEntity = new(guildId, cachedGuild);
        
        GuildMembersChunkEventArgs ea = new()
        {
            Guild = cachedGuildEntity,
            Members = new ReadOnlySet<DiscordMember>(discordMembers),
            ChunkIndex = chunkIndex,
            ChunkCount = chunkCount,
            Nonce = nonce,
        };

        if (dat["presences"] != null)
        {
            DiscordPresence[] presences = dat["presences"].ToDiscordObject<DiscordPresence[]>();

            int presCount = presences.Count();
            for (int i = 0; i < presCount; i++)
            {
                DiscordPresence xp = presences[i];
                xp.Discord = this;
                xp.Activity = new DiscordActivity(xp.RawActivity);

                if (xp.RawActivities != null)
                {
                    xp._internalActivities = new DiscordActivity[xp.RawActivities.Length];
                    for (int j = 0; j < xp.RawActivities.Length; j++)
                    {
                        xp._internalActivities[j] = new DiscordActivity(xp.RawActivities[j]);
                    }
                }

                discordPresences.Add(xp);
            }

            ea.Presences = new ReadOnlySet<DiscordPresence>(discordPresences);
        }

        if (dat["not_found"] != null)
        {
            ISet<ulong> nf = dat["not_found"].ToDiscordObject<ISet<ulong>>();
            ea.NotFound = new ReadOnlySet<ulong>(nf);
        }

        await this._guildMembersChunked.InvokeAsync(this, ea);
    }

    #endregion

    #region Guild Role

    internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, ulong guildId)
    {
        role.Discord = this;
        role._guild_id = guildId;
        
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        
        if (guild is not null)
        {
            guild._roles[role.Id] = role;
        }
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);

        GuildRoleCreateEventArgs ea = new()
        {
            Guild = cachedGuild,
            Role = role
        };
        await this._guildRoleCreated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, ulong guildId)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);

        DiscordRole? newRole = role;
        DiscordRole? oldRole = null;
        
        if (guild is not null)
        {
            oldRole = guild.GetRole(role.Id);
        }
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        
        GuildRoleUpdateEventArgs ea = new()
        {
            Guild = cachedGuild,
            RoleAfter = newRole,
            RoleBefore = oldRole
        };
        await this._guildRoleUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, ulong guildId)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordRole? role = null;
        
        if (guild is not null)
        {
            guild._roles.TryRemove(roleId, out role);
        }
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        CachedEntity<ulong, DiscordRole> cachedRole = new(roleId, role);

        GuildRoleDeleteEventArgs ea = new()
        {
            Guild = cachedGuild,
            Role = cachedRole
        };
        await this._guildRoleDeleted.InvokeAsync(this, ea);
    }

    #endregion

    #region Invite

    internal async Task OnInviteCreateEventAsync(ulong channelId, ulong guildId, DiscordInvite invite)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordChannel? channel = await this.Cache.TryGetChannelAsync(channelId);

        invite.Discord = this;
        
        if (guild is not null)
        {
            guild._invites[invite.Code] = invite;
        }
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        CachedEntity<ulong, DiscordChannel> cachedChannel = new(channelId, channel);
        
        InviteCreateEventArgs ea = new()
        {
            Channel = cachedChannel,
            Guild = cachedGuild,
            Invite = invite
        };
        await this._inviteCreated.InvokeAsync(this, ea);
    }

    internal async Task OnInviteDeleteEventAsync(ulong channelId, ulong guildId, JToken dat)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordChannel? channel = await this.Cache.TryGetChannelAsync(channelId);
        DiscordInvite? invite = null;

        if (guild is not null)
        {
            if (guild._invites.TryRemove(dat["code"].ToString(), out DiscordInvite? cachedInvite))
            {
                invite = cachedInvite;
            }
        }
        invite ??= dat.ToDiscordObject<DiscordInvite>();
        invite.Discord = this;
        invite.IsRevoked = true;
        
        CachedEntity<ulong, DiscordGuild> cachedGuild = new(guildId, guild);
        CachedEntity<ulong, DiscordChannel> cachedChannel = new(channelId, channel);

        InviteDeleteEventArgs ea = new()
        {
            Channel = cachedChannel,
            Guild = cachedGuild,
            Invite = invite
        };
        await this._inviteDeleted.InvokeAsync(this, ea);
    }

    #endregion

    #region Message
    
    internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
    {
        message.Discord = this;
        this.PopulateMessageReactionsAndCacheAsync(message, author, member);
        message.PopulateMentions();

        if (message.Channel == null && message.ChannelId == default)
        {
            this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Channel which the last message belongs to is not in cache - cache state might be invalid!");
        }

        if (message.ReferencedMessage != null)
        {
            message.ReferencedMessage.Discord = this;
            this.PopulateMessageReactionsAndCacheAsync(message.ReferencedMessage, referenceAuthor, referenceMember);
            message.ReferencedMessage.PopulateMentions();
        }

        foreach (DiscordMessageSticker sticker in message.Stickers)
        {
            sticker.Discord = this;
        }

        MessageCreateEventArgs ea = new()
        {
            Message = message,
            MentionedUsers = new ReadOnlyCollection<DiscordUser>(message._mentionedUsers),
            MentionedRoles = message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message._mentionedRoles) : null,
            MentionedChannels = message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message._mentionedChannels) : null
        };
        await this._messageCreated.InvokeAsync(this, ea);
    }

    //TODO need rework to use the new cache
    internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
    {
        DiscordGuild guild;

        message.Discord = this;
        DiscordMessage eventMessage = message;

        DiscordMessage? oldMessage = await this.Cache.TryGetMessageAsync(message.Id);
        if (oldMessage is null) // previous message was not in cache
        {
            message = eventMessage;
            this.PopulateMessageReactionsAndCacheAsync(message, author, member);
            guild = message.Channel?.Guild;

            if (message.ReferencedMessage != null)
            {
                message.ReferencedMessage.Discord = this;
                this.PopulateMessageReactionsAndCacheAsync(message.ReferencedMessage, referenceAuthor, referenceMember);
                message.ReferencedMessage.PopulateMentions();
            }
        }
        else // previous message was fetched in cache
        {
            oldMessage = new DiscordMessage(message);

            // cached message is updated with information from the event message
            guild = message.Channel?.Guild;
            message.EditedTimestamp = eventMessage.EditedTimestamp;
            if (eventMessage.Content != null)
            {
                message.Content = eventMessage.Content;
            }

            message._embeds.Clear();
            message._embeds.AddRange(eventMessage._embeds);
            message._attachments.Clear();
            message._attachments.AddRange(eventMessage._attachments);
            message.Pinned = eventMessage.Pinned;
            message.IsTTS = eventMessage.IsTTS;

            // Mentions
            message._mentionedUsers.Clear();
            message._mentionedUsers.AddRange(eventMessage._mentionedUsers ?? new());
            message._mentionedRoles.Clear();
            message._mentionedRoles.AddRange(eventMessage._mentionedRoles ?? new());
            message._mentionedChannels.Clear();
            message._mentionedChannels.AddRange(eventMessage._mentionedChannels ?? new());
            message.MentionEveryone = eventMessage.MentionEveryone;
        }

        message.PopulateMentions();

        MessageUpdateEventArgs ea = new()
        {
            Message = message,
            MessageBefore = oldMessage,
            MentionedUsers = new ReadOnlyCollection<DiscordUser>(message._mentionedUsers),
            MentionedRoles = message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message._mentionedRoles) : null,
            MentionedChannels = message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message._mentionedChannels) : null
        };
        await this._messageUpdated.InvokeAsync(this, ea);
    }

    //TODO need rework to use the new cache
    internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId)
    {
        DiscordGuild guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThreadAsync(channelId);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = ChannelType.Private,
                Recipients = Array.Empty<DiscordUser>()

            };
            this._privateChannels[channelId] = (DiscordDmChannel)channel;
        }

        if (channel == null
            || this.Configuration.MessageCacheSize == 0
            || this.MessageCache == null
            || !this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {

                Id = messageId,
                ChannelId = channelId,
                Discord = this,
            };
        }

        if (this.Configuration.MessageCacheSize > 0)
        {
            this.MessageCache?.Remove(msg.Id);
        }

        MessageDeleteEventArgs ea = new()
        {
            Message = msg,
            Channel = channel,
            Guild = guild,
        };
        await this._messageDeleted.InvokeAsync(this, ea);
    }

    internal async Task OnMessageBulkDeleteEventAsync(ulong[] messageIds, ulong channelId, ulong? guildId)
    {
        DiscordChannel? cachedChannel = await this.Cache.TryGetChannelAsync(channelId);
        DiscordGuild? cachedGuild = null;
        if (guildId.HasValue)
        {
            cachedGuild = await this.Cache.TryGetGuildAsync(guildId.Value);
        }

        List<DiscordMessage> msgs = new(messageIds.Length);
        foreach (ulong messageId in messageIds)
        {
            DiscordMessage? cachedMessage = await this.Cache.TryGetMessageAsync(messageId);
            
            cachedMessage ??= new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this,
                };

            msgs.Add(cachedMessage);
        }
        
        MessageBulkDeleteEventArgs ea = new()
        {
            Channel = cachedChannel,
            ChannelId = channelId,
            Messages = new ReadOnlyCollection<DiscordMessage>(msgs),
            Guild = cachedGuild,
            GuildId = guildId
        };
        await this._messagesBulkDeleted.InvokeAsync(this, ea);
    }

    #endregion

    #region Message Reaction

    internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, TransportMember mbr, DiscordEmoji emoji)
    {
        DiscordChannel? channel = await this.Cache.TryGetChannelAsync(channelId);
        DiscordGuild? guild = null;
        if (guildId.HasValue)
        {
            guild = await this.Cache.TryGetGuildAsync(guildId.Value);
        }

        emoji.Discord = this;

        DiscordUser? usr;
        usr = await this.Cache.TryGetUserAsync(userId);
        if (usr is null)
        {
            usr = await this.UpdateUserAsync(new DiscordUser {Id = userId, Discord = this}, guildId, guild, mbr);
        }
        else
        {
            usr = await this.UpdateUserAsync(usr, guild?.Id, guild, mbr);
        }

        if (channel is null)
        {
            channel = new DiscordChannel() {Discord = this, Id = channelId};
            await this.Cache.AddChannelAsync(channel);
        }

        DiscordMessage? cachedMessage = await this.Cache.TryGetMessageAsync(messageId);
        
        cachedMessage ??= new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this,
                _reactions = new List<DiscordReaction>()
            };
        

        DiscordReaction? react = cachedMessage._reactions.FirstOrDefault(xr => xr.Emoji == emoji);
        if (react is null)
        {
            react = new DiscordReaction {Count = 1, Emoji = emoji, IsMe = this.CurrentUser.Id == userId};
            cachedMessage._reactions.Add(react);
        }
        else
        {
            react.Count++;
            react.IsMe |= this.CurrentUser.Id == userId;
        }

        MessageReactionAddEventArgs ea = new()
        {
            Message = cachedMessage,
            User = usr,
            Guild = guild,
            GuildId = guildId,
            Emoji = emoji
        };
        await this._messageReactionAdded.InvokeAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji)
    {
        DiscordGuild? cachedGuild = null;
        if (guildId.HasValue)
        {
            cachedGuild = await this.Cache.TryGetGuildAsync(guildId.Value);
        }
        
        DiscordChannel? channel = await this.Cache.TryGetChannelAsync(channelId);
        emoji.Discord = this;

        DiscordUser? cachedUser = await this.Cache.TryGetUserAsync(userId); 
        cachedUser ??= new DiscordUser {Id = userId, Discord = this};

        if (channel is null)
        {
            channel = new DiscordChannel
            {
                Id = channelId,
                Discord = this
            };
            await this.Cache.AddChannelAsync(channel);
        }

        if (guildId.HasValue)
        {
            DiscordMember? cachedMember = await this.Cache.TryGetMemberAsync(guildId.Value, userId);
            cachedMember ??= new DiscordMember(cachedUser) {Discord = this, _guild_id = guildId.Value};
            cachedUser = cachedMember;
        }

        DiscordMessage? cachedMessage = await this.Cache.TryGetMessageAsync(messageId);
        
        cachedMessage ??= new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this
            };
        
        DiscordReaction? react = cachedMessage._reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
        if (react != null)
        {
            react.Count--;
            react.IsMe &= this.CurrentUser.Id != userId;

            if (cachedMessage._reactions != null && react.Count <= 0) // shit happens
            {
                for (int i = 0; i < cachedMessage._reactions.Count; i++)
                {
                    if (cachedMessage._reactions[i].Emoji == emoji)
                    {
                        cachedMessage._reactions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        //TODO update event args to reflect nullability
        MessageReactionRemoveEventArgs ea = new()
        {
            Message = cachedMessage,
            User = cachedUser,
            Guild = cachedGuild,
            Emoji = emoji
        };
        await this._messageReactionRemoved.InvokeAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, ulong channelId, ulong? guildId)
    {
        DiscordChannel channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThreadAsync(channelId);

        if (channel == null
            || this.Configuration.MessageCacheSize == 0
            || this.MessageCache == null
            || !this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this
            };
        }

        msg._reactions?.Clear();

        DiscordGuild guild = await this.Cache.TryGetGuildAsync(guildId);

        MessageReactionsClearEventArgs ea = new()
        {
            Message = msg,
        };

        await this._messageReactionsCleared.InvokeAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveEmojiAsync(ulong messageId, ulong channelId, ulong guildId, JToken dat)
    {
        DiscordGuild guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThreadAsync(channelId);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = ChannelType.Private,
                Recipients = Array.Empty<DiscordUser>()
            };
            this._privateChannels[channelId] = (DiscordDmChannel)channel;
        }

        if (channel == null
            || this.Configuration.MessageCacheSize == 0
            || this.MessageCache == null
            || !this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this
            };
        }

        DiscordEmoji partialEmoji = dat.ToDiscordObject<DiscordEmoji>();

        if (!guild._emojis.TryGetValue(partialEmoji.Id, out DiscordEmoji? emoji))
        {
            emoji = partialEmoji;
            emoji.Discord = this;
        }

        msg._reactions?.RemoveAll(r => r.Emoji.Equals(emoji));

        MessageReactionRemoveEmojiEventArgs ea = new()
        {
            Message = msg,
            Channel = channel,
            Guild = guild,
            Emoji = emoji
        };

        await this._messageReactionRemovedEmoji.InvokeAsync(this, ea);
    }

    #endregion

    #region User/Presence Update

    internal async Task OnPresenceUpdateEventAsync(JObject rawPresence, JObject rawUser)
    {
        ulong uid = (ulong)rawUser["id"];
        DiscordPresence old = null;

        if (this._presences.TryGetValue(uid, out DiscordPresence? presence))
        {
            old = new DiscordPresence(presence);
            DiscordJson.PopulateObject(rawPresence, presence);
        }
        else
        {
            presence = rawPresence.ToDiscordObject<DiscordPresence>();
            presence.Discord = this;
            presence.Activity = new DiscordActivity(presence.RawActivity);
            this._presences[presence.InternalUser.Id] = presence;
        }

        // reuse arrays / avoid linq (this is a hot zone)
        if (presence.Activities == null || rawPresence["activities"] == null)
        {
            presence._internalActivities = Array.Empty<DiscordActivity>();
        }
        else
        {
            if (presence._internalActivities.Length != presence.RawActivities.Length)
            {
                presence._internalActivities = new DiscordActivity[presence.RawActivities.Length];
            }

            for (int i = 0; i < presence._internalActivities.Length; i++)
            {
                presence._internalActivities[i] = new DiscordActivity(presence.RawActivities[i]);
            }

            if (presence._internalActivities.Length > 0)
            {
                presence.RawActivity = presence.RawActivities[0];

                if (presence.Activity != null)
                {
                    presence.Activity.UpdateWith(presence.RawActivity);
                }
                else
                {
                    presence.Activity = new DiscordActivity(presence.RawActivity);
                }
            }
            else
            {
                presence.RawActivity = null;
                presence.Activity = null;
            }
        }

        // Caching partial objects is not a good idea, but considering these
        // Objects will most likely be GC'd immediately after this event,
        // This probably isn't great for GC pressure because this is a hot zone.
        _ = this.UserCache.TryGetValue(uid, out DiscordUser? usr);

        DiscordUser usrafter = usr ?? new DiscordUser(presence.InternalUser);
        PresenceUpdateEventArgs ea = new()
        {
            Status = presence.Status,
            Activity = presence.Activity,
            User = usr,
            PresenceBefore = old,
            PresenceAfter = presence,
            UserBefore = old != null ? new DiscordUser(old.InternalUser) { Discord = this } : usrafter,
            UserAfter = usrafter
        };
        await this._presenceUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnUserSettingsUpdateEventAsync(TransportUser user)
    {
        DiscordUser usr = new(user) { Discord = this };

        UserSettingsUpdateEventArgs ea = new()
        {
            User = usr
        };
        await this._userSettingsUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnUserUpdateEventAsync(TransportUser user)
    {
        DiscordUser usr_old = new()
        {
            AvatarHash = this.CurrentUser.AvatarHash,
            Discord = this,
            Discriminator = this.CurrentUser.Discriminator,
            Email = this.CurrentUser.Email,
            Id = this.CurrentUser.Id,
            IsBot = this.CurrentUser.IsBot,
            MfaEnabled = this.CurrentUser.MfaEnabled,
            Username = this.CurrentUser.Username,
            Verified = this.CurrentUser.Verified
        };

        this.CurrentUser.AvatarHash = user.AvatarHash;
        this.CurrentUser.Discriminator = user.Discriminator;
        this.CurrentUser.Email = user.Email;
        this.CurrentUser.Id = user.Id;
        this.CurrentUser.IsBot = user.IsBot;
        this.CurrentUser.MfaEnabled = user.MfaEnabled;
        this.CurrentUser.Username = user.Username;
        this.CurrentUser.Verified = user.Verified;

        UserUpdateEventArgs ea = new()
        {
            UserAfter = this.CurrentUser,
            UserBefore = usr_old
        };
        await this._userUpdated.InvokeAsync(this, ea);
    }

    #endregion

    #region Voice

    internal async Task OnVoiceStateUpdateEventAsync(JObject raw)
    {
        ulong gid = (ulong)raw["guild_id"];
        ulong uid = (ulong)raw["user_id"];
        DiscordGuild gld = this._guilds[gid];

        DiscordVoiceState vstateNew = raw.ToDiscordObject<DiscordVoiceState>();
        vstateNew.Discord = this;

        gld._voiceStates.TryRemove(uid, out DiscordVoiceState? vstateOld);

        if (vstateNew.Channel != null)
        {
            gld._voiceStates[vstateNew.UserId] = vstateNew;
        }

        if (gld._members.TryGetValue(uid, out DiscordMember? mbr))
        {
            mbr.IsMuted = vstateNew.IsServerMuted;
            mbr.IsDeafened = vstateNew.IsServerDeafened;
        }
        else
        {
            TransportMember transportMbr = vstateNew.TransportMember;
            this.UpdateUserAsync(new DiscordUser(transportMbr.User) { Discord = this }, gid, gld, transportMbr);
        }

        VoiceStateUpdateEventArgs ea = new()
        {
            Guild = vstateNew.Guild,
            Channel = vstateNew.Channel,
            User = vstateNew.User,
            SessionId = vstateNew.SessionId,

            Before = vstateOld,
            After = vstateNew
        };
        await this._voiceStateUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, ulong guildId)
    {
        VoiceServerUpdateEventArgs ea = new()
        {
            Endpoint = endpoint,
            VoiceToken = token,
            Guild = guildId
        };
        await this._voiceServerUpdated.InvokeAsync(this, ea);
    }

    #endregion

    #region Thread

    internal async Task OnThreadCreateEventAsync(DiscordThreadChannel thread, bool isNew)
    {
        thread.Discord = this;
        await this.Cache.TryGetGuildAsync(thread.GuildId)._threads[thread.Id] = thread;

        await this._threadCreated.InvokeAsync(this, new ThreadCreateEventArgs { Thread = thread, Guild = thread.Guild, Parent = thread.Parent });
    }

    internal async Task OnThreadUpdateEventAsync(DiscordThreadChannel thread)
    {
        if (thread == null)
        {
            return;
        }

        DiscordThreadChannel threadOld;
        ThreadUpdateEventArgs updateEvent;

        thread.Discord = this;

        DiscordGuild guild = thread.Guild;
        guild.Discord = this;

        DiscordThreadChannel cthread = this.InternalGetCachedThreadAsync(thread.Id);

        if (cthread != null) //thread is cached
        {
            threadOld = new DiscordThreadChannel
            {
                Discord = this,
                GuildId = cthread.GuildId,
                CreatorId = cthread.CreatorId,
                ParentId = cthread.ParentId,
                Id = cthread.Id,
                Name = cthread.Name,
                Type = cthread.Type,
                LastMessageId = cthread.LastMessageId,
                MessageCount = cthread.MessageCount,
                MemberCount = cthread.MemberCount,
                ThreadMetadata = cthread.ThreadMetadata,
                CurrentMember = cthread.CurrentMember,
            };

            updateEvent = new ThreadUpdateEventArgs
            {
                ThreadAfter = thread,
                ThreadBefore = threadOld,
                Guild = thread.Guild,
                Parent = thread.Parent
            };
        }
        else
        {
            updateEvent = new ThreadUpdateEventArgs
            {
                ThreadAfter = thread,
                Guild = thread.Guild,
                Parent = thread.Parent
            };
            guild._threads[thread.Id] = thread;
        }

        await this._threadUpdated.InvokeAsync(this, updateEvent);
    }

    internal async Task OnThreadDeleteEventAsync(DiscordThreadChannel thread)
    {
        if (thread == null)
        {
            return;
        }

        thread.Discord = this;

        DiscordGuild gld = thread.Guild;
        if (gld._threads.TryRemove(thread.Id, out DiscordThreadChannel? cachedThread))
        {
            thread = cachedThread;
        }

        await this._threadDeleted.InvokeAsync(this, new ThreadDeleteEventArgs { Thread = thread, Guild = thread.Guild, Parent = thread.Parent });
    }

    internal async Task OnThreadListSyncEventAsync(ulong guildId, IReadOnlyList<ulong> channel_ids, IReadOnlyList<DiscordThreadChannel> threads, IReadOnlyList<DiscordThreadChannelMember> members)
    {
        guildId.Discord = this;
        IEnumerable<DiscordChannel> channels = channel_ids.Select(x => guildId.GetChannel(x) ?? new DiscordChannel { Id = x, GuildId = guildId.Id }); //getting channel objects

        foreach (DiscordChannel? channel in channels)
        {
            channel.Discord = this;
        }

        foreach (DiscordThreadChannel thread in threads)
        {
            thread.Discord = this;
            guildId._threads[thread.Id] = thread;
        }

        foreach (DiscordThreadChannelMember member in members)
        {
            member.Discord = this;
            member._guild_id = guildId.Id;

            DiscordThreadChannel? thread = threads.SingleOrDefault(x => x.Id == member.ThreadId);
            if (thread != null)
            {
                thread.CurrentMember = member;
            }
        }

        await this._threadListSynced.InvokeAsync(this, new ThreadListSyncEventArgs { Guild = guildId, Channels = channels.ToList().AsReadOnly(), Threads = threads, CurrentMembers = members.ToList().AsReadOnly() });
    }

    internal async Task OnThreadMemberUpdateEventAsync(DiscordThreadChannelMember member)
    {
        member.Discord = this;

        DiscordThreadChannel thread = this.InternalGetCachedThreadAsync(member.ThreadId);
        member._guild_id = thread.Guild.Id;
        thread.CurrentMember = member;
        thread.Guild._threads[thread.Id] = thread;

        await this._threadMemberUpdated.InvokeAsync(this, new ThreadMemberUpdateEventArgs { ThreadMember = member, Thread = thread });
    }

    internal async Task OnThreadMembersUpdateEventAsync(ulong guildId, ulong thread_id, IReadOnlyList<DiscordThreadChannelMember> addedMembers, IReadOnlyList<ulong?> removed_member_ids, int member_count)
    {
        DiscordThreadChannel? thread = this.InternalGetCachedThreadAsync(thread_id) ?? new DiscordThreadChannel
        {
            Id = thread_id,
            GuildId = guildId.Id,
        };
        thread.Discord = this;
        guildId.Discord = this;
        thread.MemberCount = member_count;

        List<DiscordMember> removedMembers = new();
        if (removed_member_ids != null)
        {
            foreach (ulong? removedId in removed_member_ids)
            {
                removedMembers.Add(guildId._members.TryGetValue(removedId.Value, out DiscordMember? member) ? member : new DiscordMember { Id = removedId.Value, _guild_id = guildId.Id, Discord = this });
            }

            if (removed_member_ids.Contains(this.CurrentUser.Id)) //indicates the bot was removed from the thread
            {
                thread.CurrentMember = null;
            }
        }
        else
        {
            removed_member_ids = Array.Empty<ulong?>();
        }

        if (addedMembers != null)
        {
            foreach (DiscordThreadChannelMember threadMember in addedMembers)
            {
                threadMember.Discord = this;
                threadMember._guild_id = guildId.Id;
            }

            if (addedMembers.Any(member => member.Id == this.CurrentUser.Id))
            {
                thread.CurrentMember = addedMembers.Single(member => member.Id == this.CurrentUser.Id);
            }
        }
        else
        {
            addedMembers = Array.Empty<DiscordThreadChannelMember>();
        }

        ThreadMembersUpdateEventArgs threadMembersUpdateArg = new()
        {
            Guild = guildId,
            Thread = thread,
            AddedMembers = addedMembers,
            RemovedMembers = removedMembers,
            MemberCount = member_count
        };

        await this._threadMembersUpdated.InvokeAsync(this, threadMembersUpdateArg);
    }

    #endregion



    #region Integration

    internal async Task OnIntegrationCreateAsync(DiscordIntegration integration, ulong guild_id)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };
        IntegrationCreateEventArgs ea = new()
        {
            Guild = guild,
            Integration = integration
        };

        await this._integrationCreated.InvokeAsync(this, ea);
    }

    internal async Task OnIntegrationUpdateAsync(DiscordIntegration integration, ulong guild_id)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };
        IntegrationUpdateEventArgs ea = new()
        {
            Guild = guild,
            Integration = integration
        };

        await this._integrationUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnIntegrationDeleteAsync(ulong integration_id, ulong guild_id, ulong? application_id)
    {
        DiscordGuild? guild = await this.Cache.TryGetGuildAsync(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };
        IntegrationDeleteEventArgs ea = new()
        {
            Guild = guild,
            Applicationid = application_id,
            IntegrationId = integration_id
        };

        await this._integrationDeleted.InvokeAsync(this, ea);
    }

    #endregion

    #region Commands

    internal async Task OnApplicationCommandPermissionsUpdateAsync(JObject obj)
    {
        ApplicationCommandPermissionsUpdatedEventArgs? ev = obj.ToObject<ApplicationCommandPermissionsUpdatedEventArgs>();

        await this._applicationCommandPermissionsUpdated.InvokeAsync(this, ev);
    }

    #endregion

    #region Stage Instance

    internal async Task OnStageInstanceCreateAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = await this.Cache.TryGetGuildAsync(instance.GuildId);

        guild._stageInstances[instance.Id] = instance;

        StageInstanceCreateEventArgs eventArgs = new()
        {
            StageInstance = instance
        };

        await this._stageInstanceCreated.InvokeAsync(this, eventArgs);
    }

    internal async Task OnStageInstanceUpdateAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = await this.Cache.TryGetGuildAsync(instance.GuildId);

        if (!guild._stageInstances.TryRemove(instance.Id, out DiscordStageInstance? oldInstance))
        {
            oldInstance = new DiscordStageInstance { Id = instance.Id, GuildId = instance.GuildId, ChannelId = instance.ChannelId };
        }

        guild._stageInstances[instance.Id] = instance;

        StageInstanceUpdateEventArgs eventArgs = new()
        {
            StageInstanceBefore = oldInstance,
            StageInstanceAfter = instance
        };

        await this._stageInstanceUpdated.InvokeAsync(this, eventArgs);
    }

    internal async Task OnStageInstanceDeleteAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = await this.Cache.TryGetGuildAsync(instance.GuildId);

        guild._stageInstances.TryRemove(instance.Id, out _);

        StageInstanceDeleteEventArgs eventArgs = new()
        {
            StageInstance = instance
        };

        await this._stageInstanceDeleted.InvokeAsync(this, eventArgs);
    }

    #endregion

    #region Misc

    internal async Task OnInteractionCreateAsync(ulong? guildId, ulong channelId, TransportUser user, TransportMember member, DiscordInteraction interaction)
    {
        DiscordUser usr = new(user) { Discord = this };

        interaction.ChannelId = channelId;
        interaction.GuildId = guildId;
        interaction.Discord = this;
        interaction.Data.Discord = this;

        if (member != null)
        {
            usr = new DiscordMember(member) { _guild_id = guildId.Value, Discord = this };
            await this.UpdateUserAsync(usr, guildId, interaction.Guild, member);
        }
        else
        {
            await this.Cache.AddUserAsync(usr);
        }

        interaction.User = usr;

        DiscordInteractionResolvedCollection resolved = interaction.Data.Resolved;
        if (resolved != null)
        {
            if (resolved.Users != null)
            {
                foreach (KeyValuePair<ulong, DiscordUser> c in resolved.Users)
                {
                    c.Value.Discord = this;
                    await this.Cache.AddUserAsync(c.Value);
                }
            }

            if (resolved.Members != null)
            {
                foreach (KeyValuePair<ulong, DiscordMember> c in resolved.Members)
                {
                    c.Value.Discord = this;
                    c.Value.Id = c.Key;
                    c.Value._guild_id = guildId.Value;
                    c.Value.User.Discord = this;

                    await this.Cache.AddUserAsync(c.Value.User);
                }
            }

            if (resolved.Channels != null)
            {
                foreach (KeyValuePair<ulong, DiscordChannel> c in resolved.Channels)
                {
                    c.Value.Discord = this;

                    if (guildId.HasValue)
                    {
                        c.Value.GuildId = guildId.Value;
                    }
                }
            }

            if (resolved.Roles != null)
            {
                foreach (KeyValuePair<ulong, DiscordRole> c in resolved.Roles)
                {
                    c.Value.Discord = this;

                    if (guildId.HasValue)
                    {
                        c.Value._guild_id = guildId.Value;
                    }
                }
            }

            if (resolved.Messages != null)
            {
                foreach (KeyValuePair<ulong, DiscordMessage> m in resolved.Messages)
                {
                    m.Value.Discord = this;

                    if (guildId.HasValue)
                    {
                        m.Value._guildId = guildId.Value;
                    }
                }
            }
        }

        if (interaction.Type is InteractionType.Component)
        {

            interaction.Message.Discord = this;
            interaction.Message.ChannelId = interaction.ChannelId;
            ComponentInteractionCreateEventArgs cea = new()
            {
                Message = interaction.Message,
                Interaction = interaction
            };

            await this._componentInteractionCreated.InvokeAsync(this, cea);
        }
        else if (interaction.Type is InteractionType.ModalSubmit)
        {
            ModalSubmitEventArgs mea = new(interaction);

            await this._modalSubmitted.InvokeAsync(this, mea);
        }
        else if (interaction.Data.Target.HasValue) // Context-Menu. //
        {
            ulong targetId = interaction.Data.Target.Value;
            DiscordUser targetUser = null;
            DiscordMember targetMember = null;
            DiscordMessage targetMessage = null;

            interaction.Data.Resolved.Messages?.TryGetValue(targetId, out targetMessage);
            interaction.Data.Resolved.Members?.TryGetValue(targetId, out targetMember);
            interaction.Data.Resolved.Users?.TryGetValue(targetId, out targetUser);

            ContextMenuInteractionCreateEventArgs ctea = new ContextMenuInteractionCreateEventArgs
            {
                Interaction = interaction,
                TargetUser = targetMember ?? targetUser,
                TargetMessage = targetMessage,
                Type = interaction.Data.Type,
            };
            await this._contextMenuInteractionCreated.InvokeAsync(this, ctea);
        }

        InteractionCreateEventArgs ea = new InteractionCreateEventArgs
        {
            Interaction = interaction
        };

        await this._interactionCreated.InvokeAsync(this, ea);
    }

    internal async Task OnTypingStartEventAsync(ulong userId, ulong channelId, ulong? guildId, DateTimeOffset started, TransportMember mbr)
    {
        if (channel == null)
        {
            channel = new DiscordChannel
            {
                Discord = this,
                Id = channelId,
                GuildId = guildId ?? default,
            };
        }

        DiscordGuild guild = await this.Cache.TryGetGuildAsync(guildId);
        DiscordUser usr = this.UpdateUserAsync(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr);

        TypingStartEventArgs ea = new()
        {
            Channel = channel,
            User = usr,
            Guild = guild,
            StartedAt = started
        };
        await this._typingStarted.InvokeAsync(this, ea);
    }

    internal async Task OnWebhooksUpdateAsync(ulong channelId, ulong guildId)
    {
        WebhooksUpdateEventArgs ea = new()
        {
            Channel = channelId,
            Guild = guildId
        };
        await this._webhooksUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnStickersUpdatedAsync(IEnumerable<DiscordMessageSticker> newStickers, JObject raw)
    {
        DiscordGuild guild = await this.Cache.TryGetGuildAsync((ulong)raw["guild_id"]);
        ConcurrentDictionary<ulong, DiscordMessageSticker> oldStickers = new(guild._stickers);

        guild._stickers.Clear();

        foreach (DiscordMessageSticker nst in newStickers)
        {
            if (nst.User != null)
            {
                nst.User.Discord = this;
            }

            nst.Discord = this;

            guild._stickers[nst.Id] = nst;
        }

        GuildStickersUpdateEventArgs sea = new()
        {
            Guild = guild,
            StickersBefore = oldStickers,
            StickersAfter = guild.Stickers
        };

        await this._guildStickersUpdated.InvokeAsync(this, sea);
    }

    internal async Task OnUnknownEventAsync(GatewayPayload payload)
    {
        UnknownEventArgs ea = new() { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
        await this._unknownEvent.InvokeAsync(this, ea);
    }

    #endregion

    #region AutoModeration
    internal async Task OnAutoModerationRuleCreateAsync(DiscordAutoModerationRule ruleCreated)
    {
        ruleCreated.Discord = this;
        await this._autoModerationRuleCreated.InvokeAsync(this, new AutoModerationRuleCreateEventArgs { Rule = ruleCreated });
    }

    internal async Task OnAutoModerationRuleUpdatedAsync(DiscordAutoModerationRule ruleUpdated)
    {
        ruleUpdated.Discord = this;
        await this._autoModerationRuleUpdated.InvokeAsync(this, new AutoModerationRuleUpdateEventArgs { Rule = ruleUpdated });
    }

    internal async Task OnAutoModerationRuleDeletedAsync(DiscordAutoModerationRule ruleDeleted)
    {
        ruleDeleted.Discord = this;
        await this._autoModerationRuleDeleted.InvokeAsync(this, new AutoModerationRuleDeleteEventArgs { Rule = ruleDeleted });
    }

    internal async Task OnAutoModerationRuleExecutedAsync(DiscordAutoModerationActionExecution ruleExecuted) => await this._autoModerationRuleExecuted.InvokeAsync(this, new AutoModerationRuleExecuteEventArgs { Rule = ruleExecuted });
    #endregion

    #endregion
}
