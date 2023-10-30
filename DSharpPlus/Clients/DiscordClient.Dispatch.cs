using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            case "guild_sync":
                gid = (ulong)dat["id"];

                rawMembers = (JArray)dat["members"];
                rawPresences = (JArray)dat["presences"];
                dat.Remove("members");
                dat.Remove("presences");

                await this.OnGuildSyncEventAsync(this._guilds[gid], (bool)dat["large"], rawMembers, rawPresences.ToDiscordObject<IEnumerable<DiscordPresence>>());
                break;

            case "guild_emojis_update":
                gid = (ulong)dat["guild_id"];
                IEnumerable<DiscordEmoji> ems = dat["emojis"].ToDiscordObject<IEnumerable<DiscordEmoji>>();
                await this.OnGuildEmojisUpdateEventAsync(this._guilds[gid], ems);
                break;

            case "guild_integrations_update":
                gid = (ulong)dat["guild_id"];

                // discord fires this event inconsistently if the current user leaves a guild.
                if (!this._guilds.ContainsKey(gid))
                {
                    return;
                }

                await this.OnGuildIntegrationsUpdateEventAsync(this._guilds[gid]);
                break;

            case "guild_audit_log_entry_create":
                gid = (ulong)dat["guild_id"];
                DiscordGuild guild = _guilds[gid];
                AuditLogAction auditLogAction = dat.ToDiscordObject<AuditLogAction>();
                DiscordAuditLogEntry entry = await AuditLogParser.ParseAuditLogEntryAsync(guild, auditLogAction);
                await this.OnGuildAuditLogEntryCreateEventAsync(guild, entry);
                break;

            #endregion

            #region Guild Ban

            case "guild_ban_add":
                usr = dat["user"].ToDiscordObject<TransportUser>();
                gid = (ulong)dat["guild_id"];
                await this.OnGuildBanAddEventAsync(usr, this._guilds[gid]);
                break;

            case "guild_ban_remove":
                usr = dat["user"].ToDiscordObject<TransportUser>();
                gid = (ulong)dat["guild_id"];
                await this.OnGuildBanRemoveEventAsync(usr, this._guilds[gid]);
                break;

            #endregion

            #region Guild Member

            case "guild_member_add":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildMemberAddEventAsync(dat.ToDiscordObject<TransportMember>(), this._guilds[gid]);
                break;

            case "guild_member_remove":
                gid = (ulong)dat["guild_id"];
                usr = dat["user"].ToDiscordObject<TransportUser>();

                if (!this._guilds.ContainsKey(gid))
                {
                    // discord fires this event inconsistently if the current user leaves a guild.
                    if (usr.Id != this.CurrentUser.Id)
                    {
                        this.Logger.LogError(LoggerEvents.WebSocketReceive, "Could not find {Guild} in guild cache", gid);
                    }

                    return;
                }

                await this.OnGuildMemberRemoveEventAsync(usr, this._guilds[gid]);
                break;

            case "guild_member_update":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildMemberUpdateEventAsync(dat.ToDiscordObject<TransportMember>(), this._guilds[gid]);
                break;

            case "guild_members_chunk":
                await this.OnGuildMembersChunkEventAsync(dat);
                break;

            #endregion

            #region Guild Role

            case "guild_role_create":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildRoleCreateEventAsync(dat["role"].ToDiscordObject<DiscordRole>(), this._guilds[gid]);
                break;

            case "guild_role_update":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildRoleUpdateEventAsync(dat["role"].ToDiscordObject<DiscordRole>(), this._guilds[gid]);
                break;

            case "guild_role_delete":
                gid = (ulong)dat["guild_id"];
                await this.OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], this._guilds[gid]);
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

            case "message_ack":
                cid = (ulong)dat["channel_id"];
                ulong mid = (ulong)dat["message_id"];
                await this.OnMessageAckEventAsync(this.InternalGetCachedChannel(cid), mid);
                break;

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
                await this.OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], this._guilds[gid]);
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
                await this.OnThreadListSyncEventAsync(this._guilds[gid], dat["channel_ids"].ToDiscordObject<IReadOnlyList<ulong>>(), dat["threads"].ToDiscordObject<IReadOnlyList<DiscordThreadChannel>>(), dat["members"].ToDiscordObject<IReadOnlyList<DiscordThreadChannelMember>>());
                break;

            case "thread_member_update":
                await this.OnThreadMemberUpdateEventAsync(dat.ToDiscordObject<DiscordThreadChannelMember>());
                break;

            case "thread_members_update":
                gid = (ulong)dat["guild_id"];
                await this.OnThreadMembersUpdateEventAsync(this._guilds[gid], (ulong)dat["id"], dat["added_members"]?.ToDiscordObject<IReadOnlyList<DiscordThreadChannelMember>>(), dat["removed_member_ids"]?.ToDiscordObject<IReadOnlyList<ulong?>>(), (int)dat["member_count"]);
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

                await this.OnTypingStartEventAsync((ulong)dat["user_id"], cid, this.InternalGetCachedChannel(cid), (ulong?)dat["guild_id"], Utilities.GetDateTimeOffset((long)dat["timestamp"]), mbr);
                break;

            case "webhooks_update":
                gid = (ulong)dat["guild_id"];
                cid = (ulong)dat["channel_id"];
                await this.OnWebhooksUpdateAsync(this._guilds[gid].GetChannel(cid), this._guilds[gid]);
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
        //ready.CurrentUser.Discord = this;

        TransportUser rusr = ready.CurrentUser;
        this.CurrentUser.Username = rusr.Username;
        this.CurrentUser.Discriminator = rusr.Discriminator;
        this.CurrentUser.AvatarHash = rusr.AvatarHash;
        this.CurrentUser.MfaEnabled = rusr.MfaEnabled;
        this.CurrentUser.Verified = rusr.Verified;
        this.CurrentUser.IsBot = rusr.IsBot;

        this.GatewayVersion = ready.GatewayVersion;
        this._sessionId = ready.SessionId;
        Dictionary<ulong, JObject> raw_guild_index = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

        this._privateChannels.Clear();
        foreach (JToken rawChannel in rawDmChannels)
        {
            DiscordDmChannel channel = rawChannel.ToDiscordObject<DiscordDmChannel>();

            channel.Discord = this;

            //xdc._recipients =
            //    .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this })
            //    .ToList();

            IEnumerable<TransportUser> recips_raw = rawChannel["recipients"].ToDiscordObject<IEnumerable<TransportUser>>();
            List<DiscordUser> recipients = new List<DiscordUser>();
            foreach (TransportUser xr in recips_raw)
            {
                DiscordUser xu = new DiscordUser(xr) { Discord = this };
                xu = this.UpdateUserCache(xu);

                recipients.Add(xu);
            }
            channel.Recipients = recipients;


            this._privateChannels[channel.Id] = channel;
        }

        this._guilds.Clear();

        IEnumerable<DiscordGuild> guilds = rawGuilds.ToDiscordObject<IEnumerable<DiscordGuild>>();
        foreach (DiscordGuild guild in guilds)
        {
            guild.Discord = this;
            guild._channels ??= new ConcurrentDictionary<ulong, DiscordChannel>();
            guild._threads ??= new ConcurrentDictionary<ulong, DiscordThreadChannel>();

            foreach (DiscordChannel xc in guild.Channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (DiscordOverwrite xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (DiscordThreadChannel xt in guild.Threads.Values)
            {
                xt.GuildId = guild.Id;
                xt.Discord = this;
            }

            guild._roles ??= new ConcurrentDictionary<ulong, DiscordRole>();

            foreach (DiscordRole xr in guild.Roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            JObject raw_guild = raw_guild_index[guild.Id];
            JArray? raw_members = (JArray)raw_guild["members"];

            guild._members?.Clear();
            guild._members ??= new ConcurrentDictionary<ulong, DiscordMember>();

            if (raw_members != null)
            {
                foreach (JToken xj in raw_members)
                {
                    TransportMember xtm = xj.ToDiscordObject<TransportMember>();

                    DiscordUser xu = new DiscordUser(xtm.User) { Discord = this };
                    xu = this.UpdateUserCache(xu);

                    guild._members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id };
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

            this._guilds[guild.Id] = guild;
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
        foreach (DiscordOverwrite xo in channel._permissionOverwrites)
        {
            xo.Discord = this;
            xo._channel_id = channel.Id;
        }

        this._guilds[channel.GuildId.Value]._channels[channel.Id] = channel;

        await this._channelCreated.InvokeAsync(this, new ChannelCreateEventArgs { Channel = channel, Guild = channel.Guild });
    }

    internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
    {
        if (channel == null)
        {
            return;
        }

        channel.Discord = this;

        DiscordGuild? gld = channel.Guild;

        DiscordChannel? channel_new = this.InternalGetCachedChannel(channel.Id);
        DiscordChannel channel_old = null;

        if (channel_new != null)
        {
            channel_old = new DiscordChannel
            {
                Bitrate = channel_new.Bitrate,
                Discord = this,
                GuildId = channel_new.GuildId,
                Id = channel_new.Id,
                //IsPrivate = channel_new.IsPrivate,
                LastMessageId = channel_new.LastMessageId,
                Name = channel_new.Name,
                _permissionOverwrites = new List<DiscordOverwrite>(channel_new._permissionOverwrites),
                Position = channel_new.Position,
                Topic = channel_new.Topic,
                Type = channel_new.Type,
                UserLimit = channel_new.UserLimit,
                ParentId = channel_new.ParentId,
                IsNSFW = channel_new.IsNSFW,
                PerUserRateLimit = channel_new.PerUserRateLimit,
                RtcRegionId = channel_new.RtcRegionId,
                QualityMode = channel_new.QualityMode
            };

            channel_new.Bitrate = channel.Bitrate;
            channel_new.Name = channel.Name;
            channel_new.Position = channel.Position;
            channel_new.Topic = channel.Topic;
            channel_new.UserLimit = channel.UserLimit;
            channel_new.ParentId = channel.ParentId;
            channel_new.IsNSFW = channel.IsNSFW;
            channel_new.PerUserRateLimit = channel.PerUserRateLimit;
            channel_new.Type = channel.Type;
            channel_new.RtcRegionId = channel.RtcRegionId;
            channel_new.QualityMode = channel.QualityMode;

            channel_new._permissionOverwrites.Clear();

            foreach (DiscordOverwrite po in channel._permissionOverwrites)
            {
                po.Discord = this;
                po._channel_id = channel.Id;
            }

            channel_new._permissionOverwrites.AddRange(channel._permissionOverwrites);
        }
        else if (gld != null)
        {
            gld._channels[channel.Id] = channel;
        }

        await this._channelUpdated.InvokeAsync(this, new ChannelUpdateEventArgs { ChannelAfter = channel_new, Guild = gld, ChannelBefore = channel_old });
    }

    internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
    {
        if (channel == null)
        {
            return;
        }

        channel.Discord = this;

        //if (channel.IsPrivate)
        if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
        {
            DiscordDmChannel? dmChannel = channel as DiscordDmChannel;

            _ = this._privateChannels.TryRemove(dmChannel.Id, out _);

            await this._dmChannelDeleted.InvokeAsync(this, new DmChannelDeleteEventArgs { Channel = dmChannel });
        }
        else
        {
            DiscordGuild gld = channel.Guild;

            if (gld._channels.TryRemove(channel.Id, out DiscordChannel? cachedChannel))
            {
                channel = cachedChannel;
            }

            await this._channelDeleted.InvokeAsync(this, new ChannelDeleteEventArgs { Channel = channel, Guild = gld });
        }
    }

    internal async Task OnChannelPinsUpdateAsync(ulong? guildId, ulong channelId, DateTimeOffset? lastPinTimestamp)
    {
        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = ChannelType.Private,
                Recipients = Array.Empty<DiscordUser>()
            };

            DiscordDmChannel chn = (DiscordDmChannel)channel;

            this._privateChannels[channelId] = chn;
        }

        ChannelPinsUpdateEventArgs ea = new ChannelPinsUpdateEventArgs
        {
            Guild = guild,
            Channel = channel,
            LastPinTimestamp = lastPinTimestamp
        };
        await this._channelPinsUpdated.InvokeAsync(this, ea);
    }

    #endregion

    #region Scheduled Guild Events

    private async Task OnScheduledGuildEventCreateEventAsync(DiscordScheduledGuildEvent evt)
    {
        evt.Discord = this;

        if (evt.Creator != null)
        {
            evt.Creator.Discord = this;
            this.UpdateUserCache(evt.Creator);
        }

        evt.Guild._scheduledEvents[evt.Id] = evt;

        await this._scheduledGuildEventCreated.InvokeAsync(this, new ScheduledGuildEventCreateEventArgs { Event = evt });
    }

    private async Task OnScheduledGuildEventDeleteEventAsync(DiscordScheduledGuildEvent evt)
    {
        DiscordGuild guild = this.InternalGetCachedGuild(evt.GuildId);

        if (guild == null) // ??? //
        {
            return;
        }

        guild._scheduledEvents.TryRemove(evt.Id, out _);

        evt.Discord = this;

        if (evt.Creator != null)
        {
            evt.Creator.Discord = this;
            this.UpdateUserCache(evt.Creator);
        }

        await this._scheduledGuildEventDeleted.InvokeAsync(this, new ScheduledGuildEventDeleteEventArgs { Event = evt });
    }

    private async Task OnScheduledGuildEventUpdateEventAsync(DiscordScheduledGuildEvent evt)
    {
        evt.Discord = this;

        if (evt.Creator != null)
        {
            evt.Creator.Discord = this;
            this.UpdateUserCache(evt.Creator);
        }

        DiscordGuild guild = this.InternalGetCachedGuild(evt.GuildId);
        guild._scheduledEvents.TryGetValue(evt.GuildId, out DiscordScheduledGuildEvent? oldEvt);

        evt.Guild._scheduledEvents[evt.Id] = evt;

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
        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordScheduledGuildEvent evt = guild._scheduledEvents.GetOrAdd(eventId, new DiscordScheduledGuildEvent()
        {
            Id = eventId,
            GuildId = guildId,
            Discord = this,
            UserCount = 0
        });

        evt.UserCount++;

        DiscordUser user =
            guild.Members.TryGetValue(userId, out DiscordMember? mbr) ? mbr :
            this.GetCachedOrEmptyUserInternal(userId) ?? new DiscordUser() { Id = userId, Discord = this };

        await this._scheduledGuildEventUserAdded.InvokeAsync(this, new ScheduledGuildEventUserAddEventArgs() { Event = evt, User = user });
    }

    private async Task OnScheduledGuildEventUserRemoveEventAsync(ulong guildId, ulong eventId, ulong userId)
    {
        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordScheduledGuildEvent evt = guild._scheduledEvents.GetOrAdd(eventId, new DiscordScheduledGuildEvent()
        {
            Id = eventId,
            GuildId = guildId,
            Discord = this,
            UserCount = 0
        });

        evt.UserCount = evt.UserCount is 0 ? 0 : evt.UserCount - 1;

        DiscordUser user =
            guild.Members.TryGetValue(userId, out DiscordMember? mbr) ? mbr :
            this.GetCachedOrEmptyUserInternal(userId) ?? new DiscordUser() { Id = userId, Discord = this };

        await this._scheduledGuildEventUserRemoved.InvokeAsync(this, new ScheduledGuildEventUserRemoveEventArgs() { Event = evt, User = user });
    }

    #endregion

    #region Guild

    internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray rawMembers, IEnumerable<DiscordPresence> presences)
    {
        if (presences != null)
        {
            foreach (DiscordPresence xp in presences)
            {
                xp.Discord = this;
                xp.GuildId = guild.Id;
                xp.Activity = new DiscordActivity(xp.RawActivity);
                if (xp.RawActivities != null)
                {
                    xp._internalActivities = new DiscordActivity[xp.RawActivities.Length];
                    for (int i = 0; i < xp.RawActivities.Length; i++)
                    {
                        xp._internalActivities[i] = new DiscordActivity(xp.RawActivities[i]);
                    }
                }
                this._presences[xp.User.Id] = xp;
            }
        }

        bool exists = this._guilds.TryGetValue(guild.Id, out DiscordGuild? foundGuild);

        guild.Discord = this;
        guild.IsUnavailable = false;
        DiscordGuild eventGuild = guild;

        if (exists)
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

        this.UpdateCachedGuild(eventGuild, rawMembers);

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


        foreach (KeyValuePair<ulong, DiscordVoiceState> kvp in eventGuild._voiceStates ??= new())
        {
            guild._voiceStates[kvp.Key] = kvp.Value;
        }

        foreach (DiscordScheduledGuildEvent xe in guild._scheduledEvents.Values)
        {
            xe.Discord = this;

            if (xe.Creator != null)
            {
                xe.Creator.Discord = this;
            }
        }

        foreach (DiscordChannel xc in guild._channels.Values)
        {
            xc.GuildId = guild.Id;
            xc.Discord = this;
            foreach (DiscordOverwrite xo in xc._permissionOverwrites)
            {
                xo.Discord = this;
                xo._channel_id = xc.Id;
            }
        }
        foreach (DiscordThreadChannel xt in guild._threads.Values)
        {
            xt.GuildId = guild.Id;
            xt.Discord = this;
        }
        foreach (DiscordEmoji xe in guild._emojis.Values)
        {
            xe.Discord = this;
        }

        foreach (DiscordMessageSticker xs in guild._stickers.Values)
        {
            xs.Discord = this;
        }

        foreach (DiscordVoiceState xvs in guild._voiceStates.Values)
        {
            xvs.Discord = this;
        }

        foreach (DiscordRole xr in guild._roles.Values)
        {
            xr.Discord = this;
            xr._guild_id = guild.Id;
        }

        foreach (DiscordStageInstance instance in guild._stageInstances.Values)
        {
            instance.Discord = this;
        }

        bool old = Volatile.Read(ref this._guildDownloadCompleted);
        bool dcompl = this._guilds.Values.All(xg => !xg.IsUnavailable);
        Volatile.Write(ref this._guildDownloadCompleted, dcompl);

        if (exists)
        {
            await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild });
        }
        else
        {
            await this._guildCreated.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild });
        }

        if (dcompl && !old)
        {
            await this._guildDownloadCompletedEv.InvokeAsync(this, new GuildDownloadCompletedEventArgs(this.Guilds));
        }
    }

    internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
    {
        DiscordGuild oldGuild;

        if (!this._guilds.ContainsKey(guild.Id))
        {
            this._guilds[guild.Id] = guild;
            oldGuild = null;
        }
        else
        {
            DiscordGuild gld = this._guilds[guild.Id];

            oldGuild = new DiscordGuild
            {
                Discord = gld.Discord,
                Name = gld.Name,
                _afkChannelId = gld._afkChannelId,
                AfkTimeout = gld.AfkTimeout,
                DefaultMessageNotifications = gld.DefaultMessageNotifications,
                ExplicitContentFilter = gld.ExplicitContentFilter,
                Features = gld.Features,
                IconHash = gld.IconHash,
                Id = gld.Id,
                IsLarge = gld.IsLarge,
                _isSynced = gld._isSynced,
                IsUnavailable = gld.IsUnavailable,
                JoinedAt = gld.JoinedAt,
                MemberCount = gld.MemberCount,
                MaxMembers = gld.MaxMembers,
                MaxPresences = gld.MaxPresences,
                ApproximateMemberCount = gld.ApproximateMemberCount,
                ApproximatePresenceCount = gld.ApproximatePresenceCount,
                MaxVideoChannelUsers = gld.MaxVideoChannelUsers,
                DiscoverySplashHash = gld.DiscoverySplashHash,
                PreferredLocale = gld.PreferredLocale,
                MfaLevel = gld.MfaLevel,
                OwnerId = gld.OwnerId,
                SplashHash = gld.SplashHash,
                _systemChannelId = gld._systemChannelId,
                SystemChannelFlags = gld.SystemChannelFlags,
                WidgetEnabled = gld.WidgetEnabled,
                _widgetChannelId = gld._widgetChannelId,
                VerificationLevel = gld.VerificationLevel,
                _rulesChannelId = gld._rulesChannelId,
                _publicUpdatesChannelId = gld._publicUpdatesChannelId,
                _voiceRegionId = gld._voiceRegionId,
                PremiumProgressBarEnabled = gld.PremiumProgressBarEnabled,
                IsNSFW = gld.IsNSFW,
                _channels = new ConcurrentDictionary<ulong, DiscordChannel>(),
                _threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>(),
                _emojis = new ConcurrentDictionary<ulong, DiscordEmoji>(),
                _members = new ConcurrentDictionary<ulong, DiscordMember>(),
                _roles = new ConcurrentDictionary<ulong, DiscordRole>(),
                _voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>()
            };

            foreach (KeyValuePair<ulong, DiscordChannel> kvp in gld._channels ??= new())
            {
                oldGuild._channels[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordThreadChannel> kvp in gld._threads ??= new())
            {
                oldGuild._threads[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordEmoji> kvp in gld._emojis ??= new())
            {
                oldGuild._emojis[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordRole> kvp in gld._roles ??= new())
            {
                oldGuild._roles[kvp.Key] = kvp.Value;
            }
            //new ConcurrentDictionary<ulong, DiscordVoiceState>()
            foreach (KeyValuePair<ulong, DiscordVoiceState> kvp in gld._voiceStates ??= new())
            {
                oldGuild._voiceStates[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordMember> kvp in gld._members ??= new())
            {
                oldGuild._members[kvp.Key] = kvp.Value;
            }
        }

        guild.Discord = this;
        guild.IsUnavailable = false;
        DiscordGuild eventGuild = guild;
        guild = this._guilds[eventGuild.Id];

        if (guild._channels == null)
        {
            guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
        }

        if (guild._threads == null)
        {
            guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
        }

        if (guild._roles == null)
        {
            guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
        }

        if (guild._emojis == null)
        {
            guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
        }

        if (guild._voiceStates == null)
        {
            guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
        }

        if (guild._members == null)
        {
            guild._members = new ConcurrentDictionary<ulong, DiscordMember>();
        }

        this.UpdateCachedGuild(eventGuild, rawMembers);

        foreach (DiscordChannel xc in guild._channels.Values)
        {
            xc.GuildId = guild.Id;
            xc.Discord = this;
            foreach (DiscordOverwrite xo in xc._permissionOverwrites)
            {
                xo.Discord = this;
                xo._channel_id = xc.Id;
            }
        }
        foreach (DiscordThreadChannel xc in guild._threads.Values)
        {
            xc.GuildId = guild.Id;
            xc.Discord = this;
        }
        foreach (DiscordEmoji xe in guild._emojis.Values)
        {
            xe.Discord = this;
        }

        foreach (DiscordVoiceState xvs in guild._voiceStates.Values)
        {
            xvs.Discord = this;
        }

        foreach (DiscordRole xr in guild._roles.Values)
        {
            xr.Discord = this;
            xr._guild_id = guild.Id;
        }

        await this._guildUpdated.InvokeAsync(this, new GuildUpdateEventArgs { GuildBefore = oldGuild, GuildAfter = guild });
    }

    internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray rawMembers)
    {
        if (guild.IsUnavailable)
        {
            if (!this._guilds.TryGetValue(guild.Id, out DiscordGuild? gld))
            {
                return;
            }

            gld.IsUnavailable = true;

            await this._guildUnavailable.InvokeAsync(this, new GuildDeleteEventArgs { Guild = guild, Unavailable = true });
        }
        else
        {
            if (!this._guilds.TryRemove(guild.Id, out DiscordGuild? gld))
            {
                return;
            }

            await this._guildDeleted.InvokeAsync(this, new GuildDeleteEventArgs { Guild = gld });
        }
    }

    internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool isLarge, JArray rawMembers, IEnumerable<DiscordPresence> presences)
    {
        presences = presences.Select(xp => { xp.Discord = this; xp.Activity = new DiscordActivity(xp.RawActivity); return xp; });
        foreach (DiscordPresence xp in presences)
        {
            this._presences[xp.InternalUser.Id] = xp;
        }

        guild._isSynced = true;
        guild.IsLarge = isLarge;

        this.UpdateCachedGuild(guild, rawMembers);

        await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild });
    }

    internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
    {
        ConcurrentDictionary<ulong, DiscordEmoji> oldEmojis = new ConcurrentDictionary<ulong, DiscordEmoji>(guild._emojis);
        guild._emojis.Clear();

        foreach (DiscordEmoji emoji in newEmojis)
        {
            emoji.Discord = this;
            guild._emojis[emoji.Id] = emoji;
        }

        GuildEmojisUpdateEventArgs ea = new GuildEmojisUpdateEventArgs
        {
            Guild = guild,
            EmojisAfter = guild.Emojis,
            EmojisBefore = oldEmojis
        };
        await this._guildEmojisUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
    {
        GuildIntegrationsUpdateEventArgs ea = new GuildIntegrationsUpdateEventArgs
        {
            Guild = guild
        };
        await this._guildIntegrationsUpdated.InvokeAsync(this, ea);
    }

    private async Task OnGuildAuditLogEntryCreateEventAsync(DiscordGuild guild, DiscordAuditLogEntry auditLogEntry)
    {
        GuildAuditLogCreatedEventArgs ea = new()
        {
            Guild = guild,
            AuditLogEntry = auditLogEntry
        };
        await _guildAuditLogCreated.InvokeAsync(this, ea);
    }

    #endregion

    #region Guild Ban

    internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
    {
        DiscordUser usr = new DiscordUser(user) { Discord = this };
        usr = this.UpdateUserCache(usr);

        if (!guild.Members.TryGetValue(user.Id, out DiscordMember? mbr))
        {
            mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
        }

        GuildBanAddEventArgs ea = new GuildBanAddEventArgs
        {
            Guild = guild,
            Member = mbr
        };
        await this._guildBanAdded.InvokeAsync(this, ea);
    }

    internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
    {
        DiscordUser usr = new DiscordUser(user) { Discord = this };
        usr = this.UpdateUserCache(usr);

        if (!guild.Members.TryGetValue(user.Id, out DiscordMember? mbr))
        {
            mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
        }

        GuildBanRemoveEventArgs ea = new GuildBanRemoveEventArgs
        {
            Guild = guild,
            Member = mbr
        };
        await this._guildBanRemoved.InvokeAsync(this, ea);
    }

    #endregion

    #region Guild Member

    internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
    {
        DiscordUser usr = new DiscordUser(member.User) { Discord = this };
        usr = this.UpdateUserCache(usr);

        DiscordMember mbr = new DiscordMember(member)
        {
            Discord = this,
            _guild_id = guild.Id
        };

        guild._members[mbr.Id] = mbr;
        guild.MemberCount++;

        GuildMemberAddEventArgs ea = new GuildMemberAddEventArgs
        {
            Guild = guild,
            Member = mbr
        };
        await this._guildMemberAdded.InvokeAsync(this, ea);
    }

    internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
    {
        DiscordUser usr = new DiscordUser(user);

        if (!guild._members.TryRemove(user.Id, out DiscordMember? mbr))
        {
            mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
        }

        guild.MemberCount--;

        this.UpdateUserCache(usr);

        GuildMemberRemoveEventArgs ea = new GuildMemberRemoveEventArgs
        {
            Guild = guild,
            Member = mbr
        };
        await this._guildMemberRemoved.InvokeAsync(this, ea);
    }

    internal async Task OnGuildMemberUpdateEventAsync(TransportMember member, DiscordGuild guild)
    {
        DiscordUser userAfter = new DiscordUser(member.User) { Discord = this };
        _ = this.UpdateUserCache(userAfter);

        DiscordMember memberAfter = new DiscordMember(member) { Discord = this, _guild_id = guild.Id };

        if (!guild.Members.TryGetValue(member.User.Id, out DiscordMember? memberBefore))
        {
            memberBefore = new DiscordMember(member) { Discord = this, _guild_id = guild.Id };
        }

        guild._members.AddOrUpdate(member.User.Id, memberAfter, (_, _) => memberAfter);

        GuildMemberUpdateEventArgs ea = new GuildMemberUpdateEventArgs
        {
            Guild = guild,
            MemberAfter = memberAfter,
            MemberBefore = memberBefore,
        };

        await this._guildMemberUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildMembersChunkEventAsync(JObject dat)
    {
        DiscordGuild guild = this.Guilds[(ulong)dat["guild_id"]];
        int chunkIndex = (int)dat["chunk_index"];
        int chunkCount = (int)dat["chunk_count"];
        string? nonce = (string)dat["nonce"];

        HashSet<DiscordMember> mbrs = new HashSet<DiscordMember>();
        HashSet<DiscordPresence> pres = new HashSet<DiscordPresence>();

        TransportMember[] members = dat["members"].ToDiscordObject<TransportMember[]>();

        int memCount = members.Count();
        for (int i = 0; i < memCount; i++)
        {
            DiscordMember mbr = new DiscordMember(members[i]) { Discord = this, _guild_id = guild.Id };

            if (!this.UserCache.ContainsKey(mbr.Id))
            {
                this.UserCache[mbr.Id] = new DiscordUser(members[i].User) { Discord = this };
            }

            guild._members[mbr.Id] = mbr;

            mbrs.Add(mbr);
        }

        guild.MemberCount = guild._members.Count;

        GuildMembersChunkEventArgs ea = new GuildMembersChunkEventArgs
        {
            Guild = guild,
            Members = new ReadOnlySet<DiscordMember>(mbrs),
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

                pres.Add(xp);
            }

            ea.Presences = new ReadOnlySet<DiscordPresence>(pres);
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

    internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, DiscordGuild guild)
    {
        role.Discord = this;
        role._guild_id = guild.Id;

        guild._roles[role.Id] = role;

        GuildRoleCreateEventArgs ea = new GuildRoleCreateEventArgs
        {
            Guild = guild,
            Role = role
        };
        await this._guildRoleCreated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
    {
        DiscordRole newRole = guild.GetRole(role.Id);
        DiscordRole oldRole = new DiscordRole
        {
            _guild_id = guild.Id,
            _color = newRole._color,
            Discord = this,
            IsHoisted = newRole.IsHoisted,
            Id = newRole.Id,
            IsManaged = newRole.IsManaged,
            IsMentionable = newRole.IsMentionable,
            Name = newRole.Name,
            Permissions = newRole.Permissions,
            Position = newRole.Position,
            IconHash = newRole.IconHash,
            _emoji = newRole._emoji
        };

        newRole._guild_id = guild.Id;
        newRole._color = role._color;
        newRole.IsHoisted = role.IsHoisted;
        newRole.IsManaged = role.IsManaged;
        newRole.IsMentionable = role.IsMentionable;
        newRole.Name = role.Name;
        newRole.Permissions = role.Permissions;
        newRole.Position = role.Position;
        newRole._emoji = role._emoji;
        newRole.IconHash = role.IconHash;

        GuildRoleUpdateEventArgs ea = new GuildRoleUpdateEventArgs
        {
            Guild = guild,
            RoleAfter = newRole,
            RoleBefore = oldRole
        };
        await this._guildRoleUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, DiscordGuild guild)
    {
        if (!guild._roles.TryRemove(roleId, out DiscordRole? role))
        {
            this.Logger.LogWarning($"Attempted to delete a nonexistent role ({roleId}) from guild ({guild}).");
        }

        GuildRoleDeleteEventArgs ea = new GuildRoleDeleteEventArgs
        {
            Guild = guild,
            Role = role
        };
        await this._guildRoleDeleted.InvokeAsync(this, ea);
    }

    #endregion

    #region Invite

    internal async Task OnInviteCreateEventAsync(ulong channelId, ulong guildId, DiscordInvite invite)
    {
        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordChannel channel = this.InternalGetCachedChannel(channelId);

        invite.Discord = this;

        guild._invites[invite.Code] = invite;

        InviteCreateEventArgs ea = new InviteCreateEventArgs
        {
            Channel = channel,
            Guild = guild,
            Invite = invite
        };
        await this._inviteCreated.InvokeAsync(this, ea);
    }

    internal async Task OnInviteDeleteEventAsync(ulong channelId, ulong guildId, JToken dat)
    {
        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordChannel channel = this.InternalGetCachedChannel(channelId);

        if (!guild._invites.TryRemove(dat["code"].ToString(), out DiscordInvite? invite))
        {
            invite = dat.ToDiscordObject<DiscordInvite>();
            invite.Discord = this;
        }

        invite.IsRevoked = true;

        InviteDeleteEventArgs ea = new InviteDeleteEventArgs
        {
            Channel = channel,
            Guild = guild,
            Invite = invite
        };
        await this._inviteDeleted.InvokeAsync(this, ea);
    }

    #endregion

    #region Message

    internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong messageId)
    {
        if (this.MessageCache == null || !this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = chn.Id,
                Discord = this,
            };
        }

        await this._messageAcknowledged.InvokeAsync(this, new MessageAcknowledgeEventArgs { Message = msg });
    }

    internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
    {
        message.Discord = this;
        this.PopulateMessageReactionsAndCache(message, author, member);
        message.PopulateMentions();

        if (message.Channel == null && message.ChannelId == default)
        {
            this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Channel which the last message belongs to is not in cache - cache state might be invalid!");
        }

        if (message.ReferencedMessage != null)
        {
            message.ReferencedMessage.Discord = this;
            this.PopulateMessageReactionsAndCache(message.ReferencedMessage, referenceAuthor, referenceMember);
            message.ReferencedMessage.PopulateMentions();
        }

        foreach (DiscordMessageSticker sticker in message.Stickers)
        {
            sticker.Discord = this;
        }

        MessageCreateEventArgs ea = new MessageCreateEventArgs
        {
            Message = message,

            MentionedUsers = new ReadOnlyCollection<DiscordUser>(message._mentionedUsers),
            MentionedRoles = message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message._mentionedRoles) : null,
            MentionedChannels = message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message._mentionedChannels) : null
        };
        await this._messageCreated.InvokeAsync(this, ea);
    }

    internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
    {
        DiscordGuild guild;

        message.Discord = this;
        DiscordMessage event_message = message;

        DiscordMessage oldmsg = null;
        if (this.Configuration.MessageCacheSize == 0
            || this.MessageCache == null
            || !this.MessageCache.TryGet(event_message.Id, out message)) // previous message was not in cache
        {
            message = event_message;
            this.PopulateMessageReactionsAndCache(message, author, member);
            guild = message.Channel?.Guild;

            if (message.ReferencedMessage != null)
            {
                message.ReferencedMessage.Discord = this;
                this.PopulateMessageReactionsAndCache(message.ReferencedMessage, referenceAuthor, referenceMember);
                message.ReferencedMessage.PopulateMentions();
            }
        }
        else // previous message was fetched in cache
        {
            oldmsg = new DiscordMessage(message);

            // cached message is updated with information from the event message
            guild = message.Channel?.Guild;
            message.EditedTimestamp = event_message.EditedTimestamp;
            if (event_message.Content != null)
            {
                message.Content = event_message.Content;
            }

            message._embeds.Clear();
            message._embeds.AddRange(event_message._embeds);
            message._attachments.Clear();
            message._attachments.AddRange(event_message._attachments);
            message.Pinned = event_message.Pinned;
            message.IsTTS = event_message.IsTTS;

            // Mentions
            message._mentionedUsers.Clear();
            message._mentionedUsers.AddRange(event_message._mentionedUsers ?? new());
            message._mentionedRoles.Clear();
            message._mentionedRoles.AddRange(event_message._mentionedRoles ?? new());
            message._mentionedChannels.Clear();
            message._mentionedChannels.AddRange(event_message._mentionedChannels ?? new());
            message.MentionEveryone = event_message.MentionEveryone;
        }

        message.PopulateMentions();

        MessageUpdateEventArgs ea = new MessageUpdateEventArgs
        {
            Message = message,
            MessageBefore = oldmsg,
            MentionedUsers = new ReadOnlyCollection<DiscordUser>(message._mentionedUsers),
            MentionedRoles = message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message._mentionedRoles) : null,
            MentionedChannels = message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message._mentionedChannels) : null
        };
        await this._messageUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId)
    {
        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

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

        MessageDeleteEventArgs ea = new MessageDeleteEventArgs
        {
            Message = msg,
            Channel = channel,
            Guild = guild,
        };
        await this._messageDeleted.InvokeAsync(this, ea);
    }

    internal async Task OnMessageBulkDeleteEventAsync(ulong[] messageIds, ulong channelId, ulong? guildId)
    {
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

        List<DiscordMessage> msgs = new List<DiscordMessage>(messageIds.Length);
        foreach (ulong messageId in messageIds)
        {
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

            msgs.Add(msg);
        }

        DiscordGuild guild = this.InternalGetCachedGuild(guildId);

        MessageBulkDeleteEventArgs ea = new MessageBulkDeleteEventArgs
        {
            Channel = channel,
            Messages = new ReadOnlyCollection<DiscordMessage>(msgs),
            Guild = guild
        };
        await this._messagesBulkDeleted.InvokeAsync(this, ea);
    }

    #endregion

    #region Message Reaction

    internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, TransportMember mbr, DiscordEmoji emoji)
    {
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);
        DiscordGuild? guild = this.InternalGetCachedGuild(guildId);

        emoji.Discord = this;

        DiscordUser usr = null!;
        usr = !this.TryGetCachedUserInternal(userId, out usr)
            ? this.UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr)
            : this.UpdateUser(usr, guild?.Id, guild, mbr);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = ChannelType.Private,
                Recipients = new DiscordUser[] { usr }
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
                _reactions = new List<DiscordReaction>()
            };
        }

        DiscordReaction? react = msg._reactions.FirstOrDefault(xr => xr.Emoji == emoji);
        if (react == null)
        {
            msg._reactions.Add(react = new DiscordReaction
            {
                Count = 1,
                Emoji = emoji,
                IsMe = this.CurrentUser.Id == userId
            });
        }
        else
        {
            react.Count++;
            react.IsMe |= this.CurrentUser.Id == userId;
        }

        MessageReactionAddEventArgs ea = new MessageReactionAddEventArgs
        {
            Message = msg,
            User = usr,
            Guild = guild,
            Emoji = emoji
        };
        await this._messageReactionAdded.InvokeAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji)
    {
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

        emoji.Discord = this;

        if (!this.UserCache.TryGetValue(userId, out DiscordUser? usr))
        {
            usr = new DiscordUser { Id = userId, Discord = this };
        }

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = ChannelType.Private,
                Recipients = new DiscordUser[] { usr }
            };
            this._privateChannels[channelId] = (DiscordDmChannel)channel;
        }

        if (channel?.Guild != null)
        {
            usr = channel.Guild.Members.TryGetValue(userId, out DiscordMember? member)
                ? member
                : new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId.Value };
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

        DiscordReaction? react = msg._reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
        if (react != null)
        {
            react.Count--;
            react.IsMe &= this.CurrentUser.Id != userId;

            if (msg._reactions != null && react.Count <= 0) // shit happens
            {
                for (int i = 0; i < msg._reactions.Count; i++)
                {
                    if (msg._reactions[i].Emoji == emoji)
                    {
                        msg._reactions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        DiscordGuild guild = this.InternalGetCachedGuild(guildId);

        MessageReactionRemoveEventArgs ea = new MessageReactionRemoveEventArgs
        {
            Message = msg,
            User = usr,
            Guild = guild,
            Emoji = emoji
        };
        await this._messageReactionRemoved.InvokeAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, ulong channelId, ulong? guildId)
    {
        DiscordChannel channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

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

        DiscordGuild guild = this.InternalGetCachedGuild(guildId);

        MessageReactionsClearEventArgs ea = new MessageReactionsClearEventArgs
        {
            Message = msg,
        };

        await this._messageReactionsCleared.InvokeAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveEmojiAsync(ulong messageId, ulong channelId, ulong guildId, JToken dat)
    {
        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordChannel? channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

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

        MessageReactionRemoveEmojiEventArgs ea = new MessageReactionRemoveEmojiEventArgs
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
        PresenceUpdateEventArgs ea = new PresenceUpdateEventArgs
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
        DiscordUser usr = new DiscordUser(user) { Discord = this };

        UserSettingsUpdateEventArgs ea = new UserSettingsUpdateEventArgs
        {
            User = usr
        };
        await this._userSettingsUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnUserUpdateEventAsync(TransportUser user)
    {
        DiscordUser usr_old = new DiscordUser
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

        UserUpdateEventArgs ea = new UserUpdateEventArgs
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
            this.UpdateUser(new DiscordUser(transportMbr.User) { Discord = this }, gid, gld, transportMbr);
        }

        VoiceStateUpdateEventArgs ea = new VoiceStateUpdateEventArgs
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

    internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
    {
        VoiceServerUpdateEventArgs ea = new VoiceServerUpdateEventArgs
        {
            Endpoint = endpoint,
            VoiceToken = token,
            Guild = guild
        };
        await this._voiceServerUpdated.InvokeAsync(this, ea);
    }

    #endregion

    #region Thread

    internal async Task OnThreadCreateEventAsync(DiscordThreadChannel thread, bool isNew)
    {
        thread.Discord = this;
        this.InternalGetCachedGuild(thread.GuildId)._threads[thread.Id] = thread;

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

        DiscordThreadChannel cthread = this.InternalGetCachedThread(thread.Id);

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

    internal async Task OnThreadListSyncEventAsync(DiscordGuild guild, IReadOnlyList<ulong> channel_ids, IReadOnlyList<DiscordThreadChannel> threads, IReadOnlyList<DiscordThreadChannelMember> members)
    {
        guild.Discord = this;
        IEnumerable<DiscordChannel> channels = channel_ids.Select(x => guild.GetChannel(x) ?? new DiscordChannel { Id = x, GuildId = guild.Id }); //getting channel objects

        foreach (DiscordChannel? channel in channels)
        {
            channel.Discord = this;
        }

        foreach (DiscordThreadChannel thread in threads)
        {
            thread.Discord = this;
            guild._threads[thread.Id] = thread;
        }

        foreach (DiscordThreadChannelMember member in members)
        {
            member.Discord = this;
            member._guild_id = guild.Id;

            DiscordThreadChannel? thread = threads.SingleOrDefault(x => x.Id == member.ThreadId);
            if (thread != null)
            {
                thread.CurrentMember = member;
            }
        }

        await this._threadListSynced.InvokeAsync(this, new ThreadListSyncEventArgs { Guild = guild, Channels = channels.ToList().AsReadOnly(), Threads = threads, CurrentMembers = members.ToList().AsReadOnly() });
    }

    internal async Task OnThreadMemberUpdateEventAsync(DiscordThreadChannelMember member)
    {
        member.Discord = this;

        DiscordThreadChannel thread = this.InternalGetCachedThread(member.ThreadId);
        member._guild_id = thread.Guild.Id;
        thread.CurrentMember = member;
        thread.Guild._threads[thread.Id] = thread;

        await this._threadMemberUpdated.InvokeAsync(this, new ThreadMemberUpdateEventArgs { ThreadMember = member, Thread = thread });
    }

    internal async Task OnThreadMembersUpdateEventAsync(DiscordGuild guild, ulong thread_id, IReadOnlyList<DiscordThreadChannelMember> addedMembers, IReadOnlyList<ulong?> removed_member_ids, int member_count)
    {
        DiscordThreadChannel? thread = this.InternalGetCachedThread(thread_id) ?? new DiscordThreadChannel
        {
            Id = thread_id,
            GuildId = guild.Id,
        };
        thread.Discord = this;
        guild.Discord = this;
        thread.MemberCount = member_count;

        List<DiscordMember> removedMembers = new List<DiscordMember>();
        if (removed_member_ids != null)
        {
            foreach (ulong? removedId in removed_member_ids)
            {
                removedMembers.Add(guild._members.TryGetValue(removedId.Value, out DiscordMember? member) ? member : new DiscordMember { Id = removedId.Value, _guild_id = guild.Id, Discord = this });
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
                threadMember._guild_id = guild.Id;
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

        ThreadMembersUpdateEventArgs threadMembersUpdateArg = new ThreadMembersUpdateEventArgs
        {
            Guild = guild,
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
        DiscordGuild? guild = this.InternalGetCachedGuild(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };
        IntegrationCreateEventArgs ea = new IntegrationCreateEventArgs
        {
            Guild = guild,
            Integration = integration
        };

        await this._integrationCreated.InvokeAsync(this, ea);
    }

    internal async Task OnIntegrationUpdateAsync(DiscordIntegration integration, ulong guild_id)
    {
        DiscordGuild? guild = this.InternalGetCachedGuild(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };
        IntegrationUpdateEventArgs ea = new IntegrationUpdateEventArgs
        {
            Guild = guild,
            Integration = integration
        };

        await this._integrationUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnIntegrationDeleteAsync(ulong integration_id, ulong guild_id, ulong? application_id)
    {
        DiscordGuild? guild = this.InternalGetCachedGuild(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };
        IntegrationDeleteEventArgs ea = new IntegrationDeleteEventArgs
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

        DiscordGuild guild = this.InternalGetCachedGuild(instance.GuildId);

        guild._stageInstances[instance.Id] = instance;

        StageInstanceCreateEventArgs eventArgs = new StageInstanceCreateEventArgs
        {
            StageInstance = instance
        };

        await this._stageInstanceCreated.InvokeAsync(this, eventArgs);
    }

    internal async Task OnStageInstanceUpdateAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = this.InternalGetCachedGuild(instance.GuildId);

        if (!guild._stageInstances.TryRemove(instance.Id, out DiscordStageInstance? oldInstance))
        {
            oldInstance = new DiscordStageInstance { Id = instance.Id, GuildId = instance.GuildId, ChannelId = instance.ChannelId };
        }

        guild._stageInstances[instance.Id] = instance;

        StageInstanceUpdateEventArgs eventArgs = new StageInstanceUpdateEventArgs
        {
            StageInstanceBefore = oldInstance,
            StageInstanceAfter = instance
        };

        await this._stageInstanceUpdated.InvokeAsync(this, eventArgs);
    }

    internal async Task OnStageInstanceDeleteAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = this.InternalGetCachedGuild(instance.GuildId);

        guild._stageInstances.TryRemove(instance.Id, out _);

        StageInstanceDeleteEventArgs eventArgs = new StageInstanceDeleteEventArgs
        {
            StageInstance = instance
        };

        await this._stageInstanceDeleted.InvokeAsync(this, eventArgs);
    }

    #endregion

    #region Misc

    internal async Task OnInteractionCreateAsync(ulong? guildId, ulong channelId, TransportUser user, TransportMember member, DiscordInteraction interaction)
    {
        DiscordUser usr = new DiscordUser(user) { Discord = this };

        interaction.ChannelId = channelId;
        interaction.GuildId = guildId;
        interaction.Discord = this;
        interaction.Data.Discord = this;

        if (member != null)
        {
            usr = new DiscordMember(member) { _guild_id = guildId.Value, Discord = this };
            this.UpdateUser(usr, guildId, interaction.Guild, member);
        }
        else
        {
            this.UpdateUserCache(usr);
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
                    this.UpdateUserCache(c.Value);
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

                    this.UpdateUserCache(c.Value.User);
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
            ComponentInteractionCreateEventArgs cea = new ComponentInteractionCreateEventArgs
            {
                Message = interaction.Message,
                Interaction = interaction
            };

            await this._componentInteractionCreated.InvokeAsync(this, cea);
        }
        else if (interaction.Type is InteractionType.ModalSubmit)
        {
            ModalSubmitEventArgs mea = new ModalSubmitEventArgs(interaction);

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

    internal async Task OnTypingStartEventAsync(ulong userId, ulong channelId, DiscordChannel channel, ulong? guildId, DateTimeOffset started, TransportMember mbr)
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

        DiscordGuild guild = this.InternalGetCachedGuild(guildId);
        DiscordUser usr = this.UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr);

        TypingStartEventArgs ea = new TypingStartEventArgs
        {
            Channel = channel,
            User = usr,
            Guild = guild,
            StartedAt = started
        };
        await this._typingStarted.InvokeAsync(this, ea);
    }

    internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
    {
        WebhooksUpdateEventArgs ea = new WebhooksUpdateEventArgs
        {
            Channel = channel,
            Guild = guild
        };
        await this._webhooksUpdated.InvokeAsync(this, ea);
    }

    internal async Task OnStickersUpdatedAsync(IEnumerable<DiscordMessageSticker> newStickers, JObject raw)
    {
        DiscordGuild guild = this.InternalGetCachedGuild((ulong)raw["guild_id"]);
        ConcurrentDictionary<ulong, DiscordMessageSticker> oldStickers = new ConcurrentDictionary<ulong, DiscordMessageSticker>(guild._stickers);

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

        GuildStickersUpdateEventArgs sea = new GuildStickersUpdateEventArgs
        {
            Guild = guild,
            StickersBefore = oldStickers,
            StickersAfter = guild.Stickers
        };

        await this._guildStickersUpdated.InvokeAsync(this, sea);
    }

    internal async Task OnUnknownEventAsync(GatewayPayload payload)
    {
        UnknownEventArgs ea = new UnknownEventArgs { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
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
