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

    private string sessionId;
    private string? gatewayResumeUrl;
    private bool guildDownloadCompleted = false;

    #endregion

    #region Dispatch Handler

    private async Task ReceiveGatewayEventsAsync()
    {
        while (!this.eventReader.Completion.IsCompleted)
        {
            GatewayPayload payload = await this.eventReader.ReadAsync();

            try
            {
                await HandleDispatchAsync(payload);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Dispatch threw an exception: ");
            }
        }
    }

    internal async Task HandleDispatchAsync(GatewayPayload payload)
    {
        if (payload.Data is not JObject dat)
        {
            this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Invalid payload body (this message is probably safe to ignore); opcode: {Op} event: {Event}; payload: {Payload}", payload.OpCode, payload.EventName, payload.Data);
            return;
        }

        if (payload.OpCode is not GatewayOpCode.Dispatch)
        {
            return;
        }

        DiscordChannel chn;
        DiscordThreadChannel thread;
        ulong gid;
        ulong cid;
        TransportUser usr;
        TransportMember mbr = default;
        TransportUser refUsr = default;
        TransportMember refMbr = default;
        JToken rawMbr;
        JToken? rawRefMsg = dat["referenced_message"];
        JArray rawMembers;
        JArray rawPresences;

        switch (payload.EventName.ToLowerInvariant())
        {
            #region Gateway Status

            case "ready":
                JArray? glds = (JArray)dat["guilds"];
                JArray? dmcs = (JArray)dat["private_channels"];

                dat.Remove("guilds");
                dat.Remove("private_channels");

                int readyShardId = payload is ShardIdContainingGatewayPayload { ShardId: { } id } ? id : 0;

                await OnReadyEventAsync(dat.ToDiscordObject<ReadyPayload>(), glds, dmcs, readyShardId);
                break;

            case "resumed":
                int resumedShardId = payload is ShardIdContainingGatewayPayload { ShardId: { } otherId } ? otherId : 0;

                await OnResumedAsync(resumedShardId);
                break;

            #endregion

            #region Channel

            case "channel_create":
                chn = dat.ToDiscordObject<DiscordChannel>();
                await OnChannelCreateEventAsync(chn);
                break;

            case "channel_update":
                await OnChannelUpdateEventAsync(dat.ToDiscordObject<DiscordChannel>());
                break;

            case "channel_delete":
                bool isPrivate = dat["is_private"]?.ToObject<bool>() ?? false;

                chn = isPrivate ? dat.ToDiscordObject<DiscordDmChannel>() : dat.ToDiscordObject<DiscordChannel>();
                await OnChannelDeleteEventAsync(chn);
                break;

            case "channel_pins_update":
                cid = (ulong)dat["channel_id"];
                string? ts = (string)dat["last_pin_timestamp"];
                await OnChannelPinsUpdateAsync((ulong?)dat["guild_id"], cid, ts != null ? DateTimeOffset.Parse(ts, CultureInfo.InvariantCulture) : default(DateTimeOffset?));
                break;

            #endregion

            #region Scheduled Guild Events

            case "guild_scheduled_event_create":
                DiscordScheduledGuildEvent cevt = dat.ToDiscordObject<DiscordScheduledGuildEvent>();
                await OnScheduledGuildEventCreateEventAsync(cevt);
                break;
            case "guild_scheduled_event_delete":
                DiscordScheduledGuildEvent devt = dat.ToDiscordObject<DiscordScheduledGuildEvent>();
                await OnScheduledGuildEventDeleteEventAsync(devt);
                break;
            case "guild_scheduled_event_update":
                DiscordScheduledGuildEvent uevt = dat.ToDiscordObject<DiscordScheduledGuildEvent>();
                await OnScheduledGuildEventUpdateEventAsync(uevt);
                break;
            case "guild_scheduled_event_user_add":
                gid = (ulong)dat["guild_id"];
                ulong uid = (ulong)dat["user_id"];
                ulong eid = (ulong)dat["guild_scheduled_event_id"];
                await OnScheduledGuildEventUserAddEventAsync(gid, eid, uid);
                break;
            case "guild_scheduled_event_user_remove":
                gid = (ulong)dat["guild_id"];
                uid = (ulong)dat["user_id"];
                eid = (ulong)dat["guild_scheduled_event_id"];
                await OnScheduledGuildEventUserRemoveEventAsync(gid, eid, uid);
                break;
            #endregion

            #region Guild

            case "guild_create":

                rawMembers = (JArray)dat["members"];
                rawPresences = (JArray)dat["presences"];
                dat.Remove("members");
                dat.Remove("presences");

                await OnGuildCreateEventAsync(dat.ToDiscordObject<DiscordGuild>(), rawMembers, rawPresences.ToDiscordObject<IEnumerable<DiscordPresence>>());
                break;

            case "guild_update":

                rawMembers = (JArray)dat["members"];
                dat.Remove("members");

                await OnGuildUpdateEventAsync(dat.ToDiscordObject<DiscordGuild>(), rawMembers);
                break;

            case "guild_delete":
                dat.Remove("members");

                await OnGuildDeleteEventAsync(dat.ToDiscordObject<DiscordGuild>());
                break;

            case "guild_emojis_update":
                gid = (ulong)dat["guild_id"];
                IEnumerable<DiscordEmoji> ems = dat["emojis"].ToDiscordObject<IEnumerable<DiscordEmoji>>();
                await OnGuildEmojisUpdateEventAsync(this.guilds[gid], ems);
                break;

            case "guild_integrations_update":
                gid = (ulong)dat["guild_id"];

                // discord fires this event inconsistently if the current user leaves a guild.
                if (!this.guilds.TryGetValue(gid, out DiscordGuild value))
                {
                    return;
                }

                await OnGuildIntegrationsUpdateEventAsync(value);
                break;

            case "guild_audit_log_entry_create":
                gid = (ulong)dat["guild_id"];
                DiscordGuild guild = this.guilds[gid];
                AuditLogAction auditLogAction = dat.ToDiscordObject<AuditLogAction>();
                DiscordAuditLogEntry entry = await AuditLogParser.ParseAuditLogEntryAsync(guild, auditLogAction);
                await OnGuildAuditLogEntryCreateEventAsync(guild, entry);
                break;

            #endregion

            #region Guild Ban

            case "guild_ban_add":
                usr = dat["user"].ToDiscordObject<TransportUser>();
                gid = (ulong)dat["guild_id"];
                await OnGuildBanAddEventAsync(usr, this.guilds[gid]);
                break;

            case "guild_ban_remove":
                usr = dat["user"].ToDiscordObject<TransportUser>();
                gid = (ulong)dat["guild_id"];
                await OnGuildBanRemoveEventAsync(usr, this.guilds[gid]);
                break;

            #endregion

            #region Guild Member

            case "guild_member_add":
                gid = (ulong)dat["guild_id"];
                await OnGuildMemberAddEventAsync(dat.ToDiscordObject<TransportMember>(), this.guilds[gid]);
                break;

            case "guild_member_remove":
                gid = (ulong)dat["guild_id"];
                usr = dat["user"].ToDiscordObject<TransportUser>();

                if (!this.guilds.TryGetValue(gid, out value))
                {
                    // discord fires this event inconsistently if the current user leaves a guild.
                    if (usr.Id != this.CurrentUser.Id)
                    {
                        this.Logger.LogError(LoggerEvents.WebSocketReceive, "Could not find {Guild} in guild cache", gid);
                    }

                    return;
                }

                await OnGuildMemberRemoveEventAsync(usr, value);
                break;

            case "guild_member_update":
                gid = (ulong)dat["guild_id"];
                await OnGuildMemberUpdateEventAsync(dat.ToDiscordObject<TransportMember>(), this.guilds[gid]);
                break;

            case "guild_members_chunk":
                await OnGuildMembersChunkEventAsync(dat);
                break;

            #endregion

            #region Guild Role

            case "guild_role_create":
                gid = (ulong)dat["guild_id"];
                await OnGuildRoleCreateEventAsync(dat["role"].ToDiscordObject<DiscordRole>(), this.guilds[gid]);
                break;

            case "guild_role_update":
                gid = (ulong)dat["guild_id"];
                await OnGuildRoleUpdateEventAsync(dat["role"].ToDiscordObject<DiscordRole>(), this.guilds[gid]);
                break;

            case "guild_role_delete":
                gid = (ulong)dat["guild_id"];
                await OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], this.guilds[gid]);
                break;

            #endregion

            #region Invite

            case "invite_create":
                gid = (ulong)dat["guild_id"];
                cid = (ulong)dat["channel_id"];
                await OnInviteCreateEventAsync(cid, gid, dat.ToDiscordObject<DiscordInvite>());
                break;

            case "invite_delete":
                gid = (ulong)dat["guild_id"];
                cid = (ulong)dat["channel_id"];
                await OnInviteDeleteEventAsync(cid, gid, dat);
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

                await OnMessageCreateEventAsync(dat.ToDiscordObject<DiscordMessage>(), author, mbr, refUsr, refMbr);
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

                await OnMessageUpdateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"]?.ToDiscordObject<TransportUser>(), mbr, refUsr, refMbr);
                break;

            // delete event does *not* include message object
            case "message_delete":
                await OnMessageDeleteEventAsync((ulong)dat["id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]);
                break;

            case "message_delete_bulk":
                await OnMessageBulkDeleteEventAsync(dat["ids"].ToDiscordObject<ulong[]>(), (ulong)dat["channel_id"], (ulong?)dat["guild_id"]);
                break;

            case "message_poll_vote_add":
                await OnMessagePollVoteEventAsync(dat.ToDiscordObject<DiscordPollVoteUpdate>(), true);
                break;

            case "message_poll_vote_remove":
                await OnMessagePollVoteEventAsync(dat.ToDiscordObject<DiscordPollVoteUpdate>(), false);
                break;

            #endregion

            #region Message Reaction

            case "message_reaction_add":
                rawMbr = dat["member"];

                if (rawMbr != null)
                {
                    mbr = rawMbr.ToDiscordObject<TransportMember>();
                }

                await OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], mbr, dat["emoji"].ToDiscordObject<DiscordEmoji>());
                break;

            case "message_reaction_remove":
                await OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], dat["emoji"].ToDiscordObject<DiscordEmoji>());
                break;

            case "message_reaction_remove_all":
                await OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]);
                break;

            case "message_reaction_remove_emoji":
                await OnMessageReactionRemoveEmojiAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong)dat["guild_id"], dat["emoji"]);
                break;

            #endregion

            #region User/Presence Update

            case "presence_update":
                // Presences are a mess. I'm not touching this. ~Velvet
                await OnPresenceUpdateEventAsync(dat, (JObject)dat["user"]);
                break;

            case "user_settings_update":
                await OnUserSettingsUpdateEventAsync(dat.ToDiscordObject<TransportUser>());
                break;

            case "user_update":
                await OnUserUpdateEventAsync(dat.ToDiscordObject<TransportUser>());
                break;

            #endregion

            #region Voice

            case "voice_state_update":
                await OnVoiceStateUpdateEventAsync(dat);
                break;

            case "voice_server_update":
                gid = (ulong)dat["guild_id"];
                await OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], this.guilds[gid]);
                break;

            #endregion

            #region Thread

            case "thread_create":
                thread = dat.ToDiscordObject<DiscordThreadChannel>();
                await OnThreadCreateEventAsync(thread);
                break;

            case "thread_update":
                thread = dat.ToDiscordObject<DiscordThreadChannel>();
                await OnThreadUpdateEventAsync(thread);
                break;

            case "thread_delete":
                thread = dat.ToDiscordObject<DiscordThreadChannel>();
                await OnThreadDeleteEventAsync(thread);
                break;

            case "thread_list_sync":
                gid = (ulong)dat["guild_id"]; //get guild
                await OnThreadListSyncEventAsync(this.guilds[gid], dat["channel_ids"].ToDiscordObject<IReadOnlyList<ulong>>(), dat["threads"].ToDiscordObject<IReadOnlyList<DiscordThreadChannel>>(), dat["members"].ToDiscordObject<IReadOnlyList<DiscordThreadChannelMember>>());
                break;

            case "thread_member_update":
                gid = (ulong)dat["guild_id"];
                await OnThreadMemberUpdateEventAsync(this.guilds[gid], dat.ToDiscordObject<DiscordThreadChannelMember>());
                break;

            case "thread_members_update":
                gid = (ulong)dat["guild_id"];
                await OnThreadMembersUpdateEventAsync(this.guilds[gid], (ulong)dat["id"], dat["added_members"]?.ToDiscordObject<IReadOnlyList<DiscordThreadChannelMember>>(), dat["removed_member_ids"]?.ToDiscordObject<IReadOnlyList<ulong?>>(), (int)dat["member_count"]);
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

                JToken? rawChannel = dat["channel"];
                DiscordChannel? channel = null;
                if (rawChannel is not null)
                {
                    channel = rawChannel.ToDiscordObject<DiscordChannel>();
                    channel.Discord = this;
                }

                // Re: Removing re-serialized data: This one is probably fine?
                // The user on the object is marked with [JsonIgnore].

                cid = (ulong)dat["channel_id"];
                await OnInteractionCreateAsync((ulong?)dat["guild_id"], cid, usr, mbr, channel, dat.ToDiscordObject<DiscordInteraction>());
                break;

            case "integration_create":
                await OnIntegrationCreateAsync(dat.ToDiscordObject<DiscordIntegration>(), (ulong)dat["guild_id"]);
                break;

            case "integration_update":
                await OnIntegrationUpdateAsync(dat.ToDiscordObject<DiscordIntegration>(), (ulong)dat["guild_id"]);
                break;

            case "integration_delete":
                await OnIntegrationDeleteAsync((ulong)dat["id"], (ulong)dat["guild_id"], (ulong?)dat["application_id"]);
                break;

            case "application_command_permissions_update":
                await OnApplicationCommandPermissionsUpdateAsync(dat);
                break;
            #endregion

            #region Stage Instance

            case "stage_instance_create":
                await OnStageInstanceCreateAsync(dat.ToDiscordObject<DiscordStageInstance>());
                break;

            case "stage_instance_update":
                await OnStageInstanceUpdateAsync(dat.ToDiscordObject<DiscordStageInstance>());
                break;

            case "stage_instance_delete":
                await OnStageInstanceDeleteAsync(dat.ToDiscordObject<DiscordStageInstance>());
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

                ulong? guildId = (ulong?)dat["guild_id"];
                await OnTypingStartEventAsync((ulong)dat["user_id"], cid, InternalGetCachedChannel(cid, guildId)!, guildId, Utilities.GetDateTimeOffset((long)dat["timestamp"]), mbr);
                break;

            case "webhooks_update":
                gid = (ulong)dat["guild_id"];
                cid = (ulong)dat["channel_id"];
                await OnWebhooksUpdateAsync(this.guilds[gid].GetChannel(cid), this.guilds[gid]);
                break;

            case "guild_stickers_update":
                IEnumerable<DiscordMessageSticker> strs = dat["stickers"].ToDiscordObject<IEnumerable<DiscordMessageSticker>>();
                await OnStickersUpdatedAsync(strs, dat);
                break;

            default:
                await OnUnknownEventAsync(payload);
                if (this.Configuration.LogUnknownEvents)
                {
                    this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown event: {EventName}\npayload: {@Payload}", payload.EventName, payload.Data);
                }

                break;

            #endregion

            #region AutoModeration
            case "auto_moderation_rule_create":
                await OnAutoModerationRuleCreateAsync(dat.ToDiscordObject<DiscordAutoModerationRule>());
                break;

            case "auto_moderation_rule_update":
                await OnAutoModerationRuleUpdatedAsync(dat.ToDiscordObject<DiscordAutoModerationRule>());
                break;

            case "auto_moderation_rule_delete":
                await OnAutoModerationRuleDeletedAsync(dat.ToDiscordObject<DiscordAutoModerationRule>());
                break;

            case "auto_moderation_action_execution":
                await OnAutoModerationRuleExecutedAsync(dat.ToDiscordObject<DiscordAutoModerationActionExecution>());
                break;
                #endregion
            
            #region Entitlements
            case "entitlement_create":
                await OnEntitlementCreatedAsync(dat.ToDiscordObject<DiscordEntitlement>());
                break;
            
            case "entitlement_update":
                await OnEntitlementUpdatedAsync(dat.ToDiscordObject<DiscordEntitlement>());
                break;
            
            case "entitlement_delete":
                await OnEntitlementDeletedAsync(dat.ToDiscordObject<DiscordEntitlement>());
                break;
            #endregion
        }
    }

    #endregion

    #region Events

    #region Gateway

    internal async Task OnReadyEventAsync(ReadyPayload ready, JArray rawGuilds, JArray rawDmChannels, int shardId)
    {
        TransportUser rusr = ready.CurrentUser;
        this.CurrentUser = new DiscordUser(rusr)
        {
            Discord = this
        };

        this.sessionId = ready.SessionId;
        this.gatewayResumeUrl = ready.ResumeGatewayUrl;
        Dictionary<ulong, JObject> rawGuildIndex = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

        this.privateChannels.Clear();
        foreach (JToken rawChannel in rawDmChannels)
        {
            DiscordDmChannel channel = rawChannel.ToDiscordObject<DiscordDmChannel>();

            channel.Discord = this;

            //xdc.recipients =
            //    .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this })
            //    .ToList();

            IEnumerable<TransportUser> recipsRaw = rawChannel["recipients"].ToDiscordObject<IEnumerable<TransportUser>>();
            List<DiscordUser> recipients = [];
            foreach (TransportUser xr in recipsRaw)
            {
                DiscordUser xu = new(xr) { Discord = this };
                xu = UpdateUserCache(xu);

                recipients.Add(xu);
            }

            channel.Recipients = recipients;

            this.privateChannels[channel.Id] = channel;
        }

        IEnumerable<DiscordGuild> guilds = rawGuilds.ToDiscordObject<IEnumerable<DiscordGuild>>();
        foreach (DiscordGuild guild in guilds)
        {
            guild.Discord = this;
            guild.channels ??= new ConcurrentDictionary<ulong, DiscordChannel>();
            guild.threads ??= new ConcurrentDictionary<ulong, DiscordThreadChannel>();

            foreach (DiscordChannel xc in guild.Channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (DiscordOverwrite xo in xc.permissionOverwrites)
                {
                    xo.Discord = this;
                    xo.channelId = xc.Id;
                }
            }

            foreach (DiscordThreadChannel xt in guild.Threads.Values)
            {
                xt.GuildId = guild.Id;
                xt.Discord = this;
            }

            guild.roles ??= new ConcurrentDictionary<ulong, DiscordRole>();

            foreach (DiscordRole xr in guild.Roles.Values)
            {
                xr.Discord = this;
                xr.guild_id = guild.Id;
            }

            JObject rawGuild = rawGuildIndex[guild.Id];
            JArray? rawMembers = (JArray)rawGuild["members"];

            guild.members?.Clear();
            guild.members ??= new ConcurrentDictionary<ulong, DiscordMember>();

            if (rawMembers != null)
            {
                foreach (JToken xj in rawMembers)
                {
                    TransportMember xtm = xj.ToDiscordObject<TransportMember>();

                    DiscordUser xu = new(xtm.User) { Discord = this };
                    xu = UpdateUserCache(xu);

                    guild.members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, guild_id = guild.Id };
                }
            }

            guild.emojis ??= new ConcurrentDictionary<ulong, DiscordEmoji>();

            foreach (DiscordEmoji xe in guild.Emojis.Values)
            {
                xe.Discord = this;
            }

            guild.voiceStates ??= new ConcurrentDictionary<ulong, DiscordVoiceState>();

            foreach (DiscordVoiceState xvs in guild.VoiceStates.Values)
            {
                xvs.Discord = this;
            }

            this.guilds[guild.Id] = guild;
        }

        await this.dispatcher.DispatchAsync<SessionCreatedEventArgs>
        (
            this,
            new()
            {
                ShardId = shardId,
                GuildIds = [.. guilds.Select(guild => guild.Id)]
            }
        );

        if (!guilds.Any() && this.orchestrator.AllShardsConnected)
        {
            this.guildDownloadCompleted = true;
            GuildDownloadCompletedEventArgs ea = new(this.Guilds);

            await this.dispatcher.DispatchAsync(this, ea);
        }
    }

    internal async Task OnResumedAsync(int shardId)
    {
        await this.dispatcher.DispatchAsync<SessionResumedEventArgs>
        (
            this,
            new()
            {
                ShardId = shardId
            }
        );
    }

    #endregion

    #region Channel

    internal async Task OnChannelCreateEventAsync(DiscordChannel channel)
    {
        channel.Discord = this;
        foreach (DiscordOverwrite xo in channel.permissionOverwrites)
        {
            xo.Discord = this;
            xo.channelId = channel.Id;
        }

        this.guilds[channel.GuildId.Value].channels[channel.Id] = channel;

        await this.dispatcher.DispatchAsync(this, new ChannelCreatedEventArgs
        {
            Channel = channel,
            Guild = channel.Guild
        });
    }

    internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
    {
        if (channel == null)
        {
            return;
        }

        channel.Discord = this;

        DiscordGuild? gld = channel.Guild;

        DiscordChannel? channel_new = InternalGetCachedChannel(channel.Id, channel.GuildId);
        DiscordChannel channel_old = null!;

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
                permissionOverwrites = new List<DiscordOverwrite>(channel_new.permissionOverwrites),
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

            channel_new.permissionOverwrites.Clear();

            foreach (DiscordOverwrite po in channel.permissionOverwrites)
            {
                po.Discord = this;
                po.channelId = channel.Id;
            }

            channel_new.permissionOverwrites.AddRange(channel.permissionOverwrites);
        }
        else if (gld != null)
        {
            gld.channels[channel.Id] = channel;
        }

        await this.dispatcher.DispatchAsync(this, new ChannelUpdatedEventArgs
        {
            ChannelAfter = channel_new,
            Guild = gld,
            ChannelBefore = channel_old
        });
    }

    internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
    {
        if (channel == null)
        {
            return;
        }

        channel.Discord = this;

        //if (channel.IsPrivate)
        if (channel.Type is DiscordChannelType.Group or DiscordChannelType.Private)
        {
            DiscordDmChannel? dmChannel = channel as DiscordDmChannel;

            _ = this.privateChannels.TryRemove(dmChannel.Id, out _);

            await this.dispatcher.DispatchAsync(this, new DmChannelDeletedEventArgs
            {
                Channel = dmChannel
            });
        }
        else
        {
            DiscordGuild gld = channel.Guild;

            if (gld.channels.TryRemove(channel.Id, out DiscordChannel? cachedChannel))
            {
                channel = cachedChannel;
            }

            await this.dispatcher.DispatchAsync(this, new ChannelDeletedEventArgs
            {
                Channel = channel,
                Guild = gld
            });
        }
    }

    internal async Task OnChannelPinsUpdateAsync(ulong? guildId, ulong channelId, DateTimeOffset? lastPinTimestamp)
    {
        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordChannel? channel = InternalGetCachedChannel(channelId, guildId) ?? InternalGetCachedThread(channelId, guildId);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = DiscordChannelType.Private,
                Recipients = Array.Empty<DiscordUser>()
            };

            DiscordDmChannel chn = (DiscordDmChannel)channel;

            this.privateChannels[channelId] = chn;
        }

        ChannelPinsUpdatedEventArgs ea = new()
        {
            Guild = guild,
            Channel = channel,
            LastPinTimestamp = lastPinTimestamp
        };

        await this.dispatcher.DispatchAsync<ChannelPinsUpdatedEventArgs>(this, ea);
    }

    #endregion

    #region Scheduled Guild Events

    private async Task OnScheduledGuildEventCreateEventAsync(DiscordScheduledGuildEvent evt)
    {
        evt.Discord = this;

        if (evt.Creator != null)
        {
            evt.Creator.Discord = this;
            UpdateUserCache(evt.Creator);
        }

        evt.Guild.scheduledEvents[evt.Id] = evt;

        await this.dispatcher.DispatchAsync(this, new ScheduledGuildEventCreatedEventArgs
        {
            Event = evt
        });
    }

    private async Task OnScheduledGuildEventDeleteEventAsync(DiscordScheduledGuildEvent evt)
    {
        DiscordGuild guild = InternalGetCachedGuild(evt.GuildId);

        if (guild == null) // ??? //
        {
            return;
        }

        guild.scheduledEvents.TryRemove(evt.Id, out _);

        evt.Discord = this;

        if (evt.Creator != null)
        {
            evt.Creator.Discord = this;
            UpdateUserCache(evt.Creator);
        }

        await this.dispatcher.DispatchAsync(this, new ScheduledGuildEventDeletedEventArgs
        {
            Event = evt
        });
    }

    private async Task OnScheduledGuildEventUpdateEventAsync(DiscordScheduledGuildEvent evt)
    {
        evt.Discord = this;

        if (evt.Creator != null)
        {
            evt.Creator.Discord = this;
            UpdateUserCache(evt.Creator);
        }

        DiscordGuild guild = InternalGetCachedGuild(evt.GuildId);
        guild.scheduledEvents.TryGetValue(evt.GuildId, out DiscordScheduledGuildEvent? oldEvt);

        evt.Guild.scheduledEvents[evt.Id] = evt;

        if (evt.Status is DiscordScheduledGuildEventStatus.Completed)
        {
            await this.dispatcher.DispatchAsync(this, new ScheduledGuildEventCompletedEventArgs()
            {
                Event = evt
            });
        }
        else
        {
            await this.dispatcher.DispatchAsync(this, new ScheduledGuildEventUpdatedEventArgs()
            {
                EventBefore = oldEvt,
                EventAfter = evt
            });
        }
    }

    private async Task OnScheduledGuildEventUserAddEventAsync(ulong guildId, ulong eventId, ulong userId)
    {
        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordScheduledGuildEvent evt = guild.scheduledEvents.GetOrAdd(eventId, new DiscordScheduledGuildEvent()
        {
            Id = eventId,
            GuildId = guildId,
            Discord = this,
            UserCount = 0
        });

        evt.UserCount++;

        DiscordUser user =
            guild.Members.TryGetValue(userId, out DiscordMember? mbr) ? mbr :
            GetCachedOrEmptyUserInternal(userId) ?? new DiscordUser() { Id = userId, Discord = this };

        await this.dispatcher.DispatchAsync(this, new ScheduledGuildEventUserAddedEventArgs()
        {
            Event = evt,
            User = user
        });
    }

    private async Task OnScheduledGuildEventUserRemoveEventAsync(ulong guildId, ulong eventId, ulong userId)
    {
        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordScheduledGuildEvent evt = guild.scheduledEvents.GetOrAdd(eventId, new DiscordScheduledGuildEvent()
        {
            Id = eventId,
            GuildId = guildId,
            Discord = this,
            UserCount = 0
        });

        evt.UserCount = evt.UserCount is 0 ? 0 : evt.UserCount - 1;

        DiscordUser user =
            guild.Members.TryGetValue(userId, out DiscordMember? mbr) ? mbr :
            GetCachedOrEmptyUserInternal(userId) ?? new DiscordUser() { Id = userId, Discord = this };

        await this.dispatcher.DispatchAsync(this, new ScheduledGuildEventUserRemovedEventArgs()
        {
            Event = evt,
            User = user
        });
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
                    xp.internalActivities = new DiscordActivity[xp.RawActivities.Length];
                    for (int i = 0; i < xp.RawActivities.Length; i++)
                    {
                        xp.internalActivities[i] = new DiscordActivity(xp.RawActivities[i]);
                    }
                }

                this.presences[xp.User.Id] = xp;
            }
        }

        bool exists = this.guilds.TryGetValue(guild.Id, out DiscordGuild? foundGuild);

        guild.Discord = this;
        guild.IsUnavailable = false;
        DiscordGuild eventGuild = guild;

        if (exists)
        {
            guild = foundGuild;
        }

        guild.channels ??= new ConcurrentDictionary<ulong, DiscordChannel>();
        guild.threads ??= new ConcurrentDictionary<ulong, DiscordThreadChannel>();
        guild.roles ??= new ConcurrentDictionary<ulong, DiscordRole>();
        guild.emojis ??= new ConcurrentDictionary<ulong, DiscordEmoji>();
        guild.stickers ??= new ConcurrentDictionary<ulong, DiscordMessageSticker>();
        guild.voiceStates ??= new ConcurrentDictionary<ulong, DiscordVoiceState>();
        guild.members ??= new ConcurrentDictionary<ulong, DiscordMember>();
        guild.stageInstances ??= new ConcurrentDictionary<ulong, DiscordStageInstance>();
        guild.scheduledEvents ??= new ConcurrentDictionary<ulong, DiscordScheduledGuildEvent>();

        UpdateCachedGuild(eventGuild, rawMembers);

        guild.JoinedAt = eventGuild.JoinedAt;
        guild.IsLarge = eventGuild.IsLarge;
        guild.MemberCount = Math.Max(eventGuild.MemberCount, guild.members.Count);
        guild.IsUnavailable = eventGuild.IsUnavailable;
        guild.PremiumSubscriptionCount = eventGuild.PremiumSubscriptionCount;
        guild.PremiumTier = eventGuild.PremiumTier;
        guild.Banner = eventGuild.Banner;
        guild.VanityUrlCode = eventGuild.VanityUrlCode;
        guild.Description = eventGuild.Description;
        guild.IsNSFW = eventGuild.IsNSFW;

        foreach (KeyValuePair<ulong, DiscordVoiceState> kvp in eventGuild.voiceStates ??= new())
        {
            guild.voiceStates[kvp.Key] = kvp.Value;
        }

        foreach (DiscordScheduledGuildEvent xe in guild.scheduledEvents.Values)
        {
            xe.Discord = this;

            if (xe.Creator != null)
            {
                xe.Creator.Discord = this;
            }
        }

        foreach (DiscordChannel xc in guild.channels.Values)
        {
            xc.GuildId = guild.Id;
            xc.Discord = this;
            foreach (DiscordOverwrite xo in xc.permissionOverwrites)
            {
                xo.Discord = this;
                xo.channelId = xc.Id;
            }
        }

        foreach (DiscordThreadChannel xt in guild.threads.Values)
        {
            xt.GuildId = guild.Id;
            xt.Discord = this;
        }

        foreach (DiscordEmoji xe in guild.emojis.Values)
        {
            xe.Discord = this;
        }

        foreach (DiscordMessageSticker xs in guild.stickers.Values)
        {
            xs.Discord = this;
        }

        foreach (DiscordVoiceState xvs in guild.voiceStates.Values)
        {
            xvs.Discord = this;
        }

        foreach (DiscordRole xr in guild.roles.Values)
        {
            xr.Discord = this;
            xr.guild_id = guild.Id;
        }

        foreach (DiscordStageInstance instance in guild.stageInstances.Values)
        {
            instance.Discord = this;
        }

        bool old = Volatile.Read(ref this.guildDownloadCompleted);
        bool dcompl = this.guilds.Values.All(xg => !xg.IsUnavailable) && !this.guildDownloadCompleted;

        if (exists)
        {
            await this.dispatcher.DispatchAsync(this, new GuildAvailableEventArgs
            {
                Guild = guild
            });
        }
        else
        {
            await this.dispatcher.DispatchAsync(this, new GuildCreatedEventArgs
            {
                Guild = guild
            });
        }

        if (dcompl && !old && this.orchestrator.AllShardsConnected)
        {
            this.guildDownloadCompleted = true;
            await this.dispatcher.DispatchAsync(this, new GuildDownloadCompletedEventArgs(this.Guilds));
        }
    }

    internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
    {
        DiscordGuild oldGuild;

        if (!this.guilds.TryGetValue(guild.Id, out DiscordGuild gld))
        {
            this.guilds[guild.Id] = guild;
            oldGuild = null;
        }
        else
        {
            oldGuild = new DiscordGuild
            {
                Discord = gld.Discord,
                Name = gld.Name,
                AfkChannelId = gld.AfkChannelId,
                AfkTimeout = gld.AfkTimeout,
                DefaultMessageNotifications = gld.DefaultMessageNotifications,
                ExplicitContentFilter = gld.ExplicitContentFilter,
                Features = gld.Features,
                IconHash = gld.IconHash,
                Id = gld.Id,
                IsLarge = gld.IsLarge,
                isSynced = gld.isSynced,
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
                SystemChannelId = gld.SystemChannelId,
                SystemChannelFlags = gld.SystemChannelFlags,
                WidgetEnabled = gld.WidgetEnabled,
                WidgetChannelId = gld.WidgetChannelId,
                VerificationLevel = gld.VerificationLevel,
                RulesChannelId = gld.RulesChannelId,
                PublicUpdatesChannelId = gld.PublicUpdatesChannelId,
                voiceRegionId = gld.voiceRegionId,
                PremiumProgressBarEnabled = gld.PremiumProgressBarEnabled,
                IsNSFW = gld.IsNSFW,
                channels = new ConcurrentDictionary<ulong, DiscordChannel>(),
                threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>(),
                emojis = new ConcurrentDictionary<ulong, DiscordEmoji>(),
                members = new ConcurrentDictionary<ulong, DiscordMember>(),
                roles = new ConcurrentDictionary<ulong, DiscordRole>(),
                voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>()
            };

            foreach (KeyValuePair<ulong, DiscordChannel> kvp in gld.channels ??= new())
            {
                oldGuild.channels[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordThreadChannel> kvp in gld.threads ??= new())
            {
                oldGuild.threads[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordEmoji> kvp in gld.emojis ??= new())
            {
                oldGuild.emojis[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordRole> kvp in gld.roles ??= new())
            {
                oldGuild.roles[kvp.Key] = kvp.Value;
            }
            //new ConcurrentDictionary<ulong, DiscordVoiceState>()
            foreach (KeyValuePair<ulong, DiscordVoiceState> kvp in gld.voiceStates ??= new())
            {
                oldGuild.voiceStates[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<ulong, DiscordMember> kvp in gld.members ??= new())
            {
                oldGuild.members[kvp.Key] = kvp.Value;
            }
        }

        guild.Discord = this;
        guild.IsUnavailable = false;
        DiscordGuild eventGuild = guild;
        guild = this.guilds[eventGuild.Id];
        guild.channels ??= new ConcurrentDictionary<ulong, DiscordChannel>();
        guild.threads ??= new ConcurrentDictionary<ulong, DiscordThreadChannel>();
        guild.roles ??= new ConcurrentDictionary<ulong, DiscordRole>();
        guild.emojis ??= new ConcurrentDictionary<ulong, DiscordEmoji>();
        guild.voiceStates ??= new ConcurrentDictionary<ulong, DiscordVoiceState>();
        guild.members ??= new ConcurrentDictionary<ulong, DiscordMember>();
        UpdateCachedGuild(eventGuild, rawMembers);

        foreach (DiscordChannel xc in guild.channels.Values)
        {
            xc.GuildId = guild.Id;
            xc.Discord = this;
            foreach (DiscordOverwrite xo in xc.permissionOverwrites)
            {
                xo.Discord = this;
                xo.channelId = xc.Id;
            }
        }

        foreach (DiscordThreadChannel xc in guild.threads.Values)
        {
            xc.GuildId = guild.Id;
            xc.Discord = this;
        }

        foreach (DiscordEmoji xe in guild.emojis.Values)
        {
            xe.Discord = this;
        }

        foreach (DiscordVoiceState xvs in guild.voiceStates.Values)
        {
            xvs.Discord = this;
        }

        foreach (DiscordRole xr in guild.roles.Values)
        {
            xr.Discord = this;
            xr.guild_id = guild.Id;
        }

        await this.dispatcher.DispatchAsync(this, new GuildUpdatedEventArgs
        {
            GuildBefore = oldGuild,
            GuildAfter = guild
        });
    }

    internal async Task OnGuildDeleteEventAsync(DiscordGuild guild)
    {
        if (guild.IsUnavailable)
        {
            if (!this.guilds.TryGetValue(guild.Id, out DiscordGuild? gld))
            {
                return;
            }

            gld.IsUnavailable = true;

            await this.dispatcher.DispatchAsync(this, new GuildUnavailableEventArgs
            {
                Guild = guild,
                Unavailable = true
            });
        }
        else
        {
            if (!this.guilds.TryRemove(guild.Id, out DiscordGuild? gld))
            {
                return;
            }

            await this.dispatcher.DispatchAsync(this, new GuildDeletedEventArgs
            {
                Guild = gld
            });
        }
    }

    internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
    {
        ConcurrentDictionary<ulong, DiscordEmoji> oldEmojis = new(guild.emojis);
        guild.emojis.Clear();

        foreach (DiscordEmoji emoji in newEmojis)
        {
            emoji.Discord = this;
            guild.emojis[emoji.Id] = emoji;
        }

        GuildEmojisUpdatedEventArgs ea = new()
        {
            Guild = guild,
            EmojisAfter = guild.Emojis,
            EmojisBefore = oldEmojis
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
    {
        GuildIntegrationsUpdatedEventArgs ea = new()
        {
            Guild = guild
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    private async Task OnGuildAuditLogEntryCreateEventAsync(DiscordGuild guild, DiscordAuditLogEntry auditLogEntry)
    {
        GuildAuditLogCreatedEventArgs ea = new()
        {
            Guild = guild,
            AuditLogEntry = auditLogEntry
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Guild Ban

    internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
    {
        DiscordUser usr = new(user) { Discord = this };
        usr = UpdateUserCache(usr);

        if (!guild.Members.TryGetValue(user.Id, out DiscordMember? mbr))
        {
            mbr = new DiscordMember(usr) { Discord = this, guild_id = guild.Id };
        }

        GuildBanAddedEventArgs ea = new()
        {
            Guild = guild,
            Member = mbr
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
    {
        DiscordUser usr = new(user) { Discord = this };
        usr = UpdateUserCache(usr);

        if (!guild.Members.TryGetValue(user.Id, out DiscordMember? mbr))
        {
            mbr = new DiscordMember(usr) { Discord = this, guild_id = guild.Id };
        }

        GuildBanRemovedEventArgs ea = new()
        {
            Guild = guild,
            Member = mbr
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Guild Member

    internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
    {
        DiscordUser usr = new(member.User) { Discord = this };
        UpdateUserCache(usr);

        DiscordMember mbr = new(member)
        {
            Discord = this,
            guild_id = guild.Id
        };

        guild.members[mbr.Id] = mbr;
        guild.MemberCount++;

        GuildMemberAddedEventArgs ea = new()
        {
            Guild = guild,
            Member = mbr
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
    {
        DiscordUser usr = new(user);

        if (!guild.members.TryRemove(user.Id, out DiscordMember? mbr))
        {
            mbr = new DiscordMember(usr) { Discord = this, guild_id = guild.Id };
        }

        guild.MemberCount--;

        UpdateUserCache(usr);

        GuildMemberRemovedEventArgs ea = new()
        {
            Guild = guild,
            Member = mbr
        };
        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnGuildMemberUpdateEventAsync(TransportMember member, DiscordGuild guild)
    {
        DiscordUser userAfter = new(member.User) { Discord = this };
        _ = UpdateUserCache(userAfter);

        DiscordMember memberAfter = new(member) { Discord = this, guild_id = guild.Id };

        if (!guild.Members.TryGetValue(member.User.Id, out DiscordMember? memberBefore))
        {
            memberBefore = new DiscordMember(member) { Discord = this, guild_id = guild.Id };
        }

        guild.members.AddOrUpdate(member.User.Id, memberAfter, (_, _) => memberAfter);

        GuildMemberUpdatedEventArgs ea = new()
        {
            Guild = guild,
            MemberAfter = memberAfter,
            MemberBefore = memberBefore,
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnGuildMembersChunkEventAsync(JObject dat)
    {
        DiscordGuild guild = this.Guilds[(ulong)dat["guild_id"]];
        int chunkIndex = (int)dat["chunk_index"];
        int chunkCount = (int)dat["chunk_count"];
        string? nonce = (string)dat["nonce"];

        HashSet<DiscordMember> mbrs = [];
        HashSet<DiscordPresence> pres = [];

        TransportMember[] members = dat["members"].ToDiscordObject<TransportMember[]>();

        int memCount = members.Length;
        for (int i = 0; i < memCount; i++)
        {
            DiscordMember mbr = new(members[i]) { Discord = this, guild_id = guild.Id };

            if (!this.UserCache.ContainsKey(mbr.Id))
            {
                this.UserCache[mbr.Id] = new DiscordUser(members[i].User) { Discord = this };
            }

            guild.members[mbr.Id] = mbr;

            mbrs.Add(mbr);
        }

        guild.MemberCount = guild.members.Count;

        GuildMembersChunkedEventArgs ea = new()
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

            int presCount = presences.Length;
            for (int i = 0; i < presCount; i++)
            {
                DiscordPresence xp = presences[i];
                xp.Discord = this;
                xp.Activity = new DiscordActivity(xp.RawActivity);

                if (xp.RawActivities != null)
                {
                    xp.internalActivities = new DiscordActivity[xp.RawActivities.Length];
                    for (int j = 0; j < xp.RawActivities.Length; j++)
                    {
                        xp.internalActivities[j] = new DiscordActivity(xp.RawActivities[j]);
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

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Guild Role

    internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, DiscordGuild guild)
    {
        role.Discord = this;
        role.guild_id = guild.Id;

        guild.roles[role.Id] = role;

        GuildRoleCreatedEventArgs ea = new()
        {
            Guild = guild,
            Role = role
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
    {
        DiscordRole newRole = await guild.GetRoleAsync(role.Id);
        DiscordRole oldRole = new()
        {
            guild_id = guild.Id,
            color = newRole.color,
            Discord = this,
            IsHoisted = newRole.IsHoisted,
            Id = newRole.Id,
            IsManaged = newRole.IsManaged,
            IsMentionable = newRole.IsMentionable,
            Name = newRole.Name,
            Permissions = newRole.Permissions,
            Position = newRole.Position,
            IconHash = newRole.IconHash,
            emoji = newRole.emoji
        };

        newRole.guild_id = guild.Id;
        newRole.color = role.color;
        newRole.IsHoisted = role.IsHoisted;
        newRole.IsManaged = role.IsManaged;
        newRole.IsMentionable = role.IsMentionable;
        newRole.Name = role.Name;
        newRole.Permissions = role.Permissions;
        newRole.Position = role.Position;
        newRole.emoji = role.emoji;
        newRole.IconHash = role.IconHash;

        GuildRoleUpdatedEventArgs ea = new()
        {
            Guild = guild,
            RoleAfter = newRole,
            RoleBefore = oldRole
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, DiscordGuild guild)
    {
        if (!guild.roles.TryRemove(roleId, out DiscordRole? role))
        {
            this.Logger.LogWarning("Attempted to delete a nonexistent role ({RoleId}) from guild ({Guild}).", roleId, guild);
        }

        GuildRoleDeletedEventArgs ea = new()
        {
            Guild = guild,
            Role = role
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Invite

    internal async Task OnInviteCreateEventAsync(ulong channelId, ulong guildId, DiscordInvite invite)
    {
        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordChannel channel = InternalGetCachedChannel(channelId, guildId);

        invite.Discord = this;

        guild.invites[invite.Code] = invite;

        InviteCreatedEventArgs ea = new()
        {
            Channel = channel,
            Guild = guild,
            Invite = invite
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnInviteDeleteEventAsync(ulong channelId, ulong guildId, JToken dat)
    {
        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordChannel channel = InternalGetCachedChannel(channelId, guildId);

        if (!guild.invites.TryRemove(dat["code"].ToString(), out DiscordInvite? invite))
        {
            invite = dat.ToDiscordObject<DiscordInvite>();
            invite.Discord = this;
        }

        invite.IsRevoked = true;

        InviteDeletedEventArgs ea = new()
        {
            Channel = channel,
            Guild = guild,
            Invite = invite
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Message

    internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
    {
        message.Discord = this;
        PopulateMessageReactionsAndCache(message, author, member);
        message.PopulateMentions();

        if (message.ReferencedMessage != null)
        {
            message.ReferencedMessage.Discord = this;
            PopulateMessageReactionsAndCache(message.ReferencedMessage, referenceAuthor, referenceMember);
            message.ReferencedMessage.PopulateMentions();
        }

        if (message.MessageSnapshots != null)
        {
            foreach (DiscordMessageSnapshot snapshot in message.MessageSnapshots)
            {
                if (snapshot?.Message != null)
                {
                    snapshot.Message.PopulateMentions();
                }
            }
        }

        foreach (DiscordMessageSticker sticker in message.Stickers)
        {
            sticker.Discord = this;
        }

        MessageCreatedEventArgs ea = new()
        {
            Message = message,

            MentionedUsers = new ReadOnlyCollection<DiscordUser>(message.mentionedUsers),
            MentionedRoles = message.mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message.mentionedRoles) : null,
            MentionedChannels = message.mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message.mentionedChannels) : null
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
    {
        message.Discord = this;
        DiscordMessage event_message = message;

        DiscordMessage oldmsg = null;

        if (!this.MessageCache.TryGet(event_message.Id, out message)) // previous message was not in cache
        {
            message = event_message;
            PopulateMessageReactionsAndCache(message, author, member);

            if (message.ReferencedMessage != null)
            {
                message.ReferencedMessage.Discord = this;
                PopulateMessageReactionsAndCache(message.ReferencedMessage, referenceAuthor, referenceMember);
                message.ReferencedMessage.PopulateMentions();
            }

            if (message.MessageSnapshots != null)
            {
                foreach (DiscordMessageSnapshot snapshot in message.MessageSnapshots)
                {
                    if (snapshot?.Message != null)
                    {
                        snapshot.Message.PopulateMentions();
                    }
                }
            }
        }
        else // previous message was fetched in cache
        {
            oldmsg = new DiscordMessage(message);

            // cached message is updated with information from the event message
            message.EditedTimestamp = event_message.EditedTimestamp;
            if (event_message.Content != null)
            {
                message.Content = event_message.Content;
            }

            message.embeds.Clear();
            message.embeds.AddRange(event_message.embeds);
            message.attachments.Clear();
            message.attachments.AddRange(event_message.attachments);
            message.Pinned = event_message.Pinned;
            message.IsTTS = event_message.IsTTS;
            message.Poll = event_message.Poll;

            // Mentions
            message.mentionedUsers.Clear();
            message.mentionedUsers.AddRange(event_message.mentionedUsers ?? []);
            message.mentionedRoles.Clear();
            message.mentionedRoles.AddRange(event_message.mentionedRoles ?? []);
            message.mentionedChannels.Clear();
            message.mentionedChannels.AddRange(event_message.mentionedChannels ?? []);
            message.MentionEveryone = event_message.MentionEveryone;
        }

        message.PopulateMentions();

        MessageUpdatedEventArgs ea = new()
        {
            Message = message,
            MessageBefore = oldmsg,
            MentionedUsers = new ReadOnlyCollection<DiscordUser>(message.mentionedUsers),
            MentionedRoles = message.mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message.mentionedRoles) : null,
            MentionedChannels = message.mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message.mentionedChannels) : null
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId)
    {
        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordChannel? channel = InternalGetCachedChannel(channelId, guildId) ?? InternalGetCachedThread(channelId, guildId);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = DiscordChannelType.Private,
                Recipients = Array.Empty<DiscordUser>()

            };

            this.privateChannels[channelId] = (DiscordDmChannel)channel;
        }

        if (!this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this,
            };
        }

        this.MessageCache?.Remove(msg.Id);

        MessageDeletedEventArgs ea = new()
        {
            Message = msg,
            Channel = channel,
            Guild = guild,
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    private async Task OnMessagePollVoteEventAsync(DiscordPollVoteUpdate voteUpdate, bool wasAdded)
    {
        voteUpdate.WasAdded = wasAdded;
        voteUpdate.client = this;

        MessagePollVotedEventArgs ea = new()
        {
            PollVoteUpdate = voteUpdate
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnMessageBulkDeleteEventAsync(ulong[] messageIds, ulong channelId, ulong? guildId)
    {
        DiscordChannel? channel = InternalGetCachedChannel(channelId, guildId) ?? InternalGetCachedThread(channelId, guildId);

        List<DiscordMessage> msgs = new(messageIds.Length);
        foreach (ulong messageId in messageIds)
        {
            if (!this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this,
                };
            }

            this.MessageCache?.Remove(msg.Id);

            msgs.Add(msg);
        }

        DiscordGuild guild = InternalGetCachedGuild(guildId);

        MessagesBulkDeletedEventArgs ea = new()
        {
            Channel = channel,
            Messages = new ReadOnlyCollection<DiscordMessage>(msgs),
            Guild = guild
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Message Reaction

    internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, TransportMember mbr, DiscordEmoji emoji)
    {
        DiscordChannel? channel = InternalGetCachedChannel(channelId, guildId) ?? InternalGetCachedThread(channelId, guildId);
        DiscordGuild? guild = InternalGetCachedGuild(guildId);

        emoji.Discord = this;

        DiscordUser usr = null!;
        usr = !TryGetCachedUserInternal(userId, out usr)
            ? UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr)
            : UpdateUser(usr, guild?.Id, guild, mbr);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = DiscordChannelType.Private,
                Recipients = new DiscordUser[] { usr }
            };
            this.privateChannels[channelId] = (DiscordDmChannel)channel;
        }

        if (!this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this,
                reactions = []
            };
        }

        DiscordReaction? react = msg.reactions.FirstOrDefault(xr => xr.Emoji == emoji);

        if (react == null)
        {
            msg.reactions.Add(react = new DiscordReaction
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

        MessageReactionAddedEventArgs ea = new()
        {
            Message = msg,
            User = usr,
            Guild = guild,
            Emoji = emoji
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji)
    {
        DiscordChannel? channel = InternalGetCachedChannel(channelId, guildId) ?? InternalGetCachedThread(channelId, guildId);

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
                Type = DiscordChannelType.Private,
                Recipients = new DiscordUser[] { usr }
            };
            this.privateChannels[channelId] = (DiscordDmChannel)channel;
        }

        if (channel?.Guild != null)
        {
            usr = channel.Guild.Members.TryGetValue(userId, out DiscordMember? member)
                ? member
                : new DiscordMember(usr) { Discord = this, guild_id = channel.GuildId.Value };
        }

        if (!this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this
            };
        }

        DiscordReaction? react = msg.reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
        if (react != null)
        {
            react.Count--;
            react.IsMe &= this.CurrentUser.Id != userId;

            if (msg.reactions != null && react.Count <= 0) // shit happens
            {
                for (int i = 0; i < msg.reactions.Count; i++)
                {
                    if (msg.reactions[i].Emoji == emoji)
                    {
                        msg.reactions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        DiscordGuild guild = InternalGetCachedGuild(guildId);

        MessageReactionRemovedEventArgs ea = new()
        {
            Message = msg,
            User = usr,
            Guild = guild,
            Emoji = emoji
        };
        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, ulong channelId, ulong? guildId)
    {
        DiscordChannel? channel = InternalGetCachedChannel(channelId, guildId) ?? InternalGetCachedThread(channelId, guildId);

        if (!this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this
            };
        }

        msg.reactions?.Clear();

        MessageReactionsClearedEventArgs ea = new()
        {
            Message = msg,
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnMessageReactionRemoveEmojiAsync(ulong messageId, ulong channelId, ulong guildId, JToken dat)
    {
        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordChannel? channel = InternalGetCachedChannel(channelId, guildId) ?? InternalGetCachedThread(channelId, guildId);

        if (channel == null)
        {
            channel = new DiscordDmChannel
            {
                Id = channelId,
                Discord = this,
                Type = DiscordChannelType.Private,
                Recipients = Array.Empty<DiscordUser>()
            };
            this.privateChannels[channelId] = (DiscordDmChannel)channel;
        }

        if (!this.MessageCache.TryGet(messageId, out DiscordMessage? msg))
        {
            msg = new DiscordMessage
            {
                Id = messageId,
                ChannelId = channelId,
                Discord = this
            };
        }

        DiscordEmoji partialEmoji = dat.ToDiscordObject<DiscordEmoji>();

        if (!guild.emojis.TryGetValue(partialEmoji.Id, out DiscordEmoji? emoji))
        {
            emoji = partialEmoji;
            emoji.Discord = this;
        }

        msg.reactions?.RemoveAll(r => r.Emoji.Equals(emoji));

        MessageReactionRemovedEmojiEventArgs ea = new()
        {
            Message = msg,
            Channel = channel,
            Guild = guild,
            Emoji = emoji
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region User/Presence Update

    internal async Task OnPresenceUpdateEventAsync(JObject rawPresence, JObject rawUser)
    {
        ulong uid = (ulong)rawUser["id"];
        DiscordPresence old = null;

        if (this.presences.TryGetValue(uid, out DiscordPresence? presence))
        {
            old = new DiscordPresence(presence);
            DiscordJson.PopulateObject(rawPresence, presence);
        }
        else
        {
            presence = rawPresence.ToDiscordObject<DiscordPresence>();
            presence.Discord = this;
            presence.Activity = new DiscordActivity(presence.RawActivity);
            this.presences[presence.InternalUser.Id] = presence;
        }

        // reuse arrays / avoid linq (this is a hot zone)
        if (presence.Activities == null || rawPresence["activities"] == null)
        {
            presence.internalActivities = [];
        }
        else
        {
            if (presence.internalActivities.Length != presence.RawActivities.Length)
            {
                presence.internalActivities = new DiscordActivity[presence.RawActivities.Length];
            }

            for (int i = 0; i < presence.internalActivities.Length; i++)
            {
                presence.internalActivities[i] = new DiscordActivity(presence.RawActivities[i]);
            }

            if (presence.internalActivities.Length > 0)
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
        PresenceUpdatedEventArgs ea = new()
        {
            Status = presence.Status,
            Activity = presence.Activity,
            User = usr,
            PresenceBefore = old,
            PresenceAfter = presence,
            UserBefore = old != null ? new DiscordUser(old.InternalUser) { Discord = this } : usrafter,
            UserAfter = usrafter
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnUserSettingsUpdateEventAsync(TransportUser user)
    {
        DiscordUser usr = new(user) { Discord = this };

        UserSettingsUpdatedEventArgs ea = new()
        {
            User = usr
        };

        await this.dispatcher.DispatchAsync(this, ea);
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

        UserUpdatedEventArgs ea = new()
        {
            UserAfter = this.CurrentUser,
            UserBefore = usr_old
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Voice

    internal async Task OnVoiceStateUpdateEventAsync(JObject raw)
    {
        ulong gid = (ulong)raw["guild_id"];
        ulong uid = (ulong)raw["user_id"];
        DiscordGuild gld = this.guilds[gid];

        DiscordVoiceState vstateNew = raw.ToDiscordObject<DiscordVoiceState>();
        vstateNew.Discord = this;

        gld.voiceStates.TryRemove(uid, out DiscordVoiceState? vstateOld);

        if (vstateNew.Channel != null)
        {
            gld.voiceStates[vstateNew.UserId] = vstateNew;
        }

        if (gld.members.TryGetValue(uid, out DiscordMember? mbr))
        {
            mbr.IsMuted = vstateNew.IsServerMuted;
            mbr.IsDeafened = vstateNew.IsServerDeafened;
        }
        else
        {
            TransportMember transportMbr = vstateNew.TransportMember;
            UpdateUser(new DiscordUser(transportMbr.User) { Discord = this }, gid, gld, transportMbr);
        }

        VoiceStateUpdatedEventArgs ea = new()
        {
            Guild = vstateNew.Guild,
            Channel = vstateNew.Channel,
            User = vstateNew.User,
            SessionId = vstateNew.SessionId,

            Before = vstateOld,
            After = vstateNew
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
    {
        VoiceServerUpdatedEventArgs ea = new()
        {
            Endpoint = endpoint,
            VoiceToken = token,
            Guild = guild
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Thread

    internal async Task OnThreadCreateEventAsync(DiscordThreadChannel thread)
    {
        thread.Discord = this;
        InternalGetCachedGuild(thread.GuildId).threads[thread.Id] = thread;

        await this.dispatcher.DispatchAsync(this, new ThreadCreatedEventArgs
        {
            Thread = thread,
            Guild = thread.Guild,
            Parent = thread.Parent
        });
    }

    internal async Task OnThreadUpdateEventAsync(DiscordThreadChannel thread)
    {
        if (thread == null)
        {
            return;
        }

        DiscordThreadChannel threadOld;
        ThreadUpdatedEventArgs updateEvent;

        thread.Discord = this;

        DiscordGuild guild = thread.Guild;
        guild.Discord = this;

        DiscordThreadChannel cthread = InternalGetCachedThread(thread.Id, thread.GuildId);

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

            updateEvent = new ThreadUpdatedEventArgs
            {
                ThreadAfter = thread,
                ThreadBefore = threadOld,
                Guild = thread.Guild,
                Parent = thread.Parent
            };
        }
        else
        {
            updateEvent = new ThreadUpdatedEventArgs
            {
                ThreadAfter = thread,
                Guild = thread.Guild,
                Parent = thread.Parent
            };
            guild.threads[thread.Id] = thread;
        }

        await this.dispatcher.DispatchAsync(this, updateEvent);
    }

    internal async Task OnThreadDeleteEventAsync(DiscordThreadChannel thread)
    {
        if (thread == null)
        {
            return;
        }

        thread.Discord = this;

        DiscordGuild gld = thread.Guild;
        if (gld.threads.TryRemove(thread.Id, out DiscordThreadChannel? cachedThread))
        {
            thread = cachedThread;
        }

        await this.dispatcher.DispatchAsync(this, new ThreadDeletedEventArgs
        {
            Thread = thread,
            Guild = thread.Guild,
            Parent = thread.Parent
        });
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
            guild.threads[thread.Id] = thread;
        }

        foreach (DiscordThreadChannelMember member in members)
        {
            member.Discord = this;
            member.guild_id = guild.Id;

            DiscordThreadChannel? thread = threads.SingleOrDefault(x => x.Id == member.ThreadId);
            if (thread != null)
            {
                thread.CurrentMember = member;
            }
        }

        await this.dispatcher.DispatchAsync(this, new ThreadListSyncedEventArgs
        {
            Guild = guild,
            Channels = channels.ToList().AsReadOnly(),
            Threads = threads,
            CurrentMembers = members.ToList().AsReadOnly()
        });
    }

    internal async Task OnThreadMemberUpdateEventAsync(DiscordGuild guild, DiscordThreadChannelMember member)
    {
        member.Discord = this;

        DiscordThreadChannel thread = InternalGetCachedThread(member.ThreadId, guild.Id);
        member.guild_id = guild.Id;
        thread.CurrentMember = member;
        guild.threads[thread.Id] = thread;

        await this.dispatcher.DispatchAsync(this, new ThreadMemberUpdatedEventArgs
        {
            ThreadMember = member,
            Thread = thread
        });
    }

    internal async Task OnThreadMembersUpdateEventAsync(DiscordGuild guild, ulong thread_id, IReadOnlyList<DiscordThreadChannelMember> addedMembers, IReadOnlyList<ulong?> removed_member_ids, int member_count)
    {
        DiscordThreadChannel? thread = InternalGetCachedThread(thread_id, guild.Id) ?? new DiscordThreadChannel
        {
            Id = thread_id,
            GuildId = guild.Id,
        };
        thread.Discord = this;
        guild.Discord = this;
        thread.MemberCount = member_count;

        List<DiscordMember> removedMembers = [];
        if (removed_member_ids != null)
        {
            foreach (ulong? removedId in removed_member_ids)
            {
                removedMembers.Add(guild.members.TryGetValue(removedId.Value, out DiscordMember? member) ? member : new DiscordMember { Id = removedId.Value, guild_id = guild.Id, Discord = this });
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
                threadMember.guild_id = guild.Id;
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

        ThreadMembersUpdatedEventArgs threadMembersUpdateArg = new()
        {
            Guild = guild,
            Thread = thread,
            AddedMembers = addedMembers,
            RemovedMembers = removedMembers,
            MemberCount = member_count
        };

        await this.dispatcher.DispatchAsync(this, threadMembersUpdateArg);
    }

    #endregion

    #region Integration

    internal async Task OnIntegrationCreateAsync(DiscordIntegration integration, ulong guild_id)
    {
        DiscordGuild? guild = InternalGetCachedGuild(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };

        IntegrationCreatedEventArgs ea = new()
        {
            Guild = guild,
            Integration = integration
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnIntegrationUpdateAsync(DiscordIntegration integration, ulong guild_id)
    {
        DiscordGuild? guild = InternalGetCachedGuild(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };

        IntegrationUpdatedEventArgs ea = new()
        {
            Guild = guild,
            Integration = integration
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnIntegrationDeleteAsync(ulong integration_id, ulong guild_id, ulong? application_id)
    {
        DiscordGuild? guild = InternalGetCachedGuild(guild_id) ?? new DiscordGuild
        {
            Id = guild_id,
            Discord = this
        };

        IntegrationDeletedEventArgs ea = new()
        {
            Guild = guild,
            Applicationid = application_id,
            IntegrationId = integration_id
        };

        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region Commands

    internal async Task OnApplicationCommandPermissionsUpdateAsync(JObject obj)
    {
        ApplicationCommandPermissionsUpdatedEventArgs ev = obj.ToObject<ApplicationCommandPermissionsUpdatedEventArgs>()!;

        await this.dispatcher.DispatchAsync(this, ev);
    }

    #endregion

    #region Stage Instance

    internal async Task OnStageInstanceCreateAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = InternalGetCachedGuild(instance.GuildId);

        guild.stageInstances[instance.Id] = instance;

        StageInstanceCreatedEventArgs eventArgs = new()
        {
            StageInstance = instance
        };

        await this.dispatcher.DispatchAsync(this, eventArgs);
    }

    internal async Task OnStageInstanceUpdateAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = InternalGetCachedGuild(instance.GuildId);

        if (!guild.stageInstances.TryRemove(instance.Id, out DiscordStageInstance? oldInstance))
        {
            oldInstance = new DiscordStageInstance { Id = instance.Id, GuildId = instance.GuildId, ChannelId = instance.ChannelId };
        }

        guild.stageInstances[instance.Id] = instance;

        StageInstanceUpdatedEventArgs eventArgs = new()
        {
            StageInstanceBefore = oldInstance,
            StageInstanceAfter = instance
        };

        await this.dispatcher.DispatchAsync(this, eventArgs);
    }

    internal async Task OnStageInstanceDeleteAsync(DiscordStageInstance instance)
    {
        instance.Discord = this;

        DiscordGuild guild = InternalGetCachedGuild(instance.GuildId);

        guild.stageInstances.TryRemove(instance.Id, out _);

        StageInstanceDeletedEventArgs eventArgs = new()
        {
            StageInstance = instance
        };

        await this.dispatcher.DispatchAsync(this, eventArgs);
    }

    #endregion

    #region Misc

    internal async Task OnInteractionCreateAsync(ulong? guildId, ulong channelId, TransportUser user, TransportMember member, DiscordChannel? channel, DiscordInteraction interaction)
    {
        DiscordUser usr = new(user) { Discord = this };

        interaction.ChannelId = channelId;
        interaction.GuildId = guildId;
        interaction.Discord = this;
        interaction.Data.Discord = this;

        if (member is not null && guildId is not null)
        {
            usr = new DiscordMember(member) { guild_id = guildId.Value, Discord = this };
            UpdateUser(usr, guildId, interaction.Guild, member);
        }
        else
        {
            UpdateUserCache(usr);
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
                    UpdateUserCache(c.Value);
                }
            }

            if (resolved.Members != null)
            {
                foreach (KeyValuePair<ulong, DiscordMember> c in resolved.Members)
                {
                    c.Value.Discord = this;
                    c.Value.Id = c.Key;
                    c.Value.guild_id = guildId.Value;
                    c.Value.User.Discord = this;

                    UpdateUserCache(c.Value.User);
                }
            }

            if (resolved.Channels != null)
            {
                foreach (KeyValuePair<ulong, DiscordChannel> c in resolved.Channels)
                {
                    c.Value.Discord = this;
                    UpdateChannelCache(c.Value);
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
                        c.Value.guild_id = guildId.Value;
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
                        m.Value.guildId = guildId.Value;
                    }
                }
            }
        }

        UpdateChannelCache(channel);

        if (interaction.Type is DiscordInteractionType.Component)
        {

            interaction.Message.Discord = this;
            interaction.Message.ChannelId = interaction.ChannelId;
            ComponentInteractionCreatedEventArgs cea = new()
            {
                Message = interaction.Message,
                Interaction = interaction
            };

            await this.dispatcher.DispatchAsync(this, cea);
        }
        else if (interaction.Type is DiscordInteractionType.ModalSubmit)
        {
            ModalSubmittedEventArgs mea = new(interaction);

            await this.dispatcher.DispatchAsync(this, mea);
        }
        else if (interaction.Data.Type is DiscordApplicationCommandType.MessageContextMenu or DiscordApplicationCommandType.UserContextMenu) // Context-Menu. //
        {
            ulong targetId = interaction.Data.Target.Value;
            DiscordUser targetUser = null;
            DiscordMember targetMember = null;
            DiscordMessage targetMessage = null;

            interaction.Data.Resolved.Messages?.TryGetValue(targetId, out targetMessage);
            interaction.Data.Resolved.Members?.TryGetValue(targetId, out targetMember);
            interaction.Data.Resolved.Users?.TryGetValue(targetId, out targetUser);

            ContextMenuInteractionCreatedEventArgs ctea = new()
            {
                Interaction = interaction,
                TargetUser = targetMember ?? targetUser,
                TargetMessage = targetMessage,
                Type = interaction.Data.Type,
            };

            await this.dispatcher.DispatchAsync(this, ctea);
        }

        InteractionCreatedEventArgs ea = new()
        {
            Interaction = interaction
        };

        await this.dispatcher.DispatchAsync(this, ea);
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

        DiscordGuild guild = InternalGetCachedGuild(guildId);
        DiscordUser usr = UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr);

        TypingStartedEventArgs ea = new()
        {
            Channel = channel,
            User = usr,
            Guild = guild,
            StartedAt = started
        };
        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
    {
        WebhooksUpdatedEventArgs ea = new()
        {
            Channel = channel,
            Guild = guild
        };
        await this.dispatcher.DispatchAsync(this, ea);
    }

    internal async Task OnStickersUpdatedAsync(IEnumerable<DiscordMessageSticker> newStickers, JObject raw)
    {
        DiscordGuild guild = InternalGetCachedGuild((ulong)raw["guild_id"]);
        ConcurrentDictionary<ulong, DiscordMessageSticker> oldStickers = new(guild.stickers);

        guild.stickers.Clear();

        foreach (DiscordMessageSticker nst in newStickers)
        {
            if (nst.User != null)
            {
                nst.User.Discord = this;
            }

            nst.Discord = this;

            guild.stickers[nst.Id] = nst;
        }

        GuildStickersUpdatedEventArgs sea = new()
        {
            Guild = guild,
            StickersBefore = oldStickers,
            StickersAfter = guild.Stickers
        };

        await this.dispatcher.DispatchAsync(this, sea);
    }

    internal async Task OnUnknownEventAsync(GatewayPayload payload)
    {
        UnknownEventArgs ea = new() { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
        await this.dispatcher.DispatchAsync(this, ea);
    }

    #endregion

    #region AutoModeration
    internal async Task OnAutoModerationRuleCreateAsync(DiscordAutoModerationRule ruleCreated)
    {
        ruleCreated.Discord = this;
        await this.dispatcher.DispatchAsync(this, new AutoModerationRuleCreatedEventArgs
        {
            Rule = ruleCreated
        });
    }

    internal async Task OnAutoModerationRuleUpdatedAsync(DiscordAutoModerationRule ruleUpdated)
    {
        ruleUpdated.Discord = this;
        await this.dispatcher.DispatchAsync(this, new AutoModerationRuleUpdatedEventArgs
        {
            Rule = ruleUpdated
        });
    }

    internal async Task OnAutoModerationRuleDeletedAsync(DiscordAutoModerationRule ruleDeleted)
    {
        ruleDeleted.Discord = this;
        await this.dispatcher.DispatchAsync(this, new AutoModerationRuleDeletedEventArgs
        {
            Rule = ruleDeleted
        });
    }

    internal async Task OnAutoModerationRuleExecutedAsync(DiscordAutoModerationActionExecution ruleExecuted)
    {
        await this.dispatcher.DispatchAsync(this, new AutoModerationRuleExecutedEventArgs
        {
            Rule = ruleExecuted
        });
    }
    #endregion

    #region Entitlements
    
    private async Task OnEntitlementCreatedAsync(DiscordEntitlement entitlement) 
        => await this.dispatcher.DispatchAsync(this, new EntitlementCreatedEventArgs { Entitlement = entitlement });

    private async Task OnEntitlementUpdatedAsync(DiscordEntitlement entitlement) 
        => await this.dispatcher.DispatchAsync(this, new EntitlementUpdatedEventArgs { Entitlement = entitlement });

    private async Task OnEntitlementDeletedAsync(DiscordEntitlement entitlement) 
        => await this.dispatcher.DispatchAsync(this,  new EntitlementDeletedEventArgs { Entitlement = entitlement });

    #endregion

    #endregion
}
