using System.Collections.Generic;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using DSharpPlus.Enums;

// ReSharper disable once CheckNamespace
namespace DSharpPlus
{
    public class DiscordRestClient : BaseDiscordClient
    {
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds => new ReadOnlyDictionary<ulong, DiscordGuild>(_guilds);
        // ReSharper disable once InconsistentNaming
        internal Dictionary<ulong, DiscordGuild> _guilds;

        public DiscordRestClient(DiscordConfiguration config) : base(config)
        {
            _disposed = false;
            var gs = ApiClient.GetCurrentUserGuildsAsync(100, null, null).GetAwaiter().GetResult();
            _guilds = new Dictionary<ulong, DiscordGuild>();
            foreach (DiscordGuild g in gs)
            {
                _guilds[g.Id] = g;
            }
        }

        #region Guild
        public async Task<DiscordGuild> CreateGuildAsync(string name, string regionId, string iconb64, VerificationLevel? verificationLevel,
            DefaultMessageNotifications? defaultMessageNotifications) => await ApiClient.CreateGuildAsync(name, regionId, iconb64, verificationLevel, defaultMessageNotifications);

        public async Task DeleteGuildAsync(ulong id) => await ApiClient.DeleteGuildAsync(id);

        public async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, string name, string region, VerificationLevel? verificationLevel, DefaultMessageNotifications? defaultMessageNotifications, 
            MfaLevel? mfaLevel, ExplicitContentFilter? explicitContentFilter, ulong? afkChannelId, int? afkTimeout, string iconb64, ulong? ownerId, string splashb64, string reason)
            => await ApiClient.ModifyGuildAsync(guildId, name, region, verificationLevel, defaultMessageNotifications, mfaLevel, explicitContentFilter, afkChannelId, afkTimeout, iconb64, 
                ownerId, splashb64, reason);

        public async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guildId) => await ApiClient.GetGuildBansAsync(guildId);

        public Task CreateGuildBanAsync(ulong guildId, ulong userId, int deleteMessageDays, string reason) => ApiClient.CreateGuildBanAsync(guildId, userId, deleteMessageDays, reason);

        public Task RemoveGuildBanAsync(ulong guildId, ulong userId, string reason) => ApiClient.RemoveGuildBanAsync(guildId, userId, reason);

        public Task LeaveGuildAsync(ulong guildId) => ApiClient.LeaveGuildAsync(guildId);

        public async Task<DiscordMember> AddGuildMemberAsync(ulong guildId, ulong userId, string accessToken, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
            => await ApiClient.AddGuildMemberAsync(guildId, userId, Configuration.Token, nick, roles, muted, deafened);

        public async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guildId, int? limit, ulong? after)
        {
            var recmbr = new List<DiscordMember>();

            var recd = 1000;
            var last = 0ul;
            while (recd == 1000)
            {
                var tms = await ApiClient.ListGuildMembersAsync(guildId, 1000, last == 0 ? null : (ulong?)last);
                recd = tms.Count;

                foreach (var xtm in tms)
                {
                    if (UserCache.ContainsKey(xtm.User.Id))
                    {
                        continue;
                    }

                    var usr = new DiscordUser(xtm.User) { Discord = this };
                    // ReSharper disable once RedundantAssignment
                    usr = UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
                    {
                        // ReSharper disable AccessToModifiedClosure
                        old.Username = usr.Username;
                        old.Discord = usr.Discord;
                        old.AvatarHash = usr.AvatarHash;
                        // ReSharper restore AccessToModifiedClosure

                        return old;
                    });
                }

                var tm = tms.LastOrDefault();
                if (tm != null)
                {
                    last = tm.User.Id;
                }
                else
                {
                    last = 0;
                }

                recmbr.AddRange(tms.Select(xtm => new DiscordMember(xtm) { Discord = this, GuildId = guildId }));
            }
            
            return new ReadOnlyCollection<DiscordMember>(recmbr);
        }

        public Task AddGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason) => ApiClient.AddGuildMemberRoleAsync(guildId, userId, roleId, reason);

        public Task RemoveGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason) => ApiClient.RemoveGuildMemberRoleAsync(guildId, userId, roleId, reason);

        public Task UpdateRolePositionAsync(ulong guildId, ulong roleId, int position, string reason = null)
        {
            List<RestGuildRoleReorderPayload> rgrrps = new List<RestGuildRoleReorderPayload>()
            {
                new RestGuildRoleReorderPayload { RoleId = roleId }
            };
            return ApiClient.ModifyGuildRolePosition(guildId, rgrrps, reason);
        }

        public Task UpdateChannelPositionAsync(ulong guildId, ulong channelId, int position, string reason)
        {
            List<RestGuildChannelReorderPayload> rgcrps = new List<RestGuildChannelReorderPayload>()
            {
                new RestGuildChannelReorderPayload { ChannelId = channelId, Position = position }
            };
            return ApiClient.ModifyGuildChannelPosition(guildId, rgcrps, reason);
        }


        #endregion

        #region Channel
        public Task<DiscordChannel> CreateGuildChannelAsync(ulong id, string name, ChannelType type, ulong? parent, int? bitrate, int? userLimit, IEnumerable<DiscordOverwrite> overwrites, string reason)
        {
            if (type != ChannelType.Category && type != ChannelType.Text && type != ChannelType.Voice)
            {
                throw new ArgumentException("Channel type must be text, voice, or category.", nameof(type));
            }

            return ApiClient.CreateGuildChannelAsync(id, name, type, parent, bitrate, userLimit, overwrites, reason);
        }

        public Task ModifyChannelAsync(ulong id, string name, int? position, string topic, Optional<ulong?> parent, int? bitrate, int? userLimit, string reason)
            => ApiClient.ModifyChannelAsync(id, name, position, topic, parent, bitrate, userLimit, reason);

        public Task<DiscordChannel> GetChannelAsync(ulong id) => ApiClient.GetChannelAsync(id);

        public Task DeleteChannelAsync(ulong id, string reason) => ApiClient.DeleteChannelAsync(id, reason);

        public Task<DiscordMessage> GetMessageAsync(ulong channelId, ulong messageId) => ApiClient.GetMessageAsync(channelId, messageId);

        public Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content, bool? tts, DiscordEmbed embed) =>
            ApiClient.CreateMessageAsync(channelId, content, tts, embed);

        public Task<DiscordMessage> UploadFileAsync(ulong channelId, Stream fileData, string fileName, string content, bool? tts, DiscordEmbed embed)
            => ApiClient.UploadFileAsync(channelId, fileData, fileName, content, tts, embed);

        public Task<DiscordMessage> UploadFilesAsync(ulong channelId, Dictionary<string, Stream> files, string content, bool? tts, DiscordEmbed embed)
            => ApiClient.UploadFilesAsync(channelId, files, content, tts, embed);

        public Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guildId) => ApiClient.GetGuildChannelsAsync(guildId);

        public Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channelId, int limit, ulong? before, ulong? after, ulong? around)
            => ApiClient.GetChannelMessagesAsync(channelId, limit, before, after, around);

        public Task<DiscordMessage> GetChannelMessageAsync(ulong channelId, ulong messageId) =>
            ApiClient.GetChannelMessageAsync(channelId, messageId);

        public Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<string> content, Optional<DiscordEmbed> embed)
            => ApiClient.EditMessageAsync(channelId, messageId, content, embed);

        public Task DeleteMessageAsync(ulong channelId, ulong messageId, string reason)
            => ApiClient.DeleteMessageAsync(channelId, messageId, reason);

        public Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds, string reason)
            => ApiClient.DeleteMessagesAsync(channelId, messageIds, reason);

        public Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channelId)
            => ApiClient.GetChannelInvitesAsync(channelId);

        public Task<DiscordInvite> CreateChannelInviteAsync(ulong channelId, int maxAge, int maxUses, bool temporary, bool unique, string reason)
            => ApiClient.CreateChannelInviteAsync(channelId, maxAge, maxUses, temporary, unique, reason);

        public Task DeleteChannelPermissionAsync(ulong channelId, ulong overwriteId, string reason)
            => ApiClient.DeleteChannelPermissionAsync(channelId, overwriteId, reason);

        public Task EditChannelPermissionsAsync(ulong channelId, ulong overwriteId, Permissions allow, Permissions deny, string type, string reason)
            => ApiClient.EditChannelPermissionsAsync(channelId, overwriteId, allow, deny, type, reason);

        public Task TriggerTypingAsync(ulong channelId) => ApiClient.TriggerTypingAsync(channelId);

        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channelId)
            => ApiClient.GetPinnedMessagesAsync(channelId);

        public Task UnpinMessageAsync(ulong channelId, ulong messageId)
            => ApiClient.UnpinMessageAsync(channelId, messageId);

        public Task JoinGroupDmAsync(ulong channelId, string nickname)
            => ApiClient.GroupDmAddRecipientAsync(channelId, CurrentUser.Id, Configuration.Token, nickname);

        public Task GroupDmAddRecipientAsync(ulong channelId, ulong userId, string accessToken, string nickname)
            => ApiClient.GroupDmAddRecipientAsync(channelId, userId, accessToken, nickname);

        public Task LeaveGroupDmAsync(ulong channelId)
            => ApiClient.GroupDmRemoveRecipientAsync(channelId, CurrentUser.Id);

        public Task GroupDmRemoveRecipientAsync(ulong channelId, ulong userId)
            => ApiClient.GroupDmRemoveRecipientAsync(channelId, userId);

        public Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
            => ApiClient.CreateGroupDmAsync(accessTokens, nicks);

        public Task<DiscordDmChannel> CreateGroupDmWithCurrentUserAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
        {
            var a = accessTokens.ToList();
            a.Add(Configuration.Token);
            return ApiClient.CreateGroupDmAsync(a, nicks);
        }

        public Task<DiscordDmChannel> CreateDmAsync(ulong recipientId)
            => ApiClient.CreateDmAsync(recipientId);
        #endregion

        #region Member
        public Task<DiscordUser> GetCurrentUserAsync() => ApiClient.GetCurrentUserAsync();

        public Task<DiscordUser> GetUserAsync(ulong user) => ApiClient.GetUserAsync(user);

        public Task<DiscordMember> GetGuildMemberAsync(ulong guildId, ulong memberId) => ApiClient.GetGuildMemberAsync(guildId, memberId);

        public Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string reason) => ApiClient.RemoveGuildMemberAsync(guildId, userId, reason);

        public async Task<DiscordUser> ModifyCurrentUserAsync(string username, string base64Avatar) => 
            new DiscordUser(await ApiClient.ModifyCurrentUserAsync(username, base64Avatar)) { Discord = this };

        public async Task<DiscordUser> EditCurrentUserAsync(string username = null, Stream avatar = null)
        {
            string av64 = null;
            if (avatar != null)
            {
                using (var imgtool = new ImageTool(avatar))
                {
                    av64 = imgtool.GetBase64();
                }
            }

            return new DiscordUser(await ApiClient.ModifyCurrentUserAsync(username, av64)) { Discord = this };
        }

        public Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit, ulong? before, ulong? after)
            => ApiClient.GetCurrentUserGuildsAsync(limit, before, after);

        public Task ModifyGuildMemberAsync(ulong guildId, ulong userId, string nick, IEnumerable<ulong> roleIds, bool? mute, bool? deaf, ulong? voiceChannelId, string reason)
            => ApiClient.ModifyGuildMemberAsync(guildId, userId, nick, roleIds, mute, deaf, voiceChannelId, reason);

        public Task ModifyCurrentMemberNicknameAsync(ulong guildId, string nick, string reason)
            => ApiClient.ModifyCurrentMemberNicknameAsync(guildId, nick, reason);
        #endregion

        #region Roles
        public Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guildId)
            => ApiClient.GetGuildRolesAsync(guildId);

        public Task<DiscordGuild> GetGuildAsync(ulong guildId) => ApiClient.GetGuildAsync(guildId);

        public Task<DiscordRole> ModifyGuildRoleAsync(ulong guildId, ulong roleId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
            => ApiClient.ModifyGuildRoleAsync(guildId, roleId, name, permissions, color, hoist, mentionable, reason);

        public Task DeleteRoleAsync(ulong guildId, ulong roleId, string reason)
            => ApiClient.DeleteRoleAsync(guildId, roleId, reason);

        public Task<DiscordRole> CreateGuildRole(ulong guildId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
            => ApiClient.CreateGuildRole(guildId, name, permissions, color, hoist, mentionable, reason);


        #endregion

        #region Prune
        public Task<int> GetGuildPruneCountAsync(ulong guildId, int days) => ApiClient.GetGuildPruneCountAsync(guildId, days);

        public Task<int> BeginGuildPruneAsync(ulong guildId, int days, string reason) => ApiClient.BeginGuildPruneAsync(guildId, days, reason);
        #endregion

        #region GuildVarious
        public Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guildId) => ApiClient.GetGuildIntegrationsAsync(guildId);

        public Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guildId, string type, ulong id) => ApiClient.CreateGuildIntegrationAsync(guildId, type, id);

        public Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guildId, ulong integrationId, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
            => ApiClient.ModifyGuildIntegrationAsync(guildId, integrationId, expireBehaviour, expireGracePeriod, enableEmoticons);

        public Task DeleteGuildIntegrationAsync(ulong guildId, DiscordIntegration integration) => ApiClient.DeleteGuildIntegrationAsync(guildId, integration);

        public Task SyncGuildIntegrationAsync(ulong guildId, ulong integrationId) => ApiClient.SyncGuildIntegrationAsync(guildId, integrationId);

        public Task<DiscordGuildEmbed> GetGuildEmbedAsync(ulong guildId) => ApiClient.GetGuildEmbedAsync(guildId);

        public Task<DiscordGuildEmbed> ModifyGuildEmbedAsync(ulong guildId, DiscordGuildEmbed embed) => ApiClient.ModifyGuildEmbedAsync(guildId, embed);

        public Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId) => ApiClient.GetGuildVoiceRegionsAsync(guildId);

        public Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guildId) => ApiClient.GetGuildInvitesAsync(guildId);
        #endregion

        #region Invites
        public Task<DiscordInvite> GetInvite(string inviteCode) => ApiClient.GetInviteAsync(inviteCode);

        public Task<DiscordInvite> DeleteInvite(string inviteCode, string reason) => ApiClient.DeleteInviteAsync(inviteCode, reason);
        #endregion

        #region Connections
        public Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync() => ApiClient.GetUsersConnectionsAsync();
        #endregion

        #region Webhooks
        public Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, string base64Avatar, string reason)
            => ApiClient.CreateWebhookAsync(channelId, name, base64Avatar, reason);

        public async Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, Stream avatar = null, string reason = null)
        {
            string av64 = null;
            if (avatar != null)
            {
                using (var imgtool = new ImageTool(avatar))
                {
                    av64 = imgtool.GetBase64();
                }
            }

            return await ApiClient.CreateWebhookAsync(channelId, name, av64, reason);
        }

        public Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channelId) => ApiClient.GetChannelWebhooksAsync(channelId);

        public Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guildId) => ApiClient.GetGuildWebhooksAsync(guildId);

        public Task<DiscordWebhook> GetWebhookAsync(ulong webhookId) => ApiClient.GetWebhookAsync(webhookId);

        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhookId, string webhookToken) => ApiClient.GetWebhookWithTokenAsync(webhookId, webhookToken);

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string reason) => ApiClient.ModifyWebhookAsync(webhookId, name, base64Avatar, reason);

        public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, Stream avatar, string reason)
        {
            string av64 = null;
            if (avatar != null)
            {
                using (var imgtool = new ImageTool(avatar))
                {
                    av64 = imgtool.GetBase64();
                }
            }

            return await ApiClient.ModifyWebhookAsync(webhookId, name, av64, reason);
        }

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string webhookToken, string reason)
            => ApiClient.ModifyWebhookAsync(webhookId, name, base64Avatar, webhookToken, reason);

        public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, Stream avatar, string webhookToken, string reason)
        {
            string av64 = null;
            if (avatar != null)
            {
                using (var imgtool = new ImageTool(avatar))
                {
                    av64 = imgtool.GetBase64();
                }
            }

            return await ApiClient.ModifyWebhookAsync(webhookId, name, av64, webhookToken, reason);
        }

        public Task DeleteWebhookAsync(ulong webhookId, string reason) => ApiClient.DeleteWebhookAsync(webhookId, reason);

        public Task DeleteWebhookAsync(ulong webhookId, string reason, string webhookToken) => ApiClient.DeleteWebhookAsync(webhookId, webhookToken, reason);

        public Task ExecuteWebhookAsync(ulong webhookId, string webhookToken, string content, string username, string avatarUrl, bool? tts, IEnumerable<DiscordEmbed> embeds)
            => ApiClient.ExecuteWebhookAsync(webhookId, webhookToken, content, username, avatarUrl, tts, embeds);
        #endregion

        #region Reactions
        public Task CreateReactionAsync(ulong channelId, ulong messageId, string emoji)
            => ApiClient.CreateReactionAsync(channelId, messageId, emoji);

        public Task DeleteOwnReactionAsync(ulong channelId, ulong messageId, string emoji)
            => ApiClient.DeleteOwnReactionAsync(channelId, messageId, emoji);

        public Task DeleteUserReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, string reason)
            => ApiClient.DeleteUserReactionAsync(channelId, messageId, userId, emoji, reason);

        public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, string emoji)
            => ApiClient.GetReactionsAsync(channelId, messageId, emoji);

        public Task DeleteAllReactionsAsync(ulong channelId, ulong messageId, string reason)
            => ApiClient.DeleteAllReactionsAsync(channelId, messageId, reason);
        #endregion

        #region Misc
        public Task<DiscordApplication> GetApplicationInfoAsync(ulong id)
            => ApiClient.GetApplicationInfoAsync(id);

        public Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication app)
            => ApiClient.GetApplicationAssetsAsync(app);

        public Task AcknowledgeMessageAsync(ulong msgId, ulong chnId)
            => ApiClient.AcknowledgeMessageAsync(msgId, chnId);

        public Task AcknowledgeGuildAsync(ulong id)
            => ApiClient.AcknowledgeGuildAsync(id);
        #endregion

        private bool _disposed;
        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _guilds = null;
        }
    }
}
