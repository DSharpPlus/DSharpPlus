using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Objects.Transport;
using DSharpPlus.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    internal sealed class DiscordRestClient
    {
        private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

        internal DiscordClient Discord { get; }
        internal RestClient Rest { get; }

        internal DiscordRestClient(DiscordClient client)
        {
            this.Discord = client;
            this.Rest = new RestClient(client);
        }

        internal static string BuildQueryString(IDictionary<string, string> values, bool post = false)
        {
            if (values == null || values.Count == 0)
                return string.Empty;

            var vals_collection = values.Select(xkvp => string.Concat(WebUtility.UrlEncode(xkvp.Key), "=", WebUtility.UrlEncode(xkvp.Value)));
            var vals = string.Join("&", vals_collection);

            if (!post)
                return string.Concat("?", vals);
            else
                return vals;
        }

        private Task<WebResponse> DoRequestAsync(DiscordClient client, string url, HttpRequestMethod method, IDictionary<string, string> headers = null, string payload = "")
        {
            var req = WebRequest.CreateRequest(client, url, method, headers, payload);
            return this.Rest.HandleRequestAsync(req);
        }

        private Task<WebResponse> DoMultipartAsync(DiscordClient client, string url, HttpRequestMethod method, IDictionary<string, string> headers = null, IDictionary<string, string> values = null,
            IDictionary<string, Stream> files = null)
        {
            var req = MultipartWebRequest.CreateRequest(client, url, method, headers, values, files);
            return this.Rest.HandleRequestAsync(req);
        }

        #region Guild
        internal async Task<DiscordGuild> InternalCreateGuildAsync(string name, string region_id, string iconb64, VerificationLevel? verification_level,
            DefaultMessageNotifications? default_message_notifications)
        {
            var pld = new RestGuildCreatePayload
            {
                Name = name,
                RegionId = region_id,
                DefaultMessageNotifications = default_message_notifications,
                VerificationLevel = verification_level,
                IconBase64 = iconb64
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            await this.Discord.OnGuildCreateEventAsync(guild, raw_members, null);
            return guild;
        }

        internal async Task InternalDeleteGuildAsync(ulong id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE);

            var gld = this.Discord._guilds[id];
            await this.Discord.OnGuildDeleteEventAsync(gld, null);
        }

        internal async Task<DiscordGuild> InternalModifyGuildAsync(ulong guild_id, string name, string region, VerificationLevel? verification_level,
            DefaultMessageNotifications? default_message_notifications, ulong? afk_channel_id, int? afk_timeout, string iconb64, ulong? owner_id, string splashb64, string reason)
        {
            var pld = new RestGuildModifyPayload
            {
                Name = name,
                RegionId = region,
                VerificationLevel = verification_level,
                DefaultMessageNotifications = default_message_notifications,
                AfkChannelId = afk_channel_id,
                AfkTimeout = afk_timeout,
                IconBase64 = iconb64,
                SplashBase64 = splashb64,
                OwnerId = owner_id
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            await this.Discord.OnGuildUpdateEventAsync(guild, raw_members);
            return guild;
        }

        internal async Task<IReadOnlyCollection<DiscordUser>> InternalGetGuildBansAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.BANS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var bans_raw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response).Select(xtu => this.Discord.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this.Discord });
            var bans = new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(bans_raw));

            return bans;
        }

        internal Task InternalCreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason)
        {
            var pld = new RestGuildBanCreatePayload
            {
                DeleteMessageDays = delete_message_days,
                Reason = reason
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.BANS, "/", user_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PUT, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task InternalRemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason)
        {
            var pld = new RestGuildBanRemovePayload
            {
                Reason = reason
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.BANS, "/", user_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task InternalLeaveGuildAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.USERS, Endpoints.ME, Endpoints.GUILDS, "/", guild_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE);
        }

        internal async Task<DiscordMember> InternalAddGuildMemberAsync(ulong guild_id, ulong user_id, string access_token, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
        {
            var pld = new RestGuildMemberAddPayload
            {
                AccessToken = access_token,
                Nickname = nick,
                Roles = roles,
                Deaf = deafened,
                Mute = muted
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.MEMBERS, "/", user_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PUT, payload: JsonConvert.SerializeObject(pld));

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm) { Discord = this.Discord, _guild_id = guild_id };
        }

        internal async Task<IReadOnlyCollection<DiscordMember>> InternalListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.MEMBERS);
            var urlparams = new Dictionary<string, string>();
            if (limit != null && limit > 0)
                urlparams["limit"] = limit.Value.ToString();
            if (after != null)
                urlparams["after"] = after.Value.ToString();
            url = string.Concat(url, BuildQueryString(urlparams));

            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var members_raw = JsonConvert.DeserializeObject<IEnumerable<TransportMember>>(res.Response).Select(xtm => new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild_id });

            return new ReadOnlyCollection<DiscordMember>(new List<DiscordMember>(members_raw));
        }

        internal Task InternalAddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.MEMBERS, "/", user_id, Endpoints.ROLES, "/", role_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PUT, headers);
        }

        internal Task InternalRemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.MEMBERS, "/", user_id, Endpoints.ROLES, "/", role_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal Task InternalModifyGuildChannelPosition(ulong guild_id, IEnumerable<RestGuildChannelReorderPayload> pld, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.CHANNELS);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
        }

        internal Task InternalModifyGuildRolePosition(ulong guild_id, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.ROLES);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<AuditLog> InternalGetAuditLogsAsync(ulong guild_id, int limit, ulong? after, ulong? before, ulong? responsible, int? action_type)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.AUDIT_LOGS);
            var urlparams = new Dictionary<string, string>();
            if (after != null)
                urlparams["after"] = after.Value.ToString();
            if (before != null)
                urlparams["before"] = before.Value.ToString();
            if (responsible != null)
                urlparams["user_id"] = responsible.Value.ToString();
            if (action_type != null)
                urlparams["action_type"] = action_type.Value.ToString();
            if (urlparams.Count > 0)
                url = string.Concat(url, BuildQueryString(urlparams));

            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);
            
            var audit_log_data_raw = JsonConvert.DeserializeObject<AuditLog>(res.Response);

            return audit_log_data_raw;
        }
        #endregion

        #region Channel
        internal async Task<DiscordChannel> InternalCreateGuildChannelAsync(ulong id, string name, ChannelType? type, int? bitrate, int? user_limit, IEnumerable<DiscordOverwrite> overwrites, string reason)
        {
            var pld = new RestChannelCreatePayload
            {
                Name = name,
                Type = type,
                Bitrate = bitrate,
                UserLimit = user_limit,
                PermissionOverwrites = overwrites
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", id, Endpoints.CHANNELS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task InternalModifyChannelAsync(ulong id, string name, int? position, string topic, int? bitrate, int? user_limit, string reason)
        {
            var pld = new RestChannelModifyPayload
            {
                Name = name,
                Position = position,
                Topic = topic,
                Bitrate = bitrate,
                UserLimit = user_limit
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<DiscordChannel> InternalGetChannelAsync(ulong id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task InternalDeleteChannelAsync(ulong id, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal async Task<DiscordMessage> InternalGetMessageAsync(ulong channel_id, ulong message_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> InternalCreateMessageAsync(ulong channel_id, string content, bool? tts, DiscordEmbed embed)
        {
            if (embed != null && embed.Timestamp != null)
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                Content = content,
                IsTTS = tts,
                Embed = embed
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> InternalUploadFileAsync(ulong channel_id, Stream file_data, string file_name, string content, bool? tts, DiscordEmbed embed)
        {
            var file = new Dictionary<string, Stream> { { file_name, file_data } };

            if (embed != null && embed.Timestamp != null)
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestChannelMessageCreateMultipartPayload
            {
                Embed = embed,
                Content = content,
                IsTTS = tts
            };
            if (!string.IsNullOrWhiteSpace(content) || embed != null || tts == true)
                values["payload_json"] = JsonConvert.SerializeObject(pld);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES);
            var res = await this.DoMultipartAsync(this.Discord, url, HttpRequestMethod.POST, values: values, files: file);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> InternalUploadFilesAsync(ulong channel_id, Dictionary<string, Stream> files, string content, bool? tts, DiscordEmbed embed)
        {
            if (embed != null && embed.Timestamp != null)
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestChannelMessageCreateMultipartPayload
            {
                Embed = embed,
                Content = content,
                IsTTS = tts
            };
            if (!string.IsNullOrWhiteSpace(content) || embed != null || tts == true)
                values["payload_json"] = JsonConvert.SerializeObject(pld);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES);
            var res = await this.DoMultipartAsync(this.Discord, url, HttpRequestMethod.POST, values: values, files: files);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<IReadOnlyCollection<DiscordChannel>> InternalGetGuildChannelsAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.CHANNELS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var channels_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channels_raw));
        }

        internal async Task<IReadOnlyCollection<DiscordMessage>> InternalGetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES);
            var urlparams = new Dictionary<string, string>();
            if (around != null)
                urlparams["around"] = around.ToString();
            if (before != null)
                urlparams["before"] = before.ToString();
            if (after != null)
                urlparams["after"] = after.ToString();
            if (limit > 0)
                urlparams["limit"] = limit.ToString();
            if (urlparams.Count > 0)
                url = string.Concat(url, BuildQueryString(urlparams));

            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var msgs = JsonConvert.DeserializeObject<IEnumerable<DiscordMessage>>(res.Response).Select(xm => { xm.Discord = this.Discord; return xm; });

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        internal async Task<DiscordMessage> InternalGetChannelMessageAsync(ulong channel_id, ulong message_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> InternalEditMessageAsync(ulong channel_id, ulong message_id, string content, DiscordEmbed embed)
        {
            if (embed != null && embed.Timestamp != null)
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageEditPayload
            {
                Content = content,
                Embed = embed
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task InternalDeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal Task InternalDeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
        {
            var pld = new RestChannelMessageBulkDeletePayload
            {
                Messages = message_ids
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, Endpoints.BULK_DELETE);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<IReadOnlyCollection<DiscordInvite>> InternalGetChannelInvitesAsync(ulong channel_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.INVITES);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }

        internal async Task<DiscordInvite> InternalCreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, bool temporary, bool unique, string reason)
        {
            var pld = new RestChannelInviteCreatePayload
            {
                MaxAge = max_age,
                MaxUses = max_uses,
                Temporary = temporary,
                Unique = unique
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.INVITES);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task InternalDeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.PERMISSIONS, "/", overwrite_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal Task InternalEditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
        {
            var pld = new RestChannelPermissionEditPayload
            {
                Type = type,
                Allow = allow,
                Deny = deny
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.PERMISSIONS, "/", overwrite_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PUT, headers, JsonConvert.SerializeObject(pld));
        }

        internal Task InternalTriggerTypingAsync(ulong channel_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.TYPING);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST);
        }

        internal async Task<IReadOnlyCollection<DiscordMessage>> InternalGetPinnedMessagesAsync(ulong channel_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.PINS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var messages_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordMessage>>(res.Response).Select(xm => { xm.Discord = this.Discord; return xm; });

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(messages_raw));
        }

        internal Task InternalPinMessageAsync(ulong channel_id, ulong message_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.PINS, "/", message_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PUT);
        }

        internal Task InternalUnpinMessageAsync(ulong channel_id, ulong message_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.PINS, "/", message_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE);
        }

        internal Task InternalGroupDmAddRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
        {
            var pld = new RestChannelGroupDmRecipientAddPayload
            {
                AccessToken = access_token,
                Nickname = nickname
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.RECIPIENTS, "/", user_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PUT, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task InternalGroupDmRemoveRecipientAsync(ulong channel_id, ulong user_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.RECIPIENTS, "/", user_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE);
        }

        internal async Task<DiscordDmChannel> InternalCreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var pld = new RestUserGroupDmCreatePayload
            {
                AccessTokens = access_tokens,
                Nicknames = nicks
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.USERS, Endpoints.ME, Endpoints.CHANNELS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordDmChannel> InternalCreateDmAsync(ulong recipient_id)
        {
            var pld = new RestUserDmCreatePayload
            {
                Recipient = recipient_id
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.USERS, Endpoints.ME, Endpoints.CHANNELS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }
        #endregion

        #region Member
        internal Task<DiscordUser> InternalGetCurrentUserAsync() =>
            this.InternalGetUserAsync("@me");

        internal Task<DiscordUser> InternalGetUserAsync(ulong user) =>
            this.InternalGetUserAsync(user.ToString());

        internal async Task<DiscordUser> InternalGetUserAsync(string user)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.USERS, "/", user);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var duser = new DiscordUser(user_raw) { Discord = this.Discord };

            return duser;
        }

        internal async Task<DiscordMember> InternalGetGuildMemberAsync(ulong guild_id, ulong member_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.MEMBERS, "/", member_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm)
            {
                Discord = this.Discord,
                _guild_id = guild_id
            };
        }

        internal Task InternalRemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason)
        {
            var pld = new RestGuildBanRemovePayload
            {
                Reason = reason
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.MEMBERS, "/", user_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, payload: JsonConvert.SerializeObject(pld));
        }

        internal async Task<DiscordUser> InternalModifyCurrentUserAsync(string username, string base64_avatar)
        {
            var pld = new RestUserUpdateCurrentPayload
            {
                Username = username,
                AvatarBase64 = base64_avatar
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.USERS, Endpoints.ME);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, payload: JsonConvert.SerializeObject(pld));

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var user = new DiscordUser(user_raw) { Discord = this.Discord };

            return user;
        }

        internal async Task<IReadOnlyCollection<DiscordGuild>> InternalGetCurrentUserGuildsAsync(int limit, ulong? before, ulong? after)
        {
            var pld = new RestUserGuildListPayload
            {
                Limit = limit,
                After = after,
                Before = before
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.USERS, Endpoints.ME, Endpoints.GUILDS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET, payload: JsonConvert.SerializeObject(pld));

            var guilds_raw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response).Select(xug => this.Discord._guilds[xug.Id]);

            return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(guilds_raw));
        }

        internal Task InternalModifyGuildMemberAsync(ulong guild_id, ulong user_id, string nick, IEnumerable<ulong> role_ids, bool? mute, bool? deaf, ulong? voice_channel_id, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick,
                RoleIds = role_ids,
                Deafen = deaf,
                Mute = mute,
                VoiceChannelId = voice_channel_id
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.MEMBERS, "/", user_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, payload: JsonConvert.SerializeObject(pld));
        }
        #endregion

        #region Roles
        internal async Task<IReadOnlyCollection<DiscordRole>> InternalGetGuildRolesAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.ROLES);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var roles_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = this.Discord; return xr; });

            return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(roles_raw));
        }

        internal async Task<DiscordGuild> InternalGetGuildAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild_rest = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            await this.Discord.OnGuildUpdateEventAsync(guild_rest, raw_members);
            return this.Discord._guilds[guild_rest.Id];
        }

        internal async Task<DiscordRole> InternalModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.ROLES, "/", role_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task InternalDeleteRoleAsync(ulong guild_id, ulong role_id, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.ROLES, "/", role_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal async Task<DiscordRole> InternalCreateGuildRole(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.ROLES);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }
        #endregion

        #region Prune
        internal async Task<int> InternalGetGuildPruneCountAsync(ulong guild_id, int days)
        {
            var pld = new RestGuildPrunePayload
            {
                Days = days
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.PRUNE);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET, payload: JsonConvert.SerializeObject(pld));

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        
        internal async Task<int> InternalBeginGuildPruneAsync(ulong guild_id, int days, string reason)
        {
            var pld = new RestGuildPrunePayload
            {
                Days = days
            };

            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.PRUNE);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        #endregion

        #region GuildVarious
        internal async Task<IReadOnlyCollection<DiscordIntegration>> InternalGetGuildIntegrationsAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.INTEGRATIONS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var integrations_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrations_raw));
        }

        internal async Task<DiscordIntegration> InternalCreateGuildIntegrationAsync(ulong guild_id, string type, ulong id)
        {
            var pld = new RestGuildIntegrationAttachPayload
            {
                Type = type,
                Id = id
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.INTEGRATIONS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordIntegration> InternalModifyGuildIntegrationAsync(ulong guild_id, ulong integration_id, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
        {
            var pld = new RestGuildIntegrationModifyPayload
            {
                ExpireBehavior = expire_behaviour,
                ExpireGracePeriod = expire_grace_period,
                EnableEmoticons = enable_emoticons
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.INTEGRATIONS, "/", integration_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, payload: JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task InternalDeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration)
        {
            var pld = integration;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.INTEGRATIONS, "/", integration.Id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, payload: JsonConvert.SerializeObject(integration));
        }

        internal Task InternalSyncGuildIntegrationAsync(ulong guild_id, ulong integration_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.INTEGRATIONS, "/", integration_id, Endpoints.SYNC);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST);
        }

        internal async Task<DiscordGuildEmbed> InternalGetGuildEmbedAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.EMBED);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var embed = JsonConvert.DeserializeObject<DiscordGuildEmbed>(res.Response);

            return embed;
        }

        internal async Task<DiscordGuildEmbed> InternalModifyGuildEmbedAsync(ulong guild_id, DiscordGuildEmbed embed)
        {
            var pld = embed;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.EMBED);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, payload: JsonConvert.SerializeObject(embed));

            var embed_rest = JsonConvert.DeserializeObject<DiscordGuildEmbed>(res.Response);

            return embed_rest;
        }

        internal async Task<IReadOnlyCollection<DiscordVoiceRegion>> InternalGetGuildVoiceRegionsAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.REGIONS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var regions_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions_raw));
        }

        internal async Task<IReadOnlyCollection<DiscordInvite>> InternalGetGuildInvitesAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.INVITES);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }
        #endregion

        #region Invite
        internal async Task<DiscordInvite> InternalGetInvite(string invite_code)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.INVITES, "/", invite_code);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordInvite> InternalDeleteInvite(string invite_code, string reason)
        {
            var headers = Utils.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.INVITES, "/", invite_code);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordInvite> InternalAcceptInvite(string invite_code)
        {
            this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "REST API", "Invite accept endpoint was used; this account is now likely unverified", DateTime.Now);
            
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.INVITES, "/", invite_code);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST);
            
            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }
        #endregion

        #region Connections
        internal async Task<IReadOnlyCollection<DiscordConnection>> InternalGetUsersConnectionsAsync()
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.USERS, Endpoints.ME, Endpoints.CONNECTIONS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var connections_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connections_raw));
        }
        #endregion

        #region Voice
        internal async Task<IReadOnlyCollection<DiscordVoiceRegion>> InternalListVoiceRegionsAsync()
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.VOICE, Endpoints.REGIONS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
        }
        #endregion

        #region Webhooks
        internal async Task<DiscordWebhook> InternalCreateWebhookAsync(ulong channel_id, string name, string base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.WEBHOOKS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<IReadOnlyCollection<DiscordWebhook>> InternalGetChannelWebhooksAsync(ulong channel_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.WEBHOOKS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        internal async Task<IReadOnlyCollection<DiscordWebhook>> InternalGetGuildWebhooksAsync(ulong guild_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.GUILDS, "/", guild_id, Endpoints.WEBHOOKS);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        internal async Task<DiscordWebhook> InternalGetWebhookAsync(ulong webhook_id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        // Auth header not required
        internal async Task<DiscordWebhook> InternalGetWebhookWithTokenAsync(ulong webhook_id, string webhook_token)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id, "/", webhook_token);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Token = webhook_token;
            ret.Id = webhook_id;
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordWebhook> InternalModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordWebhook> InternalModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string webhook_token, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id, "/", webhook_token);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task InternalDeleteWebhookAsync(ulong webhook_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal Task InternalDeleteWebhookAsync(ulong webhook_id, string webhook_token, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id, "/", webhook_token);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal Task InternalExecuteWebhookAsync(ulong webhook_id, string webhook_token, string content, string username, string avatar_url, bool? tts, IEnumerable<DiscordEmbed> embeds)
        {
            if (embeds != null)
                foreach (var embed in embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestWebhookExecutePayload
            {
                Content = content,
                Username = username,
                AvatarUrl = avatar_url,
                IsTTS = tts,
                Embeds = embeds
            };

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id, "/", webhook_token);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task InternalExecuteWebhookSlackAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id, "/", webhook_token, Endpoints.SLACK);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: json_payload);
        }

        internal Task InternalExecuteWebhookGithubAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.WEBHOOKS, "/", webhook_id, "/", webhook_token, Endpoints.GITHUB);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.POST, payload: json_payload);
        }
        #endregion

        #region Reactions
        internal Task InternalCreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id, Endpoints.REACTIONS, "/", emoji, Endpoints.ME);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.PUT);
        }

        internal Task InternalDeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id, Endpoints.REACTIONS, "/", emoji, Endpoints.ME);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE);
        }

        internal Task InternalDeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id, Endpoints.REACTIONS, "/", emoji, "/", user_id);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }

        internal async Task<IReadOnlyCollection<DiscordUser>> InternalGetReactionsAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id, Endpoints.REACTIONS, "/", emoji);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var reacters_raw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response).Select(xtu => this.Discord.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this.Discord });

            return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters_raw));
        }

        internal Task InternalDeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.CHANNELS, "/", channel_id, Endpoints.MESSAGES, "/", message_id, Endpoints.REACTIONS);
            return this.DoRequestAsync(this.Discord, url, HttpRequestMethod.DELETE, headers);
        }
        #endregion

        #region Misc
        internal Task<DiscordApplication> InternalGetCurrentApplicationInfoAsync() =>
            this.InternalGetApplicationInfoAsync("@me");

        internal Task<DiscordApplication> InternalGetApplicationInfoAsync(ulong id) =>
            this.InternalGetApplicationInfoAsync(id.ToString());

        private async Task<DiscordApplication> InternalGetApplicationInfoAsync(string id)
        {
            var url = string.Concat(Utils.GetApiBaseUri(this.Discord), Endpoints.OAUTH2, Endpoints.APPLICATIONS, "/", id);
            var res = await this.DoRequestAsync(this.Discord, url, HttpRequestMethod.GET);

            var app = JsonConvert.DeserializeObject<DiscordApplication>(res.Response);
            app.Discord = this.Discord;

            return app;
        }
        #endregion
    }
}
