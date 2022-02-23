// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net
{
    public sealed class DiscordApiClient
    {
        private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

        internal BaseDiscordClient Discord { get; }
        internal RestClient Rest { get; }

        internal DiscordApiClient(BaseDiscordClient client)
        {
            this.Discord = client;
            this.Rest = new RestClient(client);
        }

        internal DiscordApiClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRateLimit, ILogger logger) // This is for meta-clients, such as the webhook client
        {
            this.Rest = new RestClient(proxy, timeout, useRelativeRateLimit, logger);
        }

        private static string BuildQueryString(IDictionary<string, string> values, bool post = false)
        {
            if (values == null || values.Count == 0)
                return string.Empty;

            var vals_collection = values.Select(xkvp =>
                $"{WebUtility.UrlEncode(xkvp.Key)}={WebUtility.UrlEncode(xkvp.Value)}");
            var vals = string.Join("&", vals_collection);

            return !post ? $"?{vals}" : vals;
        }

        private DiscordMessage PrepareMessage(JToken msg_raw)
        {
            var author = msg_raw["author"].ToDiscordObject<TransportUser>();
            var ret = msg_raw.ToDiscordObject<DiscordMessage>();
            ret.Discord = this.Discord;

            this.PopulateMessage(author, ret);

            var referencedMsg = msg_raw["referenced_message"];
            if (ret.MessageType == MessageType.Reply && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
            {
                author = referencedMsg["author"].ToDiscordObject<TransportUser>();
                ret.ReferencedMessage.Discord = this.Discord;
                this.PopulateMessage(author, ret.ReferencedMessage);
            }

            if (ret.Channel != null)
                return ret;

            var channel = !ret.GuildId.HasValue
                ? new DiscordDmChannel
                {
                    Id = ret.ChannelId,
                    Discord = this.Discord,
                    Type = ChannelType.Private
                }
                : new DiscordChannel
                {
                    Id = ret.ChannelId,
                    GuildId = ret.GuildId,
                    Discord = this.Discord
                };
            ret.Channel = channel;

            return ret;
        }

        private void PopulateMessage(TransportUser author, DiscordMessage ret)
        {
            var guild = ret.Channel?.Guild;

            //If this is a webhook, it shouldn't be in the user cache.
            if (author.IsBot && int.Parse(author.Discriminator) == 0)
            {
                ret.Author = new DiscordUser(author) { Discord = this.Discord };
            }
            else
            {
                if (!this.Discord.UserCache.TryGetValue(author.Id, out var usr))
                {
                    this.Discord.UserCache[author.Id] = usr = new DiscordUser(author) { Discord = this.Discord };
                }

                if (guild != null)
                {
                    if (!guild.Members.TryGetValue(author.Id, out var mbr))
                        mbr = new DiscordMember(usr) { Discord = this.Discord, _guild_id = guild.Id };
                    ret.Author = mbr;
                }
                else
                {
                    ret.Author = usr;
                }
            }

            ret.PopulateMentions();

            if (ret._reactions == null)
                ret._reactions = new List<DiscordReaction>();
            foreach (var xr in ret._reactions)
                xr.Emoji.Discord = this.Discord;
        }

        private Task<RestResponse> DoRequestAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, string payload = null, double? ratelimitWaitOverride = null)
        {
            var req = new RestRequest(client, bucket, url, method, route, headers, payload, ratelimitWaitOverride);

            if (this.Discord != null)
                this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequestAsync(req);

            return req.WaitForCompletionAsync();
        }

        private Task<RestResponse> DoMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
            IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimitWaitOverride = null, bool removeFileCount = false)
        {
            var req = new MultipartWebRequest(client, bucket, url, method, route, headers, values, files, ratelimitWaitOverride);
            req._removeFileCount = removeFileCount;


            if (this.Discord != null)
                this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequestAsync(req);

            return req.WaitForCompletionAsync();
        }

        #region Guild

        internal async Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(ulong guild_id, string name, int? limit)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.SEARCH}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);
            var querydict = new Dictionary<string, string>
            {
                ["query"] = name,
                ["limit"] = limit.ToString()
            };
            var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JArray.Parse(res.Response);
            var tms = json.ToDiscordObject<IReadOnlyList<TransportMember>>();

            var mbrs = new List<DiscordMember>();
            foreach (var xtm in tms)
            {
                var usr = new DiscordUser(xtm.User) { Discord = this.Discord };

                this.Discord.UpdateUserCache(usr);

                mbrs.Add(new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild_id });
            }

            return mbrs;
        }

        internal async Task<DiscordBan> GetGuildBanAsync(ulong guild_id, ulong user_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id, user_id}, out var url);
            var uri = Utilities.GetApiUriFor(url);
            var res = await this.DoRequestAsync(this.Discord, bucket, uri, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JObject.Parse(res.Response);

            var ban = json.ToDiscordObject<DiscordBan>();

            if (!this.Discord.TryGetCachedUserInternal(ban.RawUser.Id, out var usr))
            {
                usr = new DiscordUser(ban.RawUser) { Discord = this.Discord };
                usr = this.Discord.UpdateUserCache(usr);
            }

            ban.User = usr;

            return ban;
        }

        internal async Task<DiscordGuild> CreateGuildAsync(string name, string region_id, Optional<string> iconb64, VerificationLevel? verification_level,
            DefaultMessageNotifications? default_message_notifications,
            SystemChannelFlags? system_channel_flags)
        {
            var pld = new RestGuildCreatePayload
            {
                Name = name,
                RegionId = region_id,
                DefaultMessageNotifications = default_message_notifications,
                VerificationLevel = verification_level,
                IconBase64 = iconb64,
                SystemChannelFlags = system_channel_flags
            };

            var route = $"{Endpoints.GUILDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildCreateEventAsync(guild, raw_members, null).ConfigureAwait(false);
            return guild;
        }

        internal async Task<DiscordGuild> CreateGuildFromTemplateAsync(string template_code, string name, Optional<string> iconb64)
        {
            var pld = new RestGuildCreateFromTemplatePayload
            {
                Name = name,
                IconBase64 = iconb64
            };

            var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { template_code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildCreateEventAsync(guild, raw_members, null).ConfigureAwait(false);
            return guild;
        }

        internal async Task DeleteGuildAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

            if (this.Discord is DiscordClient dc)
            {
                var gld = dc._guilds[guild_id];
                await dc.OnGuildDeleteEventAsync(gld, null).ConfigureAwait(false);
            }
        }

        internal async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, Optional<string> name,
            Optional<string> region, Optional<VerificationLevel> verificationLevel,
            Optional<DefaultMessageNotifications> defaultMessageNotifications, Optional<MfaLevel> mfaLevel,
            Optional<ExplicitContentFilter> explicitContentFilter, Optional<ulong?> afkChannelId,
            Optional<int> afkTimeout, Optional<string> iconb64, Optional<ulong> ownerId, Optional<string> splashb64,
            Optional<ulong?> systemChannelId, Optional<string> banner, Optional<string> description,
            Optional<string> discorverySplash, Optional<IEnumerable<string>> features, Optional<string> preferredLocale,
            Optional<ulong?> publicUpdatesChannelId, Optional<ulong?> rulesChannelId, Optional<SystemChannelFlags> systemChannelFlags,
            string reason)
        {
            var pld = new RestGuildModifyPayload
            {
                Name = name,
                RegionId = region,
                VerificationLevel = verificationLevel,
                DefaultMessageNotifications = defaultMessageNotifications,
                MfaLevel = mfaLevel,
                ExplicitContentFilter = explicitContentFilter,
                AfkChannelId = afkChannelId,
                AfkTimeout = afkTimeout,
                IconBase64 = iconb64,
                SplashBase64 = splashb64,
                OwnerId = ownerId,
                SystemChannelId = systemChannelId,
                Banner = banner,
                Description = description,
                DiscoverySplash = discorverySplash,
                Features = features,
                PreferredLocale = preferredLocale,
                PublicUpdatesChannelId = publicUpdatesChannelId,
                RulesChannelId = rulesChannelId,
                SystemChannelFlags = systemChannelFlags
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guildId }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();
            foreach (var r in guild._roles.Values)
                r._guild_id = guild.Id;

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildUpdateEventAsync(guild, rawMembers).ConfigureAwait(false);
            return guild;
        }

        internal async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var bans_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordBan>>(res.Response).Select(xb =>
            {
                if (!this.Discord.TryGetCachedUserInternal(xb.RawUser.Id, out var usr))
                {
                    usr = new DiscordUser(xb.RawUser) { Discord = this.Discord };
                    usr = this.Discord.UpdateUserCache(usr);
                }

                xb.User = usr;
                return xb;
            });
            var bans = new ReadOnlyCollection<DiscordBan>(new List<DiscordBan>(bans_raw));

            return bans;
        }

        internal Task CreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason)
        {
            if (delete_message_days < 0 || delete_message_days > 7)
                throw new ArgumentException("Delete message days must be a number between 0 and 7.", nameof(delete_message_days));

            var urlparams = new Dictionary<string, string>
            {
                ["delete_message_days"] = delete_message_days.ToString(CultureInfo.InvariantCulture)
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
        }

        internal Task RemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal Task LeaveGuildAsync(ulong guild_id)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        internal async Task<DiscordMember> AddGuildMemberAsync(ulong guild_id, ulong user_id, string access_token, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
        {
            var pld = new RestGuildMemberAddPayload
            {
                AccessToken = access_token,
                Nickname = nick ?? "",
                Roles = roles ?? new List<DiscordRole>(),
                Deaf = deafened,
                Mute = muted
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm) { Discord = this.Discord, _guild_id = guild_id };
        }

        internal async Task<IReadOnlyList<TransportMember>> ListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
        {
            var urlparams = new Dictionary<string, string>();
            if (limit != null && limit > 0)
                urlparams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
            if (after != null)
                urlparams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var members_raw = JsonConvert.DeserializeObject<List<TransportMember>>(res.Response);
            return new ReadOnlyCollection<TransportMember>(members_raw);
        }

        internal Task AddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
        }

        internal Task RemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal Task ModifyGuildChannelPositionAsync(ulong guild_id, IEnumerable<RestGuildChannelReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        internal Task ModifyGuildRolePositionAsync(ulong guild_id, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        internal async Task<AuditLog> GetAuditLogsAsync(ulong guild_id, int limit, ulong? after, ulong? before, ulong? responsible, int? action_type)
        {
            var urlparams = new Dictionary<string, string>
            {
                ["limit"] = limit.ToString(CultureInfo.InvariantCulture)
            };
            if (after != null)
                urlparams["after"] = after?.ToString(CultureInfo.InvariantCulture);
            if (before != null)
                urlparams["before"] = before?.ToString(CultureInfo.InvariantCulture);
            if (responsible != null)
                urlparams["user_id"] = responsible?.ToString(CultureInfo.InvariantCulture);
            if (action_type != null)
                urlparams["action_type"] = action_type?.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.AUDIT_LOGS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var audit_log_data_raw = JsonConvert.DeserializeObject<AuditLog>(res.Response);

            return audit_log_data_raw;
        }

        internal async Task<DiscordInvite> GetGuildVanityUrlAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VANITY_URL}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invite = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);

            return invite;
        }

        internal async Task<DiscordWidget> GetGuildWidgetAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET_JSON}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawChannels = (JArray)json["channels"];

            var ret = json.ToDiscordObject<DiscordWidget>();
            ret.Discord = this.Discord;
            ret.Guild = this.Discord.Guilds[guild_id];

            ret.Channels = ret.Guild == null
                ? rawChannels.Select(r => new DiscordChannel
                {
                    Id = (ulong)r["id"],
                    Name = r["name"].ToString(),
                    Position = (int)r["position"]
                }).ToList()
                : rawChannels.Select(r =>
                {
                    var c = ret.Guild.GetChannel((ulong)r["id"]);
                    c.Position = (int)r["position"];
                    return c;
                }).ToList();

            return ret;
        }

        internal async Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[guild_id];

            return ret;
        }

        internal async Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guild_id, bool? isEnabled, ulong? channelId, string reason)
        {
            var pld = new RestGuildWidgetSettingsPayload
            {
                Enabled = isEnabled,
                ChannelId = channelId
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[guild_id];

            return ret;
        }

        internal async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var templates_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response);

            return new ReadOnlyCollection<DiscordGuildTemplate>(new List<DiscordGuildTemplate>(templates_raw));
        }

        internal async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guild_id, string name, string description)
        {
            var pld = new RestGuildTemplateCreateOrModifyPayload
            {
                Name = name,
                Description = description
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return ret;
        }

        internal async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guild_id, string template_code)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        internal async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guild_id, string template_code, string name, string description)
        {
            var pld = new RestGuildTemplateCreateOrModifyPayload
            {
                Name = name,
                Description = description
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        internal async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guild_id, string template_code)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        internal async Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var screening_raw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

            return screening_raw;
        }

        internal async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guild_id, Optional<bool> enabled, Optional<DiscordGuildMembershipScreeningField[]> fields, Optional<string> description)
        {
            var pld = new RestGuildMembershipScreeningFormModifyPayload
            {
                Enabled = enabled,
                Description = description,
                Fields = fields
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var screening_raw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

            return screening_raw;
        }

        internal async Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        internal async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guild_id, Optional<bool> enabled, Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> welcomeChannels, Optional<string> description, string reason)
        {
            var pld = new RestGuildWelcomeScreenModifyPayload
            {
                Enabled = enabled,
                WelcomeChannels = welcomeChannels,
                Description = description
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        internal async Task UpdateCurrentUserVoiceStateAsync(ulong guild_id, ulong channelId, bool? suppress, DateTimeOffset? requestToSpeakTimestamp)
        {
            var pld = new RestGuildUpdateCurrentUserVoiceStatePayload
            {
                ChannelId = channelId,
                Suppress = suppress,
                RequestToSpeakTimestamp = requestToSpeakTimestamp
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/@me";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
        }

        internal async Task UpdateUserVoiceStateAsync(ulong guild_id, ulong user_id, ulong channelId, bool? suppress)
        {
            var pld = new RestGuildUpdateUserVoiceStatePayload
            {
                ChannelId = channelId,
                Suppress = suppress
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
        }
        #endregion

        #region Stickers

        internal async Task<DiscordMessageSticker> GetGuildStickerAsync(ulong guild_id, ulong sticker_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id, sticker_id}, out var path);
            var url = Utilities.GetApiUriFor(path);

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JObject.Parse(res.Response);
            var ret = json.ToDiscordObject<DiscordMessageSticker>();

            if (json["user"] is JObject jusr) // Null = Missing stickers perm //
            {
                var tsr = jusr.ToDiscordObject<TransportUser>();
                var usr = new DiscordUser(tsr) {Discord = this.Discord};
                ret.User = usr;
            }

            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordMessageSticker> GetStickerAsync(ulong sticker_id)
        {
            var route = $"{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {sticker_id}, out var path);
            var url = Utilities.GetApiUriFor(path);

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JObject.Parse(res.Response);
            var ret = json.ToDiscordObject<DiscordMessageSticker>();

            if (json["user"] is JObject jusr) // Null = Missing stickers perm //
            {
                var tsr = jusr.ToDiscordObject<TransportUser>();
                var usr = new DiscordUser(tsr) {Discord = this.Discord};
                ret.User = usr;
            }

            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<IReadOnlyList<DiscordMessageStickerPack>> GetStickerPacksAsync()
        {
            var route = $"{Endpoints.STICKERPACKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response)["sticker_packs"] as JArray;
            var ret = json.ToDiscordObject<DiscordMessageStickerPack[]>();

            return ret;
        }

        internal async Task<IReadOnlyList<DiscordMessageSticker>> GetGuildStickersAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id}, out var path);
            var url = Utilities.GetApiUriFor(path);

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JArray.Parse(res.Response);
            var ret = json.ToDiscordObject<DiscordMessageSticker[]>();


            for (int i = 0; i < ret.Length; i++)
            {
                var stkr = ret[i];
                stkr.Discord = this.Discord;

                if (json[i]["user"] is JObject jusr) // Null = Missing stickers perm //
                {
                    var tsr = jusr.ToDiscordObject<TransportUser>();
                    var usr = new DiscordUser(tsr) {Discord = this.Discord};
                    stkr.User = usr; // The sticker would've already populated, but this is just to ensure everything is up to date //
                }
            }

            return ret;
        }

        internal async Task<DiscordMessageSticker> CreateGuildStickerAsync(ulong guild_id, string name, string description, string tags, DiscordMessageFile file, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id}, out var path);
            var url = Utilities.GetApiUriFor(path);

            if (!string.IsNullOrEmpty(reason))
                headers[REASON_HEADER_NAME] = reason;

            var values = new Dictionary<string, string>
            {
                ["name"] = name,
                ["description"] = description,
                ["tags"] = tags,
            };

            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, values, new[] {file}, removeFileCount: true).ConfigureAwait(false);
            var json = JObject.Parse(res.Response);
            var ret = json.ToDiscordObject<DiscordMessageSticker>();

            if (json["user"] is JObject jusr) // Null = Missing stickers perm //
            {
                var tsr = jusr.ToDiscordObject<TransportUser>();
                var usr = new DiscordUser(tsr) {Discord = this.Discord};
                ret.User = usr;
            }

            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMessageSticker> ModifyStickerAsync(ulong guild_id, ulong sticker_id, Optional<string> name, Optional<string> description, Optional<string> tags, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id, sticker_id}, out var path);
            var url = Utilities.GetApiUriFor(path);

            var pld = new RestStickerModifyPayload()
            {
                Name = name,
                Description = description,
                Tags = tags
            };

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers: headers, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
            var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordMessageSticker>();
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task DeleteStickerAsync(ulong guild_id, ulong sticker_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id, sticker_id}, out var path);
            var url = Utilities.GetApiUriFor(path);

            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers: headers).ConfigureAwait(false);
        }

        #endregion

        #region Channel
        internal async Task<DiscordChannel> CreateGuildChannelAsync(ulong guild_id, string name, ChannelType type, ulong? parent, Optional<string> topic, int? bitrate, int? user_limit, IEnumerable<DiscordOverwriteBuilder> overwrites, bool? nsfw, Optional<int?> perUserRateLimit, VideoQualityMode? qualityMode, string reason)
        {
            var restoverwrites = new List<DiscordRestOverwrite>();
            if (overwrites != null)
                foreach (var ow in overwrites)
                    restoverwrites.Add(ow.Build());

            var pld = new RestChannelCreatePayload
            {
                Name = name,
                Type = type,
                Parent = parent,
                Topic = topic,
                Bitrate = bitrate,
                UserLimit = user_limit,
                PermissionOverwrites = restoverwrites,
                Nsfw = nsfw,
                PerUserRateLimit = perUserRateLimit,
                QualityMode = qualityMode
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;
            foreach (var xo in ret._permissionOverwrites)
            {
                xo.Discord = this.Discord;
                xo._channel_id = ret.Id;
            }

            return ret;
        }

        internal Task ModifyChannelAsync(ulong channel_id, string name, int? position, Optional<string> topic, bool? nsfw, Optional<ulong?> parent, int? bitrate, int? user_limit, Optional<int?> perUserRateLimit, Optional<string> rtcRegion, VideoQualityMode? qualityMode, Optional<ChannelType> type, IEnumerable<DiscordOverwriteBuilder> permissionOverwrites, string reason)
        {
            List<DiscordRestOverwrite> restoverwrites = null;
            if (permissionOverwrites != null)
            {
                restoverwrites = new List<DiscordRestOverwrite>();
                foreach (var ow in permissionOverwrites)
                    restoverwrites.Add(ow.Build());
            }

            var pld = new RestChannelModifyPayload
            {
                Name = name,
                Position = position,
                Topic = topic,
                Nsfw = nsfw,
                Parent = parent,
                Bitrate = bitrate,
                UserLimit = user_limit,
                PerUserRateLimit = perUserRateLimit,
                RtcRegion = rtcRegion,
                QualityMode = qualityMode,
                Type = type,
                PermissionOverwrites = restoverwrites
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        internal Task ModifyThreadChannelAsync(ulong channel_id, string name, int? position, Optional<string> topic, bool? nsfw, Optional<ulong?> parent, int? bitrate, int? user_limit, Optional<int?> perUserRateLimit, Optional<string> rtcRegion, VideoQualityMode? qualityMode, Optional<ChannelType> type, IEnumerable<DiscordOverwriteBuilder> permissionOverwrites, bool? isArchived, AutoArchiveDuration? autoArchiveDuration, bool? locked, string reason)
        {
            List<DiscordRestOverwrite> restoverwrites = null;
            if (permissionOverwrites != null)
            {
                restoverwrites = new List<DiscordRestOverwrite>();
                foreach (var ow in permissionOverwrites)
                    restoverwrites.Add(ow.Build());
            }

            var pld = new RestThreadChannelModifyPayload
            {
                Name = name,
                Position = position,
                Topic = topic,
                Nsfw = nsfw,
                Parent = parent,
                Bitrate = bitrate,
                UserLimit = user_limit,
                PerUserRateLimit = perUserRateLimit,
                RtcRegion = rtcRegion,
                QualityMode = qualityMode,
                Type = type,
                PermissionOverwrites = restoverwrites,
                IsArchived = isArchived,
                ArchiveDuration = autoArchiveDuration,
                Locked = locked
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        internal async Task<IReadOnlyList<DiscordScheduledGuildEvent>> GetScheduledGuildEventsAsync(ulong guild_id, bool with_user_counts = false)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EVENTS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var query = new Dictionary<string, string>() { { "with_user_count", with_user_counts.ToString() } };

            var url = Utilities.GetApiUriFor(path, BuildQueryString(query));

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, new Dictionary<string, string>(), string.Empty).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordScheduledGuildEvent>>(res.Response).ToList();

            foreach (var xe in ret)
            {
                xe.Discord = this.Discord;

                if (xe.Creator != null)
                    xe.Creator.Discord = this.Discord;
            }

            return ret.AsReadOnly();
        }

        internal async Task<DiscordScheduledGuildEvent> CreateScheduledGuildEventAsync(ulong guild_id, string name, string description, ulong? channel_id, DateTimeOffset start_time, DateTimeOffset? end_time, ScheduledGuildEventType type, ScheduledGuildEventPrivacyLevel privacy_level, DiscordScheduledGuildEventMetadata metadata, string reason = null)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EVENTS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var headers = Utilities.GetBaseHeaders();

            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = Utilities.GetApiUriFor(path);

            var pld = new RestScheduledGuildEventCreatePayload
            {
                Name = name,
                Description = description,
                ChannelId = channel_id,
                StartTime = start_time,
                EndTime = end_time,
                Type = type,
                PrivacyLevel = privacy_level,
                Metadata = metadata
            };

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordScheduledGuildEvent>(res.Response);

            ret.Discord = this.Discord;

            if (ret.Creator != null)
                ret.Creator.Discord = this.Discord;

            return ret;
        }

        internal async Task DeleteScheduledGuildEventAsync(ulong guild_id, ulong guild_scheduled_event_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EVENTS}/:guild_scheduled_event_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, guild_scheduled_event_id }, out var path);

            var headers = Utilities.GetBaseHeaders();

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, null).ConfigureAwait(false);
        }

        internal async Task<IReadOnlyList<DiscordUser>> GetScheduledGuildEventUsersAsync(ulong guild_id, ulong guild_scheduled_event_id, bool with_members = false, int limit = 1, ulong? before = null, ulong? after = null)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EVENTS}/:guild_scheduled_event_id{Endpoints.USERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, guild_scheduled_event_id }, out var path);

            var query = new Dictionary<string, string>() { { "with_members", with_members.ToString() } };

            if (limit > 0)
                query.Add("limit", limit.ToString(CultureInfo.InvariantCulture));

            if (before != null)
                query.Add("before", before.Value.ToString(CultureInfo.InvariantCulture));

            if (after != null)
                query.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));

            var url = Utilities.GetApiUriFor(path, BuildQueryString(query));

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, new Dictionary<string, string>(), string.Empty).ConfigureAwait(false);

            var jto = JToken.Parse(res.Response);

            return (jto as JArray ?? jto["users"] as JArray)
                .Select(j => (DiscordUser)
                        j
                        .SelectToken("member")?
                        .ToDiscordObject<DiscordMember>() ??
                        j
                        .SelectToken("user")
                        .ToDiscordObject<DiscordUser>())
                .ToArray();
        }

        internal async Task<DiscordScheduledGuildEvent> GetScheduledGuildEventAsync(ulong guild_id, ulong guild_scheduled_event_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EVENTS}/:guild_scheduled_event_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, guild_scheduled_event_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, Utilities.GetBaseHeaders(), string.Empty).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordScheduledGuildEvent>(res.Response);

            ret.Discord = this.Discord;

            if (ret.Creator != null)
                ret.Creator.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordScheduledGuildEvent> ModifyScheduledGuildEventAsync(ulong guild_id, ulong guild_scheduled_event_id, Optional<string> name, Optional<string> description, Optional<ulong?> channel_id, Optional<DateTimeOffset> start_time, Optional<DateTimeOffset> end_time, Optional<ScheduledGuildEventType> type, Optional<ScheduledGuildEventPrivacyLevel> privacy_level, Optional<DiscordScheduledGuildEventMetadata> metadata, Optional<ScheduledGuildEventStatus> status, string reason = null)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EVENTS}/:guild_scheduled_event_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, guild_scheduled_event_id }, out var path);

            var headers = Utilities.GetBaseHeaders();

            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var url = Utilities.GetApiUriFor(path);
            var pld = new RestScheduledGuildEventModifyPayload
            {
                Name = name,
                Description = description,
                ChannelId = channel_id,
                StartTime = start_time,
                EndTime = end_time,
                Type = type,
                PrivacyLevel = privacy_level,
                Metadata = metadata,
                Status = status
            };

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordScheduledGuildEvent>(res.Response);

            ret.Discord = this.Discord;

            if (ret.Creator != null)
                ret.Creator.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordChannel> GetChannelAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);

            if (ret.IsThread)
                ret = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);

            ret.Discord = this.Discord;
            foreach (var xo in ret._permissionOverwrites)
            {
                xo.Discord = this.Discord;
                xo._channel_id = ret.Id;
            }

            return ret;
        }

        internal Task DeleteChannelAsync(ulong channel_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal async Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content, IEnumerable<DiscordEmbed> embeds, ulong? replyMessageId, bool mentionReply, bool failOnInvalidReply)
        {
            if (content != null && content.Length > 2000)
                throw new ArgumentException("Message content length cannot exceed 2000 characters.");

            if (!embeds?.Any() ?? true)
            {
                if (content == null)
                    throw new ArgumentException("You must specify message content or an embed.");

                if (content.Length == 0)
                    throw new ArgumentException("Message content must not be empty.");
            }

            if (embeds != null)
                foreach (var embed in embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = content != null,
                Content = content,
                IsTTS = false,
                HasEmbed = embeds?.Any() ?? false,
                Embeds = embeds
            };

            if (replyMessageId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = replyMessageId, FailIfNotExists = failOnInvalidReply };

            if (replyMessageId != null)
                pld.Mentions = new DiscordMentions(Mentions.All, true, mentionReply);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, DiscordMessageBuilder builder)
        {
            builder.Validate();

            if (builder.Embeds != null)
                foreach (var embed in builder.Embeds)
                    if (embed?.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = builder.Content != null,
                Content = builder.Content,
                StickersIds = builder.Sticker is null ? Array.Empty<ulong>() : new[] {builder.Sticker.Id},
                IsTTS = builder.IsTTS,
                HasEmbed = builder.Embeds != null,
                Embeds = builder.Embeds,
                Components = builder.Components
            };

            if (builder.ReplyId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = builder.ReplyId, FailIfNotExists = builder.FailOnInvalidReply };

            //if (builder.Mentions != null || builder.ReplyId != null)
            pld.Mentions = new DiscordMentions(builder.Mentions ?? Mentions.All, builder.Mentions?.Any() ?? false, builder.MentionOnReply);

            if (builder.Files.Count == 0)
            {
                var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
                var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

                var url = Utilities.GetApiUriFor(path);
                var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));
                return ret;
            }
            else
            {
                var values = new Dictionary<string, string>
                {
                    ["payload_json"] = DiscordJson.SerializeObject(pld)
                };

                var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
                var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

                var url = Utilities.GetApiUriFor(path);
                var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));

                foreach (var file in builder._files.Where(x => x.ResetPositionTo.HasValue))
                {
                    file.Stream.Position = file.ResetPositionTo.Value;
                }

                return ret;
            }
        }

        internal async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var channels_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            foreach (var ret in channels_raw)
                foreach (var xo in ret._permissionOverwrites)
                {
                    xo.Discord = this.Discord;
                    xo._channel_id = ret.Id;
                }

            return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channels_raw));
        }

        internal async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
        {
            var urlparams = new Dictionary<string, string>();
            if (around != null)
                urlparams["around"] = around?.ToString(CultureInfo.InvariantCulture);
            if (before != null)
                urlparams["before"] = before?.ToString(CultureInfo.InvariantCulture);
            if (after != null)
                urlparams["after"] = after?.ToString(CultureInfo.InvariantCulture);
            if (limit > 0)
                urlparams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var msgs_raw = JArray.Parse(res.Response);
            var msgs = new List<DiscordMessage>();
            foreach (var xj in msgs_raw)
                msgs.Add(this.PrepareMessage(xj));

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        internal async Task<DiscordMessage> GetChannelMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds, IEnumerable<IMention> mentions, IReadOnlyList<DiscordActionRowComponent> components, IReadOnlyCollection<DiscordMessageFile> files, MessageFlags? flags, IEnumerable<DiscordAttachment> attachments)
        {
            if (embeds.HasValue && embeds.Value != null)
                foreach (var embed in embeds.Value)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageEditPayload
            {
                HasContent = content.HasValue,
                Content = content.HasValue ? (string)content : null,
                HasEmbed = embeds.HasValue && (embeds.Value?.Any() ?? false),
                Embeds = embeds.HasValue && (embeds.Value?.Any() ?? false) ? embeds.Value : null,
                Components = components,
                Flags = flags,
                Attachments = attachments
            };

            pld.Mentions = new DiscordMentions(mentions ?? Mentions.None, false, mentions?.OfType<RepliedUserMention>().Any() ?? false);

            var values = new Dictionary<string, string>
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: files).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            foreach (var file in files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo.Value;
            }

            return ret;
        }

        internal Task DeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal Task DeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
        {
            var pld = new RestChannelMessageBulkDeletePayload
            {
                Messages = message_ids
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}{Endpoints.BULK_DELETE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));
        }

        internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }

        internal async Task<DiscordInvite> CreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, bool temporary, bool unique, string reason, InviteTargetType? targetType, ulong? targetUserId, ulong? targetApplicationId)
        {
            var pld = new RestChannelInviteCreatePayload
            {
                MaxAge = max_age,
                MaxUses = max_uses,
                Temporary = temporary,
                Unique = unique,
                TargetType = targetType,
                TargetUserId = targetUserId,
                TargetApplicationId = targetApplicationId
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, overwrite_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal Task EditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
        {
            var pld = new RestChannelPermissionEditPayload
            {
                Type = type,
                Allow = allow & PermissionMethods.FULL_PERMS,
                Deny = deny & PermissionMethods.FULL_PERMS
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, overwrite_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld));
        }

        internal Task TriggerTypingAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.TYPING}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
        }

        internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var msgs_raw = JArray.Parse(res.Response);
            var msgs = new List<DiscordMessage>();
            foreach (var xj in msgs_raw)
                msgs.Add(this.PrepareMessage(xj));

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        internal Task PinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        internal Task UnpinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        internal Task AddGroupDmRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
        {
            var pld = new RestChannelGroupDmRecipientAddPayload
            {
                AccessToken = access_token,
                Nickname = nickname
            };

            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));
        }

        internal Task RemoveGroupDmRecipientAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        internal async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var pld = new RestUserGroupDmCreatePayload
            {
                AccessTokens = access_tokens,
                Nicknames = nicks
            };

            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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

            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            if (this.Discord is DiscordClient dc)
                _ = dc._privateChannels.TryAdd(ret.Id, ret);

            return ret;
        }

        internal async Task<DiscordFollowedChannel> FollowChannelAsync(ulong channel_id, ulong webhook_channel_id)
        {
            var pld = new FollowedChannelAddPayload
            {
                WebhookChannelId = webhook_channel_id
            };

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.FOLLOWERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<DiscordFollowedChannel>(response.Response);
        }

        internal async Task<DiscordMessage> CrosspostMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.CROSSPOST}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal async Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channelId, string topic, PrivacyLevel? privacyLevel, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestCreateStageInstancePayload
            {
                ChannelId = channelId,
                Topic = topic,
                PrivacyLevel = privacyLevel
            };

            var route = $"{Endpoints.STAGE_INSTANCES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var stage = JsonConvert.DeserializeObject<DiscordStageInstance>(response.Response);
            stage.Discord = this.Discord;

            return stage;
        }

        internal async Task<DiscordStageInstance> GetStageInstanceAsync(ulong channel_id)
        {
            var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var stage = JsonConvert.DeserializeObject<DiscordStageInstance>(response.Response);
            stage.Discord = this.Discord;

            return stage;
        }

        internal async Task<DiscordStageInstance> ModifyStageInstanceAsync(ulong channel_id, Optional<string> topic, Optional<PrivacyLevel> privacyLevel, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestModifyStageInstancePayload
            {
                Topic = topic,
                PrivacyLevel = privacyLevel
            };

            var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var stage = JsonConvert.DeserializeObject<DiscordStageInstance>(response.Response);
            stage.Discord = this.Discord;

            return stage;
        }

        internal async Task DeleteStageInstanceAsync(ulong channel_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);
        }

        #endregion

        #region Threads

        internal async Task<DiscordThreadChannel> CreateThreadFromMessageAsync(ulong channel_id, ulong message_id, string name, AutoArchiveDuration archiveAfter, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var payload = new RestThreadCreatePayload
            {
                Name = name,
                ArchiveAfter = archiveAfter
            };

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.THREADS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, message_id }, out var path); //???

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(payload)).ConfigureAwait(false);

            var thread = JsonConvert.DeserializeObject<DiscordThreadChannel>(response.Response);
            thread.Discord = this.Discord;

            return thread;
        }

        internal async Task<DiscordThreadChannel> CreateThreadAsync(ulong channel_id, string name, AutoArchiveDuration archiveAfter, ChannelType type, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var payload = new RestThreadCreatePayload
            {
                Name = name,
                ArchiveAfter = archiveAfter,
                Type = type
            };

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id}, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(payload)).ConfigureAwait(false);

            var thread = JsonConvert.DeserializeObject<DiscordThreadChannel>(response.Response);
            thread.Discord = this.Discord;

            return thread;
        }

        internal Task JoinThreadAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id}, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        internal Task LeaveThreadAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id}, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        internal async Task<DiscordThreadChannelMember> GetThreadMemberAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<DiscordThreadChannelMember>(response.Response);
        }

        internal Task AddThreadMemberAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, user_id}, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        internal Task RemoveThreadMemberAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, user_id}, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        internal async Task<IReadOnlyList<DiscordThreadChannelMember>> ListThreadMembersAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id}, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var threadMembers = JsonConvert.DeserializeObject<List<DiscordThreadChannelMember>>(response.Response);
            return new ReadOnlyCollection<DiscordThreadChannelMember>(threadMembers);
        }

        internal async Task<ThreadQueryResult> ListActiveThreadsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.THREADS}{Endpoints.ACTIVE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id}, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response);
            result.HasMore = false;

            foreach (var thread in result.Threads)
                thread.Discord = this.Discord;
            foreach (var member in result.Members)
            {
                member.Discord = this.Discord;
                member._guild_id = guild_id;
                var thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
                if (thread != null)
                    thread.CurrentMember = member;
            }

            return result;
        }

        internal async Task<ThreadQueryResult> ListPublicArchivedThreadsAsync(ulong guild_id, ulong channel_id, ulong? before, int limit)
        {
            var queryParams = new Dictionary<string, string>();
            if (before != null)
                queryParams["before"] = before?.ToString(CultureInfo.InvariantCulture);
            if (limit > 0)
                queryParams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.ARCHIVED}{Endpoints.PUBLIC}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id}, out var path);

            var url = Utilities.GetApiUriFor(path, queryParams.Any() ? BuildQueryString(queryParams) : "");
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response);

            foreach (var thread in result.Threads)
                thread.Discord = this.Discord;
            foreach (var member in result.Members)
            {
                member.Discord = this.Discord;
                member._guild_id = guild_id;
                var thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
                if (thread != null)
                    thread.CurrentMember = member;
            }

            return result;
        }

        internal async Task<ThreadQueryResult> ListPrivateArchivedThreadsAsync(ulong guild_id, ulong channel_id, ulong? before, int limit)
        {
            var queryParams = new Dictionary<string, string>();
            if (before != null)
                queryParams["before"] = before?.ToString(CultureInfo.InvariantCulture);
            if (limit > 0)
                queryParams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.ARCHIVED}{Endpoints.PRIVATE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id}, out var path);

            var url = Utilities.GetApiUriFor(path, queryParams.Any() ? BuildQueryString(queryParams) : "");
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response);

            foreach (var thread in result.Threads)
                thread.Discord = this.Discord;
            foreach (var member in result.Members)
            {
                member.Discord = this.Discord;
                member._guild_id = guild_id;
                var thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
                if (thread != null)
                    thread.CurrentMember = member;
            }

            return result;
        }

        internal async Task<ThreadQueryResult> ListJoinedPrivateArchivedThreadsAsync(ulong guild_id, ulong channel_id, ulong? before, int limit)
        {
            var queryParams = new Dictionary<string, string>();
            if (before != null)
                queryParams["before"] = before?.ToString(CultureInfo.InvariantCulture);
            if (limit > 0)
                queryParams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.USERS}{Endpoints.ME}{Endpoints.THREADS}{Endpoints.ARCHIVED}{Endpoints.PUBLIC}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id}, out var path);

            var url = Utilities.GetApiUriFor(path, queryParams.Any() ? BuildQueryString(queryParams) : "");
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response);

            foreach (var thread in result.Threads)
                thread.Discord = this.Discord;
            foreach (var member in result.Members)
            {
                member.Discord = this.Discord;
                member._guild_id = guild_id;
                var thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
                if (thread != null)
                    thread.CurrentMember = member;
            }

            return result;
        }

        #endregion

        #region Member
        internal Task<DiscordUser> GetCurrentUserAsync()
            => this.GetUserAsync("@me");

        internal Task<DiscordUser> GetUserAsync(ulong user_id)
            => this.GetUserAsync(user_id.ToString(CultureInfo.InvariantCulture));

        internal async Task<DiscordUser> GetUserAsync(string user_id)
        {
            var route = $"{Endpoints.USERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var duser = new DiscordUser(user_raw) { Discord = this.Discord };

            return duser;
        }

        internal async Task<DiscordMember> GetGuildMemberAsync(ulong guild_id, ulong user_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            var usr = new DiscordUser(tm.User) { Discord = this.Discord };
            usr = this.Discord.UpdateUserCache(usr);

            return new DiscordMember(tm)
            {
                Discord = this.Discord,
                _guild_id = guild_id
            };
        }

        internal Task RemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason)
        {
            var urlparams = new Dictionary<string, string>();
            if (reason != null)
                urlparams["reason"] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        internal async Task<TransportUser> ModifyCurrentUserAsync(string username, Optional<string> base64_avatar)
        {
            var pld = new RestUserUpdateCurrentPayload
            {
                Username = username,
                AvatarBase64 = base64_avatar.HasValue ? base64_avatar.Value : null,
                AvatarSet = base64_avatar.HasValue
            };

            var route = $"{Endpoints.USERS}{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);

            return user_raw;
        }

        internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}";

            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path)
                .AddParameter($"limit", limit.ToString(CultureInfo.InvariantCulture));

            if (before != null)
                url.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
            if (after != null)
                url.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));

            var res = await this.DoRequestAsync(this.Discord, bucket, url.Build(), RestRequestMethod.GET, route).ConfigureAwait(false);

            if (this.Discord is DiscordClient)
            {
                var guilds_raw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response);
                var glds = guilds_raw.Select(xug => (this.Discord as DiscordClient)?._guilds[xug.Id]);
                return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(glds));
            }
            else
            {
                return new ReadOnlyCollection<DiscordGuild>(JsonConvert.DeserializeObject<List<DiscordGuild>>(res.Response));
            }
        }

        internal Task ModifyGuildMemberAsync(ulong guild_id, ulong user_id, Optional<string> nick,
            Optional<IEnumerable<ulong>> role_ids, Optional<bool> mute, Optional<bool> deaf,
            Optional<ulong?> voice_channel_id, Optional<DateTimeOffset?> communication_disabled_until, string reason)
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
                VoiceChannelId = voice_channel_id,
                CommunicationDisabledUntil = communication_disabled_until
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
        }

        internal Task ModifyCurrentMemberAsync(ulong guild_id, string nick, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
        }
        #endregion

        #region Roles
        internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var roles_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = this.Discord; xr._guild_id = guild_id; return xr; });

            return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(roles_raw));
        }

        internal async Task<DiscordGuild> GetGuildAsync(ulong guildId, bool? with_counts)
        {
            var urlparams = new Dictionary<string, string>();
            if (with_counts.HasValue)
                urlparams["with_counts"] = with_counts?.ToString();

            var route = $"{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guildId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, urlparams).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guildRest = json.ToDiscordObject<DiscordGuild>();
            foreach (var r in guildRest._roles.Values)
                r._guild_id = guildRest.Id;

            if (this.Discord is DiscordClient dc)
            {
                await dc.OnGuildUpdateEventAsync(guildRest, rawMembers).ConfigureAwait(false);
                return dc._guilds[guildRest.Id];
            }
            else
            {
                guildRest.Discord = this.Discord;
                return guildRest;
            }
        }

        internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason, Stream icon, string emoji)
        {
            string image = null;

            if (icon != null)
            {
                using var it = new ImageTool(icon);
                image = it.GetBase64();
            }

            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions & PermissionMethods.FULL_PERMS,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable,
                Emoji = emoji,
                Icon = image
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;
            ret._guild_id = guild_id;

            return ret;
        }

        internal Task DeleteRoleAsync(ulong guild_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal async Task<DiscordRole> CreateGuildRoleAsync(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason, Stream icon, string emoji)
        {
            string image = null;

            if (icon != null)
            {
                using var it = new ImageTool(icon);
                image = it.GetBase64();
            }

            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions & PermissionMethods.FULL_PERMS,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable,
                Emoji = emoji,
                Icon = image
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;
            ret._guild_id = guild_id;

            return ret;
        }
        #endregion

        #region Prune
        internal async Task<int> GetGuildPruneCountAsync(ulong guild_id, int days, IEnumerable<ulong> include_roles)
        {
            if (days < 0 || days > 30)
                throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));

            var urlparams = new Dictionary<string, string>
            {
                ["days"] = days.ToString(CultureInfo.InvariantCulture)
            };

            var sb = new StringBuilder();

            if (include_roles != null)
            {
                var roleArray = include_roles.ToArray();
                var roleArrayCount = roleArray.Count();

                for (var i = 0; i < roleArrayCount; i++)
                    sb.Append($"&include_roles={roleArray[i]}");
            }

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PRUNE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);
            var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlparams)}{sb}");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned.Value;
        }

        internal async Task<int?> BeginGuildPruneAsync(ulong guild_id, int days, bool compute_prune_count, IEnumerable<ulong> include_roles, string reason)
        {
            if (days < 0 || days > 30)
                throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));

            var urlparams = new Dictionary<string, string>
            {
                ["days"] = days.ToString(CultureInfo.InvariantCulture),
                ["compute_prune_count"] = compute_prune_count.ToString()
            };

            var sb = new StringBuilder();

            if (include_roles != null)
            {
                var roleArray = include_roles.ToArray();
                var roleArrayCount = roleArray.Count();

                for (var i = 0; i < roleArrayCount; i++)
                    sb.Append($"&include_roles={roleArray[i]}");
            }

            var headers = Utilities.GetBaseHeaders();
            if (string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PRUNE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlparams)}{sb}");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers).ConfigureAwait(false);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        #endregion

        #region GuildVarious
        internal async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
        {
            var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var templates_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return templates_raw;
        }

        internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var integrations_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrations_raw));
        }

        internal async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PREVIEW}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildPreview>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guild_id, string type, ulong id)
        {
            var pld = new RestGuildIntegrationAttachPayload
            {
                Type = type,
                Id = id
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, integration_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal Task DeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, integration_id = integration.Id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, DiscordJson.SerializeObject(integration));
        }

        internal Task SyncGuildIntegrationAsync(ulong guild_id, ulong integration_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id{Endpoints.SYNC}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id, integration_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
        }

        internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REGIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var regions_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions_raw));
        }

        internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }
        #endregion

        #region Invite
        internal async Task<DiscordInvite> GetInviteAsync(string invite_code, bool? with_counts, bool? with_expiration)
        {
            var urlparams = new Dictionary<string, string>();
            if (with_counts.HasValue)
            {
                urlparams["with_counts"] = with_counts?.ToString();
                urlparams["with_expiration"] = with_expiration?.ToString();
            }

            var route = $"{Endpoints.INVITES}/:invite_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { invite_code }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordInvite> DeleteInviteAsync(string invite_code, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.INVITES}/:invite_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { invite_code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);

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
         *     var url = new Uri($"{Utils.GetApiBaseUri(), Endpoints.INVITES}/{invite_code));
         *     var bucket = this.Rest.GetBucket(0, MajorParameterType.Unbucketed, url, HttpRequestMethod.POST);
         *     var res = await this.DoRequestAsync(this.Discord, bucket, url, HttpRequestMethod.POST).ConfigureAwait(false);
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
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CONNECTIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var connections_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connections_raw));
        }
        #endregion

        #region Voice
        internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        {
            var route = $"{Endpoints.VOICE}{Endpoints.REGIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
        }
        #endregion

        #region Webhooks
        internal async Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, Optional<string> base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar.HasValue ? base64_avatar.Value : null,
                AvatarSet = base64_avatar.HasValue
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhook_id)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        // Auth header not required
        internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhook_id, string webhook_token)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Token = webhook_token;
            ret.Id = webhook_id;
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        internal async Task<DiscordMessage> GetWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id, webhook_token, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, ulong channelId, string name, Optional<string> base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar.HasValue ? base64_avatar.Value : null,
                AvatarSet = base64_avatar.HasValue,
                ChannelId = channelId
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

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

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        internal Task DeleteWebhookAsync(ulong webhook_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal Task DeleteWebhookAsync(ulong webhook_id, string webhook_token, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        internal async Task<DiscordMessage> ExecuteWebhookAsync(ulong webhook_id, string webhook_token, DiscordWebhookBuilder builder)
        {
            builder.Validate();

            if (builder.Embeds != null)
                foreach (var embed in builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestWebhookExecutePayload
            {
                Content = builder.Content,
                Username = builder.Username.HasValue ? builder.Username.Value : null,
                AvatarUrl = builder.AvatarUrl.HasValue ? builder.AvatarUrl.Value : null,
                IsTTS = builder.IsTTS,
                Embeds = builder.Embeds,
                Components = builder.Components,
            };

            if (builder.Mentions != null)
                pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

            if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count() > 0 || builder.IsTTS == true || builder.Mentions != null)
                values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var url = builder.ThreadId == null
                ? Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build()
                : Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").AddParameter("thread_id", builder.ThreadId.ToString()).Build();

            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

            foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo.Value;
            }

            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordMessage> ExecuteWebhookSlackAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.SLACK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: json_payload).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordMessage> ExecuteWebhookGithubAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.GITHUB}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: json_payload).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhook_id, string webhook_token, string message_id, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments)
        {
            builder.Validate(true);

            var pld = new RestWebhookMessageEditPayload
            {
                Content = builder.Content,
                Embeds = builder.Embeds,
                Mentions = builder.Mentions,
                Components = builder.Components,
                Attachments = attachments
            };

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id, webhook_token, message_id }, out var path);

            var values = new Dictionary<string, string>
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: builder.Files).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
                file.Stream.Position = file.ResetPositionTo.Value;

            return ret;
        }

        internal Task<DiscordMessage> EditWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments) =>
            this.EditWebhookMessageAsync(webhook_id, webhook_token, message_id.ToString(), builder, attachments);

        internal async Task DeleteWebhookMessageAsync(ulong webhook_id, string webhook_token, string message_id)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id, webhook_token, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
        }
        internal Task DeleteWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id) =>
            this.DeleteWebhookMessageAsync(webhook_id, webhook_token, message_id.ToString());
        #endregion

        #region Reactions
        internal Task CreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        internal Task DeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        internal Task DeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        internal async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, string emoji, ulong? after_id = null, int limit = 25)
        {
            var urlparams = new Dictionary<string, string>();
            if (after_id.HasValue)
                urlparams["after"] = after_id.Value.ToString(CultureInfo.InvariantCulture);

            urlparams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams));
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var reacters_raw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response);
            var reacters = new List<DiscordUser>();
            foreach (var xr in reacters_raw)
            {
                var usr = new DiscordUser(xr) { Discord = this.Discord };
                usr = this.Discord.UpdateUserCache(usr);

                reacters.Add(usr);
            }

            return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters));
        }

        internal Task DeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        internal Task DeleteReactionsEmojiAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }
        #endregion

        #region Emoji
        internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);
            var users = new Dictionary<ulong, DiscordUser>();
            var emojis = new List<DiscordGuildEmoji>();
            foreach (var rawEmoji in emojisRaw)
            {
                var xge = rawEmoji.ToDiscordObject<DiscordGuildEmoji>();
                xge.Guild = gld;

                var xtu = rawEmoji["user"]?.ToDiscordObject<TransportUser>();
                if (xtu != null)
                {
                    if (!users.ContainsKey(xtu.Id))
                    {
                        var user = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);
                        users[user.Id] = user;
                    }

                    xge.User = users[xtu.Id];
                }

                emojis.Add(xge);
            }

            return new ReadOnlyCollection<DiscordGuildEmoji>(emojis);
        }

        internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guild_id, ulong emoji_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, emoji_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);

            var emoji_raw = JObject.Parse(res.Response);
            var emoji = emoji_raw.ToDiscordObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToDiscordObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guild_id, string name, string imageb64, IEnumerable<ulong> roles, string reason)
        {
            var pld = new RestGuildEmojiCreatePayload
            {
                Name = name,
                ImageB64 = imageb64,
                Roles = roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);

            var emoji_raw = JObject.Parse(res.Response);
            var emoji = emoji_raw.ToDiscordObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToDiscordObject<TransportUser>();
            emoji.User = xtu != null
                ? gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu)
                : this.Discord.CurrentUser;

            return emoji;
        }

        internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guild_id, ulong emoji_id, string name, IEnumerable<ulong> roles, string reason)
        {
            var pld = new RestGuildEmojiModifyPayload
            {
                Name = name,
                Roles = roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, emoji_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);

            var emoji_raw = JObject.Parse(res.Response);
            var emoji = emoji_raw.ToDiscordObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToDiscordObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        internal Task DeleteGuildEmojiAsync(ulong guild_id, ulong emoji_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, emoji_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }
        #endregion

        #region Application Commands
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(ulong application_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ulong application_id, IEnumerable<DiscordApplicationCommand> commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Type = command.Type,
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options,
                    DefaultPermission = command.DefaultPermission,
                    NameLocalizations = command.NameLocalizations,
                    DescriptionLocalizations = command.DescriptionLocalizations
                });
            }

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        internal async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(ulong application_id, DiscordApplicationCommand command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Type = command.Type,
                Name = command.Name,
                Description = command.Description,
                Options = command.Options,
                DefaultPermission = command.DefaultPermission,
                NameLocalizations = command.NameLocalizations,
                DescriptionLocalizations = command.DescriptionLocalizations
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong application_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong application_id, ulong command_id, Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options, Optional<bool?> defaultPermission, IReadOnlyDictionary<string, string> name_localizations = null, IReadOnlyDictionary<string, string> description_localizations = null)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = name,
                Description = description,
                Options = options,
                DefaultPermission = defaultPermission,
                NameLocalizations = name_localizations,
                DescriptionLocalizations = description_localizations
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task DeleteGlobalApplicationCommandAsync(ulong application_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
        }

        internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong application_id, ulong guild_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong application_id, ulong guild_id, IEnumerable<DiscordApplicationCommand> commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Type = command.Type,
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options,
                    DefaultPermission = command.DefaultPermission,
                    NameLocalizations = command.NameLocalizations,
                    DescriptionLocalizations = command.DescriptionLocalizations
                });
            }

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        internal async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong application_id, ulong guild_id, DiscordApplicationCommand command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Type = command.Type,
                Name = command.Name,
                Description = command.Description,
                Options = command.Options,
                DefaultPermission = command.DefaultPermission,
                NameLocalizations = command.NameLocalizations,
                DescriptionLocalizations = command.DescriptionLocalizations
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id, Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options, Optional<bool?> defaultPermission, IReadOnlyDictionary<string, string> name_localizations = null, IReadOnlyDictionary<string, string> description_localizations = null)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = name,
                Description = description,
                Options = options,
                DefaultPermission = defaultPermission,
                NameLocalizations = name_localizations,
                DescriptionLocalizations = description_localizations
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task DeleteGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
        }

        internal async Task CreateInteractionResponseAsync(ulong interaction_id, string interaction_token, InteractionResponseType type, DiscordInteractionResponseBuilder builder)
        {
            if (builder?.Embeds != null)
                foreach (var embed in builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestInteractionResponsePayload
            {
                Type = type,
                Data = builder != null ? new DiscordInteractionApplicationCommandCallbackData
                {
                    Content = builder.Content,
                    Title = builder.Title,
                    CustomId = builder.CustomId,
                    Embeds = builder.Embeds,
                    IsTTS = builder.IsTTS,
                    Mentions = new DiscordMentions(builder.Mentions ?? Mentions.All, builder.Mentions?.Any() ?? false),
                    Flags = builder.IsEphemeral ? MessageFlags.Ephemeral : 0,
                    Components = builder.Components,
                    Choices = builder.Choices
                } : null
            };

            var values = new Dictionary<string, string>();

            if (builder != null)
                if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count() > 0 || builder.IsTTS == true || builder.Mentions != null)
                    values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { interaction_id, interaction_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            if (builder != null)
            {
                await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);

                foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
                {
                    file.Stream.Position = file.ResetPositionTo.Value;
                }
            }
            else
            {
                await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
            }
        }

        internal async Task<DiscordMessage> GetOriginalInteractionResponseAsync(ulong application_id, string interaction_token)
        {
            var route = $"{Endpoints.WEBHOOKS}/:application_id/:interaction_token{Endpoints.MESSAGES}{Endpoints.ORIGINAL}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, interaction_token }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

            ret.Discord = this.Discord;
            return ret;
        }

        internal Task<DiscordMessage> EditOriginalInteractionResponseAsync(ulong application_id, string interaction_token, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments) =>
            this.EditWebhookMessageAsync(application_id, interaction_token, "@original", builder, attachments);

        internal Task DeleteOriginalInteractionResponseAsync(ulong application_id, string interaction_token) =>
            this.DeleteWebhookMessageAsync(application_id, interaction_token, "@original");

        internal async Task<DiscordMessage> CreateFollowupMessageAsync(ulong application_id, string interaction_token, DiscordFollowupMessageBuilder builder)
        {
            builder.Validate();

            if (builder.Embeds != null)
                foreach (var embed in builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestFollowupMessageCreatePayload
            {
                Content = builder.Content,
                IsTTS = builder.IsTTS,
                Embeds = builder.Embeds,
                Flags = builder.Flags,
                Components = builder.Components
            };

            if (builder.Mentions != null)
                pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

            if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count() > 0 || builder.IsTTS == true || builder.Mentions != null)
                values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.WEBHOOKS}/:application_id/:interaction_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id, interaction_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

            foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo.Value;
            }

            ret.Discord = this.Discord;
            return ret;
        }

        internal Task<DiscordMessage> GetFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id) =>
            this.GetWebhookMessageAsync(application_id, interaction_token, message_id);

        internal Task<DiscordMessage> EditFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments) =>
            this.EditWebhookMessageAsync(application_id, interaction_token, message_id, builder, attachments);

        internal Task DeleteFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id) =>
            this.DeleteWebhookMessageAsync(application_id, interaction_token, message_id);

        internal async Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> GetGuildApplicationCommandPermissionsAsync(ulong application_id, ulong guild_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}{Endpoints.PERMISSIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermissions>>(res.Response);

            foreach (var perm in ret)
                perm.Discord = this.Discord;
            return ret.ToList();
        }

        internal async Task<DiscordGuildApplicationCommandPermissions> GetApplicationCommandPermissionsAsync(ulong application_id, ulong guild_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id{Endpoints.PERMISSIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermissions>(res.Response);

            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordGuildApplicationCommandPermissions> EditApplicationCommandPermissionsAsync(ulong application_id, ulong guild_id, ulong command_id, IEnumerable<DiscordApplicationCommandPermission> permissions)
        {
            var pld = new RestEditApplicationCommmandPermissionsPayload
            {
                Permissions = permissions
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id{Endpoints.PERMISSIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermissions>(res.Response);

            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> BatchEditApplicationCommandPermissionsAsync(ulong application_id, ulong guild_id, IEnumerable<DiscordGuildApplicationCommandPermissions> permissions)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}{Endpoints.PERMISSIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(permissions)).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermissions>>(res.Response);

            foreach (var perm in ret)
                perm.Discord = this.Discord;
            return ret.ToList();
        }
        #endregion

        #region Misc
        internal Task<TransportApplication> GetCurrentApplicationInfoAsync()
            => this.GetApplicationInfoAsync("@me");

        internal Task<TransportApplication> GetApplicationInfoAsync(ulong application_id)
            => this.GetApplicationInfoAsync(application_id.ToString(CultureInfo.InvariantCulture));

        private async Task<TransportApplication> GetApplicationInfoAsync(string application_id)
        {
            var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TransportApplication>(res.Response);
        }

        internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
        {
            var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id{Endpoints.ASSETS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id = application.Id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
            foreach (var asset in assets)
            {
                asset.Discord = application.Discord;
                asset.Application = application;
            }

            return new ReadOnlyCollection<DiscordApplicationAsset>(new List<DiscordApplicationAsset>(assets));
        }

        internal async Task<GatewayInfo> GetGatewayInfoAsync()
        {
            var headers = Utilities.GetBaseHeaders();
            var route = Endpoints.GATEWAY;
            if (this.Discord.Configuration.TokenType == TokenType.Bot)
                route += Endpoints.BOT;
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, headers).ConfigureAwait(false);

            var info = JObject.Parse(res.Response).ToDiscordObject<GatewayInfo>();
            info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.ResetAfterInternal);
            return info;
        }
        #endregion

    }
}
