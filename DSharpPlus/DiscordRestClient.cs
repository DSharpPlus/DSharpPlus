using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    internal sealed class DiscordRestClient
    {
        private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

        internal DiscordClient Discord { get; }
        internal RestClient Rest { get; }

        private string LastAckToken { get; set; } = null;
        private SemaphoreSlim TokenSemaphore { get; } = new SemaphoreSlim(1, 1);

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

        private Task<RestResponse> DoRequestAsync(DiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, IDictionary<string, string> headers = null, string payload = "")
        {
            var req = new RestRequest(client, bucket, url, method, headers, payload);
            _ = this.Rest.ExecuteRequestAsync(req);
            return req.WaitForCompletionAsync();
        }

        private Task<RestResponse> DoMultipartAsync(DiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, IDictionary<string, string> headers = null, IDictionary<string, string> values = null,
            IDictionary<string, Stream> files = null)
        {
            var req = new MultipartWebRequest(client, bucket, url, method, headers, values, files);
            _ = this.Rest.ExecuteRequestAsync(req);
            return req.WaitForCompletionAsync();
        }

        #region Guild
        internal async Task<DiscordGuild> CreateGuildAsync(string name, string region_id, string iconb64, VerificationLevel? verification_level,
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

            var route = string.Concat(Endpoints.GUILDS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            await this.Discord.OnGuildCreateEventAsync(guild, raw_members, null);
            return guild;
        }

        internal async Task DeleteGuildAsync(ulong id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE);

            var gld = this.Discord._guilds[id];
            await this.Discord.OnGuildDeleteEventAsync(gld, null);
        }

        internal async Task<DiscordGuild> ModifyGuildAsync(ulong guild_id, string name, string region, VerificationLevel? verification_level,
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

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            await this.Discord.OnGuildUpdateEventAsync(guild, raw_members);
            return guild;
        }

        internal async Task<IReadOnlyList<DiscordUser>> GetGuildBansAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.BANS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var bans_raw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response).Select(xtu => this.Discord.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this.Discord });
            var bans = new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(bans_raw));

            return bans;
        }

        internal Task CreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason)
        {
            if (delete_message_days < 0 || delete_message_days > 7)
                throw new ArgumentException("Delete message days must be a number between 0-7", nameof(delete_message_days));

            var urlparams = new Dictionary<string, string>();
            urlparams["delete_message_days"] = delete_message_days.ToString();
            if (reason != null)
                urlparams["reason"] = reason;

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.BANS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id = guild_id.ToString(), user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, BuildQueryString(urlparams)));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT);
        }

        internal Task RemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason)
        {
            var pld = new RestGuildBanRemovePayload
            {
                Reason = reason
            };

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.BANS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = guild_id.ToString(), user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task LeaveGuildAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.USERS, Endpoints.ME, Endpoints.GUILDS, "/:guild_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE);
        }

        internal async Task<DiscordMember> AddGuildMemberAsync(ulong guild_id, ulong user_id, string access_token, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
        {
            var pld = new RestGuildMemberAddPayload
            {
                AccessToken = access_token,
                Nickname = nick,
                Roles = roles,
                Deaf = deafened,
                Mute = muted
            };

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id = guild_id.ToString(), user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, payload: JsonConvert.SerializeObject(pld));

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm) { Discord = this.Discord, _guild_id = guild_id };
        }

        internal async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
        {
            var urlparams = new Dictionary<string, string>();
            if (limit != null && limit > 0)
                urlparams["limit"] = limit.Value.ToString();
            if (after != null)
                urlparams["after"] = after.Value.ToString();
            
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, urlparams.Any() ? BuildQueryString(urlparams) : ""));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var members_raw = JsonConvert.DeserializeObject<IEnumerable<TransportMember>>(res.Response).Select(xtm => new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild_id });

            return new ReadOnlyCollection<DiscordMember>(new List<DiscordMember>(members_raw));
        }

        internal Task AddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS, "/:user_id", Endpoints.ROLES, "/:role_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id = guild_id.ToString(), user_id = user_id.ToString(), role_id = role_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, headers);
        }

        internal Task RemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS, "/:user_id", Endpoints.ROLES, "/:role_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = guild_id.ToString(), user_id = user_id.ToString(), role_id = role_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal Task ModifyGuildChannelPosition(ulong guild_id, IEnumerable<RestGuildChannelReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.CHANNELS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
        }

        internal Task ModifyGuildRolePosition(ulong guild_id, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.ROLES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<AuditLog> GetAuditLogsAsync(ulong guild_id, int limit, ulong? after, ulong? before, ulong? responsible, int? action_type)
        {
            var urlparams = new Dictionary<string, string>();
            if (after != null)
                urlparams["after"] = after.Value.ToString();
            if (before != null)
                urlparams["before"] = before.Value.ToString();
            if (responsible != null)
                urlparams["user_id"] = responsible.Value.ToString();
            if (action_type != null)
                urlparams["action_type"] = action_type.Value.ToString();

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.AUDIT_LOGS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, urlparams.Any() ? BuildQueryString(urlparams) : ""));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);
            
            var audit_log_data_raw = JsonConvert.DeserializeObject<AuditLog>(res.Response);

            return audit_log_data_raw;
        }
        #endregion

        #region Channel
        internal async Task<DiscordChannel> CreateGuildChannelAsync(ulong id, string name, ChannelType? type, int? bitrate, int? user_limit, IEnumerable<DiscordOverwrite> overwrites, string reason)
        {
            var pld = new RestChannelCreatePayload
            {
                Name = name,
                Type = type,
                Bitrate = bitrate,
                UserLimit = user_limit,
                PermissionOverwrites = overwrites
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);
            
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.CHANNELS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task ModifyChannelAsync(ulong id, string name, int? position, string topic, int? bitrate, int? user_limit, string reason)
        {
            var pld = new RestChannelModifyPayload
            {
                Name = name,
                Position = position,
                Topic = topic,
                Bitrate = bitrate,
                UserLimit = user_limit
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id = id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<DiscordChannel> GetChannelAsync(ulong id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteChannelAsync(ulong id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal async Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, Optional<string> content, bool? tts, Optional<DiscordEmbed> embed)
        {
            if (embed.HasValue && embed.Value != null && embed.Value.Timestamp != null)
                embed.Value.Timestamp = embed.Value.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = content.HasValue,
                Content = content.HasValue ? (string)content : null,
                IsTTS = tts,
                HasEmbed = embed.HasValue,
                Embed = embed.HasValue ? (DiscordEmbed)embed : null
            };

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> UploadFileAsync(ulong channel_id, Stream file_data, string file_name, string content, bool? tts, DiscordEmbed embed)
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

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, values: values, files: file);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> UploadFilesAsync(ulong channel_id, Dictionary<string, Stream> files, string content, bool? tts, DiscordEmbed embed)
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

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, values: values, files: files);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.CHANNELS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var channels_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channels_raw));
        }

        internal async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
        {
            var urlparams = new Dictionary<string, string>();
            if (around != null)
                urlparams["around"] = around.ToString();
            if (before != null)
                urlparams["before"] = before.ToString();
            if (after != null)
                urlparams["after"] = after.ToString();
            if (limit > 0)
                urlparams["limit"] = limit.ToString();

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, urlparams.Any() ? BuildQueryString(urlparams) : ""));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var msgs = JsonConvert.DeserializeObject<IEnumerable<DiscordMessage>>(res.Response).Select(xm => { xm.Discord = this.Discord; return xm; });

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        internal async Task<DiscordMessage> GetChannelMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content, Optional<DiscordEmbed> embed)
        {
            if (embed.HasValue && embed.Value != null && embed.Value.Timestamp != null)
                embed.Value.Timestamp = embed.Value.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageEditPayload
            {
                HasContent = content.HasValue,
                Content = content.HasValue ? (string)content : null,
                HasEmbed = embed.HasValue,
                Embed = embed.HasValue ? (DiscordEmbed)embed : null
            };

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);
            
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal Task DeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
        {
            var pld = new RestChannelMessageBulkDeletePayload
            {
                Messages = message_ids
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);
            
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, Endpoints.BULK_DELETE);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.INVITES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }

        internal async Task<DiscordInvite> CreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, bool temporary, bool unique, string reason)
        {
            var pld = new RestChannelInviteCreatePayload
            {
                MaxAge = max_age,
                MaxUses = max_uses,
                Temporary = temporary,
                Unique = unique
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);
            
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.INVITES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.PERMISSIONS, "/:overwrite_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal Task EditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
        {
            var pld = new RestChannelPermissionEditPayload
            {
                Type = type,
                Allow = allow,
                Deny = deny
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);
            
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.PERMISSIONS, "/:overwrite_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, headers, JsonConvert.SerializeObject(pld));
        }

        internal Task TriggerTypingAsync(ulong channel_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.TYPING);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST);
        }

        internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channel_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.PINS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var messages_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordMessage>>(res.Response).Select(xm => { xm.Discord = this.Discord; return xm; });

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(messages_raw));
        }

        internal Task PinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.PINS, "/:message_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT);
        }

        internal Task UnpinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.PINS, "/:message_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE);
        }

        internal Task GroupDmAddRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
        {
            var pld = new RestChannelGroupDmRecipientAddPayload
            {
                AccessToken = access_token,
                Nickname = nickname
            };

            var route = string.Concat(Endpoints.USERS, Endpoints.ME, Endpoints.CHANNELS, "/:channel_id", Endpoints.RECIPIENTS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id = channel_id.ToString(), user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task GroupDmRemoveRecipientAsync(ulong channel_id, ulong user_id)
        {
            var route = string.Concat(Endpoints.USERS, Endpoints.ME, Endpoints.CHANNELS, "/:channel_id", Endpoints.RECIPIENTS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = channel_id.ToString(), user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE);
        }

        internal async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var pld = new RestUserGroupDmCreatePayload
            {
                AccessTokens = access_tokens,
                Nicknames = nicks
            };

            var route = string.Concat(Endpoints.USERS, Endpoints.ME, Endpoints.CHANNELS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordDmChannel> CreateDmAsync(ulong recipient_id)
        {
            var pld = new RestUserDmCreatePayload
            {
                Recipient = recipient_id
            };
            
            var route = string.Concat(Endpoints.USERS, Endpoints.ME, Endpoints.CHANNELS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }
        #endregion

        #region Member
        internal Task<DiscordUser> GetCurrentUserAsync() =>
            this.GetUserAsync("@me");

        internal Task<DiscordUser> GetUserAsync(ulong user) =>
            this.GetUserAsync(user.ToString());

        internal async Task<DiscordUser> GetUserAsync(string user_id)
        {
            var route = string.Concat(Endpoints.USERS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { user_id = user_id }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var duser = new DiscordUser(user_raw) { Discord = this.Discord };

            return duser;
        }

        internal async Task<DiscordMember> GetGuildMemberAsync(ulong guild_id, ulong member_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString(), user_id = member_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm)
            {
                Discord = this.Discord,
                _guild_id = guild_id
            };
        }

        internal Task RemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason)
        {
            var pld = new RestGuildBanRemovePayload
            {
                Reason = reason
            };
            
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = guild_id.ToString(), user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, payload: JsonConvert.SerializeObject(pld));
        }

        internal async Task<DiscordUser> ModifyCurrentUserAsync(string username, string base64_avatar)
        {
            var pld = new RestUserUpdateCurrentPayload
            {
                Username = username,
                AvatarBase64 = base64_avatar
            };

            var route = string.Concat(Endpoints.USERS, Endpoints.ME);
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, payload: JsonConvert.SerializeObject(pld));

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var user = new DiscordUser(user_raw) { Discord = this.Discord };

            return user;
        }

        internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit, ulong? before, ulong? after)
        {
            var pld = new RestUserGuildListPayload
            {
                Limit = limit,
                After = after,
                Before = before
            };
            
            var route = string.Concat(Endpoints.USERS, Endpoints.ME, Endpoints.GUILDS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, payload: JsonConvert.SerializeObject(pld));

            var guilds_raw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response).Select(xug => this.Discord._guilds[xug.Id]);

            return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(guilds_raw));
        }

        internal Task ModifyGuildMemberAsync(ulong guild_id, ulong user_id, string nick, IEnumerable<ulong> role_ids, bool? mute, bool? deaf, ulong? voice_channel_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
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

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS, "/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString(), user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task ModifyCurrentMemberNicknameAsync(ulong guild_id, string nick, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick
            };
            
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.MEMBERS, Endpoints.ME, Endpoints.NICK);
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, payload: JsonConvert.SerializeObject(pld));
        }
        #endregion

        #region Roles
        internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.ROLES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var roles_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = this.Discord; return xr; });

            return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(roles_raw));
        }

        internal async Task<DiscordGuild> GetGuildAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild_rest = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            await this.Discord.OnGuildUpdateEventAsync(guild_rest, raw_members);
            return this.Discord._guilds[guild_rest.Id];
        }

        internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.ROLES, "/:role_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString(), role_id = role_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteRoleAsync(ulong guild_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.ROLES, "/:role_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = guild_id.ToString(), role_id = role_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal async Task<DiscordRole> CreateGuildRole(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;
            
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.ROLES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }
        #endregion

        #region Prune
        internal async Task<int> GetGuildPruneCountAsync(ulong guild_id, int days)
        {
            var pld = new RestGuildPrunePayload
            {
                Days = days
            };

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.PRUNE);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, payload: JsonConvert.SerializeObject(pld));

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        
        internal async Task<int> BeginGuildPruneAsync(ulong guild_id, int days, string reason)
        {
            var pld = new RestGuildPrunePayload
            {
                Days = days
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;
            
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.PRUNE);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        #endregion

        #region GuildVarious
        internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.INTEGRATIONS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var integrations_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrations_raw));
        }

        internal async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guild_id, string type, ulong id)
        {
            var pld = new RestGuildIntegrationAttachPayload
            {
                Type = type,
                Id = id
            };

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.INTEGRATIONS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guild_id, ulong integration_id, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
        {
            var pld = new RestGuildIntegrationModifyPayload
            {
                ExpireBehavior = expire_behaviour,
                ExpireGracePeriod = expire_grace_period,
                EnableEmoticons = enable_emoticons
            };

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.INTEGRATIONS, "/:integration_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString(), integration_id = integration_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, payload: JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration)
        {
            var pld = integration;

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.INTEGRATIONS, "/:integration_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = guild_id.ToString(), integration_id = integration.Id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, payload: JsonConvert.SerializeObject(integration));
        }

        internal Task SyncGuildIntegrationAsync(ulong guild_id, ulong integration_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.INTEGRATIONS, "/:integration_id", Endpoints.SYNC);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = guild_id.ToString(), integration_id = integration_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST);
        }

        internal async Task<DiscordGuildEmbed> GetGuildEmbedAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.EMBED);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var embed = JsonConvert.DeserializeObject<DiscordGuildEmbed>(res.Response);

            return embed;
        }

        internal async Task<DiscordGuildEmbed> ModifyGuildEmbedAsync(ulong guild_id, DiscordGuildEmbed embed)
        {
            var pld = embed;
            
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.EMBED);
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, payload: JsonConvert.SerializeObject(embed));

            var embed_rest = JsonConvert.DeserializeObject<DiscordGuildEmbed>(res.Response);

            return embed_rest;
        }

        internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.REGIONS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var regions_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions_raw));
        }

        internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.INVITES);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }
        #endregion

        #region Invite
        internal async Task<DiscordInvite> GetInvite(string invite_code)
        {
            var route = string.Concat(Endpoints.INVITES, "/:invite_code");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { invite_code = invite_code }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordInvite> DeleteInvite(string invite_code, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;
            
            var route = string.Concat(Endpoints.INVITES, "/:invite_code");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { invite_code = invite_code }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /* 
         * Disabled due to API restrictions
         * 
         * internal async Task<DiscordInvite> InternalAcceptInvite(string invite_code)
         * {
         *     this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "REST API", "Invite accept endpoint was used; this account is now likely unverified", DateTime.Now);
         *     
         *     var url = new Uri(string.Concat(Utils.GetApiBaseUri(), Endpoints.INVITES, "/", invite_code));
         *     var bucket = this.Rest.GetBucket(0, MajorParameterType.Unbucketed, url, HttpRequestMethod.POST);
         *     var res = await this.DoRequestAsync(this.Discord, bucket, url, HttpRequestMethod.POST);
         *     
         *     var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
         *     ret.Discord = this.Discord;
         * 
         *     return ret;
         * }
         */
        #endregion

        #region Connections
        internal async Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
        {
            var route = string.Concat(Endpoints.USERS, Endpoints.ME, Endpoints.CONNECTIONS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var connections_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connections_raw));
        }
        #endregion

        #region Voice
        internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        {
            var route = string.Concat(Endpoints.VOICE, Endpoints.REGIONS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
        }
        #endregion

        #region Webhooks
        internal async Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, string base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.WEBHOOKS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channel_id)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.WEBHOOKS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = channel_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guild_id)
        {
            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.WEBHOOKS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guild_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhook_id)
        {
            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id = webhook_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        // Auth header not required
        internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhook_id, string webhook_token)
        {
            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id/:webhook_token");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id = webhook_id.ToString(), webhook_token = webhook_token }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Token = webhook_token;
            ret.Id = webhook_id;
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id = webhook_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string webhook_token, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id/:webhook_token");
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id = webhook_id.ToString(), webhook_token = webhook_token }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteWebhookAsync(ulong webhook_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id = webhook_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal Task DeleteWebhookAsync(ulong webhook_id, string webhook_token, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id/:webhook_token");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id = webhook_id.ToString(), webhook_token = webhook_token }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal Task ExecuteWebhookAsync(ulong webhook_id, string webhook_token, string content, string username, string avatar_url, bool? tts, IEnumerable<DiscordEmbed> embeds)
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

            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id/:webhook_token");
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id = webhook_id.ToString(), webhook_token = webhook_token }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task ExecuteWebhookSlackAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id/:webhook_token", Endpoints.SLACK);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id = webhook_id.ToString(), webhook_token = webhook_token }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: json_payload);
        }

        internal Task ExecuteWebhookGithubAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = string.Concat(Endpoints.WEBHOOKS, "/:webhook_id/:webhook_token", Endpoints.GITHUB);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id = webhook_id.ToString(), webhook_token = webhook_token }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: json_payload);
        }
        #endregion

        #region Reactions
        internal Task CreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id", Endpoints.REACTIONS, "/:emoji", Endpoints.ME);
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString(), emoji = emoji }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT);
        }

        internal Task DeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id", Endpoints.REACTIONS, "/:emoji", Endpoints.ME);
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString(), emoji = emoji }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE);
        }

        internal Task DeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id", Endpoints.REACTIONS, "/:emoji/:user_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString(), emoji = emoji, user_id = user_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }

        internal async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id", Endpoints.REACTIONS, "/:emoji");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString(), emoji = emoji }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var reacters_raw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response).Select(xtu => this.Discord.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this.Discord });

            return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters_raw));
        }

        internal Task DeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/:message_id", Endpoints.REACTIONS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id = channel_id.ToString(), message_id = message_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, headers);
        }
        #endregion

        #region Misc
        internal Task<DiscordApplication> GetCurrentApplicationInfoAsync() =>
            this.GetApplicationInfoAsync("@me");

        internal Task<DiscordApplication> GetApplicationInfoAsync(ulong id) =>
            this.GetApplicationInfoAsync(id.ToString());

        private async Task<DiscordApplication> GetApplicationInfoAsync(string id)
        {
            var route = string.Concat(Endpoints.OAUTH2, Endpoints.APPLICATIONS, "/:app_id");
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { app_id = id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var app = JsonConvert.DeserializeObject<DiscordApplication>(res.Response);
            app.Discord = this.Discord;

            return app;
        }

        internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication app)
        {
            var route = string.Concat(Endpoints.OAUTH2, Endpoints.APPLICATIONS, "/:app_id", Endpoints.ASSETS);
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { app_id = app.Id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET);

            var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
            foreach (var asset in assets)
            {
                asset.Discord = app.Discord;
                asset.Application = app;
            }

            return new ReadOnlyCollection<DiscordApplicationAsset>(new List<DiscordApplicationAsset>(assets));
        }

        internal async Task AcknowledgeMessageAsync(ulong msg_id, ulong chn_id)
        {
            await this.TokenSemaphore.WaitAsync();

            var pld = new AcknowledgePayload
            {
                Token = this.LastAckToken
            };

            var route = string.Concat(Endpoints.CHANNELS, "/:channel_id", Endpoints.MESSAGES, "/", msg_id, Endpoints.ACK);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id = chn_id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<AcknowledgePayload>(res.Response);
            this.LastAckToken = ret.Token;

            this.TokenSemaphore.Release();
        }

        internal async Task AcknowledgeGuildAsync(ulong id)
        {
            await this.TokenSemaphore.WaitAsync();

            var pld = new AcknowledgePayload
            {
                Token = this.LastAckToken
            };

            var route = string.Concat(Endpoints.GUILDS, "/:guild_id", Endpoints.ACK);
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = id.ToString() }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, payload: JsonConvert.SerializeObject(pld));

            if (res.ResponseCode != 204)
            {
                var ret = JsonConvert.DeserializeObject<AcknowledgePayload>(res.Response);
                this.LastAckToken = ret.Token;
            }
            else
                this.LastAckToken = null;

            this.TokenSemaphore.Release();
        }
        #endregion
    }
}
