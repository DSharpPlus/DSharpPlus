using System.Collections.Generic;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace DSharpPlus
{
    public class DiscordRestClient : BaseDiscordClient
    {
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds => new ReadOnlyDictionary<ulong, DiscordGuild>(_guilds);
        internal Dictionary<ulong, DiscordGuild> _guilds;

        public DiscordRestClient(DiscordConfiguration config) : base(config)
        {
            disposed = false;
            var gs = ApiClient.GetCurrentUserGuildsAsync(100, null, null).GetAwaiter().GetResult();
            _guilds = new Dictionary<ulong, DiscordGuild>();
            foreach (DiscordGuild g in gs)
            {
                _guilds[g.Id] = g;
            }
        }

        #region Guild
        public async Task<DiscordGuild> CreateGuildAsync(string name, string region_id, string iconb64, VerificationLevel? verification_level,
            DefaultMessageNotifications? default_message_notifications) => await ApiClient.CreateGuildAsync(name, region_id, iconb64, verification_level, default_message_notifications);

        public async Task DeleteGuildAsync(ulong id) => await ApiClient.DeleteGuildAsync(id);

        public async Task<DiscordGuild> ModifyGuildAsync(ulong guild_id, string name, string region, VerificationLevel? verification_level,
            DefaultMessageNotifications? default_message_notifications, ulong? afk_channel_id, int? afk_timeout, string iconb64, ulong? owner_id, string splashb64, string reason)
            => await ApiClient.ModifyGuildAsync(guild_id, name, region, verification_level, default_message_notifications, afk_channel_id, afk_timeout, iconb64, owner_id, splashb64, reason);

        public async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guild_id) => await ApiClient.GetGuildBansAsync(guild_id);

        public Task CreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason) => ApiClient.CreateGuildBanAsync(guild_id, user_id, delete_message_days, reason);

        public Task RemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason) => ApiClient.RemoveGuildBanAsync(guild_id, user_id, reason);

        public Task LeaveGuildAsync(ulong guild_id) => ApiClient.LeaveGuildAsync(guild_id);

        public async Task<DiscordMember> AddGuildMemberAsync(ulong guild_id, ulong user_id, string access_token, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
            => await ApiClient.AddGuildMemberAsync(guild_id, user_id, this.Configuration.Token, nick, roles, muted, deafened);

        public async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guild_id, int? limit, ulong? after) => await ApiClient.ListGuildMembersAsync(guild_id, limit, after);

        public Task AddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason) => ApiClient.AddGuildMemberRoleAsync(guild_id, user_id, role_id, reason);

        public Task RemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason) => ApiClient.RemoveGuildMemberRoleAsync(guild_id, user_id, role_id, reason);

        public Task UpdateRolePositionAsync(ulong guild_id, ulong role_id, int position, string reason = null)
        {
            List<RestGuildRoleReorderPayload> rgrrps = new List<RestGuildRoleReorderPayload>()
            {
                new RestGuildRoleReorderPayload { RoleId = role_id }
            };
            return this.ApiClient.ModifyGuildRolePosition(guild_id, rgrrps, reason);
        }

        public Task UpdateChannelPositionAsync(ulong guild_id, ulong channel_id, int position, string reason)
        {
            List<RestGuildChannelReorderPayload> rgcrps = new List<RestGuildChannelReorderPayload>()
            {
                new RestGuildChannelReorderPayload { ChannelId = channel_id, Position = position }
            };
            return this.ApiClient.ModifyGuildChannelPosition(guild_id, rgcrps, reason);
        }


        #endregion

        #region Channel
        public Task<DiscordChannel> CreateGuildChannelAsync(ulong id, string name, ChannelType type, ulong? parent, int? bitrate, int? user_limit, IEnumerable<DiscordOverwrite> overwrites, string reason)
        {
            if (type != ChannelType.Category && type != ChannelType.Text && type != ChannelType.Voice)
                throw new ArgumentException("Channel type must be text, voice, or category.", nameof(type));

            return ApiClient.CreateGuildChannelAsync(id, name, type, parent, bitrate, user_limit, overwrites, reason);
        }

        public Task ModifyChannelAsync(ulong id, string name, int? position, string topic, Optional<ulong?> parent, int? bitrate, int? user_limit, string reason)
            => ApiClient.ModifyChannelAsync(id, name, position, topic, parent, bitrate, user_limit, reason);

        public Task<DiscordChannel> GetChannelAsync(ulong id) => ApiClient.GetChannelAsync(id);

        public Task DeleteChannelAsync(ulong id, string reason) => ApiClient.DeleteChannelAsync(id, reason);

        public Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id) => ApiClient.GetMessageAsync(channel_id, message_id);

        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content, bool? tts, DiscordEmbed embed) =>
            ApiClient.CreateMessageAsync(channel_id, content, tts, embed);

        public Task<DiscordMessage> UploadFileAsync(ulong channel_id, Stream file_data, string file_name, string content, bool? tts, DiscordEmbed embed)
            => ApiClient.UploadFileAsync(channel_id, file_data, file_name, content, tts, embed);

        public Task<DiscordMessage> UploadFilesAsync(ulong channel_id, Dictionary<string, Stream> files, string content, bool? tts, DiscordEmbed embed)
            => ApiClient.UploadFilesAsync(channel_id, files, content, tts, embed);

        public Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guild_id) => ApiClient.GetGuildChannelsAsync(guild_id);

        public Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
            => ApiClient.GetChannelMessagesAsync(channel_id, limit, before, after, around);

        public Task<DiscordMessage> GetChannelMessageAsync(ulong channel_id, ulong message_id) =>
            ApiClient.GetChannelMessageAsync(channel_id, message_id);

        public Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content, Optional<DiscordEmbed> embed)
            => ApiClient.EditMessageAsync(channel_id, message_id, content, embed);

        public Task DeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
            => ApiClient.DeleteMessageAsync(channel_id, message_id, reason);

        public Task DeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
            => ApiClient.DeleteMessagesAsync(channel_id, message_ids, reason);

        public Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
            => ApiClient.GetChannelInvitesAsync(channel_id);

        public Task<DiscordInvite> CreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, bool temporary, bool unique, string reason)
            => ApiClient.CreateChannelInviteAsync(channel_id, max_age, max_uses, temporary, unique, reason);

        public Task DeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
            => ApiClient.DeleteChannelPermissionAsync(channel_id, overwrite_id, reason);

        public Task EditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
            => ApiClient.EditChannelPermissionsAsync(channel_id, overwrite_id, allow, deny, type, reason);

        public Task TriggerTypingAsync(ulong channel_id) => ApiClient.TriggerTypingAsync(channel_id);

        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channel_id)
            => ApiClient.GetPinnedMessagesAsync(channel_id);

        public Task UnpinMessageAsync(ulong channel_id, ulong message_id)
            => ApiClient.UnpinMessageAsync(channel_id, message_id);

        public Task JoinGroupDmAsync(ulong channel_id, string nickname)
            => ApiClient.GroupDmAddRecipientAsync(channel_id, CurrentUser.Id, Configuration.Token, nickname);

        public Task GroupDmAddRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
            => ApiClient.GroupDmAddRecipientAsync(channel_id, user_id, access_token, nickname);

        public Task LeaveGroupDmAsync(ulong channel_id)
            => ApiClient.GroupDmRemoveRecipientAsync(channel_id, CurrentUser.Id);

        public Task GroupDmRemoveRecipientAsync(ulong channel_id, ulong user_id)
            => ApiClient.GroupDmRemoveRecipientAsync(channel_id, user_id);

        public Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
            => ApiClient.CreateGroupDmAsync(access_tokens, nicks);

        public Task<DiscordDmChannel> CreateGroupDmWithCurrentUserAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var a = access_tokens.ToList();
            a.Add(this.Configuration.Token);
            return ApiClient.CreateGroupDmAsync(a, nicks);
        }

        public Task<DiscordDmChannel> CreateDmAsync(ulong recipient_id)
            => ApiClient.CreateDmAsync(recipient_id);
        #endregion

        #region Member
        public Task<DiscordUser> GetCurrentUserAsync() => ApiClient.GetCurrentUserAsync();

        public Task<DiscordUser> GetUserAsync(ulong user) => ApiClient.GetUserAsync(user);

        public Task<DiscordMember> GetGuildMemberAsync(ulong guild_id, ulong member_id) => ApiClient.GetGuildMemberAsync(guild_id, member_id);

        public Task RemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason) => ApiClient.RemoveGuildMemberAsync(guild_id, user_id, reason);

        public Task<DiscordUser> ModifyCurrentUserAsync(string username, string base64_avatar) => ApiClient.ModifyCurrentUserAsync(username, base64_avatar);

        public async Task<DiscordUser> EditCurrentUserAsync(string username = null, Stream avatar = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return await ApiClient.ModifyCurrentUserAsync(username, av64);
        }

        public Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit, ulong? before, ulong? after)
            => ApiClient.GetCurrentUserGuildsAsync(limit, before, after);

        public Task ModifyGuildMemberAsync(ulong guild_id, ulong user_id, string nick, IEnumerable<ulong> role_ids, bool? mute, bool? deaf, ulong? voice_channel_id, string reason)
            => ApiClient.ModifyGuildMemberAsync(guild_id, user_id, nick, role_ids, mute, deaf, voice_channel_id, reason);

        public Task ModifyCurrentMemberNicknameAsync(ulong guild_id, string nick, string reason)
            => ApiClient.ModifyCurrentMemberNicknameAsync(guild_id, nick, reason);
        #endregion

        #region Roles
        public Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guild_id)
            => ApiClient.GetGuildRolesAsync(guild_id);

        public Task<DiscordGuild> GetGuildAsync(ulong guild_id) => ApiClient.GetGuildAsync(guild_id);

        public Task<DiscordRole> ModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
            => ModifyGuildRoleAsync(guild_id, role_id, name, permissions, color, hoist, mentionable, reason);

        public Task DeleteRoleAsync(ulong guild_id, ulong role_id, string reason)
            => ApiClient.DeleteRoleAsync(guild_id, role_id, reason);

        public Task<DiscordRole> CreateGuildRole(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
            => ApiClient.CreateGuildRole(guild_id, name, permissions, color, hoist, mentionable, reason);


        #endregion

        #region Prune
        public Task<int> GetGuildPruneCountAsync(ulong guild_id, int days) => ApiClient.GetGuildPruneCountAsync(guild_id, days);

        public Task<int> BeginGuildPruneAsync(ulong guild_id, int days, string reason) => ApiClient.BeginGuildPruneAsync(guild_id, days, reason);
        #endregion

        #region GuildVarious
        public Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guild_id) => ApiClient.GetGuildIntegrationsAsync(guild_id);

        public Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guild_id, string type, ulong id) => ApiClient.CreateGuildIntegrationAsync(guild_id, type, id);

        public Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guild_id, ulong integration_id, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
            => ModifyGuildIntegrationAsync(guild_id, integration_id, expire_behaviour, expire_grace_period, enable_emoticons);

        public Task DeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration) => ApiClient.DeleteGuildIntegrationAsync(guild_id, integration);

        public Task SyncGuildIntegrationAsync(ulong guild_id, ulong integration_id) => ApiClient.SyncGuildIntegrationAsync(guild_id, integration_id);

        public Task<DiscordGuildEmbed> GetGuildEmbedAsync(ulong guild_id) => GetGuildEmbedAsync(guild_id);

        public Task<DiscordGuildEmbed> ModifyGuildEmbedAsync(ulong guild_id, DiscordGuildEmbed embed) => ApiClient.ModifyGuildEmbedAsync(guild_id, embed);

        public Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guild_id) => ApiClient.GetGuildVoiceRegionsAsync(guild_id);

        public Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id) => ApiClient.GetGuildInvitesAsync(guild_id);
        #endregion

        #region Invites
        public Task<DiscordInvite> GetInvite(string invite_code) => ApiClient.GetInvite(invite_code);

        public Task<DiscordInvite> DeleteInvite(string invite_code, string reason) => ApiClient.DeleteInvite(invite_code, reason);
        #endregion

        #region Connections
        public Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync() => ApiClient.GetUsersConnectionsAsync();
        #endregion

        #region Webhooks
        public Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, string base64_avatar, string reason)
            => ApiClient.CreateWebhookAsync(channel_id, name, base64_avatar, reason);

        public async Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, Stream avatar = null, string reason = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return await this.ApiClient.CreateWebhookAsync(channel_id, name, av64, reason);
        }

        public Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channel_id) => ApiClient.GetChannelWebhooksAsync(channel_id);

        public Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guild_id) => ApiClient.GetGuildWebhooksAsync(guild_id);

        public Task<DiscordWebhook> GetWebhookAsync(ulong webhook_id) => ApiClient.GetWebhookAsync(webhook_id);

        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhook_id, string webhook_token) => ApiClient.GetWebhookWithTokenAsync(webhook_id, webhook_token);

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string reason) => ApiClient.ModifyWebhookAsync(webhook_id, name, base64_avatar, reason);

        public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, Stream avatar, string reason)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return await this.ApiClient.ModifyWebhookAsync(webhook_id, name, av64, reason);
        }

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string webhook_token, string reason)
            => ApiClient.ModifyWebhookAsync(webhook_id, name, base64_avatar, webhook_token, reason);

        public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, Stream avatar, string webhook_token, string reason)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return await this.ApiClient.ModifyWebhookAsync(webhook_id, name, av64, webhook_token, reason);
        }

        public Task DeleteWebhookAsync(ulong webhook_id, string reason) => ApiClient.DeleteWebhookAsync(webhook_id, reason);

        public Task DeleteWebhookAsync(ulong webhook_id, string reason, string webhook_token) => ApiClient.DeleteWebhookAsync(webhook_id, webhook_token, reason);

        public Task ExecuteWebhookAsync(ulong webhook_id, string webhook_token, string content, string username, string avatar_url, bool? tts, IEnumerable<DiscordEmbed> embeds)
            => ApiClient.ExecuteWebhookAsync(webhook_id, webhook_token, content, username, avatar_url, tts, embeds);
        #endregion

        #region Reactions
        public Task CreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
            => ApiClient.CreateReactionAsync(channel_id, message_id, emoji);

        public Task DeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
            => ApiClient.DeleteOwnReactionAsync(channel_id, message_id, emoji);

        public Task DeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
            => ApiClient.DeleteUserReactionAsync(channel_id, message_id, user_id, emoji, reason);

        public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, string emoji)
            => ApiClient.GetReactionsAsync(channel_id, message_id, emoji);

        public Task DeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
            => ApiClient.DeleteAllReactionsAsync(channel_id, message_id, reason);
        #endregion

        #region Misc
        public Task<DiscordApplication> GetApplicationInfoAsync(ulong id)
            => ApiClient.GetApplicationInfoAsync(id);

        public Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication app)
            => ApiClient.GetApplicationAssetsAsync(app);

        public Task AcknowledgeMessageAsync(ulong msg_id, ulong chn_id)
            => ApiClient.AcknowledgeMessageAsync(msg_id, chn_id);

        public Task AcknowledgeGuildAsync(ulong id)
            => ApiClient.AcknowledgeGuildAsync(id);
        #endregion

        private bool disposed;
        public override void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            _guilds = null;
        }
    }
}
