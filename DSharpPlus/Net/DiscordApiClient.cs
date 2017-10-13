using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Enums;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net
{
    public sealed class DiscordApiClient
    {
        private const string ReasonHeaderName = "X-Audit-Log-Reason";

        internal BaseDiscordClient Discord { get; }
        internal RestClient Rest { get; }

        private string LastAckToken { get; set; }
        private SemaphoreSlim TokenSemaphore { get; } = new SemaphoreSlim(1, 1);

        internal DiscordApiClient(BaseDiscordClient client)
        {
            Discord = client;
            Rest = new RestClient(client);
        }

        internal DiscordApiClient() // This is for meta-clients, such as the webhook client
        {
            Rest = new RestClient();
        }

        private static string BuildQueryString(IDictionary<string, string> values, bool post = false)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            var valsCollection = values.Select(xkvp => string.Concat(WebUtility.UrlEncode(xkvp.Key), "=", WebUtility.UrlEncode(xkvp.Value)));
            var vals = string.Join("&", valsCollection);

            if (!post)
            {
                return string.Concat("?", vals);
            }

            return vals;
        }

        private DiscordMessage PrepareMessage(JToken msgRaw)
        {
            var author = msgRaw["author"].ToObject<TransportUser>();
            var ret = msgRaw.ToObject<DiscordMessage>();
            ret.Discord = Discord;

            var guild = ret.Channel?.Guild;

            if (!Discord.UserCache.TryGetValue(author.Id, out var usr))
            {
                Discord.UserCache[author.Id] = usr = new DiscordUser(author) { Discord = Discord };
            }

            if (guild != null)
            {
                var mbr = guild.Members.FirstOrDefault(xm => xm.Id == author.Id) ?? new DiscordMember(usr) { Discord = Discord, GuildId = guild.Id };
                ret.Author = mbr;
            }
            else
            {
                ret.Author = usr;
            }

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(ret.Content))
            {
                if (guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(ret).Select(xid => guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(ret).Select(xid => guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(ret).Select(xid => guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(ret).Select(Discord.InternalGetCachedUser).ToList();
                }
            }

            ret._mentionedUsers = mentionedUsers;
            ret._mentionedRoles = mentionedRoles;
            ret._mentionedChannels = mentionedChannels;

            if (ret._reactions == null)
            {
                ret._reactions = new List<DiscordReaction>();
            }
            foreach (var xr in ret._reactions)
            {
                xr.Emoji.Discord = Discord;
            }

            return ret;
        }

        private Task<RestResponse> DoRequestAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, IDictionary<string, string> headers = null, string payload = null, double? ratelimitWaitOverride = null)
        {
            var req = new RestRequest(client, bucket, url, method, headers, payload, ratelimitWaitOverride);
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = Rest.ExecuteRequestAsync(req);
            return req.WaitForCompletionAsync();
        }

        private Task<RestResponse> DoMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, IDictionary<string, string> headers = null, IDictionary<string, string> values = null,
            IDictionary<string, Stream> files = null, double? ratelimitWaitOverride = null)
        {
            var req = new MultipartWebRequest(client, bucket, url, method, headers, values, files, ratelimitWaitOverride);
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = Rest.ExecuteRequestAsync(req);
            return req.WaitForCompletionAsync();
        }

        #region Guild
        internal async Task<DiscordGuild> CreateGuildAsync(string name, string regionId, string iconb64, VerificationLevel? verificationLevel,
            DefaultMessageNotifications? defaultMessageNotifications)
        {
            var pld = new RestGuildCreatePayload
            {
                Name = name,
                RegionId = regionId,
                DefaultMessageNotifications = defaultMessageNotifications,
                VerificationLevel = verificationLevel,
                IconBase64 = iconb64
            };

            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            var route = string.Concat(Endpoints.Guilds);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            if (Discord is DiscordClient dc)
            {
                await dc.OnGuildCreateEventAsync(guild, rawMembers, null);
            }
            return guild;
        }

        internal async Task DeleteGuildAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete);

            if (Discord is DiscordClient dc)
            {
                var gld = dc._guilds[guildId];
                await dc.OnGuildDeleteEventAsync(gld, null);
            }
        }

        internal async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, string name, string region, VerificationLevel? verificationLevel,
            DefaultMessageNotifications? defaultMessageNotifications, MfaLevel? mfaLevel, ExplicitContentFilter? explicitContentFilter, ulong? afkChannelId, int? afkTimeout, string iconb64, 
            ulong? ownerId, string splashb64, string reason)
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
                OwnerId = ownerId
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            if (Discord is DiscordClient dc)
            {
                await dc.OnGuildUpdateEventAsync(guild, rawMembers);
            }
            return guild;
        }

        internal async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Bans);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var bansRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordBan>>(res.Response).Select(xb => 
            {
                var usr = Discord.InternalGetCachedUser(xb.RawUser.Id);
                if (usr == null)
                {
                    usr = new DiscordUser(xb.RawUser) { Discord = Discord };
                    usr = Discord.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
                    {
                        // ReSharper disable AccessToModifiedClosure
                        old.Username = usr.Username;
                        old.Discriminator = usr.Discriminator;
                        old.AvatarHash = usr.AvatarHash;
                        // ReSharper restore AccessToModifiedClosure
                        return old;
                    });
                }

                xb.User = usr;
                return xb;
            });
            var bans = new ReadOnlyCollection<DiscordBan>(new List<DiscordBan>(bansRaw));

            return bans;
        }

        internal Task CreateGuildBanAsync(ulong guildId, ulong userId, int deleteMessageDays, string reason)
        {
            if (deleteMessageDays < 0 || deleteMessageDays > 7)
            {
                throw new ArgumentException("Delete message days must be a number between 0 and 7.", nameof(deleteMessageDays));
            }

            var urlparams = new Dictionary<string, string>
            {
                ["delete-message-days"] = deleteMessageDays.ToString(CultureInfo.InvariantCulture)
            };
            if (reason != null)
            {
                urlparams["reason"] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Bans, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Put, route, new { guild_id = guildId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, BuildQueryString(urlparams)));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Put);
        }

        internal Task RemoveGuildBanAsync(ulong guildId, ulong userId, string reason)
        {
            var urlparams = new Dictionary<string, string>();
            if (reason != null)
            {
                urlparams["reason"] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Bans, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, BuildQueryString(urlparams)));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete);
        }

        internal Task LeaveGuildAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Users, Endpoints.Me, Endpoints.Guilds, "/:guild_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete);
        }

        internal async Task<DiscordMember> AddGuildMemberAsync(ulong guildId, ulong userId, string accessToken, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
        {
            var pld = new RestGuildMemberAddPayload
            {
                AccessToken = accessToken,
                Nickname = nick,
                Roles = roles,
                Deaf = deafened,
                Mute = muted
            };

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Put, route, new { guild_id = guildId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Put, payload: JsonConvert.SerializeObject(pld));

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm) { Discord = Discord, GuildId = guildId };
        }

        internal async Task<IReadOnlyList<TransportMember>> ListGuildMembersAsync(ulong guildId, int? limit, ulong? after)
        {
            var urlparams = new Dictionary<string, string>();
            if (limit != null && limit > 0)
            {
                urlparams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (after != null)
            {
                urlparams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, urlparams.Any() ? BuildQueryString(urlparams) : ""));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var membersRaw = JsonConvert.DeserializeObject<List<TransportMember>>(res.Response);
            return new ReadOnlyCollection<TransportMember>(membersRaw);
        }

        internal Task AddGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members, "/:user_id", Endpoints.Roles, "/:role_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Put, route, new { guild_id = guildId, user_id = userId, role_id = roleId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Put, headers);
        }

        internal Task RemoveGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members, "/:user_id", Endpoints.Roles, "/:role_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId, user_id = userId, role_id = roleId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }

        internal Task ModifyGuildChannelPosition(ulong guildId, IEnumerable<RestGuildChannelReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Channels);
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));
        }

        internal Task ModifyGuildRolePosition(ulong guildId, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Roles);
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<AuditLog> GetAuditLogsAsync(ulong guildId, int limit, ulong? after, ulong? before, ulong? responsible, int? actionType)
        {
            var urlparams = new Dictionary<string, string>
            {
                ["limit"] = limit.ToString(CultureInfo.InvariantCulture)
            };
            if (after != null)
            {
                urlparams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (before != null)
            {
                urlparams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (responsible != null)
            {
                urlparams["user_id"] = responsible.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (actionType != null)
            {
                urlparams["action_type"] = actionType.Value.ToString(CultureInfo.InvariantCulture);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.AuditLogs);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, urlparams.Any() ? BuildQueryString(urlparams) : ""));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);
            
            var auditLogDataRaw = JsonConvert.DeserializeObject<AuditLog>(res.Response);

            return auditLogDataRaw;
        }
        #endregion

        #region Channel
        internal async Task<DiscordChannel> CreateGuildChannelAsync(ulong guildId, string name, ChannelType type, ulong? parent, int? bitrate, int? userLimit, IEnumerable<DiscordOverwrite> overwrites, string reason)
        {
            var pld = new RestChannelCreatePayload
            {
                Name = name,
                Type = type,
                Parent = parent,
                Bitrate = bitrate,
                UserLimit = userLimit,
                PermissionOverwrites = overwrites
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Channels);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, headers, JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal Task ModifyChannelAsync(ulong channelId, string name, int? position, string topic, Optional<ulong?> parent, int? bitrate, int? userLimit, string reason)
        {
            var pld = new RestChannelModifyPayload
            {
                Name = name,
                Position = position,
                Topic = topic,
                Parent = parent.HasValue ? parent.Value : null,
                ParentSet = parent.HasValue,
                Bitrate = bitrate,
                UserLimit = userLimit
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<DiscordChannel> GetChannelAsync(ulong channelId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal Task DeleteChannelAsync(ulong channelId, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }

        internal async Task<DiscordMessage> GetMessageAsync(ulong channelId, ulong messageId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId, message_id = messageId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var ret = PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content, bool? tts, DiscordEmbed embed)
        {
            if (content != null && content.Length >= 2000)
            {
                throw new ArgumentException("Max message length is 2000");
            }
            if (string.IsNullOrEmpty(content) && embed == null)
            {
                throw new ArgumentException("Cannot send empty message");
            }
            if (content == null && embed == null)
            {
                throw new ArgumentException("Message must have text or embed");
            }

            if (embed?.Timestamp != null)
            {
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
            }

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = content != null,
                Content = content,
                IsTts = tts,
                HasEmbed = embed != null,
                Embed = embed
            };

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));

            var ret = PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<DiscordMessage> UploadFileAsync(ulong channelId, Stream fileData, string fileName, string content, bool? tts, DiscordEmbed embed)
        {
            var file = new Dictionary<string, Stream> { { fileName, fileData } };

            if (content != null && content.Length >= 2000)
            {
                throw new ArgumentException("Max message length is 2000");
            }

            if (embed?.Timestamp != null)
            {
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
            }

            var values = new Dictionary<string, string>();
            var pld = new RestChannelMessageCreateMultipartPayload
            {
                Embed = embed,
                Content = content,
                IsTts = tts
            };
            
            if (!string.IsNullOrEmpty(content) || embed != null || tts == true)
            {
                values["payload_json"] = JsonConvert.SerializeObject(pld);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoMultipartAsync(Discord, bucket, url, RestRequestMethod.Post, values: values, files: file);

            var ret = PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<DiscordMessage> UploadFilesAsync(ulong channelId, Dictionary<string, Stream> files, string content, bool? tts, DiscordEmbed embed)
        {
            if (embed?.Timestamp != null)
            {
                embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
            }

            if (content != null && content.Length >= 2000)
            {
                throw new ArgumentException("Message content length cannot exceed 2000 characters.");
            }
            if (files.Count == 0 && string.IsNullOrEmpty(content) && embed == null)
            {
                throw new ArgumentException("You must specify content, an embed, or at least one file.");
            }

            var values = new Dictionary<string, string>();
            var pld = new RestChannelMessageCreateMultipartPayload
            {
                Embed = embed,
                Content = content,
                IsTts = tts
            };
            if (!string.IsNullOrWhiteSpace(content) || embed != null || tts == true)
            {
                values["payload_json"] = JsonConvert.SerializeObject(pld);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoMultipartAsync(Discord, bucket, url, RestRequestMethod.Post, values: values, files: files);

            var ret = PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Channels);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var channelsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(xc => { xc.Discord = Discord; return xc; });

            return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channelsRaw));
        }

        internal async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channelId, int limit, ulong? before, ulong? after, ulong? around)
        {
            var urlparams = new Dictionary<string, string>();
            if (around != null)
            {
                urlparams["around"] = around.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (before != null)
            {
                urlparams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (after != null)
            {
                urlparams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (limit > 0)
            {
                urlparams["limit"] = limit.ToString(CultureInfo.InvariantCulture);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, urlparams.Any() ? BuildQueryString(urlparams) : ""));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var msgsRaw = JArray.Parse(res.Response);
            var msgs = msgsRaw.Select(PrepareMessage).ToList();

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        internal async Task<DiscordMessage> GetChannelMessageAsync(ulong channelId, ulong messageId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId, message_id = messageId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var ret = PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal async Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<string> content, Optional<DiscordEmbed> embed)
        {
            if (embed.HasValue && embed.Value?.Timestamp != null)
            {
                embed.Value.Timestamp = embed.Value.Timestamp.Value.ToUniversalTime();
            }

            var pld = new RestChannelMessageEditPayload
            {
                HasContent = content.HasValue,
                Content = content.HasValue ? (string)content : null,
                HasEmbed = embed.HasValue,
                Embed = embed.HasValue ? (DiscordEmbed)embed : null
            };

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { channel_id = channelId, message_id = messageId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, payload: JsonConvert.SerializeObject(pld));

            var ret = PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        internal Task DeleteMessageAsync(ulong channelId, ulong messageId, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId, message_id = messageId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }

        internal Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds, string reason)
        {
            var pld = new RestChannelMessageBulkDeletePayload
            {
                Messages = messageIds
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, Endpoints.BulkDelete);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, headers, JsonConvert.SerializeObject(pld));
        }

        internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channelId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Invites);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
        }

        internal async Task<DiscordInvite> CreateChannelInviteAsync(ulong channelId, int maxAge, int maxUses, bool temporary, bool unique, string reason)
        {
            var pld = new RestChannelInviteCreatePayload
            {
                MaxAge = maxAge,
                MaxUses = maxUses,
                Temporary = temporary,
                Unique = unique
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Invites);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, headers, JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal Task DeleteChannelPermissionAsync(ulong channelId, ulong overwriteId, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Permissions, "/:overwrite_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId, overwrite_id = overwriteId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }

        internal Task EditChannelPermissionsAsync(ulong channelId, ulong overwriteId, Permissions allow, Permissions deny, string type, string reason)
        {
            var pld = new RestChannelPermissionEditPayload
            {
                Type = type,
                Allow = allow,
                Deny = deny
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers.Add(ReasonHeaderName, reason);
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Permissions, "/:overwrite_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = channelId, overwrite_id = overwriteId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Put, headers, JsonConvert.SerializeObject(pld));
        }

        internal Task TriggerTypingAsync(ulong channelId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Typing);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post);
        }

        internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channelId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Pins);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var msgsRaw = JArray.Parse(res.Response);
            var msgs = msgsRaw.Select(xj => PrepareMessage(xj)).ToList();

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        internal Task PinMessageAsync(ulong channelId, ulong messageId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Pins, "/:message_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = channelId, message_id = messageId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Put);
        }

        internal Task UnpinMessageAsync(ulong channelId, ulong messageId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Pins, "/:message_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId, message_id = messageId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete);
        }

        internal Task GroupDmAddRecipientAsync(ulong channelId, ulong userId, string accessToken, string nickname)
        {
            var pld = new RestChannelGroupDmRecipientAddPayload
            {
                AccessToken = accessToken,
                Nickname = nickname
            };

            var route = string.Concat(Endpoints.Users, Endpoints.Me, Endpoints.Channels, "/:channel_id", Endpoints.Recipients, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = channelId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Put, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task GroupDmRemoveRecipientAsync(ulong channelId, ulong userId)
        {
            var route = string.Concat(Endpoints.Users, Endpoints.Me, Endpoints.Channels, "/:channel_id", Endpoints.Recipients, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete);
        }

        internal async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
        {
            var pld = new RestUserGroupDmCreatePayload
            {
                AccessTokens = accessTokens,
                Nicknames = nicks
            };

            var route = string.Concat(Endpoints.Users, Endpoints.Me, Endpoints.Channels);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal async Task<DiscordDmChannel> CreateDmAsync(ulong recipientId)
        {
            var pld = new RestUserDmCreatePayload
            {
                Recipient = recipientId
            };
            
            var route = string.Concat(Endpoints.Users, Endpoints.Me, Endpoints.Channels);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = Discord;

            return ret;
        }
        #endregion

        #region Member
        internal Task<DiscordUser> GetCurrentUserAsync() =>
            GetUserAsync("@me");

        internal Task<DiscordUser> GetUserAsync(ulong user) =>
            GetUserAsync(user.ToString(CultureInfo.InvariantCulture));

        internal async Task<DiscordUser> GetUserAsync(string userId)
        {
            var route = string.Concat(Endpoints.Users, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var duser = new DiscordUser(userRaw) { Discord = Discord };

            return duser;
        }

        internal async Task<DiscordMember> GetGuildMemberAsync(ulong guildId, ulong userId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            var usr = new DiscordUser(tm.User) { Discord = Discord };
            Discord.UserCache.AddOrUpdate(tm.User.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            return new DiscordMember(tm)
            {
                Discord = Discord,
                GuildId = guildId
            };
        }

        internal Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string reason)
        {
            var urlparams = new Dictionary<string, string>();
            if (reason != null)
            {
                urlparams["reason"] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, BuildQueryString(urlparams)));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete);
        }

        internal async Task<TransportUser> ModifyCurrentUserAsync(string username, string base64Avatar)
        {
            var pld = new RestUserUpdateCurrentPayload
            {
                Username = username,
                AvatarBase64 = base64Avatar
            };

            var route = string.Concat(Endpoints.Users, Endpoints.Me);
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, payload: JsonConvert.SerializeObject(pld));

            var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);

            return userRaw;
        }

        internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit, ulong? before, ulong? after)
        {
            var pld = new RestUserGuildListPayload
            {
                Limit = limit,
                After = after,
                Before = before
            };
            
            var route = string.Concat(Endpoints.Users, Endpoints.Me, Endpoints.Guilds);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get, payload: JsonConvert.SerializeObject(pld));

            if (Discord is DiscordClient)
            {
                var guildsRaw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response);
                var glds = guildsRaw.Select(xug => (Discord as DiscordClient)?._guilds[xug.Id]);
                return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(glds));
            }
            else
            {
                var guildsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuild>>(res.Response);
                var glds = guildsRaw.Select(xug =>
                {
                    xug.Discord = Discord;
                    return xug;
                });
                return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(glds));
            }
        }

        internal Task ModifyGuildMemberAsync(ulong guildId, ulong userId, string nick, IEnumerable<ulong> roleIds, bool? mute, bool? deaf, ulong? voiceChannelId, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick,
                RoleIds = roleIds,
                Deafen = deaf,
                Mute = mute,
                VoiceChannelId = voiceChannelId
            };

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members, "/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task ModifyCurrentMemberNicknameAsync(ulong guildId, string nick, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick
            };
            
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Members, Endpoints.Me, Endpoints.Nick);
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, payload: JsonConvert.SerializeObject(pld));
        }
        #endregion

        #region Roles
        internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Roles);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var rolesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = Discord; return xr; });

            return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(rolesRaw));
        }

        internal async Task<DiscordGuild> GetGuildAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guildRest = JsonConvert.DeserializeObject<DiscordGuild>(res.Response);

            if (Discord is DiscordClient dc)
            {
                await dc.OnGuildUpdateEventAsync(guildRest, rawMembers);
                return dc._guilds[guildRest.Id];
            }

            guildRest.Discord = Discord;
            return guildRest;
        }

        internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong guildId, ulong roleId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
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
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Roles, "/:role_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId, role_id = roleId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal Task DeleteRoleAsync(ulong guildId, ulong roleId, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Roles, "/:role_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId, role_id = roleId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }

        internal async Task<DiscordRole> CreateGuildRole(ulong guildId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
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
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Roles);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = Discord;

            return ret;
        }
        #endregion

        #region Prune
        internal async Task<int> GetGuildPruneCountAsync(ulong guildId, int days)
        {
            if (days < 0 || days > 30)
            {
                throw new ArgumentException("Prune inactivity days must be a number between 0 and 7.", nameof(days));
            }

            var urlparams = new Dictionary<string, string>
            {
                ["days"] = days.ToString(CultureInfo.InvariantCulture)
            };

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Prune);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, BuildQueryString(urlparams)));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        
        internal async Task<int> BeginGuildPruneAsync(ulong guildId, int days, string reason)
        {
            var urlparams = new Dictionary<string, string>
            {
                ["days"] = days.ToString(CultureInfo.InvariantCulture)
            };
            if (reason != null)
            {
                urlparams["reason"] = reason;
            }

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Prune);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path, BuildQueryString(urlparams)));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        #endregion

        #region GuildVarious
        internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Integrations);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var integrationsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = Discord; return xi; });

            return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrationsRaw));
        }

        internal async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guildId, string type, ulong id)
        {
            var pld = new RestGuildIntegrationAttachPayload
            {
                Type = type,
                Id = id
            };

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Integrations);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guildId, ulong integrationId, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
        {
            var pld = new RestGuildIntegrationModifyPayload
            {
                ExpireBehavior = expireBehaviour,
                ExpireGracePeriod = expireGracePeriod,
                EnableEmoticons = enableEmoticons
            };

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Integrations, "/:integration_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId, integration_id = integrationId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, payload: JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal Task DeleteGuildIntegrationAsync(ulong guildId, DiscordIntegration integration)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Integrations, "/:integration_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId, integration_id = integration.Id }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, payload: JsonConvert.SerializeObject(integration));
        }

        internal Task SyncGuildIntegrationAsync(ulong guildId, ulong integrationId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Integrations, "/:integration_id", Endpoints.Sync);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = guildId, integration_id = integrationId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post);
        }

        internal async Task<DiscordGuildEmbed> GetGuildEmbedAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Embed);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var embed = JsonConvert.DeserializeObject<DiscordGuildEmbed>(res.Response);

            return embed;
        }

        internal async Task<DiscordGuildEmbed> ModifyGuildEmbedAsync(ulong guildId, DiscordGuildEmbed embed)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Embed);
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, payload: JsonConvert.SerializeObject(embed));

            var embedRest = JsonConvert.DeserializeObject<DiscordGuildEmbed>(res.Response);

            return embedRest;
        }

        internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Regions);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var regionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regionsRaw));
        }

        internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Invites);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
        }
        #endregion

        #region Invite
        internal async Task<DiscordInvite> GetInviteAsync(string inviteCode)
        {
            var route = string.Concat(Endpoints.Invites, "/:invite_code");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { invite_code = inviteCode }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = Discord;

            return ret;
        }

        internal async Task<DiscordInvite> DeleteInviteAsync(string inviteCode, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Invites, "/:invite_code");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { invite_code = inviteCode }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = Discord;

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
            var route = string.Concat(Endpoints.Users, Endpoints.Me, Endpoints.Connections);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var connectionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = Discord; return xc; });

            return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connectionsRaw));
        }
        #endregion

        #region Voice
        internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        {
            var route = string.Concat(Endpoints.Voice, Endpoints.Regions);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
        }
        #endregion

        #region Webhooks
        internal async Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, string base64Avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64Avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Webhooks);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = Discord;
            ret.ApiClient = this;

            return ret;
        }

        internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channelId)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Webhooks);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
        }

        internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Webhooks);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
        }

        internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhookId)
        {
            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { webhook_id = webhookId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = Discord;
            ret.ApiClient = this;

            return ret;
        }

        // Auth header not required
        internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhookId, string webhookToken)
        {
            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id/:webhook_token");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { webhook_id = webhookId, webhook_token = webhookToken }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Token = webhookToken;
            ret.Id = webhookId;
            ret.Discord = Discord;
            ret.ApiClient = this;

            return ret;
        }

        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64Avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { webhook_id = webhookId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = Discord;
            ret.ApiClient = this;

            return ret;
        }

        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string webhookToken, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64Avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id/:webhook_token");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { webhook_id = webhookId, webhook_token = webhookToken }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));
            
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = Discord;
            ret.ApiClient = this;

            return ret;
        }

        internal Task DeleteWebhookAsync(ulong webhookId, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { webhook_id = webhookId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }

        internal Task DeleteWebhookAsync(ulong webhookId, string webhookToken, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id/:webhook_token");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { webhook_id = webhookId, webhook_token = webhookToken }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }

        internal Task ExecuteWebhookAsync(ulong webhookId, string webhookToken, string content, string username, string avatarUrl, bool? tts, IEnumerable<DiscordEmbed> embeds)
        {
            var discordEmbeds = embeds as DiscordEmbed[] ?? embeds.ToArray();

            foreach (var embed in discordEmbeds)
            {
                if (embed.Timestamp != null)
                {
                    embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
                }
            }

            var pld = new RestWebhookExecutePayload
            {
                Content = content,
                Username = username,
                AvatarUrl = avatarUrl,
                IsTts = tts,
                Embeds = discordEmbeds
            };

            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id/:webhook_token");
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { webhook_id = webhookId, webhook_token = webhookToken }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));
        }

        internal Task ExecuteWebhookSlackAsync(ulong webhookId, string webhookToken, string jsonPayload)
        {
            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id/:webhook_token", Endpoints.Slack);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { webhook_id = webhookId, webhook_token = webhookToken }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: jsonPayload);
        }

        internal Task ExecuteWebhookGithubAsync(ulong webhookId, string webhookToken, string jsonPayload)
        {
            var route = string.Concat(Endpoints.Webhooks, "/:webhook_id/:webhook_token", Endpoints.Github);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { webhook_id = webhookId, webhook_token = webhookToken }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: jsonPayload);
        }
        #endregion

        #region Reactions
        internal Task CreateReactionAsync(ulong channelId, ulong messageId, string emoji)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id", Endpoints.Reactions, "/:emoji", Endpoints.Me);
            var bucket = Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = channelId, message_id = messageId, emoji }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Put, ratelimitWaitOverride: 0.25);
        }

        internal Task DeleteOwnReactionAsync(ulong channelId, ulong messageId, string emoji)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id", Endpoints.Reactions, "/:emoji", Endpoints.Me);
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId, message_id = messageId, emoji }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, ratelimitWaitOverride: 0.25);
        }

        internal Task DeleteUserReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id", Endpoints.Reactions, "/:emoji/:user_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId, message_id = messageId, emoji, user_id = userId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers, ratelimitWaitOverride: 0.25);
        }

        internal async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, string emoji)
        {
            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id", Endpoints.Reactions, "/:emoji");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = channelId, message_id = messageId, emoji }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var reactersRaw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response);
            var reacters = (from xr in reactersRaw
                            let usr = new DiscordUser(xr) { Discord = Discord }
                            select Discord.UserCache.AddOrUpdate(xr.Id, usr, (id, old) =>
                            {
                                old.Username = usr.Username;
                                old.Discriminator = usr.Discriminator;
                                old.AvatarHash = usr.AvatarHash;
                                return old;
                            })).ToList();

            return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters));
        }

        internal Task DeleteAllReactionsAsync(ulong channelId, ulong messageId, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/:message_id", Endpoints.Reactions);
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = channelId, message_id = messageId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers, ratelimitWaitOverride: 0.25);
        }
        #endregion

        #region Emoji
        internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guildId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Emojis);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId }, out var path);
            
            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response);

            Discord.Guilds.TryGetValue(guildId, out var gld);
            var users = new Dictionary<ulong, DiscordUser>();
            var emojis = new List<DiscordGuildEmoji>();
            foreach (var xj in emojisRaw)
            {
                var xge = xj.ToObject<DiscordGuildEmoji>();
                xge.Guild = gld;

                var xtu = xj["user"]?.ToObject<TransportUser>();
                if (xtu != null)
                {
                    if (!users.ContainsKey(xtu.Id))
                    {
                        var xu = gld?.Members.FirstOrDefault(xm => xm.Id == xtu.Id) ?? new DiscordUser(xtu);
                        users[xu.Id] = xu;
                    }

                    xge.User = users[xtu.Id];
                }

                emojis.Add(xge);
            }

            return new ReadOnlyCollection<DiscordGuildEmoji>(emojis);
        }

        internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guildId, ulong emojiId)
        {
            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Emojis, "/:emoji_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = guildId, emoji_id = emojiId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            Discord.Guilds.TryGetValue(guildId, out var gld);

            var emojiRaw = JObject.Parse(res.Response);
            var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
            {
                emoji.User = gld?.Members.FirstOrDefault(xm => xm.Id == xtu.Id) ?? new DiscordUser(xtu);
            }

            return emoji;
        }

        internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guildId, string name, string imageb64, IEnumerable<ulong> roles, string reason)
        {
            var pld = new RestGuildEmojiCreatePayload
            {
                Name = name,
                ImageB64 = imageb64,
                Roles = roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Emojis);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, headers, JsonConvert.SerializeObject(pld));

            Discord.Guilds.TryGetValue(guildId, out var gld);

            var emojiRaw = JObject.Parse(res.Response);
            var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
            {
                emoji.User = gld?.Members.FirstOrDefault(xm => xm.Id == xtu.Id) ?? new DiscordUser(xtu);
            }
            else
            {
                emoji.User = Discord.CurrentUser;
            }

            return emoji;
        }

        internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guildId, ulong emojiId, string name, IEnumerable<ulong> roles, string reason)
        {
            var pld = new RestGuildEmojiModifyPayload
            {
                Name = name,
                Roles = roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Emojis, "/:emoji_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = guildId, emoji_id = emojiId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Patch, headers, JsonConvert.SerializeObject(pld));

            Discord.Guilds.TryGetValue(guildId, out var gld);

            var emojiRaw = JObject.Parse(res.Response);
            var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
            {
                emoji.User = gld?.Members.FirstOrDefault(xm => xm.Id == xtu.Id) ?? new DiscordUser(xtu);
            }

            return emoji;
        }

        internal Task DeleteGuildEmojiAsync(ulong guildId, ulong emojiId, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                headers[ReasonHeaderName] = reason;
            }

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Emojis, "/:emoji_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = guildId, emoji_id = emojiId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            return DoRequestAsync(Discord, bucket, url, RestRequestMethod.Delete, headers);
        }
        #endregion

        #region Misc
        internal Task<DiscordApplication> GetCurrentApplicationInfoAsync() =>
            GetApplicationInfoAsync("@me");

        internal Task<DiscordApplication> GetApplicationInfoAsync(ulong id) =>
            GetApplicationInfoAsync(id.ToString(CultureInfo.InvariantCulture));

        private async Task<DiscordApplication> GetApplicationInfoAsync(string appId)
        {
            var route = string.Concat(Endpoints.Oauth2, Endpoints.Applications, "/:app_id");
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { app_id = appId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var app = JsonConvert.DeserializeObject<DiscordApplication>(res.Response);
            app.Discord = Discord;

            return app;
        }

        internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication app)
        {
            var route = string.Concat(Endpoints.Oauth2, Endpoints.Applications, "/:app_id", Endpoints.Assets);
            var bucket = Rest.GetBucket(RestRequestMethod.Get, route, new { app_id = app.Id }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Get);

            var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
            var discordApplicationAssets = assets as DiscordApplicationAsset[] ?? assets.ToArray();
            foreach (var asset in discordApplicationAssets)
            {
                asset.Discord = app.Discord;
                asset.Application = app;
            }

            return new ReadOnlyCollection<DiscordApplicationAsset>(new List<DiscordApplicationAsset>(discordApplicationAssets));
        }

        internal async Task AcknowledgeMessageAsync(ulong msgId, ulong channelId)
        {
            await TokenSemaphore.WaitAsync();

            var pld = new AcknowledgePayload
            {
                Token = LastAckToken
            };

            var route = string.Concat(Endpoints.Channels, "/:channel_id", Endpoints.Messages, "/", msgId, Endpoints.Ack);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = channelId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<AcknowledgePayload>(res.Response);
            LastAckToken = ret.Token;

            TokenSemaphore.Release();
        }

        internal async Task AcknowledgeGuildAsync(ulong guildId)
        {
            await TokenSemaphore.WaitAsync();

            var pld = new AcknowledgePayload
            {
                Token = LastAckToken
            };

            var route = string.Concat(Endpoints.Guilds, "/:guild_id", Endpoints.Ack);
            var bucket = Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = guildId }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var res = await DoRequestAsync(Discord, bucket, url, RestRequestMethod.Post, payload: JsonConvert.SerializeObject(pld));

            if (res.ResponseCode != 204)
            {
                var ret = JsonConvert.DeserializeObject<AcknowledgePayload>(res.Response);
                LastAckToken = ret.Token;
            }
            else
            {
                LastAckToken = null;
            }

            TokenSemaphore.Release();
        }
        #endregion
    }
}
