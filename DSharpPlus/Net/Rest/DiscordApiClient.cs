// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net
{
    public sealed class DiscordApiClient
    {
        private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

        /// <summary>
        /// The discord client associated with this ApiClient.
        /// </summary>
        public BaseDiscordClient Discord { get; }
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

        /// <summary>
        /// Builds a query string.
        /// </summary>
        /// <param name="values">Values to use.</param>
        /// <param name="post"></param>
        /// <returns>The built query string.</returns>
        public static string BuildQueryString(IDictionary<string, string> values, bool post = false)
        {
            if (values == null || values.Count == 0)
                return string.Empty;

            var vals_collection = values.Select(xkvp =>
                $"{WebUtility.UrlEncode(xkvp.Key)}={WebUtility.UrlEncode(xkvp.Value)}");
            var vals = string.Join("&", vals_collection);

            return !post ? $"?{vals}" : vals;
        }

        /// <summary>
        /// Prepares a message.
        /// </summary>
        /// <param name="msg_raw">The raw message json object.</param>
        /// <returns>The prepared message.</returns>
        public DiscordMessage PrepareMessage(JToken msg_raw)
        {
            var author = msg_raw["author"].ToObject<TransportUser>();
            var ret = msg_raw.ToDiscordObject<DiscordMessage>();
            ret.Discord = this.Discord;

            this.PopulateMessage(author, ret);

            var referencedMsg = msg_raw["referenced_message"];
            if (ret.MessageType == MessageType.Reply && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
            {
                author = referencedMsg["author"].ToObject<TransportUser>();
                ret.ReferencedMessage.Discord = this.Discord;
                this.PopulateMessage(author, ret.ReferencedMessage);
            }

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

        /// <summary>
        /// Makes a REST request to the discord API.
        /// </summary>
        /// <param name="route">The API route to make a request to. It should start with a /.</param>
        /// <param name="routeParams">The route parameters.</param>
        /// <param name="method">The type of REST request to make.</param>
        /// <param name="headers">The headers to send with the request.</param>
        /// <param name="payload">The payload to send with the request.</param>
        /// <param name="queryString">The query string to add on the url.</param>
        /// <returns>The API response.</returns>
        public Task<RestResponse> DoRequestAsync(string route, object routeParams, RestRequestMethod method, IReadOnlyDictionary<string, string> headers = null, string payload = null, string queryString = null)
        {
            var req = new RestRequest(this.Discord, this.Rest.GetBucket(method, route, routeParams, out var path), Utilities.GetApiUriFor(path, queryString), method, route, headers, payload, null);

            if (this.Discord != null)
                this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequestAsync(req);

            return req.WaitForCompletionAsync();
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

        /// <summary>
        /// Makes a multipart request to the discord API.
        /// </summary>
        /// <param name="route">The API route to make a request to. It should start with a /.</param>
        /// <param name="routeParams">The route parameters.</param>
        /// <param name="method">The type of REST request to make.</param>
        /// <param name="headers">The headers to send with the request.</param>
        /// <param name="values">Values to use.</param>
        /// <param name="files">The files to send on this multipart request.</param>
        /// <param name="queryString">The query string to add on the url.</param>
        /// <returns>The API response.</returns>
        public Task<RestResponse> DoMultipartAsync(string route, object routeParams, RestRequestMethod method, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
            IReadOnlyCollection<DiscordMessageFile> files = null, string queryString = null)
        {
            var req = new MultipartWebRequest(this.Discord, this.Rest.GetBucket(method, route, routeParams, out var path), Utilities.GetApiUriFor(path, queryString), method, route, headers, values, files, null);

            if (this.Discord != null)
                this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequestAsync(req);

            return req.WaitForCompletionAsync();
        }
        private Task<RestResponse> DoMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
            IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimitWaitOverride = null)
        {
            var req = new MultipartWebRequest(client, bucket, url, method, route, headers, values, files, ratelimitWaitOverride);

            if (this.Discord != null)
                this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequestAsync(req);

            return req.WaitForCompletionAsync();
        }

        #region Guild
        /// <summary>
        /// Creates a new guild
        /// </summary>
        /// <param name="name">New guild's name</param>
        /// <param name="region_id">New guild's region ID</param>
        /// <param name="iconb64">New guild's icon (base64)</param>
        /// <param name="verification_level">New guild's verification level</param>
        /// <param name="default_message_notifications">New guild's default message notification level</param>
        /// <returns></returns>
        public async Task<DiscordGuild> CreateGuildAsync(string name, string region_id, Optional<string> iconb64, VerificationLevel? verification_level,
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

        /// <summary>
        /// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
        /// </summary>
        /// <param name="template_code">The template code.</param>
        /// <param name="name">Name of the guild.</param>
        /// <param name="iconb64">Stream containing the icon for the guild.</param>
        /// <returns>The created guild.</returns>
        public async Task<DiscordGuild> CreateGuildFromTemplateAsync(string template_code, string name, Optional<string> iconb64)
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

        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="guild_id">guild id</param>
        /// <returns></returns>
        public async Task DeleteGuildAsync(ulong guild_id)
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

        /// <summary>
        /// Modifies a guild
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="name">New guild Name</param>
        /// <param name="region">New guild voice region</param>
        /// <param name="verificationLevel">New guild verification level</param>
        /// <param name="defaultMessageNotifications">New guild default message notification level</param>
        /// <param name="mfaLevel">New guild MFA level</param>
        /// <param name="explicitContentFilter">New guild explicit content filter level</param>
        /// <param name="afkChannelId">New guild AFK channel id</param>
        /// <param name="afkTimeout">New guild AFK timeout in seconds</param>
        /// <param name="iconb64">New guild icon (base64)</param>
        /// <param name="ownerId">New guild owner id</param>
        /// <param name="splashb64">New guild spalsh (base64)</param>
        /// <param name="systemChannelId">New guild system channel id</param>
        /// <param name="reason">Modify reason</param>
        /// <returns></returns>
        public async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, Optional<string> name,
            Optional<string> region, Optional<VerificationLevel> verificationLevel,
            Optional<DefaultMessageNotifications> defaultMessageNotifications, Optional<MfaLevel> mfaLevel,
            Optional<ExplicitContentFilter> explicitContentFilter, Optional<ulong?> afkChannelId,
            Optional<int> afkTimeout, Optional<string> iconb64, Optional<ulong> ownerId, Optional<string> splashb64,
            Optional<ulong?> systemChannelId, string reason)
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
                SystemChannelId = systemChannelId
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

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
        /// <summary>
        /// Modifies a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="action">Guild modifications</param>
        /// <returns></returns>
        public async Task<DiscordGuild> ModifyGuildAsync(ulong guild_id, Action<GuildEditModel> action)
        {
            var mdl = new GuildEditModel();
            action(mdl);

            if (mdl.AfkChannel.HasValue)
                if (mdl.AfkChannel.Value.Type != ChannelType.Voice)
                    throw new ArgumentException("AFK channel needs to be a voice channel!");

            var iconb64 = Optional.FromNoValue<string>();
            if (mdl.Icon.HasValue && mdl.Icon.Value != null)
                using (var imgtool = new ImageTool(mdl.Icon.Value))
                    iconb64 = imgtool.GetBase64();
            else if (mdl.Icon.HasValue)
                iconb64 = null;

            var splashb64 = Optional.FromNoValue<string>();
            if (mdl.Splash.HasValue && mdl.Splash.Value != null)
                using (var imgtool = new ImageTool(mdl.Splash.Value))
                    splashb64 = imgtool.GetBase64();
            else if (mdl.Splash.HasValue)
                splashb64 = null;

            return await this.ModifyGuildAsync(guild_id, mdl.Name, mdl.Region.IfPresent(x => x.Id), mdl.VerificationLevel, mdl.DefaultMessageNotifications,
                mdl.MfaLevel, mdl.ExplicitContentFilter, mdl.AfkChannel.IfPresent(x => x?.Id), mdl.AfkTimeout, iconb64, mdl.Owner.IfPresent(x => x.Id),
                splashb64, mdl.SystemChannel.IfPresent(x => x?.Id), mdl.AuditLogReason).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets guild bans
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guild_id)
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
                    usr = this.Discord.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
                    {
                        old.Username = usr.Username;
                        old.Discriminator = usr.Discriminator;
                        old.AvatarHash = usr.AvatarHash;
                        return old;
                    });
                }

                xb.User = usr;
                return xb;
            });
            var bans = new ReadOnlyCollection<DiscordBan>(new List<DiscordBan>(bans_raw));

            return bans;
        }

        /// <summary>
        /// Creates guild ban
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="delete_message_days">Days to delete messages</param>
        /// <param name="reason">Reason why this member was banned</param>
        /// <returns></returns>
        public Task CreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason)
        {
            if (delete_message_days < 0 || delete_message_days > 7)
                throw new ArgumentException("Delete message days must be a number between 0 and 7.", nameof(delete_message_days));

            var urlparams = new Dictionary<string, string>
            {
                ["delete_message_days"] = delete_message_days.ToString(CultureInfo.InvariantCulture)
            };
            if (reason != null)
                urlparams["reason"] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        /// <summary>
        /// Removes a guild ban
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User to unban</param>
        /// <param name="reason">Reason why this member was unbanned</param>
        /// <returns></returns>
        public Task RemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason)
        {
            var urlparams = new Dictionary<string, string>();
            if (reason != null)
                urlparams["reason"] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Leaves a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public Task LeaveGuildAsync(ulong guild_id)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Adds a member to a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="access_token">Access token</param>
        /// <param name="nick">User nickname</param>
        /// <param name="roles">User roles</param>
        /// <param name="muted">Whether this user should be muted on join</param>
        /// <param name="deafened">Whether this user should be deafened on join</param>
        /// <returns></returns>
        public async Task<DiscordMember> AddGuildMemberAsync(ulong guild_id, ulong user_id, string access_token, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
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

        /// <summary>
        /// Gets all guild members
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="limit">Member download limit</param>
        /// <param name="after">Gets members after this ID</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordMember>> ListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
        {
            var recmbr = new List<DiscordMember>();

            var recd = limit ?? 1000;
            var lim = limit ?? 1000;
            var last = after;
            while (recd == lim)
            {
                var tms = await this.InternalListGuildMembersAsync(guild_id, lim, last == 0 ? null : (ulong?)last).ConfigureAwait(false);
                recd = tms.Count;

                foreach (var xtm in tms)
                {
                    last = xtm.User.Id;

                    if (this.Discord.UserCache.ContainsKey(xtm.User.Id))
                        continue;

                    var usr = new DiscordUser(xtm.User) { Discord = this.Discord };
                    this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
                    {
                        old.Username = usr.Username;
                        old.Discord = usr.Discord;
                        old.AvatarHash = usr.AvatarHash;

                        return old;
                    });
                }

                recmbr.AddRange(tms.Select(xtm => new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild_id }));
            }

            return new ReadOnlyCollection<DiscordMember>(recmbr);
        }
        internal async Task<IReadOnlyList<TransportMember>> InternalListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
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

        /// <summary>
        /// Add role to guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="reason">Reason this role gets added</param>
        /// <returns></returns>
        public Task AddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
        }

        /// <summary>
        /// Remove role from member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="reason">Reason this role gets removed</param>
        /// <returns></returns>
        public Task RemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Updates a channel's position
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="channel_id">Channel id</param>
        /// <param name="position">Channel position</param>
        /// <param name="reason">Reason this position was modified</param>
        /// <returns></returns>
        public Task ModifyGuildChannelPositionAsync(ulong guild_id, ulong channel_id, int position, string reason)
        {
            var pld = new List<RestGuildChannelReorderPayload>()
            {
                new RestGuildChannelReorderPayload { ChannelId = channel_id }
            };

            return this.ModifyGuildChannelPositionAsync(guild_id, pld, reason);
        }
        internal Task ModifyGuildChannelPositionAsync(ulong guild_id, IEnumerable<RestGuildChannelReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Updates a role's position
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="role_id">Role id</param>
        /// <param name="position">Role position</param>
        /// <param name="reason">Reason this position was modified</param>
        /// <returns></returns>
        public Task ModifyGuildRolePositionAsync(ulong guild_id, ulong role_id, int position, string reason = null)
        {
            var pld = new List<RestGuildRoleReorderPayload>()
            {
                new RestGuildRoleReorderPayload { RoleId = role_id }
            };

            return this.ModifyGuildRolePositionAsync(guild_id, pld, reason);
        }
        internal Task ModifyGuildRolePositionAsync(ulong guild_id, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

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

        /// <summary>
        /// Gets the vanity invite for a guild.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns>A partial vanity invite.</returns>
        public async Task<DiscordInvite> GetGuildVanityUrlAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VANITY_URL}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invite = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);

            return invite;
        }

        /// <summary>
        /// Gets a guild's widget
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public async Task<DiscordWidget> GetGuildWidgetAsync(ulong guild_id)
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

        /// <summary>
        /// Gets a guild's widget settings
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public async Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[guild_id];

            return ret;
        }

        /// <summary>
        /// Modifies a guild's widget settings
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="isEnabled">If the widget is enabled or not</param>
        /// <param name="channelId">Widget channel id</param>
        /// <param name="reason">Reason the widget settings were modified</param>
        /// <returns></returns>
        public async Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guild_id, bool? isEnabled, ulong? channelId, string reason)
        {
            var pld = new RestGuildWidgetSettingsPayload
            {
                Enabled = isEnabled,
                ChannelId = channelId
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[guild_id];

            return ret;
        }

        /// <summary>
        /// Gets a guild's templates.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns>All of the guild's templates.</returns>
        public async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var templates_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response);

            return new ReadOnlyCollection<DiscordGuildTemplate>(new List<DiscordGuildTemplate>(templates_raw));
        }

        /// <summary>
        /// Creates a guild template.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="name">Name of the template.</param>
        /// <param name="description">Description of the template.</param>
        /// <returns>The template created.</returns>
        public async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guild_id, string name, string description)
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

        /// <summary>
        /// Syncs the template to the current guild's state.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="template_code">The code of the template to sync.</param>
        /// <returns>The template synced.</returns>
        public async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guild_id, string template_code)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        /// <summary>
        /// Modifies the template's metadata.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="template_code">The template's code.</param>
        /// <param name="name">Name of the template.</param>
        /// <param name="description">Description of the template.</param>
        /// <returns>The template modified.</returns>
        public async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guild_id, string template_code, string name, string description)
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

        /// <summary>
        /// Deletes the template.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="template_code">The code of the template to delete.</param>
        /// <returns>The deleted template.</returns>
        public async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guild_id, string template_code)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        /// <summary>
        /// Gets a guild's membership screening form.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns>The guild's membership screening form.</returns>
        public async Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var screening_raw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

            return screening_raw;
        }

        /// <summary>
        /// Modifies a guild's membership screening form.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="action">Action to perform</param>
        /// <returns>The modified screening form.</returns>
        public async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guild_id, Action<MembershipScreeningEditModel> action)
        {
            var mdl = new MembershipScreeningEditModel();
            action(mdl);
            return await this.ModifyGuildMembershipScreeningFormAsync(guild_id, mdl.Enabled, mdl.Fields, mdl.Description);
        }
        /// <summary>
        /// Modifies a guild's membership screening form.
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="enabled">Sets whether membership screening should be enabled.</param>
        /// <param name="fields">Set the fields.</param>
        /// <param name="description">Sets the server description.</param>
        /// <returns>The modified screening form.</returns>
        public async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guild_id, Optional<bool> enabled, Optional<DiscordGuildMembershipScreeningField[]> fields, Optional<string> description)
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

        /// <summary>
        /// Gets a guild's welcome screen.
        /// </summary>
        /// <returns>The guild's welcome screen object.</returns>
        public async Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        /// <summary>
        /// Modifies a guild's welcome screen.
        /// </summary>
        /// <param name="guildId">The guild ID to modify.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns>The modified welcome screen.</returns>
        public async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guildId, Action<WelcomeScreenEditModel> action)
        {
            var mdl = new WelcomeScreenEditModel();
            action(mdl);
            return await this.ModifyGuildWelcomeScreenAsync(guildId, mdl.Enabled, mdl.WelcomeChannels, mdl.Description);
        }
        /// <summary>
        /// Modifies a guild's welcome screen.
        /// </summary>
        /// <param name="guild_id">The guild ID to modify.</param>
        /// <param name="enabled">Sets whether the welcome screen should be enabled.</param>
        /// <param name="welcomeChannels">Sets the welcome channels.</param>
        /// <param name="description">Sets the server description.</param>
        /// <returns>The modified welcome screen.</returns>
        public async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guild_id, Optional<bool> enabled, Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> welcomeChannels, Optional<string> description)
        {
            var pld = new RestGuildWelcomeScreenModifyPayload
            {
                Enabled = enabled,
                WelcomeChannels = welcomeChannels,
                Description = description
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        /// <summary>
        /// Updates the current user's suppress state in a channel, if stage channel.
        /// </summary>
        /// <param name="guild_id">Guild id.</param>
        /// <param name="channelId">Channel id.</param>
        /// <param name="suppress">Toggles the suppress state.</param>
        /// <param name="requestToSpeakTimestamp">Sets the time the user requested to speak.</param>
        /// <exception cref="ArgumentException">Thrown when the channel is not a stage channel.</exception>
        public async Task UpdateCurrentUserVoiceStateAsync(ulong guild_id, ulong channelId, bool? suppress, DateTimeOffset? requestToSpeakTimestamp)
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
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Updates a member's suppress state in a stage channel.
        /// </summary>
        /// <param name="guild_id">Guild id.</param>
        /// <param name="user_id">User id.</param>
        /// <param name="channelId">The channel the member is currently in.</param>
        /// <param name="suppress">Toggles the member's suppress state.</param>
        /// <exception cref="ArgumentException">Thrown when the channel in not a voice channel.</exception>
        public async Task UpdateUserVoiceStateAsync(ulong guild_id, ulong user_id, ulong channelId, bool? suppress)
        {
            var pld = new RestGuildUpdateUserVoiceStatePayload
            {
                ChannelId = channelId,
                Suppress = suppress
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
        }
        #endregion

        #region Channel
        /// <summary>
        /// Creates a guild channel
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="name">Channel name</param>
        /// <param name="type">Channel type</param>
        /// <param name="parent">Channel parent id</param>
        /// <param name="topic">Channel topic</param>
        /// <param name="bitrate">Voice channel bitrate</param>
        /// <param name="user_limit">Voice channel user limit</param>
        /// <param name="overwrites">Channel overwrites</param>
        /// <param name="nsfw">Whether this channel should be marked as NSFW</param>
        /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
        /// <param name="qualityMode">Video quality.</param>
        /// <param name="reason">Reason this channel was created</param>
        /// <returns></returns>
        public async Task<DiscordChannel> CreateGuildChannelAsync(ulong guild_id, string name, ChannelType type, ulong? parent, Optional<string> topic, int? bitrate, int? user_limit, IEnumerable<DiscordOverwriteBuilder> overwrites, bool? nsfw, Optional<int?> perUserRateLimit, VideoQualityMode? qualityMode, string reason)
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
                headers.Add(REASON_HEADER_NAME, reason);

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

        /// <summary>
        /// Modifies a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="name">New channel name</param>
        /// <param name="position">New channel position</param>
        /// <param name="topic">New channel topic</param>
        /// <param name="nsfw">Whether this channel should be marked as NSFW</param>
        /// <param name="parent">New channel parent</param>
        /// <param name="bitrate">New voice channel bitrate</param>
        /// <param name="user_limit">New voice channel user limit</param>
        /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
        /// <param name="rtcRegion">New region override.</param>
        /// <param name="qualityMode">New video quality for this channel.</param>
        /// <param name="reason">Reason why this channel was modified</param>
        /// <returns></returns>
        public Task ModifyChannelAsync(ulong channel_id, string name, int? position, Optional<string> topic, bool? nsfw, Optional<ulong?> parent, int? bitrate, int? user_limit, Optional<int?> perUserRateLimit, Optional<string> rtcRegion, VideoQualityMode? qualityMode, string reason)
        {
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
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }
        /// <summary>
        /// Modifies a channel
        /// </summary>
        /// <param name="channelId">Channel id</param>
        /// <param name="action">Channel modifications</param>
        /// <returns></returns>
        public Task ModifyChannelAsync(ulong channelId, Action<ChannelEditModel> action)
        {
            var mdl = new ChannelEditModel();
            action(mdl);

            return this.ModifyChannelAsync(channelId, mdl.Name, mdl.Position, mdl.Topic, mdl.Nsfw,
                mdl.Parent.HasValue ? mdl.Parent.Value?.Id : default(Optional<ulong?>), mdl.Bitrate, mdl.Userlimit, mdl.PerUserRateLimit, mdl.RtcRegion.IfPresent(e => e?.Id), mdl.QualityMode,
                mdl.AuditLogReason);
        }

        /// <summary>
        /// Gets a channel object
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannelAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;
            foreach (var xo in ret._permissionOverwrites)
            {
                xo.Discord = this.Discord;
                xo._channel_id = ret.Id;
            }

            return ret;
        }

        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="reason">Reason why this channel was deleted</param>
        /// <returns></returns>
        public Task DeleteChannelAsync(ulong channel_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Gets message in a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="content">Message (text) content</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content)
            => this.CreateMessageAsync(channel_id, content, null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="embed">Embed to attach</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, DiscordEmbed embed)
            => this.CreateMessageAsync(channel_id, null, embed, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="content">Message (text) content</param>
        /// <param name="embed">Embed to attach</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content, DiscordEmbed embed)
            => this.CreateMessageAsync(channel_id, content, embed, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="action">The Discord Mesage builder.</param>
        /// <returns></returns>
        public Task<DiscordMessage> CreateMessageAsync(ulong channel_id, Action<DiscordMessageBuilder> action)
        {
            var builder = new DiscordMessageBuilder();
            action(builder);
            return this.CreateMessageAsync(channel_id, builder);
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id.</param>
        /// <param name="content">Content.</param>
        /// <param name="embed">Embed.</param>
        /// <param name="replyMessageId">Reply message id.</param>
        /// <param name="mentionReply">Whether to mention on reply.</param>
        /// <param name="failOnInvalidReply">Whether to fail on invalid reply.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content, DiscordEmbed embed, ulong? replyMessageId, bool mentionReply, bool failOnInvalidReply)
        {
            if (content != null && content.Length > 2000)
                throw new ArgumentException("Message content length cannot exceed 2000 characters.");

            if (embed == null)
            {
                if (content == null)
                    throw new ArgumentException("You must specify message content or an embed.");

                if (content.Length == 0)
                    throw new ArgumentException("Message content must not be empty.");
            }

            if (embed?.Timestamp != null)
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = content != null,
                Content = content,
                IsTTS = false,
                HasEmbed = embed != null,
                Embed = embed
            };

            if (replyMessageId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = replyMessageId, FailIfNotExists = failOnInvalidReply };

            if (replyMessageId != null)
                pld.Mentions = new DiscordMentions(Mentions.None, mentionReply);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="builder">The Discord Mesage builder.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, DiscordMessageBuilder builder)
        {
            builder.Validate();

            if (builder.Embed?.Timestamp != null)
                builder.Embed.Timestamp = builder.Embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = builder.Content != null,
                Content = builder.Content,
                IsTTS = builder.IsTTS,
                HasEmbed = builder.Embed != null,
                Embed = builder.Embed
            };

            if (builder.ReplyId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = builder.ReplyId, FailIfNotExists = builder.FailOnInvalidReply };


            if (builder.Mentions != null || builder.ReplyId != null)
                pld.Mentions = new DiscordMentions(builder.Mentions ?? Mentions.None, builder.MentionOnReply);

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

        /// <summary>
        /// Gets channels from a guild
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guild_id)
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

        /// <summary>
        /// Gets messages from a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="limit">Limit of messages to get</param>
        /// <param name="before">Gets messages before this id</param>
        /// <param name="after">Gets messages after this id</param>
        /// <param name="around">Gets messages around this id</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
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

        /// <summary>
        /// Gets a message from a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetChannelMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        //Should be removed in v9
        public Task ModifyEmbedSuppressionAsync(bool suppress, ulong channel_id, ulong message_id)
        {
            var pld = new RestChannelMessageSuppressEmbedsPayload
            {
                Suppress = suppress
            };

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.SUPPRESS_EMBEDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Edits a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="content">New message content</param>
        /// <returns></returns>
        public Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content)
            => this.EditMessageAsync(channel_id, message_id, content, default, default);

        /// <summary>
        /// Edits a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="embed">New message embed</param>
        /// <returns></returns>
        public Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<DiscordEmbed> embed)
            => this.EditMessageAsync(channel_id, message_id, default, embed, default);

        /// <summary>
        /// Edits a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="builder">The builder of the message to edit.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, DiscordMessageBuilder builder)
        {
            builder.Validate(true);

            return await this.EditMessageAsync(channel_id, message_id, builder.Content, builder.Embed, builder.Mentions).ConfigureAwait(false);
        }

        /// <summary>
        /// Edits a message
        /// </summary>
        /// <param name="channel_id">Channel id.</param>
        /// <param name="message_id">Message id.</param>
        /// <param name="content">New message content.</param>
        /// <param name="embed">New message embed.</param>
        /// <param name="mentions">Allowed mentions.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content, Optional<DiscordEmbed> embed, IEnumerable<IMention> mentions)
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

            if (mentions != null)
                pld.Mentions = new DiscordMentions(mentions);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <param name="reason">Why this message was deleted</param>
        /// <returns></returns>
        public Task DeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_ids">Message ids</param>
        /// <param name="reason">Reason these messages were deleted</param>
        /// <returns></returns>
        public Task DeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
        {
            var pld = new RestChannelMessageBulkDeletePayload
            {
                Messages = message_ids
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}{Endpoints.BULK_DELETE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets a channel's invites
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }

        /// <summary>
        /// Creates a channel invite
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="max_age">For how long the invite should exist</param>
        /// <param name="max_uses">How often the invite may be used</param>
        /// <param name="temporary">Whether this invite should be temporary</param>
        /// <param name="unique">Whether this invite should be unique (false might return an existing invite)</param>
        /// <param name="reason">Why you made an invite</param>
        /// <returns></returns>
        public async Task<DiscordInvite> CreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, bool temporary, bool unique, string reason)
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

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes channel overwrite
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="overwrite_id">Overwrite id</param>
        /// <param name="reason">Reason it was deleted</param>
        /// <returns></returns>
        public Task DeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, overwrite_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Edits channel overwrite
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="overwrite_id">Overwrite id</param>
        /// <param name="allow">Permissions to allow</param>
        /// <param name="deny">Permissions to deny</param>
        /// <param name="type">Overwrite type</param>
        /// <param name="reason">Reason this overwrite was created</param>
        /// <returns></returns>
        public Task EditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
        {
            var pld = new RestChannelPermissionEditPayload
            {
                Type = type,
                Allow = allow & PermissionMethods.FULL_PERMS,
                Deny = deny & PermissionMethods.FULL_PERMS
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, overwrite_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Send a typing indicator to a channel
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public Task TriggerTypingAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.TYPING}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
        }

        /// <summary>
        /// Gets pinned messages
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channel_id)
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

        /// <summary>
        /// Pins a message to a channel.
        /// </summary>
        /// <param name="channel_id">Channel id.</param>
        /// <param name="message_id">Message id.</param>
        /// <returns></returns>
        public Task PinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        /// <summary>
        /// Unpins a message
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="message_id">Message id</param>
        /// <returns></returns>
        public Task UnpinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Adds a member to a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="user_id">User id</param>
        /// <param name="access_token">User's access token</param>
        /// <param name="nickname">Nickname for user</param>
        /// <returns></returns>
        public Task AddGroupDmRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
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

        /// <summary>
        /// Joins a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="nickname">Dm nickname</param>
        /// <returns></returns>
        public Task JoinGroupDmAsync(ulong channel_id, string nickname)
            => this.AddGroupDmRecipientAsync(channel_id, this.Discord.CurrentUser.Id, this.Discord.Configuration.Token, nickname);

        /// <summary>
        /// Removes a member from a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <param name="user_id">User id</param>
        /// <returns></returns>
        public Task RemoveGroupDmRecipientAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Leaves a group DM
        /// </summary>
        /// <param name="channel_id">Channel id</param>
        /// <returns></returns>
        public Task LeaveGroupDmAsync(ulong channel_id)
            => this.RemoveGroupDmRecipientAsync(channel_id, this.Discord.CurrentUser.Id);

        /// <summary>
        /// Creates a group DM
        /// </summary>
        /// <param name="access_tokens">Access tokens</param>
        /// <param name="nicks">Nicknames per user</param>
        /// <returns></returns>
        public async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
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

        /// <summary>
        /// Creates a group DM with current user
        /// </summary>
        /// <param name="access_tokens">Access tokens</param>
        /// <param name="nicks">Nicknames</param>
        /// <returns></returns>
        public Task<DiscordDmChannel> CreateGroupDmWithCurrentUserAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var a = access_tokens.ToList();
            a.Add(this.Discord.Configuration.Token);
            return this.CreateGroupDmAsync(a, nicks);
        }

        /// <summary>
        /// Creates a DM
        /// </summary>
        /// <param name="recipient_id">Recipient user id</param>
        /// <returns></returns>
        public async Task<DiscordDmChannel> CreateDmAsync(ulong recipient_id)
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

        /// <summary>
        /// Follows a news channel
        /// </summary>
        /// <param name="channel_id">Id of the channel to follow</param>
        /// <param name="webhook_channel_id">Id of the channel to crosspost messages to</param>
        public async Task<DiscordFollowedChannel> FollowChannelAsync(ulong channel_id, ulong webhook_channel_id)
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

        /// <summary>
        /// Publishes a message in a news channel to following channels
        /// </summary>
        /// <param name="channel_id">Id of the news channel the message to crosspost belongs to</param>
        /// <param name="message_id">Id of the message to crosspost</param>
        public async Task<DiscordMessage> CrosspostMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.CROSSPOST}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        #endregion

        #region Member
        /// <summary>
        /// Gets current user object
        /// </summary>
        /// <returns></returns>
        public Task<DiscordUser> GetCurrentUserAsync()
            => this.GetUserAsync("@me");

        /// <summary>
        /// Gets user object
        /// </summary>
        /// <param name="user_id">User id</param>
        /// <returns></returns>
        public Task<DiscordUser> GetUserAsync(ulong user_id)
            => this.GetUserAsync(user_id.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets user object
        /// </summary>
        /// <param name="user_id">User id</param>
        /// <returns></returns>
        public async Task<DiscordUser> GetUserAsync(string user_id)
        {
            var route = $"{Endpoints.USERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var duser = new DiscordUser(user_raw) { Discord = this.Discord };

            return duser;
        }

        /// <summary>
        /// Gets guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">Member id</param>
        /// <returns></returns>
        public async Task<DiscordMember> GetGuildMemberAsync(ulong guild_id, ulong user_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            var usr = new DiscordUser(tm.User) { Discord = this.Discord };
            usr = this.Discord.UserCache.AddOrUpdate(tm.User.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            return new DiscordMember(tm)
            {
                Discord = this.Discord,
                _guild_id = guild_id
            };
        }

        /// <summary>
        /// Removes guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="reason">Why this user was removed</param>
        /// <returns></returns>
        public Task RemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason)
        {
            var urlparams = new Dictionary<string, string>();
            if (reason != null)
                urlparams["reason"] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams));
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Modifies current user
        /// </summary>
        /// <param name="username">New username</param>
        /// <param name="base64_avatar">New avatar (base64)</param>
        /// <returns></returns>
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

        /// <summary>
        /// Modifies current user
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="avatar">avatar</param>
        /// <returns></returns>
        public async Task<DiscordUser> ModifyCurrentUserAsync(string username = null, Stream avatar = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return new DiscordUser(await this.ModifyCurrentUserAsync(username, av64).ConfigureAwait(false)) { Discord = this.Discord };
        }

        /// <summary>
        /// Gets current user's guilds
        /// </summary>
        /// <param name="limit">Limit of guilds to get</param>
        /// <param name="before">Gets guild before id</param>
        /// <param name="after">Gets guilds after id</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
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

        /// <summary>
        /// Modifies guild member
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="user_id">User id</param>
        /// <param name="nick">New nickname</param>
        /// <param name="role_ids">New roles</param>
        /// <param name="mute">Whether this user should be muted</param>
        /// <param name="deaf">Whether this user should be deafened</param>
        /// <param name="voice_channel_id">Voice channel to move this user to</param>
        /// <param name="reason">Reason this user was modified</param>
        /// <returns></returns>
        public Task ModifyGuildMemberAsync(ulong guild_id, ulong user_id, Optional<string> nick,
            Optional<IEnumerable<ulong>> role_ids, Optional<bool> mute, Optional<bool> deaf,
            Optional<ulong?> voice_channel_id, string reason)
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

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Changes current user's nickname
        /// </summary>
        /// <param name="guild_id">Guild id</param>
        /// <param name="nick">Nickname</param>
        /// <param name="reason">Reason why you set it to this</param>
        /// <returns></returns>
        public Task ModifyCurrentMemberNicknameAsync(ulong guild_id, string nick, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.ME}{Endpoints.NICK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
        }
        #endregion

        #region Roles
        public async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var roles_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = this.Discord; xr._guild_id = guild_id; return xr; });

            return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(roles_raw));
        }

        public async Task<DiscordGuild> GetGuildAsync(ulong guildId, bool? with_counts)
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

        public async Task<DiscordRole> ModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions & PermissionMethods.FULL_PERMS,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
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

        public Task DeleteRoleAsync(ulong guild_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        public async Task<DiscordRole> CreateGuildRoleAsync(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions & PermissionMethods.FULL_PERMS,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
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
        public async Task<int> GetGuildPruneCountAsync(ulong guild_id, int days, IEnumerable<ulong> include_roles)
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

        public async Task<int?> BeginGuildPruneAsync(ulong guild_id, int days, bool compute_prune_count, IEnumerable<ulong> include_roles, string reason)
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

            if (reason != null)
                urlparams["reason"] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PRUNE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlparams)}{sb}");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route).ConfigureAwait(false);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        #endregion

        #region GuildVarious
        public async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
        {
            var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { code }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var templates_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return templates_raw;
        }

        public async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var integrations_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrations_raw));
        }

        public async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PREVIEW}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildPreview>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guild_id, string type, ulong id)
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

        public async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guild_id, ulong integration_id, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
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

        public Task DeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, integration_id = integration.Id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, payload: DiscordJson.SerializeObject(integration));
        }

        public Task SyncGuildIntegrationAsync(ulong guild_id, ulong integration_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id{Endpoints.SYNC}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id, integration_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
        }

        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REGIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var regions_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions_raw));
        }

        public async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id)
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
        public async Task<DiscordInvite> GetInviteAsync(string invite_code, bool? with_counts)
        {
            var urlparams = new Dictionary<string, string>();
            if (with_counts.HasValue)
                urlparams["with_counts"] = with_counts?.ToString();

            var route = $"{Endpoints.INVITES}/:invite_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { invite_code }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "");
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task<DiscordInvite> DeleteInviteAsync(string invite_code, string reason)
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
         * public async Task<DiscordInvite> publicAcceptInvite(string invite_code)
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
        public async Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
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
        public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
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
        public async Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, Optional<string> base64_avatar, string reason)
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

        public async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        public async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        public async Task<DiscordWebhook> GetWebhookAsync(ulong webhook_id)
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
        public async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhook_id, string webhook_token)
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

        public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, ulong channelId, string name, Optional<string> base64_avatar, string reason)
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

        public async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string webhook_token, string reason)
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

        public Task DeleteWebhookAsync(ulong webhook_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        public Task DeleteWebhookAsync(ulong webhook_id, string webhook_token, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        public async Task<DiscordMessage> ExecuteWebhookAsync(ulong webhook_id, string webhook_token, DiscordWebhookBuilder builder)
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
                Embeds = builder.Embeds
            };

            if (builder.Mentions != null)
                pld.Mentions = new DiscordMentions(builder.Mentions);

            if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count() > 0 || builder.IsTTS == true || builder.Mentions != null)
                values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

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

        public async Task<DiscordMessage> ExecuteWebhookSlackAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.SLACK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: json_payload).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        public async Task<DiscordMessage> ExecuteWebhookGithubAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.GITHUB}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: json_payload).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        public async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhook_id, string webhook_token, string message_id, DiscordWebhookBuilder builder)
        {
            var pld = new RestWebhookMessageEditPayload
            {
                Content = builder.Content,
                Embeds = builder.Embeds,
                Mentions = builder.Mentions
            };

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id, webhook_token, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }
        public Task<DiscordMessage> EditWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id, DiscordWebhookBuilder builder) =>
            this.EditWebhookMessageAsync(webhook_id, webhook_token, message_id.ToString(), builder);

        public async Task DeleteWebhookMessageAsync(ulong webhook_id, string webhook_token, string message_id)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id, webhook_token, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }
        public Task DeleteWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id) =>
            this.DeleteWebhookMessageAsync(webhook_id, webhook_token, message_id.ToString());
        #endregion

        #region Reactions
        public Task CreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        public Task DeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        public Task DeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        public async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, string emoji, ulong? after_id = null, int limit = 25)
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
                usr = this.Discord.UserCache.AddOrUpdate(xr.Id, usr, (id, old) =>
                {
                    old.Username = usr.Username;
                    old.Discriminator = usr.Discriminator;
                    old.AvatarHash = usr.AvatarHash;
                    return old;
                });

                reacters.Add(usr);
            }

            return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters));
        }

        public Task DeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        public Task DeleteReactionsEmojiAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }
        #endregion

        #region Emoji
        public async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guild_id)
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
                var xge = rawEmoji.ToObject<DiscordGuildEmoji>();
                xge.Guild = gld;

                var xtu = rawEmoji["user"]?.ToObject<TransportUser>();
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

        public async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guild_id, ulong emoji_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, emoji_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);

            var emoji_raw = JObject.Parse(res.Response);
            var emoji = emoji_raw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        public async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guild_id, string name, string imageb64, IEnumerable<ulong> roles, string reason)
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
            var emoji = emoji_raw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToObject<TransportUser>();
            emoji.User = xtu != null
                ? gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu)
                : this.Discord.CurrentUser;

            return emoji;
        }

        public async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guild_id, ulong emoji_id, string name, IEnumerable<ulong> roles, string reason)
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
            var emoji = emoji_raw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        public Task DeleteGuildEmojiAsync(ulong guild_id, ulong emoji_id, string reason)
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

        #region Slash Commands
        public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(ulong application_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ulong application_id, IEnumerable<DiscordApplicationCommand> commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options
                });
            }

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        public async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(ulong application_id, DiscordApplicationCommand command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Name = command.Name,
                Description = command.Description,
                Options = command.Options
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong application_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong application_id, ulong command_id, Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = name,
                Description = description,
                Options = options
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task DeleteGlobalApplicationCommandAsync(ulong application_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong application_id, ulong guild_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong application_id, ulong guild_id, IEnumerable<DiscordApplicationCommand> commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options
                });
            }

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        public async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong application_id, ulong guild_id, DiscordApplicationCommand command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Name = command.Name,
                Description = command.Description,
                Options = command.Options
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id, Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = name,
                Description = description,
                Options = options
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        public async Task DeleteGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        public async Task CreateInteractionResponseAsync(ulong interaction_id, string interaction_token, InteractionResponseType type, DiscordInteractionResponseBuilder builder)
        {
            var pld = new RestInteractionResponsePayload
            {
                Type = type,
                Data = builder != null ? new DiscordInteractionApplicationCommandCallbackData
                {
                    Content = builder.Content,
                    Embeds = builder.Embeds,
                    IsTTS = builder.IsTTS,
                    Mentions = builder.Mentions,
                    Flags = builder.IsEphemeral ? 64 : null
                } : null
            };

            var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { interaction_id, interaction_token }, out var path);

            var url = Utilities.GetApiUriFor(path);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
        }

        public Task<DiscordMessage> EditOriginalInteractionResponseAsync(ulong application_id, string interaction_token, DiscordWebhookBuilder builder) =>
            this.EditWebhookMessageAsync(application_id, interaction_token, "@original", builder);

        public Task DeleteOriginalInteractionResponseAsync(ulong application_id, string interaction_token) =>
            this.DeleteWebhookMessageAsync(application_id, interaction_token, "@original");

        public async Task<DiscordMessage> CreateFollowupMessageAsync(ulong application_id, string interaction_token, DiscordFollowupMessageBuilder builder)
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
                Flags = builder.Flags
            };

            if (builder.Mentions != null)
                pld.Mentions = new DiscordMentions(builder.Mentions);

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

        public Task<DiscordMessage> EditFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id, DiscordWebhookBuilder builder) =>
            this.EditWebhookMessageAsync(application_id, interaction_token, message_id, builder);

        public Task DeleteFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id) =>
            this.DeleteWebhookMessageAsync(application_id, interaction_token, message_id);

        //TODO: edit, delete, follow up
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

        public async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
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

        public async Task<GatewayInfo> GetGatewayInfoAsync()
        {
            var headers = Utilities.GetBaseHeaders();
            var route = Endpoints.GATEWAY;
            if (this.Discord.Configuration.TokenType == TokenType.Bot)
                route += Endpoints.BOT;
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, headers).ConfigureAwait(false);

            var info = JObject.Parse(res.Response).ToObject<GatewayInfo>();
            info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.resetAfter);
            return info;
        }
        #endregion
    }
}
