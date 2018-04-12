using System.Collections.Generic;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using DSharpPlus.Rest;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace DSharpPlus
{
    public class DiscordRestClient : BaseDiscordClient
    {
        /// <summary>
        /// Gets the dictionary of guilds cached by this client.
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds
            => _guilds_lazy.Value;

        internal Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordGuild>> _guilds_lazy;

        public string Token
        {
            get
            {
                return Configuration.Token;
            }
        }

        public string RefreshToken { get; set; }
        public Scope[] scopes { get; set; }
        public bool UseRefresh
        {
            get
            { return string.IsNullOrWhiteSpace(RefreshToken); }
        }
        public DateTime TokenExpireDate;

        private string clientId;
        private string clientSecret;
        private string redirectUri;

        /// <summary>
        /// Creates a instance
        /// </summary>
        /// <param name="response">See static Functions</param>
        /// <param name="config">The Config to use</param>
        public DiscordRestClient(BaseTokenResponse response, DiscordConfiguration config)
        : base(new DiscordConfiguration(config)
        {
            Token = response.AccessToken,
            TokenType = response.TokenType
        })
        {
            //TODO: What about guilds.join needing a bot account?
            //TODO: rpc support?
            this.scopes = response.Scopes;
            this.TokenExpireDate = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
            if (Configuration.TokenType != TokenType.Bearer)
                throw new NotImplementedException("OAuth2 Only supports the Bearer token currently");
        }

        public DiscordRestClient(DiscordConfiguration config, Scope[] scopes) : base(config)
        {
            this.scopes = scopes;
            if (Configuration.TokenType != TokenType.Bearer)
                throw new NotImplementedException("OAuth2 Only supports the Bearer token currently");
        }

        public override async Task InitializeAsync()
        {
            if (this.CurrentUser == null && scopes.Contains(Scope.identify))
            {
                this.CurrentUser = await this.ApiClient.GetCurrentUserAsync().ConfigureAwait(false);
                this.UserCache.AddOrUpdate(this.CurrentUser.Id, this.CurrentUser, (id, xu) => this.CurrentUser);
            }
            if ((this.Configuration.TokenType != TokenType.User && this.CurrentApplication == null) && scopes.Contains(Scope.bot))
                this.CurrentApplication = await this.GetCurrentApplicationAsync().ConfigureAwait(false);

            if (this.InternalVoiceRegions.Count == 0 && scopes.Contains(Scope.bot))
            {
                var vrs = await this.ListVoiceRegionsAsync().ConfigureAwait(false);
                foreach (var xvr in vrs)
                    this.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
            }
        }

        public async Task InitializeCacheAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
            _guilds_lazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(_guilds));
            if (scopes.Contains(Scope.bot))
            {
                var gs = this.GetCurrentUserGuildsAsync(100, null, null).ConfigureAwait(false).GetAwaiter().GetResult();
                foreach (DiscordGuild g in gs)
                {
                    _guilds[g.Id] = g;
                }
            }
        }


        #region Guild
        public Task<DiscordGuild> CreateGuildAsync(string name, string region_id, string iconb64, VerificationLevel? verification_level, DefaultMessageNotifications? default_message_notifications)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateGuildAsync(name, region_id, iconb64, verification_level, default_message_notifications);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteGuildAsync(ulong id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteGuildAsync(id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordGuild> ModifyGuildAsync(ulong guild_id, Optional<string> name,
                    Optional<string> region, Optional<VerificationLevel> verification_level,
                    Optional<DefaultMessageNotifications> default_message_notifications, Optional<MfaLevel> mfa_level,
                    Optional<ExplicitContentFilter> explicit_content_filter, Optional<ulong?> afk_channel_id,
                    Optional<int> afk_timeout, Optional<string> iconb64, Optional<ulong> owner_id, Optional<string> splashb64,
                    Optional<ulong?> systemChannelId, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyGuildAsync(guild_id, name, region, verification_level, default_message_notifications, mfa_level, explicit_content_filter, afk_channel_id, afk_timeout, iconb64,
                    owner_id, splashb64, systemChannelId, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildBansAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task CreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateGuildBanAsync(guild_id, user_id, delete_message_days, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task RemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.RemoveGuildBanAsync(guild_id, user_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task LeaveGuildAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.LeaveGuildAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMember> AddGuildMemberAsync(ulong guild_id, ulong user_id, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
        {
            if (scopes.Contains(Scope.guilds_join))
                return ApiClient.AddGuildMemberAsync(guild_id, user_id, this.Configuration.Token, nick, roles, muted, deafened);
            else
                throw new NotSupportedException("this is only Supported with the guilds.join scope!");
        }

        public async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
        {
            if (scopes.Contains(Scope.bot))
            {
                var recmbr = new List<DiscordMember>();

                var recd = 1000;
                var last = 0ul;
                while (recd == 1000)
                {
                    var tms = await this.ApiClient.ListGuildMembersAsync(guild_id, 1000, last == 0 ? null : (ulong?)last).ConfigureAwait(false);
                    recd = tms.Count;

                    foreach (var xtm in tms)
                    {
                        if (this.UserCache.ContainsKey(xtm.User.Id))
                            continue;

                        var usr = new DiscordUser(xtm.User) { Discord = this };
                        this.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
                        {
                            old.Username = usr.Username;
                            old.Discord = usr.Discord;
                            old.AvatarHash = usr.AvatarHash;

                            return old;
                        });
                    }

                    var tm = tms.LastOrDefault();
                    last = tm?.User.Id ?? 0;

                    recmbr.AddRange(tms.Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild_id }));
                }

                return new ReadOnlyCollection<DiscordMember>(recmbr);
            }
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task AddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.AddGuildMemberRoleAsync(guild_id, user_id, role_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task RemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.RemoveGuildMemberRoleAsync(guild_id, user_id, role_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task UpdateRolePositionAsync(ulong guild_id, ulong role_id, int position, string reason = null)
        {
            if (scopes.Contains(Scope.bot))
            {
                var rgrrps = new List<RestGuildRoleReorderPayload>()
            {
                new RestGuildRoleReorderPayload { RoleId = role_id }
            };
                return this.ApiClient.ModifyGuildRolePosition(guild_id, rgrrps, reason);
            }
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task UpdateChannelPositionAsync(ulong guild_id, ulong channel_id, int position, string reason)
        {
            var rgcrps = new List<RestGuildChannelReorderPayload>()
            {
                new RestGuildChannelReorderPayload { ChannelId = channel_id, Position = position }
            };
            return this.ApiClient.ModifyGuildChannelPosition(guild_id, rgcrps, reason);
        }
        #endregion

        #region Channel
        public Task<DiscordChannel> CreateGuildChannelAsync(ulong id, string name, ChannelType type, ulong? parent, int? bitrate, int? user_limit, IEnumerable<DiscordOverwriteBuilder> overwrites, bool? nsfw, string reason)
        {
            if (type != ChannelType.Category && type != ChannelType.Text && type != ChannelType.Voice)
                throw new ArgumentException("Channel type must be text, voice, or category.", nameof(type));

            return ApiClient.CreateGuildChannelAsync(id, name, type, parent, bitrate, user_limit, overwrites, nsfw, reason);
        }

        public Task ModifyChannelAsync(ulong id, string name, int? position, string topic, Optional<ulong?> parent, int? bitrate, int? user_limit, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyChannelAsync(id, name, position, topic, parent, bitrate, user_limit, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordChannel> GetChannelAsync(ulong id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetChannelAsync(id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteChannelAsync(ulong id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteChannelAsync(id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetMessageAsync(channel_id, message_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content, bool? tts, DiscordEmbed embed)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateMessageAsync(channel_id, content, tts, embed);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMessage> UploadFileAsync(ulong channel_id, Stream file_data, string file_name, string content, bool? tts, DiscordEmbed embed)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.UploadFileAsync(channel_id, file_data, file_name, content, tts, embed);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMessage> UploadFilesAsync(ulong channel_id, Dictionary<string, Stream> files, string content, bool? tts, DiscordEmbed embed)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.UploadFilesAsync(channel_id, files, content, tts, embed);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildChannelsAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetChannelMessagesAsync(channel_id, limit, before, after, around);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMessage> GetChannelMessageAsync(ulong channel_id, ulong message_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetChannelMessageAsync(channel_id, message_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content, Optional<DiscordEmbed> embed)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.EditMessageAsync(channel_id, message_id, content, embed);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteMessageAsync(channel_id, message_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteMessagesAsync(channel_id, message_ids, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetChannelInvitesAsync(channel_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordInvite> CreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, bool temporary, bool unique, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateChannelInviteAsync(channel_id, max_age, max_uses, temporary, unique, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteChannelPermissionAsync(channel_id, overwrite_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task EditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.EditChannelPermissionsAsync(channel_id, overwrite_id, allow, deny, type, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task TriggerTypingAsync(ulong channel_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.TriggerTypingAsync(channel_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channel_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetPinnedMessagesAsync(channel_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task UnpinMessageAsync(ulong channel_id, ulong message_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.UnpinMessageAsync(channel_id, message_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task JoinGroupDmAsync(ulong channel_id, string nickname)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GroupDmAddRecipientAsync(channel_id, CurrentUser.Id, Configuration.Token, nickname);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task GroupDmAddRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GroupDmAddRecipientAsync(channel_id, user_id, access_token, nickname);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task LeaveGroupDmAsync(ulong channel_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GroupDmRemoveRecipientAsync(channel_id, CurrentUser.Id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task GroupDmRemoveRecipientAsync(ulong channel_id, ulong user_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GroupDmRemoveRecipientAsync(channel_id, user_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateGroupDmAsync(access_tokens, nicks);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordDmChannel> CreateGroupDmWithCurrentUserAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var a = access_tokens.ToList();
            a.Add(this.Configuration.Token);
            return ApiClient.CreateGroupDmAsync(a, nicks);
        }

        public Task<DiscordDmChannel> CreateDmAsync(ulong recipient_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateDmAsync(recipient_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Member
        public Task<DiscordUser> GetCurrentUserAsync()
        {
            if (scopes.Contains(Scope.identify))
                return ApiClient.GetCurrentUserAsync();
            else
                throw new NotSupportedException("this is only Supported with the identify scope!");
        }

        public Task<DiscordUser> GetUserAsync(ulong user)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetUserAsync(user);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordMember> GetGuildMemberAsync(ulong guild_id, ulong member_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildMemberAsync(guild_id, member_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task RemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.RemoveGuildMemberAsync(guild_id, user_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public async Task<DiscordUser> ModifyCurrentUserAsync(string username, string base64_avatar)
        {
            if (scopes.Contains(Scope.bot))
                return new DiscordUser(await ApiClient.ModifyCurrentUserAsync(username, base64_avatar).ConfigureAwait(false)) { Discord = this };

            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public async Task<DiscordUser> EditCurrentUserAsync(string username = null, Stream avatar = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return new DiscordUser(await ApiClient.ModifyCurrentUserAsync(username, av64).ConfigureAwait(false)) { Discord = this };
        }

        public Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit, ulong? before, ulong? after)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetCurrentUserGuildsAsync(limit, before, after);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task ModifyGuildMemberAsync(ulong guild_id, ulong user_id, Optional<string> nick,
            Optional<IEnumerable<ulong>> role_ids, Optional<bool> mute, Optional<bool> deaf,
            Optional<ulong> voice_channel_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyGuildMemberAsync(guild_id, user_id, nick, role_ids, mute, deaf, voice_channel_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task ModifyCurrentMemberNicknameAsync(ulong guild_id, string nick, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyCurrentMemberNicknameAsync(guild_id, nick, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Roles
        public Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildRolesAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordGuild> GetGuildAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordRole> ModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyGuildRoleAsync(guild_id, role_id, name, permissions, color, hoist, mentionable, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteRoleAsync(ulong guild_id, ulong role_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteRoleAsync(guild_id, role_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordRole> CreateGuildRole(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateGuildRole(guild_id, name, permissions, color, hoist, mentionable, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Prune
        public Task<int> GetGuildPruneCountAsync(ulong guild_id, int days)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildPruneCountAsync(guild_id, days);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<int> BeginGuildPruneAsync(ulong guild_id, int days, string reason)
            => ApiClient.BeginGuildPruneAsync(guild_id, days, reason);
        #endregion

        #region GuildVarious
        public Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildIntegrationsAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guild_id, string type, ulong id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateGuildIntegrationAsync(guild_id, type, id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guild_id, ulong integration_id, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyGuildIntegrationAsync(guild_id, integration_id, expire_behaviour, expire_grace_period, enable_emoticons);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteGuildIntegrationAsync(guild_id, integration);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task SyncGuildIntegrationAsync(ulong guild_id, ulong integration_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.SyncGuildIntegrationAsync(guild_id, integration_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordGuildEmbed> GetGuildEmbedAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildEmbedAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordGuildEmbed> ModifyGuildEmbedAsync(ulong guild_id, DiscordGuildEmbed embed)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyGuildEmbedAsync(guild_id, embed);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildVoiceRegionsAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildInvitesAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Invites
        public Task<DiscordInvite> GetInvite(string invite_code)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetInviteAsync(invite_code);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordInvite> DeleteInvite(string invite_code, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteInviteAsync(invite_code, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Connections
        public Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
        {
            if (scopes.Contains(Scope.connections))
                return ApiClient.GetUsersConnectionsAsync();
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Webhooks
        public Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, string base64_avatar, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateWebhookAsync(channel_id, name, base64_avatar, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, Stream avatar = null, string reason = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return this.ApiClient.CreateWebhookAsync(channel_id, name, av64, reason);
        }

        public Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channel_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetChannelWebhooksAsync(channel_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guild_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetGuildWebhooksAsync(guild_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordWebhook> GetWebhookAsync(ulong webhook_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetWebhookAsync(webhook_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhook_id, string webhook_token)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetWebhookWithTokenAsync(webhook_id, webhook_token);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyWebhookAsync(webhook_id, name, base64_avatar, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, Stream avatar, string reason)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return this.ApiClient.ModifyWebhookAsync(webhook_id, name, av64, reason);
        }

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string webhook_token, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ModifyWebhookAsync(webhook_id, name, base64_avatar, webhook_token, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, Stream avatar, string webhook_token, string reason)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return this.ApiClient.ModifyWebhookAsync(webhook_id, name, av64, webhook_token, reason);
        }

        public Task DeleteWebhookAsync(ulong webhook_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteWebhookAsync(webhook_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteWebhookAsync(ulong webhook_id, string reason, string webhook_token)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteWebhookAsync(webhook_id, webhook_token, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task ExecuteWebhookAsync(ulong webhook_id, string webhook_token, string content, string username, string avatar_url, bool? tts, IEnumerable<DiscordEmbed> embeds)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.ExecuteWebhookAsync(webhook_id, webhook_token, content, username, avatar_url, tts, embeds);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Reactions
        public Task CreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.CreateReactionAsync(channel_id, message_id, emoji);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteOwnReactionAsync(channel_id, message_id, emoji);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteUserReactionAsync(channel_id, message_id, user_id, emoji, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, string emoji)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetReactionsAsync(channel_id, message_id, emoji);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task DeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.DeleteAllReactionsAsync(channel_id, message_id, reason);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        #region Misc
        public Task<DiscordApplication> GetApplicationInfoAsync(ulong id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetApplicationInfoAsync(id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication app)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.GetApplicationAssetsAsync(app);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task AcknowledgeMessageAsync(ulong msg_id, ulong chn_id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.AcknowledgeMessageAsync(msg_id, chn_id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }

        public Task AcknowledgeGuildAsync(ulong id)
        {
            if (scopes.Contains(Scope.bot))
                return ApiClient.AcknowledgeGuildAsync(id);
            else
                throw new NotSupportedException("this is only Supported with the bot scope!");
        }
        #endregion

        private bool disposed;
        public override void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            _guilds = null;
        }

        private bool CheckTokenRefresh()
        {
            DateTime discordEpoch = new DateTime(2015, 0, 0, 0, 0, 0);
            return ((DateTime.UtcNow - discordEpoch).TotalSeconds >= (TokenExpireDate - discordEpoch).TotalSeconds);
        }

        private async Task RefreshTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", "refresh_token" },
                    { "refresh_token", RefreshToken },
                    { "redirect_uri", redirectUri }
                };
                var res = await client.PostAsync($"{Utilities.GetApiBaseUri()}/oauth2/token", new FormUrlEncodedContent(content));
                if (res.IsSuccessStatusCode)
                {
                    var resContent = await res.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<RefreshTokenResponse>(resContent);
                    this.EnableRefresh(
                        response.RefreshToken,
                    redirectUri,
                    clientId,
                    clientSecret);
                }
                else
                    throw new Exception("Coudnt get token! Check your Parameters and try again!");
            }
        }

        public void EnableRefresh(string refreshToken, string redirectUri, string clientId, string clientSecret)
        {
            this.RefreshToken = refreshToken;
            this.redirectUri = redirectUri;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        #region TokenGrants
        //Not Included in NetStandart1.1 because of Encoding.ASCII
#if !NETSTANDARD1_1
        //https://discordapp.com/developers/docs/topics/oauth2#client-credentials-grant
        /// <summary>
        /// This shoud only be used for Debug purposes
        /// </summary>
        public static async Task<DiscordRestClient> ClientCredentialsGrantAsync(string ClientId, string ClientSecret, Scope[] scopes, DiscordConfiguration config = null)
        {
            if (scopes.Length == 0)
                throw new Exception("Empty Scope");

            using (HttpClient client = new HttpClient())
            {
                var content = new Dictionary<string, string>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}")));
                content.Add("grant_type", "refresh_token");
                string scope = "";
                foreach (var s in scopes)
                {
                    scope += " " + s.ToString().Replace('_', '.'); //Hack: Use EnumMember instead

                }
                scope = scope.Trim();
                content.Add("scope", scope);
                var res = await client.PostAsync($"{Utilities.GetApiBaseUri()}/oauth2/token", new FormUrlEncodedContent(content));
                if (res.IsSuccessStatusCode)
                {
                    var resContent = await res.Content.ReadAsStringAsync();
                    dynamic dyn = JsonConvert.DeserializeObject<dynamic>(resContent);
                    var response = JsonConvert.DeserializeObject<ClientCredentialsResponse>(resContent);
                    //Hack: Please just fix the Serialization
                    if (dyn.scope == scope)
                        response.Scopes = scopes;
                    return new DiscordRestClient(response, config);
                }
                else
                    throw new Exception("Coud not get Token!");
            }
        }
#endif
        //https://discordapp.com/developers/docs/topics/oauth2#authorization-code-grant
        /// <summary>
        /// Please visit https://discordapp.com/developers/docs/topics/oauth2#authorization-code-grant for more information
        /// </summary>
        /// <param name="clientId">Your Client id</param>
        /// <param name="clientSecret">Your Client secret</param>
        /// <param name="redirectUri">The Redirect Uri just used</param>
        /// <param name="code">The Code you shoud have</param>
        /// <param name="scopes">The Scopes used</param>
        /// <param name="config">Optional Config</param>
        /// <returns>Your Response, can be used to Create a OAuth2Client</returns>
        public static async Task<DiscordRestClient> AuthorizationCodeGrantAsync(string clientId, string clientSecret, string redirectUri, string code, Scope[] scopes, DiscordConfiguration config = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri }
                };
                var res = await client.PostAsync($"{Utilities.GetApiBaseUri()}/oauth2/token", new FormUrlEncodedContent(content));
                if (res.IsSuccessStatusCode)
                {
                    var resContent = await res.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<AuthorizationCodeGrantResponse>(resContent);
                    string scope = "";
                    foreach (var s in scopes)
                    {
                        scope += " " + s.ToString().Replace('_', '.'); //Hack: Use EnumMember instead

                    }
                    scope = scope.Trim();
                    if (scope == response.scope)
                        response.Scopes = scopes;
                    else
                        throw new ArgumentException("Please enter the Right Scopes!");
                    var discord = new DiscordRestClient(response, config);
                    discord.EnableRefresh(
                        response.RefreshToken,
                    redirectUri,
                    clientId,
                    clientSecret);
                    return discord;
                }
                else
                    throw new Exception("Coudnt get token! Check your Parameters and try again!");
            }
        }

        #endregion
    }
}