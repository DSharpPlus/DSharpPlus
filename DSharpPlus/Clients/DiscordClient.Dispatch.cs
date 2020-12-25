using System;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;

namespace DSharpPlus
{
    public sealed partial class DiscordClient
    {
        #region Private Fields

        private string _sessionId;
        private bool _guildDownloadCompleted = false;

        #endregion

        #region Dispatch Handler

        internal async Task HandleDispatchAsync(GatewayPayload payload)
        {
            if (!(payload.Data is JObject dat))
            {
                this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Invalid payload body (this message is probaby safe to ignore); opcode: {0} event: {1}; payload: {2}", payload.OpCode, payload.EventName, payload.Data);
                return;
            }

            DiscordChannel chn;
            ulong gid;
            ulong cid;
            TransportUser usr = default;
            TransportMember mbr = default;
            JToken rawMbr = default;

            switch (payload.EventName.ToLowerInvariant())
            {
                #region Gateway Status

                case "ready":
                    var glds = (JArray)dat["guilds"];
                    var dmcs = (JArray)dat["private_channels"];
                    await OnReadyEventAsync(dat.ToObject<ReadyPayload>(), glds, dmcs).ConfigureAwait(false);
                    break;

                case "resumed":
                    await OnResumedAsync().ConfigureAwait(false);
                    break;

                #endregion

                #region Channel

                case "channel_create":
                    chn = dat.ToObject<DiscordChannel>();
                    await OnChannelCreateEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn, dat["recipients"] as JArray).ConfigureAwait(false);
                    break;

                case "channel_update":
                    await OnChannelUpdateEventAsync(dat.ToObject<DiscordChannel>()).ConfigureAwait(false);
                    break;

                case "channel_delete":
                    chn = dat.ToObject<DiscordChannel>();
                    await OnChannelDeleteEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn).ConfigureAwait(false);
                    break;

                case "channel_pins_update":
                    cid = (ulong)dat["channel_id"];
                    var ts = (string)dat["last_pin_timestamp"];
                    await this.OnChannelPinsUpdate((ulong?)dat["guild_id"], this.InternalGetCachedChannel(cid), ts != null ? DateTimeOffset.Parse(ts, CultureInfo.InvariantCulture) : default(DateTimeOffset?)).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild

                case "guild_create":
                    await OnGuildCreateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
                    break;

                case "guild_update":
                    await OnGuildUpdateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]).ConfigureAwait(false);
                    break;

                case "guild_delete":
                    await OnGuildDeleteEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]).ConfigureAwait(false);
                    break;

                case "guild_sync":
                    gid = (ulong)dat["id"];
                    await this.OnGuildSyncEventAsync(this._guilds[gid], (bool)dat["large"], (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
                    break;

                case "guild_emojis_update":
                    gid = (ulong)dat["guild_id"];
                    var ems = dat["emojis"].ToObject<IEnumerable<DiscordEmoji>>();
                    await OnGuildEmojisUpdateEventAsync(this._guilds[gid], ems).ConfigureAwait(false);
                    break;

                case "guild_integrations_update":
                    gid = (ulong)dat["guild_id"];

                    // discord fires this event inconsistently if the current user leaves a guild.
                    if (!this._guilds.ContainsKey(gid))
                        return;

                    await OnGuildIntegrationsUpdateEventAsync(this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild Ban

                case "guild_ban_add":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanAddEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_ban_remove":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanRemoveEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild Member

                case "guild_member_add":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberAddEventAsync(dat.ToObject<TransportMember>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_member_remove":
                    gid = (ulong)dat["guild_id"];
                    usr = dat["user"].ToObject<TransportUser>();

                    if (!this._guilds.ContainsKey(gid))
                    {
                        // discord fires this event inconsistently if the current user leaves a guild.
                        if (usr.Id != this.CurrentUser.Id)
                            this.Logger.LogError(LoggerEvents.WebSocketReceive, "Could not find {0} in guild cache", gid);
                        return;
                    }

                    await OnGuildMemberRemoveEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_member_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberUpdateEventAsync(dat["user"].ToObject<TransportUser>(), this._guilds[gid], dat["roles"].ToObject<IEnumerable<ulong>>(), (string)dat["nick"]).ConfigureAwait(false);
                    break;

                case "guild_members_chunk":
                    await OnGuildMembersChunkEventAsync(dat).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild Role

                case "guild_role_create":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleCreateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_role_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleUpdateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_role_delete":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Invite

                case "invite_create":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnInviteCreateEventAsync(cid, gid, dat.ToObject<DiscordInvite>()).ConfigureAwait(false);
                    break;

                case "invite_delete":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnInviteDeleteEventAsync(cid, gid, dat).ConfigureAwait(false);
                    break;

                #endregion

                #region Message

                case "message_ack":
                    cid = (ulong)dat["channel_id"];
                    var mid = (ulong)dat["message_id"];
                    await OnMessageAckEventAsync(this.InternalGetCachedChannel(cid), mid).ConfigureAwait(false);
                    break;

                case "message_create":
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    await OnMessageCreateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"].ToObject<TransportUser>(), mbr).ConfigureAwait(false);
                    break;

                case "message_update":
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    await OnMessageUpdateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"]?.ToObject<TransportUser>(), mbr).ConfigureAwait(false);
                    break;

                // delete event does *not* include message object 
                case "message_delete":
                    await OnMessageDeleteEventAsync((ulong)dat["id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "message_delete_bulk":
                    await OnMessageBulkDeleteEventAsync(dat["ids"].ToObject<ulong[]>(), (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                #endregion

                #region Message Reaction

                case "message_reaction_add":
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    await OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], mbr, dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                    break;

                case "message_reaction_remove":
                    await OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                    break;

                case "message_reaction_remove_all":
                    await OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "message_reaction_remove_emoji":
                    await OnMessageReactionRemoveEmojiAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong)dat["guild_id"], dat["emoji"]).ConfigureAwait(false);
                    break;

                #endregion

                #region User/Presence Update

                case "presence_update":
                    await OnPresenceUpdateEventAsync(dat, (JObject)dat["user"]).ConfigureAwait(false);
                    break;

                case "user_settings_update":
                    await OnUserSettingsUpdateEventAsync(dat.ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                case "user_update":
                    await OnUserUpdateEventAsync(dat.ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                #endregion

                #region Voice

                case "voice_state_update":
                    await OnVoiceStateUpdateEventAsync(dat).ConfigureAwait(false);
                    break;

                case "voice_server_update":
                    gid = (ulong)dat["guild_id"];
                    await OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Interaction/Integration 

                case "interaction_create":
                    mbr = dat["member"].ToObject<TransportMember>();
                    cid = (ulong)dat["channel_id"];
                    gid = (ulong)dat["guild_id"];
                    await OnInteractionCreateAsync(gid, cid, mbr, dat.ToObject<DiscordInteraction>()).ConfigureAwait(false);
                    break;

                case "integration_create":
                    break;

                case "integration_update":
                    break;

                case "integration_delete":
                    break;

                #endregion

                #region Misc

                case "gift_code_update": //Not supposed to be dispatched to bots
                    break;

                case "typing_start":
                    cid = (ulong)dat["channel_id"];
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    await OnTypingStartEventAsync((ulong)dat["user_id"], cid, this.InternalGetCachedChannel(cid), (ulong?)dat["guild_id"], Utilities.GetDateTimeOffset((long)dat["timestamp"]), mbr).ConfigureAwait(false);
                    break;

                case "webhooks_update":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnWebhooksUpdateAsync(this._guilds[gid].GetChannel(cid), this._guilds[gid]).ConfigureAwait(false);
                    break;

                default:
                    await OnUnknownEventAsync(payload).ConfigureAwait(false);
                    this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown event: {0}\npayload: {1}", payload.EventName, payload.Data);
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

            var rusr = ready.CurrentUser;
            this.CurrentUser.Username = rusr.Username;
            this.CurrentUser.Discriminator = rusr.Discriminator;
            this.CurrentUser.AvatarHash = rusr.AvatarHash;
            this.CurrentUser.MfaEnabled = rusr.MfaEnabled;
            this.CurrentUser.Verified = rusr.Verified;
            this.CurrentUser.IsBot = rusr.IsBot;

            this.GatewayVersion = ready.GatewayVersion;
            this._sessionId = ready.SessionId;
            var raw_guild_index = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

            this._privateChannels.Clear();
            foreach (var rawChannel in rawDmChannels)
            {
                var channel = rawChannel.ToObject<DiscordDmChannel>();

                channel.Discord = this;

                //xdc._recipients = 
                //    .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this })
                //    .ToList();

                var recips_raw = rawChannel["recipients"].ToObject<IEnumerable<TransportUser>>();
                channel._recipients = new List<DiscordUser>();
                foreach (var xr in recips_raw)
                {
                    var xu = new DiscordUser(xr) { Discord = this };
                    xu = this.UserCache.AddOrUpdate(xr.Id, xu, (id, old) =>
                    {
                        old.Username = xu.Username;
                        old.Discriminator = xu.Discriminator;
                        old.AvatarHash = xu.AvatarHash;
                        return old;
                    });

                    channel._recipients.Add(xu);
                }

                this._privateChannels[channel.Id] = channel;
            }

            this._guilds.Clear();
            foreach (var guild in ready.Guilds)
            {
                guild.Discord = this;

                if (guild._channels == null)
                    guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();

                foreach (var xc in guild.Channels.Values)
                {
                    xc.GuildId = guild.Id;
                    xc.Discord = this;
                    foreach (var xo in xc._permissionOverwrites)
                    {
                        xo.Discord = this;
                        xo._channel_id = xc.Id;
                    }
                }

                if (guild._roles == null)
                    guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();

                foreach (var xr in guild.Roles.Values)
                {
                    xr.Discord = this;
                    xr._guild_id = guild.Id;
                }

                var raw_guild = raw_guild_index[guild.Id];
                var raw_members = (JArray)raw_guild["members"];

                if (guild._members != null)
                    guild._members.Clear();
                else
                    guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

                if (raw_members != null)
                {
                    foreach (var xj in raw_members)
                    {
                        var xtm = xj.ToObject<TransportMember>();

                        var xu = new DiscordUser(xtm.User) { Discord = this };
                        xu = this.UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                        {
                            old.Username = xu.Username;
                            old.Discriminator = xu.Discriminator;
                            old.AvatarHash = xu.AvatarHash;
                            return old;
                        });

                        guild._members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id };
                    }
                }

                if (guild._emojis == null)
                    guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();

                foreach (var xe in guild.Emojis.Values)
                    xe.Discord = this;

                if (guild._voiceStates == null)
                    guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();

                foreach (var xvs in guild.VoiceStates.Values)
                    xvs.Discord = this;

                this._guilds[guild.Id] = guild;
            }

            await this._ready.InvokeAsync(this, new ReadyEventArgs()).ConfigureAwait(false);
        }

        internal Task OnResumedAsync()
        {
            this.Logger.LogInformation(LoggerEvents.SessionUpdate, "Session resumed");
            return this._resumed.InvokeAsync(this, new ReadyEventArgs());
        }

        #endregion

        #region Channel

        internal async Task OnChannelCreateEventAsync(DiscordChannel channel, JArray rawRecipients)
        {
            channel.Discord = this;

            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var dmChannel = channel as DiscordDmChannel;

                var recips = rawRecipients.ToObject<IEnumerable<TransportUser>>()
                    .Select(xtu => this.TryGetCachedUserInternal(xtu.Id, out var usr) ? usr : new DiscordUser(xtu) { Discord = this });
                dmChannel._recipients = recips.ToList();

                this._privateChannels[dmChannel.Id] = dmChannel;

                await this._dmChannelCreated.InvokeAsync(this, new DmChannelCreateEventArgs { Channel = dmChannel }).ConfigureAwait(false);
            }
            else
            {
                channel.Discord = this;
                foreach (var xo in channel._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = channel.Id;
                }

                this._guilds[channel.GuildId]._channels[channel.Id] = channel;

                await this._channelCreated.InvokeAsync(this, new ChannelCreateEventArgs { Channel = channel, Guild = channel.Guild }).ConfigureAwait(false);
            }
        }

        internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            var gld = channel.Guild;

            var channel_new = this.InternalGetCachedChannel(channel.Id);
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
                    PerUserRateLimit = channel_new.PerUserRateLimit
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

                channel_new._permissionOverwrites.Clear();

                foreach (var po in channel._permissionOverwrites)
                {
                    po.Discord = this;
                    po._channel_id = channel.Id;
                }

                channel_new._permissionOverwrites.AddRange(channel._permissionOverwrites);
            }
            else if(gld != null)
            {
                gld._channels[channel.Id] = channel;
            }

            await this._channelUpdated.InvokeAsync(this, new ChannelUpdateEventArgs { ChannelAfter = channel_new, Guild = gld, ChannelBefore = channel_old }).ConfigureAwait(false);
        }

        internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            //if (channel.IsPrivate)
            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var dmChannel = channel as DiscordDmChannel;

                if (this._privateChannels.TryRemove(dmChannel.Id, out var cachedDmChannel)) dmChannel = cachedDmChannel;

                await this._dmChannelDeleted.InvokeAsync(this, new DmChannelDeleteEventArgs { Channel = dmChannel }).ConfigureAwait(false);
            }
            else
            {
                var gld = channel.Guild;

                if (gld._channels.TryRemove(channel.Id, out var cachedChannel)) channel = cachedChannel;

                await this._channelDeleted.InvokeAsync(this, new ChannelDeleteEventArgs { Channel = channel, Guild = gld }).ConfigureAwait(false);
            }
        }

        internal async Task OnChannelPinsUpdate(ulong? guildId, DiscordChannel channel, DateTimeOffset? lastPinTimestamp)
        {
            if (channel == null)
                return;

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new ChannelPinsUpdateEventArgs
            {
                Guild = guild,
                Channel = channel,
                LastPinTimestamp = lastPinTimestamp
            };
            await this._channelPinsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild

        internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            if (presences != null)
            {
                foreach (var xp in presences)
                {
                    xp.Discord = this;
                    xp.GuildId = guild.Id;
                    xp.Activity = new DiscordActivity(xp.RawActivity);
                    if (xp.RawActivities != null)
                    {
                        xp.InternalActivities = new DiscordActivity[xp.RawActivities.Length];
                        for (int i = 0; i < xp.RawActivities.Length; i++)
                            xp.InternalActivities[i] = new DiscordActivity(xp.RawActivities[i]);
                    }
                    this._presences[xp.InternalUser.Id] = xp;
                }
            }

            var exists = this._guilds.TryGetValue(guild.Id, out var foundGuild);

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            if (exists)
                guild = foundGuild;

            if (guild._channels == null)
                guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if (guild._roles == null)
                guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (guild._emojis == null)
                guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (guild._voiceStates == null)
                guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (guild._members == null)
                guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

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

            foreach (var kvp in eventGuild._voiceStates) guild._voiceStates[kvp.Key] = kvp.Value;

            foreach (var xc in guild._channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (var xe in guild._emojis.Values)
                xe.Discord = this;
            foreach (var xvs in guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xr in guild._roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            var old = Volatile.Read(ref this._guildDownloadCompleted);
            var dcompl = this._guilds.Values.All(xg => !xg.IsUnavailable);
            Volatile.Write(ref this._guildDownloadCompleted, dcompl);

            if (exists)
                await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild }).ConfigureAwait(false);
            else
                await this._guildCreated.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild }).ConfigureAwait(false);

            if (dcompl && !old)
                await this._guildDownloadCompletedEv.InvokeAsync(this, new GuildDownloadCompletedEventArgs(this.Guilds)).ConfigureAwait(false);
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
                var gld = this._guilds[guild.Id];

                oldGuild = new DiscordGuild
                {
                    Discord = gld.Discord,
                    Name = gld.Name,
                    AfkChannelId = gld.AfkChannelId,
                    AfkTimeout = gld.AfkTimeout,
                    DefaultMessageNotifications = gld.DefaultMessageNotifications,
                    EmbedChannelId = gld.EmbedChannelId,
                    EmbedEnabled = gld.EmbedEnabled,
                    ExplicitContentFilter = gld.ExplicitContentFilter,
                    Features = gld.Features,
                    IconHash = gld.IconHash,
                    Id = gld.Id,
                    IsLarge = gld.IsLarge,
                    IsSynced = gld.IsSynced,
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
                    VoiceRegionId = gld.VoiceRegionId,
                    _channels = new ConcurrentDictionary<ulong, DiscordChannel>(),
                    _emojis = new ConcurrentDictionary<ulong, DiscordEmoji>(),
                    _members = new ConcurrentDictionary<ulong, DiscordMember>(),
                    _roles = new ConcurrentDictionary<ulong, DiscordRole>(),
                    _voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>()
                };

                foreach (var kvp in gld._channels) oldGuild._channels[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._emojis) oldGuild._emojis[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._roles) oldGuild._roles[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._voiceStates) oldGuild._voiceStates[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._members) oldGuild._members[kvp.Key] = kvp.Value;
            }

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            guild = this._guilds[eventGuild.Id];

            if (guild._channels == null)
                guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if (guild._roles == null)
                guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (guild._emojis == null)
                guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (guild._voiceStates == null)
                guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (guild._members == null)
                guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

            this.UpdateCachedGuild(eventGuild, rawMembers);

            foreach (var xc in guild._channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (var xe in guild._emojis.Values)
                xe.Discord = this;
            foreach (var xvs in guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xr in guild._roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            await this._guildUpdated.InvokeAsync(this, new GuildUpdateEventArgs { GuildBefore = oldGuild, GuildAfter = guild }).ConfigureAwait(false);
        }

        internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            if (guild.IsUnavailable)
            {
                if (!this._guilds.TryGetValue(guild.Id, out var gld))
                    return;

                gld.IsUnavailable = true;

                await this._guildUnavailable.InvokeAsync(this, new GuildDeleteEventArgs { Guild = guild, Unavailable = true }).ConfigureAwait(false);
            }
            else
            {
                if (!this._guilds.TryRemove(guild.Id, out var gld))
                    return;

                await this._guildDeleted.InvokeAsync(this, new GuildDeleteEventArgs { Guild = gld }).ConfigureAwait(false);
            }
        }

        internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool isLarge, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            presences = presences.Select(xp => { xp.Discord = this; xp.Activity = new DiscordActivity(xp.RawActivity); return xp; });
            foreach (var xp in presences)
                this._presences[xp.InternalUser.Id] = xp;

            guild.IsSynced = true;
            guild.IsLarge = isLarge;

            this.UpdateCachedGuild(guild, rawMembers);

            await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild }).ConfigureAwait(false);
        }

        internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
        {
            var oldEmojis = new ConcurrentDictionary<ulong, DiscordEmoji>(guild._emojis);
            guild._emojis.Clear();

            foreach (var emoji in newEmojis)
            {
                emoji.Discord = this;
                guild._emojis[emoji.Id] = emoji;
            }

            var ea = new GuildEmojisUpdateEventArgs
            {
                Guild = guild,
                EmojisAfter = guild.Emojis,
                EmojisBefore = new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(oldEmojis)
            };
            await this._guildEmojisUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
        {
            var ea = new GuildIntegrationsUpdateEventArgs
            {
                Guild = guild
            };
            await this._guildIntegrationsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Ban

        internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanAddEventArgs
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildBanAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanRemoveEventArgs
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildBanRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Member

        internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
        {
            var usr = new DiscordUser(member.User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(member.User.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            var mbr = new DiscordMember(member)
            {
                Discord = this,
                _guild_id = guild.Id
            };

            guild._members[mbr.Id] = mbr;
            guild.MemberCount++;

            var ea = new GuildMemberAddEventArgs
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildMemberAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            if (!guild._members.TryRemove(user.Id, out var mbr))
                mbr = new DiscordMember(new DiscordUser(user)) { Discord = this, _guild_id = guild.Id };
            guild.MemberCount--;

            var ea = new GuildMemberRemoveEventArgs
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildMemberRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberUpdateEventAsync(TransportUser user, DiscordGuild guild, IEnumerable<ulong> roles, string nick)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };

            var nick_old = mbr.Nickname;
            var roles_old = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));

            mbr.Nickname = nick;
            mbr._role_ids.Clear();
            mbr._role_ids.AddRange(roles);

            var ea = new GuildMemberUpdateEventArgs
            {
                Guild = guild,
                Member = mbr,

                NicknameAfter = mbr.Nickname,
                RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),

                NicknameBefore = nick_old,
                RolesBefore = roles_old
            };
            await this._guildMemberUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMembersChunkEventAsync(JObject dat)
        {
            var guild = this.Guilds[(ulong)dat["guild_id"]];
            var chunkIndex = (int)dat["chunk_index"];
            var chunkCount = (int)dat["chunk_count"];
            var nonce = (string)dat["nonce"];

            var mbrs = new HashSet<DiscordMember>();
            var pres = new HashSet<DiscordPresence>();

            var members = dat["members"].ToObject<TransportMember[]>();

            var memCount = members.Count();
            for (int i = 0; i < memCount; i++)
            {
                var mbr = new DiscordMember(members[i]) { Discord = this, _guild_id = guild.Id };

                if (!this.UserCache.ContainsKey(mbr.Id))
                    this.UserCache[mbr.Id] = new DiscordUser(members[i].User) { Discord = this };

                guild._members[mbr.Id] = mbr;

                mbrs.Add(mbr);
            }

            guild.MemberCount = guild._members.Count;

            var ea = new GuildMembersChunkEventArgs
            {
                Guild = guild,
                Members = new ReadOnlySet<DiscordMember>(mbrs),
                ChunkIndex = chunkIndex,
                ChunkCount = chunkCount,
                Nonce = nonce,
            };

            if (dat["presences"] != null)
            {
                var presences = dat["presences"].ToObject<DiscordPresence[]>();

                var presCount = presences.Count();
                for (int i = 0; i < presCount; i++)
                {
                    var xp = presences[i];
                    xp.Discord = this;
                    xp.Activity = new DiscordActivity(xp.RawActivity);

                    if (xp.RawActivities != null)
                    {
                        xp.InternalActivities = new DiscordActivity[xp.RawActivities.Length];
                        for (int j = 0; j < xp.RawActivities.Length; j++)
                            xp.InternalActivities[j] = new DiscordActivity(xp.RawActivities[j]);
                    }

                    pres.Add(xp);
                }

                ea.Presences = new ReadOnlySet<DiscordPresence>(pres);
            }

            if (dat["not_found"] != null)
            {
                var nf = dat["not_found"].ToObject<ISet<ulong>>();
                ea.NotFound = new ReadOnlySet<ulong>(nf);
            }

            await this._guildMembersChunked.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Role

        internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            role.Discord = this;
            role._guild_id = guild.Id;

            guild._roles[role.Id] = role;

            var ea = new GuildRoleCreateEventArgs
            {
                Guild = guild,
                Role = role
            };
            await this._guildRoleCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            var newRole = guild.GetRole(role.Id);
            var oldRole = new DiscordRole
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
                Position = newRole.Position
            };

            newRole._guild_id = guild.Id;
            newRole._color = role._color;
            newRole.IsHoisted = role.IsHoisted;
            newRole.IsManaged = role.IsManaged;
            newRole.IsMentionable = role.IsMentionable;
            newRole.Name = role.Name;
            newRole.Permissions = role.Permissions;
            newRole.Position = role.Position;

            var ea = new GuildRoleUpdateEventArgs
            {
                Guild = guild,
                RoleAfter = newRole,
                RoleBefore = oldRole
            };
            await this._guildRoleUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, DiscordGuild guild)
        {
            if (!guild._roles.TryRemove(roleId, out var role))
                throw new InvalidOperationException("Attempted to delete a nonexistent role.");

            var ea = new GuildRoleDeleteEventArgs
            {
                Guild = guild,
                Role = role
            };
            await this._guildRoleDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Invite

        internal async Task OnInviteCreateEventAsync(ulong channelId, ulong guildId, DiscordInvite invite)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            invite.Discord = this;

            guild._invites[invite.Code] = invite;

            var ea = new InviteCreateEventArgs
            {
                Channel = channel,
                Guild = guild,
                Invite = invite
            };
            await this._inviteCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnInviteDeleteEventAsync(ulong channelId, ulong guildId, JToken dat)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            if (!guild._invites.TryRemove(dat["code"].ToString(), out var invite))
            {
                invite = dat.ToObject<DiscordInvite>();
                invite.Discord = this;
            }

            invite.IsRevoked = true;

            var ea = new InviteDeleteEventArgs
            {
                Channel = channel,
                Guild = guild,
                Invite = invite
            };
            await this._inviteDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Message

        internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong messageId)
        {
            if (this.MessageCache == null || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == chn.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = chn.Id,
                    Discord = this,
                };
            }

            await this._messageAcknowledged.InvokeAsync(this, new MessageAcknowledgeEventArgs { Message = msg }).ConfigureAwait(false);
        }

        internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author, TransportMember member)
        {
            message.Discord = this;

            if (message.Channel == null)
                this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Channel which the last message belongs to is not in cache - cache state might be invalid!");
            else
                message.Channel.LastMessageId = message.Id;

            var guild = message.Channel?.Guild;

            this.UpdateMessage(message, author, guild, member);

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(xid => guild._members.TryGetValue(xid, out var member) ? member : new DiscordUser { Id = xid, Discord = this }).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(message).Select(xid => guild.GetRole(xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(message).Select(xid => guild.GetChannel(xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(this.GetCachedOrEmptyUserInternal).ToList();
                }
            }

            message._mentionedUsers = mentionedUsers;
            message._mentionedRoles = mentionedRoles;
            message._mentionedChannels = mentionedChannels;

            if (message._reactions == null)
                message._reactions = new List<DiscordReaction>();
            foreach (var xr in message._reactions)
                xr.Emoji.Discord = this;

            if (this.Configuration.MessageCacheSize > 0 && message.Channel != null)
                this.MessageCache?.Add(message);

            var ea = new MessageCreateEventArgs
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentionedUsers),
                MentionedRoles = mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(mentionedRoles) : null,
                MentionedChannels = mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(mentionedChannels) : null
            };
            await this._messageCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author, TransportMember member)
        {
            DiscordGuild guild;

            message.Discord = this;
            var event_message = message;

            DiscordMessage oldmsg = null;
            if (this.Configuration.MessageCacheSize == 0 
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == event_message.Id && xm.ChannelId == event_message.ChannelId, out message))
            {
                message = event_message;
                guild = message.Channel?.Guild;

                this.UpdateMessage(message, author, guild, member);

                if (message._reactions == null)
                    message._reactions = new List<DiscordReaction>();
                foreach (var xr in message._reactions)
                    xr.Emoji.Discord = this;
            }
            else
            {
                oldmsg = new DiscordMessage(message);

                guild = message.Channel?.Guild;
                message.EditedTimestampRaw = event_message.EditedTimestampRaw;
                if (event_message.Content != null)
                    message.Content = event_message.Content;
                message._embeds.Clear();
                message._embeds.AddRange(event_message._embeds);
                message.Pinned = event_message.Pinned;
                message.IsTTS = event_message.IsTTS;
            }

            var mentioned_users = new List<DiscordUser>();
            var mentioned_roles = guild != null ? new List<DiscordRole>() : null;
            var mentioned_channels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(xid => guild._members.TryGetValue(xid, out var member) ? member : null).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utilities.GetRoleMentions(message).Select(xid => guild.GetRole(xid)).ToList();
                    mentioned_channels = Utilities.GetChannelMentions(message).Select(xid => guild.GetChannel(xid)).ToList();
                }
                else
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(this.GetCachedOrEmptyUserInternal).ToList();
                }
            }

            message._mentionedUsers = mentioned_users;
            message._mentionedRoles = mentioned_roles;
            message._mentionedChannels = mentioned_channels;

            var ea = new MessageUpdateEventArgs
            {
                Message = message,
                MessageBefore = oldmsg,
                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentioned_users),
                MentionedRoles = mentioned_roles != null ? new ReadOnlyCollection<DiscordRole>(mentioned_roles) : null,
                MentionedChannels = mentioned_channels != null ? new ReadOnlyCollection<DiscordChannel>(mentioned_channels) : null
            };
            await this._messageUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId);
            var guild = this.InternalGetCachedGuild(guildId);

            if (channel == null 
                || this.Configuration.MessageCacheSize == 0 
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {

                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this,
                };
            }

            if (this.Configuration.MessageCacheSize > 0)
                this.MessageCache?.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channelId);

            var ea = new MessageDeleteEventArgs
            {
                Channel = channel,
                Message = msg,
                Guild = guild
            };
            await this._messageDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnMessageBulkDeleteEventAsync(ulong[] messageIds, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId);

            var msgs = new List<DiscordMessage>(messageIds.Length);
            foreach (var messageId in messageIds)
            {
                if (channel == null 
                    || this.Configuration.MessageCacheSize == 0 
                    || this.MessageCache == null
                    || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
                {
                    msg = new DiscordMessage
                    {
                        Id = messageId,
                        ChannelId = channelId,
                        Discord = this,
                    };
                }
                if (this.Configuration.MessageCacheSize > 0)
                    this.MessageCache?.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channelId);
                msgs.Add(msg);
            }

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageBulkDeleteEventArgs
            {
                Channel = channel,
                Messages = new ReadOnlyCollection<DiscordMessage>(msgs),
                Guild = guild
            };
            await this._messagesBulkDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Message Reaction

        internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, TransportMember mbr, DiscordEmoji emoji)
        {
            var channel = this.InternalGetCachedChannel(channelId);
            var guild = this.InternalGetCachedGuild(guildId);
            emoji.Discord = this;

            var usr = this.UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr);

            if (channel == null 
                || this.Configuration.MessageCacheSize == 0 
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this,
                    _reactions = new List<DiscordReaction>()
                };
            }

            var react = msg._reactions.FirstOrDefault(xr => xr.Emoji == emoji);
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

            var ea = new MessageReactionAddEventArgs
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = emoji
            };
            await this._messageReactionAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji)
        {
            var channel = this.InternalGetCachedChannel(channelId);

            emoji.Discord = this;

            if (!this.UserCache.TryGetValue(userId, out var usr))
                usr = new DiscordUser { Id = userId, Discord = this };

            if (channel?.Guild != null)
                usr = channel.Guild.Members.TryGetValue(userId, out var member)
                    ? member
                    : new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId };

            if (channel == null 
                || this.Configuration.MessageCacheSize == 0 
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            var react = msg._reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react != null)
            {
                react.Count--;
                react.IsMe &= this.CurrentUser.Id != userId;

                if (msg._reactions != null && react.Count <= 0) // shit happens
                    for (var i = 0; i < msg._reactions.Count; i++)
                        if (msg._reactions[i].Emoji == emoji)
                        {
                            msg._reactions.RemoveAt(i);
                            break;
                        }
            }

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageReactionRemoveEventArgs
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = emoji
            };
            await this._messageReactionRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId);

            if (channel == null 
                || this.Configuration.MessageCacheSize == 0 
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            msg._reactions?.Clear();

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageReactionsClearEventArgs
            {
                Message = msg,
                Guild = guild
            };

            await this._messageReactionsCleared.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveEmojiAsync(ulong messageId, ulong channelId, ulong guildId, JToken dat)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            if (channel == null 
                || this.Configuration.MessageCacheSize == 0 
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            var partialEmoji = dat.ToObject<DiscordEmoji>();

            if (!guild._emojis.TryGetValue(partialEmoji.Id, out var emoji))
            {
                emoji = partialEmoji;
                emoji.Discord = this;
            }

            msg._reactions?.RemoveAll(r => r.Emoji.Equals(emoji));

            var ea = new MessageReactionRemoveEmojiEventArgs
            {
                Channel = channel,
                Guild = guild,
                Message = msg,
                Emoji = emoji
            };

            await this._messageReactionRemovedEmoji.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region User/Presence Update

        internal async Task OnPresenceUpdateEventAsync(JObject rawPresence, JObject rawUser)
        {
            var uid = (ulong)rawUser["id"];
            DiscordPresence old = null;

            if (this._presences.TryGetValue(uid, out var presence))
            {
                old = new DiscordPresence(presence);
                DiscordJson.PopulateObject(rawPresence, presence);

                if (rawPresence["game"] == null || rawPresence["game"].Type == JTokenType.Null)
                    presence.RawActivity = null;

                if (presence.Activity != null)
                    presence.Activity.UpdateWith(presence.RawActivity);
                else
                    presence.Activity = new DiscordActivity(presence.RawActivity);
            }
            else
            {
                presence = rawPresence.ToObject<DiscordPresence>();
                presence.Discord = this;
                presence.Activity = new DiscordActivity(presence.RawActivity);
                this._presences[presence.InternalUser.Id] = presence;
            }

            // reuse arrays / avoid linq (this is a hot zone)
            if (presence.Activities == null || rawPresence["activities"] == null)
            {
                presence.InternalActivities = Array.Empty<DiscordActivity>();
            }
            else
            {
                if (presence.InternalActivities.Length != presence.RawActivities.Length)
                    presence.InternalActivities = new DiscordActivity[presence.RawActivities.Length];

                for (var i = 0; i < presence.InternalActivities.Length; i++)
                    presence.InternalActivities[i] = new DiscordActivity(presence.RawActivities[i]);
            }

            if (this.UserCache.TryGetValue(uid, out var usr))
            {
                if (old != null)
                {
                    old.InternalUser.Username = usr.Username;
                    old.InternalUser.Discriminator = usr.Discriminator;
                    old.InternalUser.AvatarHash = usr.AvatarHash;
                }

                if (rawUser["username"] is object)
                    usr.Username = (string)rawUser["username"];
                if (rawUser["discriminator"] is object)
                    usr.Discriminator = (string)rawUser["discriminator"];
                if (rawUser["avatar"] is object)
                    usr.AvatarHash = (string)rawUser["avatar"];

                presence.InternalUser.Username = usr.Username;
                presence.InternalUser.Discriminator = usr.Discriminator;
                presence.InternalUser.AvatarHash = usr.AvatarHash;
            }

            var usrafter = usr ?? new DiscordUser(presence.InternalUser);
            var ea = new PresenceUpdateEventArgs
            {
                Status = presence.Status,
                Activity = presence.Activity,
                User = usr,
                PresenceBefore = old,
                PresenceAfter = presence,
                UserBefore = old != null ? new DiscordUser(old.InternalUser) : usrafter,
                UserAfter = usrafter
            };
            await this._presenceUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnUserSettingsUpdateEventAsync(TransportUser user)
        {
            var usr = new DiscordUser(user) { Discord = this };

            var ea = new UserSettingsUpdateEventArgs
            {
                User = usr
            };
            await this._userSettingsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnUserUpdateEventAsync(TransportUser user)
        {
            var usr_old = new DiscordUser
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

            var ea = new UserUpdateEventArgs
            {
                UserAfter = this.CurrentUser,
                UserBefore = usr_old
            };
            await this._userUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Voice

        internal async Task OnVoiceStateUpdateEventAsync(JObject raw)
        {
            var gid = (ulong)raw["guild_id"];
            var uid = (ulong)raw["user_id"];
            var gld = this._guilds[gid];

            var vstateNew = raw.ToObject<DiscordVoiceState>();
            vstateNew.Discord = this;

            gld._voiceStates.TryGetValue(uid, out var vstateOld);

            gld._voiceStates[vstateNew.UserId] = vstateNew;

            if (gld._members.TryGetValue(uid, out var mbr))
            {
                mbr.IsMuted = vstateNew.IsServerMuted;
                mbr.IsDeafened = vstateNew.IsServerDeafened;
            }

            var ea = new VoiceStateUpdateEventArgs
            {
                Guild = vstateNew.Guild,
                Channel = vstateNew.Channel,
                User = vstateNew.User,
                SessionId = vstateNew.SessionId,

                Before = vstateOld,
                After = vstateNew
            };
            await this._voiceStateUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
        {
            var ea = new VoiceServerUpdateEventArgs
            {
                Endpoint = endpoint,
                VoiceToken = token,
                Guild = guild
            };
            await this._voiceServerUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Misc

        internal async Task OnInteractionCreateAsync(ulong guildId, ulong channelId, TransportMember member, DiscordInteraction interaction)
        {
            interaction.Member = new DiscordMember(member);
            interaction.ChannelId = channelId;
            interaction.GuildId = guildId;
            interaction.Discord = this;
            interaction.Data.Discord = this;

            var ea = new InteractionCreateEventArgs
            {
                Interaction = interaction
            };

            await this._interactionCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnTypingStartEventAsync(ulong userId, ulong channelId, DiscordChannel channel, ulong? guildId, DateTimeOffset started, TransportMember mbr)
        {
            if (channel == null)
            {
                channel = new DiscordChannel
                {
                    Discord = this,
                    Id = channelId,
                    GuildId = guildId.HasValue ? guildId.Value : default,
                };
            }

            var guild = this.InternalGetCachedGuild(guildId);
            var usr = this.UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr);

            var ea = new TypingStartEventArgs
            {
                Channel = channel,
                User = usr,
                Guild = guild,
                StartedAt = started
            };
            await this._typingStarted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
        {
            var ea = new WebhooksUpdateEventArgs
            {
                Channel = channel,
                Guild = guild
            };
            await this._webhooksUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        internal async Task OnUnknownEventAsync(GatewayPayload payload)
        {
            var ea = new UnknownEventArgs { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
            await this._unknownEvent.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}