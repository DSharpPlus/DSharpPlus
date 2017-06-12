using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.Objects.Transport;
using DSharpPlus.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    internal sealed class DiscordRestClient
    {
        internal DiscordClient Discord { get; }
        internal RestClient Rest { get; }

        internal DiscordRestClient(DiscordClient client)
        {
            this.Discord = client;
            this.Rest = new RestClient(client);
        }

        internal static string BuildQueryString(IDictionary<string, string> values) =>
            string.Concat("?", string.Join("&", values.Select(xkvp => string.Concat(WebUtility.UrlEncode(xkvp.Key), "=", WebUtility.UrlEncode(xkvp.Value)))));

        #region Guild
        internal async void InternalDeleteGuildAsync(ulong id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + $"/{id}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<DiscordGuild> InternalModifyGuild(ulong guild_id, string name = "", string region = "", int verification_level = -1, int default_message_notifications = -1,
            ulong afk_channel_id = 0, int afk_timeout = -1, string icon = "", ulong owner_id = 0, string splash = "")
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (name != "")
                j.Add("name", name);
            if (region != "")
                j.Add("region", region);
            if (verification_level != -1)
                j.Add("verification_level", verification_level);
            if (default_message_notifications != -1)
                j.Add("default_message_notifications", default_message_notifications);
            if (afk_channel_id != 0)
                j.Add("akf_channel_id", afk_channel_id);
            if (afk_timeout != -1)
                j.Add("akf_timeout", afk_timeout);
            if (icon != "")
                j.Add("icon", icon);
            if (owner_id != 0)
                j.Add("owner_id", owner_id);
            if (splash != "")
                j.Add("splash", splash);

            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);

            var jo = JObject.Parse(response.Response);
            var guild = jo.ToObject<DiscordGuild>();

            guild.Discord = this.Discord;

            if (guild._channels == null)
                guild._channels = new List<DiscordChannel>();
            foreach (var xc in guild.Channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this.Discord;
            }

            if (guild._roles == null)
                guild._roles = new List<DiscordRole>();
            foreach (var xr in guild.Roles)
                xr.Discord = this.Discord;

            var raw_members = (JArray)jo["members"];
            guild._members = raw_members == null ? new List<DiscordMember>() : raw_members.ToObject<IEnumerable<TransportMember>>()
                .Select(xtm => new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild.Id })
                .ToList();

            if (guild._emojis == null)
                guild._emojis = new List<DiscordEmoji>();
            foreach (var xe in guild.Emojis)
                xe.Discord = this.Discord;

            if (guild._presences == null)
                guild._presences = new List<DiscordPresence>();
            foreach (var xp in guild.Presences)
                xp.Discord = this.Discord;

            if (guild._voice_states == null)
                guild._voice_states = new List<DiscordVoiceState>();
            foreach (var xvs in guild.VoiceStates)
                xvs.Discord = this.Discord;

            return guild;
        }

        internal async Task<List<DiscordMember>> InternalGetGuildBans(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Bans;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordMember> bans = j.Select(xt => new DiscordMember(xt.ToObject<TransportMember>()) { Discord = this.Discord, _guild_id = guild_id }).ToList();
            return bans;
        }

        internal async Task InternalCreateGuildBan(ulong guild_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Bans + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PUT, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalRemoveGuildBan(ulong guild_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Bans + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalLeaveGuild(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me" + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<DiscordGuild> InternalCreateGuildAsync(string name, string region, string icon, int verification_level, int default_message_notifications)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "region", region },
                { "icon", icon },
                { "verification_level", verification_level },
                { "default_message_notifications", default_message_notifications }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);

            var jo = JObject.Parse(response.Response);
            var guild = jo.ToObject<DiscordGuild>();

            guild.Discord = this.Discord;

            if (guild._channels == null)
                guild._channels = new List<DiscordChannel>();
            foreach (var xc in guild.Channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this.Discord;
            }

            if (guild._roles == null)
                guild._roles = new List<DiscordRole>();
            foreach (var xr in guild.Roles)
                xr.Discord = this.Discord;

            var raw_members = (JArray)jo["members"];
            guild._members = raw_members == null ? new List<DiscordMember>() : raw_members.ToObject<IEnumerable<TransportMember>>()
                .Select(xtm => new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild.Id })
                .ToList();

            if (guild._emojis == null)
                guild._emojis = new List<DiscordEmoji>();
            foreach (var xe in guild.Emojis)
                xe.Discord = this.Discord;

            if (guild._presences == null)
                guild._presences = new List<DiscordPresence>();
            foreach (var xp in guild.Presences)
                xp.Discord = this.Discord;

            if (guild._voice_states == null)
                guild._voice_states = new List<DiscordVoiceState>();
            foreach (var xvs in guild.VoiceStates)
                xvs.Discord = this.Discord;

            return guild;
        }

        internal async Task<DiscordMember> InternalAddGuildMember(ulong guild_id, ulong user_id, string access_token, string nick = "", List<DiscordRole> roles = null,
            bool muted = false, bool deafened = false)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "access_token", access_token }
            };
            if (nick != "")
                j.Add("nick", nick);
            if (roles != null)
            {
                JArray r = new JArray();
                foreach (DiscordRole role in roles)
                {
                    r.Add(JsonConvert.SerializeObject(role));
                }
                j.Add("roles", r);
            }
            if (muted)
                j.Add("mute", true);
            if (deafened)
                j.Add("deaf", true);
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var tm = JsonConvert.DeserializeObject<TransportMember>(response.Response);
            return new DiscordMember(tm) { Discord = this.Discord, _guild_id = guild_id };
        }

        internal async Task<List<DiscordMember>> InternalListGuildMembers(ulong guild_id, int limit, int after)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + $"?limit={limit}&after={after}";
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();

            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordMember> members = ja.Select(xt => new DiscordMember(xt.ToObject<TransportMember>()) { Discord = this.Discord, _guild_id = guild_id }).ToList();
            return members;
        }

        internal async Task InternalAddGuildMemberRole(ulong guild_id, ulong user_id, ulong role_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + $"/{user_id}" + Endpoints.Roles + $"/{role_id}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PUT, headers);
            await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalRemoveGuildMemberRole(ulong guild_id, ulong user_id, ulong role_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + $"/{user_id}" + Endpoints.Roles + $"/{role_id}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            await this.Rest.HandleRequestAsync(request);
        }
        #endregion

        #region Channel
        internal async Task<DiscordChannel> InternalCreateGuildChannelAsync(ulong id, string name, ChannelType type)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Channel names can't be longer than 200 or shorter than 2 characters!");

            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + $"/{id}" + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject payload = new JObject { { "name", name }, { "type", type.ToString() }, { "permission_overwrites", null } };

            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordChannel> InternalGetChannel(ulong id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task InternalDeleteChannel(ulong id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<DiscordMessage> InternalGetMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordMessage> InternalCreateMessage(ulong channel_id, string content, bool tts, DiscordEmbed embed = null)
        {
            if (content.Length > 2000)
                throw new Exception("Messages are limited to a total of 2000 characters!");
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages;
            JObject j = new JObject
            {
                { "content", content },
                { "tts", tts }
            };
            if (embed != null)
            {
                JObject jembed = JObject.FromObject(embed);
                if (embed.Timestamp == new DateTime())
                {
                    jembed.Remove("timestamp");
                }
                else
                {
                    jembed["timestamp"] = embed.Timestamp.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
                }
                j.Add("embed", jembed);
            }
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordMessage> InternalUploadFile(ulong channel_id, Stream file_data, string file_name, string content = "", bool tts = false, DiscordEmbed embed = null)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages;
            var headers = Utils.GetBaseHeaders();
            var values = new Dictionary<string, string>();
            if (content != "")
                values.Add("content", content);
            if (tts)
                values.Add("tts", tts.ToString());
            Dictionary<string, Stream> file = new Dictionary<string, Stream>
            {
                { file_name, file_data }
            };
            WebRequest request = WebRequest.CreateMultipartRequest(this.Discord, url, HttpRequestMethod.POST, headers, values, file, embed);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordMessage> InternalUploadMultipleFiles(ulong channel_id, Dictionary<string, Stream> files, string content = "", bool tts = false, DiscordEmbed embed = null)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages;
            var headers = Utils.GetBaseHeaders();
            var values = new Dictionary<string, string>();
            if (content != "")
                values.Add("content", content);
            if (tts)
                values.Add("tts", tts.ToString());
            WebRequest request = WebRequest.CreateMultipartRequest(this.Discord, url, HttpRequestMethod.POST, headers, values, files, embed);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<List<DiscordChannel>> InternalGetGuildChannels(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordChannel> channels = new List<DiscordChannel>();
            foreach (JObject jj in j)
            {
                var ret = JsonConvert.DeserializeObject<DiscordChannel>(jj.ToString());
                ret.Discord = this.Discord;
                channels.Add(ret);
            }
            return channels;
        }

        internal async Task<DiscordChannel> InternalCreateChannel(ulong guild_id, string name, ChannelType type, int bitrate = 0, int user_limit = 0)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "type", type == ChannelType.Text ? "text" : "voice" }
            };
            if (type == ChannelType.Voice)
            {
                j.Add("bitrate", bitrate);
                j.Add("userlimit", user_limit);
            }
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        // TODO
        internal async Task InternalModifyGuildChannelPosition(ulong guild_id, ulong channel_id, int position)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "id", channel_id },
                { "position", position }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<List<DiscordMessage>> InternalGetChannelMessages(ulong channel_id, ulong around = 0, ulong before = 0, ulong after = 0, int limit = -1)
        {
            // ONLY ONE OUT OF around, before or after MAY BE USED.
            // THESE ARE MESSAGE IDs

            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages;
            var headers = Utils.GetBaseHeaders();
            var urlparams = new Dictionary<string, string>();
            if (around != 0)
                urlparams["around"] = around.ToString();
            if (before != 0)
                urlparams["before"] = before.ToString();
            if (after != 0)
                urlparams["after"] = after.ToString();
            if (limit > -1)
                urlparams["limit"] = limit.ToString();
            if (urlparams.Count > 0)
                url = string.Concat(url, BuildQueryString(urlparams));

            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordMessage> messages = new List<DiscordMessage>();
            foreach (JObject jo in ja)
            {
                var ret = jo.ToObject<DiscordMessage>();
                ret.Discord = this.Discord;
                messages.Add(ret);
            }
            return messages;
        }

        internal async Task<DiscordMessage> InternalGetChannelMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        // hi c:

        internal async Task<DiscordMessage> InternalEditMessage(ulong channel_id, ulong message_id, string content = null, DiscordEmbed embed = null)
        {
            if (content != null && content.Length > 2000)
                throw new Exception("Messages are limited to a total of 2000 characters!");
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (content != null)
                j.Add("content", content);
            if (embed != null)
            {
                JObject jembed = JObject.FromObject(embed);
                if (embed.Timestamp == new DateTime())
                {
                    jembed.Remove("timestamp");
                }
                else
                {
                    jembed["timestamp"] = embed.Timestamp.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
                }
                j.Add("embed", jembed);
            }
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task InternalDeleteMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalBulkDeleteMessages(ulong channel_id, IEnumerable<ulong> message_ids)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + Endpoints.BulkDelete;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "messages", new JArray(message_ids.ToArray()) }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<List<DiscordInvite>> InternalGetChannelInvites(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Invites;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordInvite> invites = new List<DiscordInvite>();
            foreach (JObject jo in ja)
            {
                var ret = JsonConvert.DeserializeObject<DiscordInvite>(jo.ToString());
                ret.Discord = this.Discord;
                invites.Add(ret);
            }
            return invites;
        }

        internal async Task<DiscordInvite> InternalCreateChannelInvite(ulong channel_id, int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Invites;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "max_age", max_age },
                { "max_uses", max_uses },
                { "temporary", temporary },
                { "unique", unique }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task InternalDeleteChannelPermission(ulong channel_id, ulong overwrite_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Permissions + "/" + overwrite_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalTriggerTypingIndicator(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Typing;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<List<DiscordMessage>> InternalGetPinnedMessages(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Pins;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordMessage> messages = new List<DiscordMessage>();
            foreach (JObject obj in j)
            {
                var ret = obj.ToObject<DiscordMessage>();
                ret.Discord = this.Discord;
                messages.Add(ret);
            }
            return messages;
        }

        internal async Task InternalAddPinnedChannelMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Pins + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PUT, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalDeletePinnedChannelMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Pins + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalGroupDMAddRecipient(ulong channel_id, ulong user_id, string access_token)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Recipients + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "access_token", access_token }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalGroupDMRemoveRecipient(ulong channel_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Recipients + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }


        internal async Task InternalEditChannelPermissions(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Permissions + "/" + overwrite_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "allow", (ulong)allow },
                { "deny", (ulong)deny },
                { "type", type }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<DiscordDmChannel> InternalCreateDM(ulong recipient_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me" + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "recipient_id", recipient_id }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordDmChannel> InternalCreateGroupDM(List<string> access_tokens)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me" + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JArray tokens = new JArray();
            foreach (string token in access_tokens)
            {
                tokens.Add(token);
            }
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, tokens.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }
        #endregion

        #region Member
        internal async Task<List<DiscordMember>> InternalGetGuildMembers(ulong guild_id, int member_count)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members;
            var headers = Utils.GetBaseHeaders();
            List<DiscordMember> result = new List<DiscordMember>();
            int pages = (int)Math.Ceiling((double)member_count / 1000);
            ulong? lastId = 0;

            for (int i = 0; i < pages; i++)
            {
                WebRequest request = WebRequest.CreateRequest(this.Discord, $"{url}?limit=1000&after={lastId.Value}", HttpRequestMethod.GET, headers);
                WebResponse response = await this.Rest.HandleRequestAsync(request);
                
                var items = JsonConvert.DeserializeObject<List<TransportMember>>(response.Response)
                    .Select(xtm => new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild_id });
                result.AddRange(items);
                lastId = result.LastOrDefault()?.Id;
            }
            return result;
        }

        internal Task<DiscordUser> InternalGetUser(ulong user) =>
            InternalGetUser(user.ToString());

        internal async Task<DiscordUser> InternalGetUser(string user)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + $"/{user}";
            var headers = Utils.GetBaseHeaders();

            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);

            var ret = JsonConvert.DeserializeObject<DiscordUser>(response.Response);

            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<DiscordMember> InternalGetGuildMember(ulong guild_id, ulong member_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + member_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var tm = JsonConvert.DeserializeObject<TransportMember>(response.Response);
            return new DiscordMember(tm)
            {
                Discord = this.Discord,
                _guild_id = guild_id
            };
        }

        internal async Task InternalRemoveGuildMember(ulong guild_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<DiscordUser> InternalGetCurrentUser()
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordUser>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordUser> InternalModifyCurrentUser(string username = "", string base64_avatar = "")
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me";
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (username != "")
                j.Add("", username);
            if (base64_avatar != "")
                j.Add("avatar", base64_avatar);
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordUser>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<List<DiscordGuild>> InternalGetCurrentUserGuilds()
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me" + Endpoints.Guilds;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            List<DiscordGuild> guilds = new List<DiscordGuild>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                var guild = j.ToObject<DiscordGuild>();

                guild.Discord = this.Discord;

                if (guild._channels == null)
                    guild._channels = new List<DiscordChannel>();
                foreach (var xc in guild.Channels)
                {
                    xc.GuildId = guild.Id;
                    xc.Discord = this.Discord;
                }

                if (guild._roles == null)
                    guild._roles = new List<DiscordRole>();
                foreach (var xr in guild.Roles)
                    xr.Discord = this.Discord;

                var raw_members = (JArray)j["members"];
                guild._members = raw_members == null ? new List<DiscordMember>() : raw_members.ToObject<IEnumerable<TransportMember>>()
                    .Select(xtm => new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild.Id })
                    .ToList();

                if (guild._emojis == null)
                    guild._emojis = new List<DiscordEmoji>();
                foreach (var xe in guild.Emojis)
                    xe.Discord = this.Discord;

                if (guild._presences == null)
                    guild._presences = new List<DiscordPresence>();
                foreach (var xp in guild.Presences)
                    xp.Discord = this.Discord;

                if (guild._voice_states == null)
                    guild._voice_states = new List<DiscordVoiceState>();
                foreach (var xvs in guild.VoiceStates)
                    xvs.Discord = this.Discord;

                guilds.Add(guild);
            }
            return guilds;
        }

        internal async Task InternalModifyGuildMember(ulong guild_id, ulong user_id, string nick = null,
            List<ulong> roles = null, bool muted = false, bool deafened = false, ulong voicechannel_id = 0)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (nick != null)
                j.Add("nick", nick);
            if (roles != null)
            {
                JArray r = new JArray();
                foreach (ulong role in roles)
                {
                    r.Add(role);
                }
                j.Add("roles", r);
            }
            if (muted)
                j.Add("mute", true);
            if (deafened)
                j.Add("deaf", true);
            if (voicechannel_id != 0)
                j.Add("channel_id", voicechannel_id);
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            await this.Rest.HandleRequestAsync(request);
        }
        #endregion

        #region Roles
        internal async Task<List<DiscordRole>> InternalGetGuildRolesAsync(ulong guild_id)
        {
            string url = $"{Utils.GetApiBaseUri(this.Discord)}{Endpoints.Guilds}/{guild_id}{Endpoints.Roles}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<List<DiscordRole>>(response.Response);
            foreach (var xr in ret)
                xr.Discord = this.Discord;
            return ret;
        }
        
        internal async Task<List<DiscordRole>> InternalModifyGuildRolePosition(ulong guild_id, ulong id, int position)
        {
            var jo = new JArray
            {
                new JObject
                {
                    { "id", id },
                    { "position", position }
                }
            };
            string url = $"{Utils.GetApiBaseUri(this.Discord)}{Endpoints.Guilds}/{guild_id}{Endpoints.Roles}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, jo.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<List<DiscordRole>>(response.Response);
            foreach (var xr in ret)
                xr.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordGuild> InternalGetGuildAsync(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            DiscordGuild guild = JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
            if (this.Discord._guilds.ContainsKey(guild_id))
            {
                this.Discord._guilds[guild_id] = guild;
            }
            else
            {
                this.Discord._guilds.Add(guild.Id, guild);
            }
            return guild;
        }

        internal async Task<DiscordGuild> InternalModifyGuild(string name = "", string region = "", string icon = "", int verification_level = -1,
            int default_message_notifications = -1, ulong afk_channel_id = 0, int afk_timeout = -1, ulong owner_id = 0, string splash = "")
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (name != "")
                j.Add("name", name);
            if (region != "")
                j.Add("region", region);
            if (icon != "")
                j.Add("icon", icon);
            if (verification_level > -1)
                j.Add("verification_level", verification_level);
            if (default_message_notifications > -1)
                j.Add("default_message_notifications", default_message_notifications);
            if (afk_channel_id > 0)
                j.Add("afk_channel_id", afk_channel_id);
            if (afk_timeout > -1)
                j.Add("afk_timeout", afk_timeout);
            if (owner_id > 0)
                j.Add("owner_id", owner_id);
            if (splash != "")
                j.Add("splash", splash);

            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordGuild> InternalDeleteGuild(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordRole> InternalModifyGuildRole(ulong guild_id, ulong role_id, string name, Permissions permissions, int position, int color, bool separate, bool mentionable)
        {
            string url = $"{Utils.GetApiBaseUri(this.Discord)}{Endpoints.Guilds}/{guild_id}{Endpoints.Roles}/{role_id}";
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "permissions", (ulong)permissions },
                { "position", position },
                { "color", color },
                { "hoist", separate },
                { "mentionable", mentionable }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordRole>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task InternalDeleteRole(ulong guild_id, ulong role_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Roles + "/" + role_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<DiscordRole> InternalCreateGuildRole(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Roles;
            var headers = Utils.GetBaseHeaders();

            var jo = new JObject();
            if (!string.IsNullOrWhiteSpace(name))
                jo.Add("name", name);
            if (permissions != null)
                jo.Add("permissions", (ulong)permissions.Value);
            if (color != null)
                jo.Add("color", color.Value);
            if (hoist != null)
                jo.Add("hoist", hoist.Value);
            if (mentionable != null)
                jo.Add("mentionable", mentionable.Value);

            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, jo.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordRole>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        #endregion

        #region Prune
        // TODO
        internal async Task<int> InternalGetGuildPruneCount(ulong guild_id, int days)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Prune;
            var headers = Utils.GetBaseHeaders();
            JObject payload = new JObject
            {
                { "days", days }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers, payload.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JObject j = JObject.Parse(response.Response);
            return int.Parse(j["pruned"].ToString());
        }

        // TODO
        internal async Task<int> InternalBeginGuildPrune(ulong guild_id, int days)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Prune;
            var headers = Utils.GetBaseHeaders();
            JObject payload = new JObject
            {
                { "days", days }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JObject j = JObject.Parse(response.Response);
            return int.Parse(j["pruned"].ToString());
        }
        #endregion

        #region GuildVarious
        internal async Task<List<DiscordIntegration>> InternalGetGuildIntegrations(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordIntegration> integrations = new List<DiscordIntegration>();
            foreach (JObject obj in j)
            {
                var ret = obj.ToObject<DiscordIntegration>();
                ret.Discord = this.Discord;
                integrations.Add(ret);
            }
            return integrations;
        }

        internal async Task<DiscordIntegration> InternalCreateGuildIntegration(ulong guild_id, string type, ulong id)
        {
            // Attach from user
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "type", type },
                { "id", id }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordIntegration> InternalModifyGuildIntegration(ulong guild_id, ulong integration_id, int expire_behaviour,
            int expire_grace_period, bool enable_emoticons)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations + "/" + integration_id;
            JObject j = new JObject
            {
                { "expire_behaviour", expire_behaviour },
                { "expire_grace_period", expire_grace_period },
                { "enable_emoticons", enable_emoticons }
            };
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task InternalDeleteGuildIntegration(ulong guild_id, DiscordIntegration integration)
        {
            ulong IntegrationID = integration.Id;
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations + "/" + IntegrationID;
            var headers = Utils.GetBaseHeaders();
            JObject j = JObject.FromObject(integration);
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalSyncGuildIntegration(ulong guild_id, ulong integration_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations + "/" + integration_id + Endpoints.Sync;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<DiscordGuildEmbed> InternalGetGuildEmbed(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Embed;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordGuildEmbed>(response.Response);
        }

        internal async Task<DiscordGuildEmbed> InternalModifyGuildEmbed(ulong guild_id, DiscordGuildEmbed embed)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Embed;
            var headers = Utils.GetBaseHeaders();
            JObject j = JObject.FromObject(embed);
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordGuildEmbed>(response.Response);
        }

        internal async Task<List<DiscordVoiceRegion>> InternalGetGuildVoiceRegions(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Regions;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordVoiceRegion> regions = new List<DiscordVoiceRegion>();
            foreach (JObject obj in j)
            {
                regions.Add(obj.ToObject<DiscordVoiceRegion>());
            }
            return regions;
        }

        internal async Task<List<DiscordInvite>> InternalGetGuildInvites(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Invites;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordInvite> invites = new List<DiscordInvite>();
            foreach (JObject obj in j)
            {
                var ret = obj.ToObject<DiscordInvite>();
                ret.Discord = this.Discord;
                invites.Add(ret);
            }
            return invites;
        }

        #endregion

        #region Invite
        internal async Task<DiscordInvite> InternalGetInvite(string invite_code)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Invites + "/" + invite_code;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordInvite> InternalDeleteInvite(string invite_code)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Invites + "/" + invite_code;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordInvite> InternalAcceptInvite(string invite_code)
        {
            // USER ONLY
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Invites + "/" + invite_code;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }
        #endregion

        #region Connections
        internal async Task<List<DiscordConnection>> InternalGetUsersConnections()
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me" + Endpoints.Connections;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            List<DiscordConnection> connections = new List<DiscordConnection>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                var ret = j.ToObject<DiscordConnection>();
                ret.Discord = this.Discord;
                connections.Add(ret);
            }
            return connections;
        }
        #endregion

        #region Voice
        internal async Task<List<DiscordVoiceRegion>> InternalListVoiceRegions()
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Voice + Endpoints.Regions;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            List<DiscordVoiceRegion> regions = new List<DiscordVoiceRegion>();
            JArray j = JArray.Parse(response.Response);
            foreach (JObject obj in j)
            {
                regions.Add(obj.ToObject<DiscordVoiceRegion>());
            }
            return regions;
        }
        #endregion

        #region Webhooks
        internal async Task<DiscordWebhook> InternalCreateWebhook(ulong channel_id, string name, string base64_avatar)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Webhook name has to be between 2 and 200 characters!");

            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Webhooks;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "avatar", base64_avatar }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);

            ret.Discord = this.Discord;

            return ret;
        }

        internal async Task<List<DiscordWebhook>> InternalGetChannelWebhooks(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Webhooks;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            List<DiscordWebhook> webhooks = new List<DiscordWebhook>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                var ret = j.ToObject<DiscordWebhook>();
                ret.Discord = this.Discord;
                webhooks.Add(ret);
            }
            return webhooks;
        }

        internal async Task<List<DiscordWebhook>> InternalGetGuildWebhooks(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Guilds + "/" + guild_id + Endpoints.Webhooks;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            List<DiscordWebhook> webhooks = new List<DiscordWebhook>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                var ret = j.ToObject<DiscordWebhook>();
                ret.Discord = this.Discord;
                webhooks.Add(ret);
            }
            return webhooks;
        }

        internal async Task<DiscordWebhook> InternalGetWebhook(ulong webhook_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        // Auth header not required
        internal async Task<DiscordWebhook> InternalGetWebhookWithToken(ulong webhook_id, string webhook_token)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            WebRequest request = WebRequest.CreateRequest(this.Discord, url);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            DiscordWebhook wh = JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
            wh.Token = webhook_token;
            wh.Id = webhook_id;
            return wh;
        }

        internal async Task<DiscordWebhook> InternalModifyWebhook(ulong webhook_id, string name, string base64_avatar)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Webhook name has to be between 2 and 200 characters!");
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "avatar", base64_avatar }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task<DiscordWebhook> InternalModifyWebhook(ulong webhook_id, string name, string base64_avatar, string webhook_token)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Webhook name has to be between 2 and 200 characters!");
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            JObject j = new JObject
            {
                { "name", name },
                { "avatar", base64_avatar }
            };
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, payload: j.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        internal async Task InternalDeleteWebhook(ulong webhook_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalDeleteWebhook(ulong webhook_id, string webhook_token)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalExecuteWebhook(ulong webhook_id, string webhook_token, string content = "", string username = "", string avatar_url = "",
            bool tts = false, List<DiscordEmbed> embeds = null)
        {
            if (content.Length > 2000)
                throw new Exception("Messages are limited to a total of 2000 characters!");

            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            JObject req = new JObject();
            if (content != "")
                req.Add("content", content);
            if (username != "")
                req.Add("username", username);
            if (avatar_url != "")
                req.Add("avatar_url", avatar_url);
            if (tts)
                req.Add("tts", tts);
            if (embeds != null)
            {
                JArray arr = new JArray();
                foreach (DiscordEmbed e in embeds)
                {
                    arr.Add(JsonConvert.SerializeObject(e));
                }
                req.Add(arr);
            }
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, payload: req.ToString());
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalExecuteWebhookSlack(ulong webhook_id, string webhook_token, string json_payload)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token + Endpoints.Slack;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, payload: json_payload);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalExecuteWebhookGithub(ulong webhook_id, string webhook_token, string json_payload)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token + Endpoints.Github;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.POST, payload: json_payload);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        #endregion

        #region Reactions
        internal async Task InternalCreateReaction(ulong channel_id, ulong message_id, string emoji)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji + "/@me";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PUT, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalDeleteOwnReaction(ulong channel_id, ulong message_id, string emoji)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji + "/@me";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task InternalDeleteUserReaction(ulong channel_id, ulong message_id, ulong user_id, string emoji)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }

        internal async Task<List<DiscordUser>> InternalGetReactions(ulong channel_id, ulong message_id, string emoji)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            List<DiscordUser> reacters = new List<DiscordUser>();
            foreach (JObject obj in JArray.Parse(response.Response))
            {
                reacters.Add(obj.ToObject<DiscordUser>());
            }
            return reacters;
        }

        internal async Task InternalDeleteAllReactions(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
        }
        #endregion
        
        #region Misc
        internal async Task<DiscordApplication> InternalGetApplicationInfo(string id = "@me")
        {
            string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.OAuth2 + Endpoints.Applications + "/" + id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this.Rest.HandleRequestAsync(request);
            return JObject.Parse(response.Response).ToObject<DiscordApplication>();
        }

        internal async Task InternalSetAvatarAsync(Stream image)
        {
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                var b64 = Convert.ToBase64String(ms.ToArray());

                string url = Utils.GetApiBaseUri(this.Discord) + Endpoints.Users + "/@me";
                var headers = Utils.GetBaseHeaders();
                JObject jo = new JObject
                {
                    { "avatar", $"data:image/jpeg;base64,{b64}" }
                };
                WebRequest request = WebRequest.CreateRequest(this.Discord, url, HttpRequestMethod.PATCH, headers, jo.ToString());
                WebResponse response = await this.Rest.HandleRequestAsync(request);
            }
        }
        #endregion
    }
}
