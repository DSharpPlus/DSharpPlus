using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net;

// huge credits to dvoraks 8th symphony for being a source of sanity in the trying times of
// fixing this absolute catastrophy up at least somewhat

public sealed class DiscordApiClient
{
    private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

    internal BaseDiscordClient? _discord { get; }
    internal RestClient _rest { get; }

    internal DiscordApiClient(BaseDiscordClient client, RestClient? rest = null)
    {
        this._discord = client;
        rest ??= new RestClient(client.Configuration, client.Logger);
        this._rest = rest;
    }

    internal DiscordApiClient
    (
        IWebProxy proxy,
        TimeSpan timeout,
        ILogger logger
    ) // This is for meta-clients, such as the webhook client
        => this._rest = new(proxy, timeout, logger);

    private DiscordMessage PrepareMessage(JToken msgRaw)
    {
        TransportUser author = msgRaw["author"]!.ToDiscordObject<TransportUser>();
        DiscordMessage message = msgRaw.ToDiscordObject<DiscordMessage>();

        message.Discord = this._discord!;

        this.PopulateMessage(author, message);

        JToken? referencedMsg = msgRaw["referenced_message"];

        if (message.MessageType == MessageType.Reply && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
        {
            TransportUser referencedAuthor = referencedMsg["author"]!.ToDiscordObject<TransportUser>();
            message.ReferencedMessage.Discord = this._discord!;
            this.PopulateMessage(referencedAuthor, message.ReferencedMessage);
        }

        if (message.Channel is not null)
        {
            return message;
        }

        message.Channel = !message._guildId.HasValue
            ? new DiscordDmChannel
            {
                Id = message.ChannelId,
                Discord = this._discord!,
                Type = ChannelType.Private
            }
            : new DiscordChannel
            {
                Id = message.ChannelId,
                GuildId = message._guildId,
                Discord = this._discord!
            };

        return message;
    }

    private void PopulateMessage(TransportUser author, DiscordMessage ret)
    {
        DiscordGuild? guild = ret.Channel?.Guild;

        //If this is a webhook, it shouldn't be in the user cache.
        if (author.IsBot && int.Parse(author.Discriminator) == 0)
        {
            ret.Author = new(author)
            {
                Discord = this._discord!
            };
        }
        else
        {
            // get and cache the user
            if (!this._discord!.UserCache.TryGetValue(author.Id, out DiscordUser? user))
            {
                user = new DiscordUser(author)
                {
                    Discord = this._discord
                };
            }

            this._discord.UserCache[author.Id] = user;

            // get the member object if applicable, if not set the message author to an user
            if (guild is not null)
            {
                if (!guild.Members.TryGetValue(author.Id, out DiscordMember? member))
                {
                    member = new(user)
                    {
                        Discord = this._discord,
                        _guild_id = guild.Id
                    };
                }

                ret.Author = member;
            }
            else
            {
                ret.Author = user!;
            }
        }

        ret.PopulateMentions();

        ret._reactions ??= new List<DiscordReaction>();
        foreach (DiscordReaction reaction in ret._reactions)
        {
            reaction.Emoji.Discord = this._discord!;
        }
    }

    #region Guild

    internal async ValueTask<IReadOnlyList<DiscordMember>> SearchMembersAsync
    (
        ulong guildId,
        string name,
        int? limit = null
    )
    {
        QueryUriBuilder builder = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{Endpoints.SEARCH}");
        builder.AddParameter("query", name);

        if (limit is not null)
        {
            builder.AddParameter("limit", limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{Endpoints.SEARCH}",
            Url = builder.Build(),
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        JArray array = JArray.Parse(response.Response!);
        IReadOnlyList<TransportMember> transportMembers = array.ToDiscordObject<IReadOnlyList<TransportMember>>();

        List<DiscordMember> members = new();

        foreach (TransportMember transport in transportMembers)
        {
            DiscordUser usr = new(transport.User) { Discord = this._discord! };

            this._discord!.UpdateUserCache(usr);

            members.Add(new DiscordMember(transport) { Discord = this._discord, _guild_id = guildId });
        }

        return members;
    }

    internal async ValueTask<DiscordBan> GetGuildBanAsync
    (
        ulong guildId,
        ulong userId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}/:user_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}/{userId}",
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        JObject json = JObject.Parse(response.Response!);

        DiscordBan ban = json.ToDiscordObject<DiscordBan>();

        if (!this._discord!.TryGetCachedUserInternal(ban.RawUser.Id, out DiscordUser? user))
        {
            user = new DiscordUser(ban.RawUser) { Discord = this._discord };
            user = this._discord.UpdateUserCache(user);
        }

        ban.User = user;

        return ban;
    }

    internal async ValueTask<DiscordGuild> CreateGuildAsync
    (
        string name,
        string regionId,
        Optional<string> iconb64 = default,
        VerificationLevel? verificationLevel = null,
        DefaultMessageNotifications? defaultMessageNotifications = null,
        SystemChannelFlags? systemChannelFlags = null
    )
    {
        RestGuildCreatePayload payload = new()
        {
            Name = name,
            RegionId = regionId,
            DefaultMessageNotifications = defaultMessageNotifications,
            VerificationLevel = verificationLevel,
            IconBase64 = iconb64,
            SystemChannelFlags = systemChannelFlags
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}",
            Url = $"{Endpoints.GUILDS}",
            Payload = DiscordJson.SerializeObject(payload),
            Method = HttpMethod.Post
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        JObject json = JObject.Parse(response.Response!);
        JArray rawMembers = (JArray)json["members"]!;
        DiscordGuild guild = json.ToDiscordObject<DiscordGuild>();

        if (this._discord is DiscordClient dc)
        {
            // this looks wrong. TODO: investigate double-fired event?
            await dc.OnGuildCreateEventAsync(guild, rawMembers, null!);
        }

        return guild;
    }

    internal async ValueTask<DiscordGuild> CreateGuildFromTemplateAsync
    (
        string templateCode,
        string name,
        Optional<string> iconb64 = default
    )
    {
        RestGuildCreateFromTemplatePayload payload = new()
        {
            Name = name,
            IconBase64 = iconb64
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{Endpoints.TEMPLATES}/:template_code",
            Url = $"{Endpoints.GUILDS}/{Endpoints.TEMPLATES}/{templateCode}",
            Payload = DiscordJson.SerializeObject(payload),
            Method = HttpMethod.Post
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        JObject json = JObject.Parse(res.Response!);
        JArray rawMembers = (JArray)json["members"]!;
        DiscordGuild guild = json.ToDiscordObject<DiscordGuild>();

        if (this._discord is DiscordClient dc)
        {
            await dc.OnGuildCreateEventAsync(guild, rawMembers, null!);
        }

        return guild;
    }

    internal async ValueTask DeleteGuildAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}",
            Url = $"{Endpoints.GUILDS}/{guildId}",
            Method = HttpMethod.Delete
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordGuild> ModifyGuildAsync
    (
        ulong guildId,
        Optional<string> name = default,
        Optional<string> region = default,
        Optional<VerificationLevel> verificationLevel = default,
        Optional<DefaultMessageNotifications> defaultMessageNotifications = default,
        Optional<MfaLevel> mfaLevel = default,
        Optional<ExplicitContentFilter> explicitContentFilter = default,
        Optional<ulong?> afkChannelId = default,
        Optional<int> afkTimeout = default,
        Optional<string> iconb64 = default,
        Optional<ulong> ownerId = default,
        Optional<string> splashb64 = default,
        Optional<ulong?> systemChannelId = default,
        Optional<string> banner = default,
        Optional<string> description = default,
        Optional<string> discoverySplash = default,
        Optional<IEnumerable<string>> features = default,
        Optional<string> preferredLocale = default,
        Optional<ulong?> publicUpdatesChannelId = default,
        Optional<ulong?> rulesChannelId = default,
        Optional<SystemChannelFlags> systemChannelFlags = default,
        string? reason = null
    )
    {
        RestGuildModifyPayload payload = new()
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
            DiscoverySplash = discoverySplash,
            Features = features,
            PreferredLocale = preferredLocale,
            PublicUpdatesChannelId = publicUpdatesChannelId,
            RulesChannelId = rulesChannelId,
            SystemChannelFlags = systemChannelFlags
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}",
            Url = $"{Endpoints.GUILDS}/{guildId}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload),
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        JObject json = JObject.Parse(res.Response!);
        JArray rawMembers = (JArray)json["members"]!;
        DiscordGuild guild = json.ToDiscordObject<DiscordGuild>();
        foreach (DiscordRole r in guild._roles.Values)
        {
            r._guild_id = guild.Id;
        }

        if (this._discord is DiscordClient dc)
        {
            await dc.OnGuildUpdateEventAsync(guild, rawMembers!);
        }

        return guild;
    }

    internal async ValueTask<IReadOnlyList<DiscordBan>> GetGuildBansAsync
    (
        ulong guildId,
        int? limit = null,
        ulong? before = null,
        ulong? after = null
    )
    {
        QueryUriBuilder builder = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}");

        if (limit is not null)
        {
            builder.AddParameter("limit", limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (before is not null)
        {
            builder.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (after is not null)
        {
            builder.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}",
            Url = builder.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordBan> bansRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordBan>>(res.Response!)!
        .Select(xb =>
        {
            if (!this._discord!.TryGetCachedUserInternal(xb.RawUser.Id, out DiscordUser? user))
            {
                user = new DiscordUser(xb.RawUser) { Discord = this._discord };
                user = this._discord.UpdateUserCache(user);
            }

            xb.User = user;
            return xb;
        });

        ReadOnlyCollection<DiscordBan> bans = new(new List<DiscordBan>(bansRaw));

        return bans;
    }

    internal async ValueTask CreateGuildBanAsync
    (
        ulong guildId,
        ulong userId,
        int deleteMessageDays,
        string? reason = null
    )
    {
        if (deleteMessageDays < 0 || deleteMessageDays > 7)
        {
            throw new ArgumentException("Delete message days must be a number between 0 and 7.", nameof(deleteMessageDays));
        }

        QueryUriBuilder builder = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}/{userId}");

        builder.AddParameter("delete_message_days", deleteMessageDays.ToString(CultureInfo.InvariantCulture));

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}/:user_id",
            Url = builder.Build(),
            Method = HttpMethod.Put,
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask RemoveGuildBanAsync
    (
        ulong guildId,
        ulong userId,
        string? reason = null
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}/:user_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.BANS}/{userId}",
            Method = HttpMethod.Delete,
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask LeaveGuildAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.GUILDS}/{guildId}",
            Url = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.GUILDS}/{guildId}",
            Method = HttpMethod.Delete
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordMember> AddGuildMemberAsync
    (
        ulong guildId,
        ulong userId,
        string accessToken,
        bool? muted = null,
        bool? deafened = null,
        string? nick = null,
        IEnumerable<DiscordRole>? roles = null
    )
    {
        RestGuildMemberAddPayload payload = new()
        {
            AccessToken = accessToken,
            Nickname = nick ?? "",
            Roles = roles ?? new List<DiscordRole>(),
            Deaf = deafened ?? false,
            Mute = muted ?? false
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/:user_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/:{userId}",
            Method = HttpMethod.Put,
            Payload = DiscordJson.SerializeObject(payload)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        TransportMember transport = JsonConvert.DeserializeObject<TransportMember>(res.Response!)!;

        return new DiscordMember(transport) { Discord = this._discord!, _guild_id = guildId };
    }

    internal async ValueTask<IReadOnlyList<TransportMember>> ListGuildMembersAsync
    (
        ulong guildId,
        int? limit = null,
        ulong? after = null
    )
    {
        QueryUriBuilder builder = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}");

        if (limit is not null && limit > 0)
        {
            builder.AddParameter("limit", limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (after is not null)
        {
            builder.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}",
            Url = builder.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        List<TransportMember> rawMembers = JsonConvert.DeserializeObject<List<TransportMember>>(res.Response!)!;
        return new ReadOnlyCollection<TransportMember>(rawMembers);
    }

    internal async ValueTask AddGuildMemberRoleAsync
    (
        ulong guildId,
        ulong userId,
        ulong roleId,
        string? reason = null
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/:user_id/{Endpoints.ROLES}/:role_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{userId}/{Endpoints.ROLES}/{roleId}",
            Method = HttpMethod.Put,
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask RemoveGuildMemberRoleAsync
    (
        ulong guildId,
        ulong userId,
        ulong roleId,
        string reason
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/:user_id/{Endpoints.ROLES}/:role_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{userId}/{Endpoints.ROLES}/{roleId}",
            Method = HttpMethod.Delete,
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask ModifyGuildChannelPositionAsync
    (
        ulong guildId,
        IEnumerable<RestGuildChannelReorderPayload> payload,
        string? reason = null
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.CHANNELS}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.CHANNELS}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload),
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    // TODO: should probably return an IReadOnlyList here, unsure as to the extent of the breaking change
    internal async ValueTask<DiscordRole[]> ModifyGuildRolePositionsAsync
    (
        ulong guildId,
        IEnumerable<RestGuildRoleReorderPayload> newRolePositions,
        string? reason = null
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(newRolePositions),
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordRole[] ret = JsonConvert.DeserializeObject<DiscordRole[]>(res.Response!)!;
        foreach (DiscordRole role in ret)
        {
            role.Discord = this._discord!;
            role._guild_id = guildId;
        }

        return ret;
    }

    internal async ValueTask<AuditLog> GetAuditLogsAsync
    (
        ulong guildId,
        int limit,
        ulong? after = null,
        ulong? before = null,
        ulong? userId = null,
        DiscordAuditLogActionType? actionType = null
    )
    {
        QueryUriBuilder builder = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUDIT_LOGS}");

        builder.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

        if (after is not null)
        {
            builder.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (before is not null)
        {
            builder.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (userId is not null)
        {
            builder.AddParameter("user_id", userId.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (actionType is not null)
        {
            builder.AddParameter("action_type", ((int)actionType.Value).ToString(CultureInfo.InvariantCulture));
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUDIT_LOGS}",
            Url = builder.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<AuditLog>(res.Response!)!;
    }

    internal async ValueTask<DiscordInvite> GetGuildVanityUrlAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.VANITY_URL}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.VANITY_URL}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordInvite>(res.Response!)!;
    }

    internal async ValueTask<DiscordWidget> GetGuildWidgetAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WIDGET_JSON}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WIDGET_JSON}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        // TODO: this should really be cleaned up
        JObject json = JObject.Parse(res.Response!);
        JArray rawChannels = (JArray)json["channels"]!;

        DiscordWidget ret = json.ToDiscordObject<DiscordWidget>();
        ret.Discord = this._discord!;
        ret.Guild = this._discord!.Guilds[guildId];

        ret.Channels = ret.Guild is null
            ? rawChannels.Select(r => new DiscordChannel
            {
                Id = (ulong)r["id"]!,
                Name = r["name"]!.ToString(),
                Position = (int)r["position"]!
            }).ToList()
            : rawChannels.Select(r =>
            {
                DiscordChannel c = ret.Guild.GetChannel((ulong)r["id"]!);
                c.Position = (int)r["position"]!;
                return c;
            }).ToList();

        return ret;
    }

    internal async ValueTask<DiscordWidgetSettings> GetGuildWidgetSettingsAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WIDGET}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WIDGET}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordWidgetSettings ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response!)!;
        ret.Guild = this._discord!.Guilds[guildId];

        return ret;
    }

    internal async ValueTask<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync
    (
        ulong guildId,
        bool? isEnabled = null,
        ulong? channelId = null,
        string? reason = null
    )
    {
        RestGuildWidgetSettingsPayload payload = new()
        {
            Enabled = isEnabled,
            ChannelId = channelId
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WIDGET}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WIDGET}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload),
            Headers = reason is null
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordWidgetSettings ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response!)!;
        ret.Guild = this._discord!.Guilds[guildId];

        return ret;
    }

    internal async ValueTask<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordGuildTemplate> templates =
            JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response!)!;

        return new ReadOnlyCollection<DiscordGuildTemplate>(new List<DiscordGuildTemplate>(templates));
    }

    internal async ValueTask<DiscordGuildTemplate> CreateGuildTemplateAsync
    (
        ulong guildId,
        string name,
        string description
    )
    {
        RestGuildTemplateCreateOrModifyPayload payload = new()
        {
            Name = name,
            Description = description
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}",
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(payload)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response!)!;
    }

    internal async ValueTask<DiscordGuildTemplate> SyncGuildTemplateAsync
    (
        ulong guildId,
        string templateCode
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}/:template_code",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}/{templateCode}",
            Method = HttpMethod.Put
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response!)!;
    }

    internal async ValueTask<DiscordGuildTemplate> ModifyGuildTemplateAsync
    (
        ulong guildId,
        string templateCode,
        string? name = null,
        string? description = null
    )
    {
        RestGuildTemplateCreateOrModifyPayload payload = new()
        {
            Name = name,
            Description = description
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}/:template_code",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}/{templateCode}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response!)!;
    }

    internal async ValueTask<DiscordGuildTemplate> DeleteGuildTemplateAsync
    (
        ulong guildId,
        string templateCode
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}/:template_code",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.TEMPLATES}/{templateCode}",
            Method = HttpMethod.Delete
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response!)!;
    }

    internal async ValueTask<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBER_VERIFICATION}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBER_VERIFICATION}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response!)!;
    }

    internal async ValueTask<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync
    (
        ulong guildId,
        Optional<bool> enabled = default,
        Optional<DiscordGuildMembershipScreeningField[]> fields = default,
        Optional<string> description = default
    )
    {
        RestGuildMembershipScreeningFormModifyPayload payload = new()
        {
            Enabled = enabled,
            Description = description,
            Fields = fields
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBER_VERIFICATION}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBER_VERIFICATION}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response!)!;
    }

    internal async ValueTask<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WELCOME_SCREEN}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WELCOME_SCREEN}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response!)!;
    }

    internal async ValueTask<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync
    (
        ulong guildId,
        Optional<bool> enabled = default,
        Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> welcomeChannels = default,
        Optional<string> description = default,
        string? reason = null
    )
    {
        RestGuildWelcomeScreenModifyPayload payload = new()
        {
            Enabled = enabled,
            WelcomeChannels = welcomeChannels,
            Description = description
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WELCOME_SCREEN}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WELCOME_SCREEN}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload),
            Headers = reason is null
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response!)!;
    }

    internal async ValueTask UpdateCurrentUserVoiceStateAsync
    (
        ulong guildId,
        ulong channelId,
        bool? suppress = null,
        DateTimeOffset? requestToSpeakTimestamp = null
    )
    {
        RestGuildUpdateCurrentUserVoiceStatePayload payload = new()
        {
            ChannelId = channelId,
            Suppress = suppress,
            RequestToSpeakTimestamp = requestToSpeakTimestamp
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.VOICE_STATES}/@me",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.VOICE_STATES}/@me",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload)
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask UpdateUserVoiceStateAsync
    (
        ulong guildId,
        ulong userId,
        ulong channelId,
        bool? suppress = null
    )
    {
        RestGuildUpdateUserVoiceStatePayload payload = new()
        {
            ChannelId = channelId,
            Suppress = suppress
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.VOICE_STATES}/:user_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.VOICE_STATES}/{userId}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload)
        };

        _ = await this._rest.ExecuteRequestAsync(request);
    }
    #endregion

    #region Stickers

    internal async ValueTask<DiscordMessageSticker> GetGuildStickerAsync
    (
        ulong guildId,
        ulong stickerId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}/:sticker_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}/{stickerId}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        JObject json = JObject.Parse(res.Response!);

        DiscordMessageSticker ret = json.ToDiscordObject<DiscordMessageSticker>();

        if (json["user"] is JObject jusr) // Null = Missing stickers perm //
        {
            TransportUser tsr = jusr.ToDiscordObject<TransportUser>();
            DiscordUser usr = new(tsr) { Discord = this._discord! };
            ret.User = usr;
        }

        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<DiscordMessageSticker> GetStickerAsync
    (
        ulong stickerId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.STICKERS}/:sticker_id",
            Url = $"{Endpoints.STICKERS}/{stickerId}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        JObject json = JObject.Parse(res.Response!);

        DiscordMessageSticker ret = json.ToDiscordObject<DiscordMessageSticker>();

        if (json["user"] is JObject jusr) // Null = Missing stickers perm //
        {
            TransportUser tsr = jusr.ToDiscordObject<TransportUser>();
            DiscordUser usr = new(tsr) { Discord = this._discord! };
            ret.User = usr;
        }

        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<IReadOnlyList<DiscordMessageStickerPack>> GetStickerPacksAsync()
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.STICKERPACKS}",
            Url = $"{Endpoints.STICKERPACKS}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        JArray json = (JArray)JObject.Parse(res.Response!)["sticker_packs"]!;
        DiscordMessageStickerPack[] ret = json.ToDiscordObject<DiscordMessageStickerPack[]>();

        return ret;
    }

    internal async ValueTask<IReadOnlyList<DiscordMessageSticker>> GetGuildStickersAsync
    (
        ulong guildId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}",
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        JArray json = JArray.Parse(res.Response!);

        DiscordMessageSticker[] ret = json.ToDiscordObject<DiscordMessageSticker[]>();

        for (int i = 0; i < ret.Length; i++)
        {
            DiscordMessageSticker sticker = ret[i];
            sticker.Discord = this._discord!;

            if (json[i]["user"] is JObject jusr) // Null = Missing stickers perm //
            {
                TransportUser transportUser = jusr.ToDiscordObject<TransportUser>();
                DiscordUser user = new(transportUser)
                {
                    Discord = this._discord!
                };

                // The sticker would've already populated, but this is just to ensure everything is up to date
                sticker.User = user;
            }
        }

        return ret;
    }

    internal async ValueTask<DiscordMessageSticker> CreateGuildStickerAsync
    (
        ulong guildId,
        string name,
        string description,
        string tags,
        DiscordMessageFile file,
        string? reason = null
    )
    {
        MultipartRestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}",
            Method = HttpMethod.Post,
            Headers = reason is null
                ? null
                : new Dictionary<string, string>()
                {
                    [REASON_HEADER_NAME] = reason
                },
            Files = new DiscordMessageFile[]
            {
                file
            },
            Values = new Dictionary<string, string>()
            {
                ["name"] = name,
                ["description"] = description,
                ["tags"] = tags,
            }
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        JObject json = JObject.Parse(res.Response!);

        DiscordMessageSticker ret = json.ToDiscordObject<DiscordMessageSticker>();

        if (json["user"] is JObject rawUser) // Null = Missing stickers perm //
        {
            TransportUser transportUser = rawUser.ToDiscordObject<TransportUser>();

            DiscordUser user = new(transportUser)
            {
                Discord = this._discord!
            };

            ret.User = user;
        }

        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordMessageSticker> ModifyStickerAsync
    (
        ulong guildId,
        ulong stickerId,
        Optional<string> name = default,
        Optional<string> description = default,
        Optional<string> tags = default,
        string? reason = null
    )
    {
        RestStickerModifyPayload payload = new()
        {
            Name = name,
            Description = description,
            Tags = tags
        };

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}/:sticker_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}/{stickerId}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(payload),
            Headers = reason is null
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordMessageSticker ret = JObject.Parse(res.Response!).ToDiscordObject<DiscordMessageSticker>();
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask DeleteStickerAsync
    (
        ulong guildId,
        ulong stickerId,
        string? reason = null
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}/:sticker_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.STICKERS}/{stickerId}",
            Method = HttpMethod.Delete,
            Headers = reason is null
                ? null
                : new Dictionary<string, string>()
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    #endregion

    #region Channel
    internal async ValueTask<DiscordChannel> CreateGuildChannelAsync
    (
        ulong guildId,
        string name,
        ChannelType type,
        ulong? parent,
        Optional<string> topic,
        int? bitrate,
        int? userLimit,
        IEnumerable<DiscordOverwriteBuilder> overwrites,
        bool? nsfw,
        Optional<int?> perUserRateLimit,
        VideoQualityMode? qualityMode,
        int? position,
        string reason,
        AutoArchiveDuration? defaultAutoArchiveDuration,
        DefaultReaction? defaultReactionEmoji,
        IEnumerable<DiscordForumTagBuilder> forumTags,
        DefaultSortOrder? defaultSortOrder

    )
    {
        List<DiscordRestOverwrite> restOverwrites = new();
        if (overwrites != null)
        {
            foreach (DiscordOverwriteBuilder ow in overwrites)
            {
                restOverwrites.Add(ow.Build());
            }
        }

        RestChannelCreatePayload pld = new()
        {
            Name = name,
            Type = type,
            Parent = parent,
            Topic = topic,
            Bitrate = bitrate,
            UserLimit = userLimit,
            PermissionOverwrites = restOverwrites,
            Nsfw = nsfw,
            PerUserRateLimit = perUserRateLimit,
            QualityMode = qualityMode,
            Position = position,
            DefaultAutoArchiveDuration = defaultAutoArchiveDuration,
            DefaultReaction = defaultReactionEmoji,
            AvailableTags = forumTags,
            DefaultSortOrder = defaultSortOrder
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.CHANNELS}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.CHANNELS}",
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordChannel ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response!)!;
        ret.Discord = this._discord!;

        foreach (DiscordOverwrite xo in ret._permissionOverwrites)
        {
            xo.Discord = this._discord!;
            xo._channel_id = ret.Id;
        }

        return ret;
    }

    internal async ValueTask ModifyChannelAsync
    (
        ulong channelId,
        string name,
        int? position = null,
        Optional<string> topic = default,
        bool? nsfw = null,
        Optional<ulong?> parent = default,
        int? bitrate = null,
        int? userLimit = null,
        Optional<int?> perUserRateLimit = default,
        Optional<string> rtcRegion = default,
        VideoQualityMode? qualityMode = null,
        Optional<ChannelType> type = default,
        IEnumerable<DiscordOverwriteBuilder>? permissionOverwrites = null,
        Optional<ChannelFlags> flags = default,
        IEnumerable<DiscordForumTagBuilder>? availableTags = null,
        Optional<AutoArchiveDuration?> defaultAutoArchiveDuration = default,
        Optional<DefaultReaction?> defaultReactionEmoji = default,
        Optional<int> defaultPerUserRatelimit = default,
        Optional<DefaultSortOrder?> defaultSortOrder = default,
        Optional<DefaultForumLayout> defaultForumLayout = default,
        string? reason = null
    )
    {
        List<DiscordRestOverwrite>? restOverwrites = null;
        if (permissionOverwrites is not null)
        {
            restOverwrites = new();
            foreach (DiscordOverwriteBuilder ow in permissionOverwrites)
            {
                restOverwrites.Add(ow.Build());
            }
        }

        RestChannelModifyPayload pld = new()
        {
            Name = name,
            Position = position,
            Topic = topic,
            Nsfw = nsfw,
            Parent = parent,
            Bitrate = bitrate,
            UserLimit = userLimit,
            PerUserRateLimit = perUserRateLimit,
            RtcRegion = rtcRegion,
            QualityMode = qualityMode,
            Type = type,
            PermissionOverwrites = restOverwrites,
            Flags = flags,
            AvailableTags = availableTags,
            DefaultAutoArchiveDuration = defaultAutoArchiveDuration,
            DefaultReaction = defaultReactionEmoji,
            DefaultPerUserRateLimit = defaultPerUserRatelimit,
            DefaultForumLayout = defaultForumLayout,
            DefaultSortOrder = defaultSortOrder
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.CHANNELS}/{channelId}",
            Url = $"{Endpoints.CHANNELS}/{channelId}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask ModifyThreadChannelAsync
    (
        ulong channelId,
        string name,
        int? position = null,
        Optional<string> topic = default,
        bool? nsfw = null,
        Optional<ulong?> parent = default,
        int? bitrate = null,
        int? userLimit = null,
        Optional<int?> perUserRateLimit = default,
        Optional<string> rtcRegion = default,
        VideoQualityMode? qualityMode = null,
        Optional<ChannelType> type = default,
        IEnumerable<DiscordOverwriteBuilder>? permissionOverwrites = null,
        bool? isArchived = null,
        AutoArchiveDuration? autoArchiveDuration = null,
        bool? locked = null,
        IEnumerable<ulong>? appliedTags = null,
        string? reason = null
    )
    {
        List<DiscordRestOverwrite>? restOverwrites = null;
        if (permissionOverwrites is not null)
        {
            restOverwrites = new List<DiscordRestOverwrite>();
            foreach (DiscordOverwriteBuilder ow in permissionOverwrites)
            {
                restOverwrites.Add(ow.Build());
            }
        }

        RestThreadChannelModifyPayload pld = new()
        {
            Name = name,
            Position = position,
            Topic = topic,
            Nsfw = nsfw,
            Parent = parent,
            Bitrate = bitrate,
            UserLimit = userLimit,
            PerUserRateLimit = perUserRateLimit,
            RtcRegion = rtcRegion,
            QualityMode = qualityMode,
            Type = type,
            PermissionOverwrites = restOverwrites,
            IsArchived = isArchived,
            ArchiveDuration = autoArchiveDuration,
            Locked = locked,
            AppliedTags = appliedTags
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers.Add(REASON_HEADER_NAME, reason);
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.CHANNELS}/{channelId}",
            Url = $"{Endpoints.CHANNELS}/{channelId}",
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordScheduledGuildEvent>> GetScheduledGuildEventsAsync
    (
        ulong guildId,
        bool withUserCounts = false
    )
    {
        QueryUriBuilder url = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}");
        url.AddParameter("with_user_count", withUserCounts.ToString());

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}",
            Url = url.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordScheduledGuildEvent[] ret = JsonConvert.DeserializeObject<DiscordScheduledGuildEvent[]>(res.Response!)!;

        foreach (DiscordScheduledGuildEvent? scheduledGuildEvent in ret)
        {
            scheduledGuildEvent.Discord = this._discord!;

            if (scheduledGuildEvent.Creator is not null)
            {
                scheduledGuildEvent.Creator.Discord = this._discord!;
            }
        }

        return ret.AsReadOnly();
    }

    internal async ValueTask<DiscordScheduledGuildEvent> CreateScheduledGuildEventAsync
    (
        ulong guildId,
        string name,
        string description,
        DateTimeOffset startTime,
        ScheduledGuildEventType type,
        ScheduledGuildEventPrivacyLevel privacyLevel,
        DiscordScheduledGuildEventMetadata? metadata = null,
        DateTimeOffset? endTime = null,
        ulong? channelId = null,
        Stream? image = null,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();

        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestScheduledGuildEventCreatePayload pld = new()
        {
            Name = name,
            Description = description,
            ChannelId = channelId,
            StartTime = startTime,
            EndTime = endTime,
            Type = type,
            PrivacyLevel = privacyLevel,
            Metadata = metadata
        };

        if (image is not null)
        {
            using ImageTool imageTool = new(image);

            pld.CoverImage = imageTool.GetBase64();
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}",
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordScheduledGuildEvent ret = JsonConvert.DeserializeObject<DiscordScheduledGuildEvent>(res.Response!)!;

        ret.Discord = this._discord!;

        if (ret.Creator is not null)
        {
            ret.Creator.Discord = this._discord!;
        }

        return ret;
    }

    internal async ValueTask DeleteScheduledGuildEventAsync
    (
        ulong guildId,
        ulong guildScheduledEventId
    )
    {
        RestRequest request = new()
        {
            Route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/:guild_scheduled_event_id",
            Url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/{guildScheduledEventId}",
            Method = HttpMethod.Delete
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordUser>> GetScheduledGuildEventUsersAsync
    (
        ulong guildId,
        ulong guildScheduledEventId,
        bool withMembers = false,
        int limit = 1,
        ulong? before = null,
        ulong? after = null
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/:guild_scheduled_event_id/{Endpoints.USERS}";

        QueryUriBuilder url = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/{guildScheduledEventId}/{Endpoints.USERS}");

        url.AddParameter("with_members", withMembers.ToString());

        if (limit > 0)
        {
            url.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));
        }

        if (before != null)
        {
            url.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (after != null)
        {
            url.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));
        }

        RestRequest request = new()
        {
            Route = route,
            Url = url.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        JToken jto = JToken.Parse(res.Response!);

        return (jto as JArray ?? jto["users"] as JArray)!
            .Select
            (
                j => (DiscordUser)j.SelectToken("member")?.ToDiscordObject<DiscordMember>()!
                    ?? j.SelectToken("user")!.ToDiscordObject<DiscordUser>()
            )
            .ToArray();
    }

    internal async ValueTask<DiscordScheduledGuildEvent> GetScheduledGuildEventAsync
    (
        ulong guildId,
        ulong guildScheduledEventId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/:guild_scheduled_event_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/{guildScheduledEventId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordScheduledGuildEvent ret = JsonConvert.DeserializeObject<DiscordScheduledGuildEvent>(res.Response!)!;

        ret.Discord = this._discord!;

        if (ret.Creator is not null)
        {
            ret.Creator.Discord = this._discord!;
        }

        return ret;
    }

    internal async ValueTask<DiscordScheduledGuildEvent> ModifyScheduledGuildEventAsync
    (
        ulong guildId,
        ulong guildScheduledEventId,
        Optional<string> name = default,
        Optional<string> description = default,
        Optional<ulong?> channelId = default,
        Optional<DateTimeOffset> startTime = default,
        Optional<DateTimeOffset> endTime = default,
        Optional<ScheduledGuildEventType> type = default,
        Optional<ScheduledGuildEventPrivacyLevel> privacyLevel = default,
        Optional<DiscordScheduledGuildEventMetadata> metadata = default,
        Optional<ScheduledGuildEventStatus> status = default,
        Optional<Stream> coverImage = default,
        string? reason = null
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/:guild_scheduled_event_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EVENTS}/{guildScheduledEventId}";

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestScheduledGuildEventModifyPayload pld = new()
        {
            Name = name,
            Description = description,
            ChannelId = channelId,
            StartTime = startTime,
            EndTime = endTime,
            Type = type,
            PrivacyLevel = privacyLevel,
            Metadata = metadata,
            Status = status
        };

        if (coverImage.HasValue)
        {
            using ImageTool imageTool = new(coverImage.Value);

            pld.CoverImage = imageTool.GetBase64();
        }

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordScheduledGuildEvent ret = JsonConvert.DeserializeObject<DiscordScheduledGuildEvent>(res.Response!)!;

        ret.Discord = this._discord!;

        if (ret.Creator is not null)
        {
            ret.Creator.Discord = this._discord!;
        }

        return ret;
    }

    internal async ValueTask<DiscordChannel> GetChannelAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}";
        string url = $"{Endpoints.CHANNELS}/{channelId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordChannel ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response!)!;

        // this is really weird, we should consider doing this better
        if (ret.IsThread)
        {
            ret = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response!)!;
        }

        ret.Discord = this._discord!;
        foreach (DiscordOverwrite xo in ret._permissionOverwrites)
        {
            xo.Discord = this._discord!;
            xo._channel_id = ret.Id;
        }

        return ret;
    }

    internal async ValueTask DeleteChannelAsync
    (
        ulong channelId,
        string reason
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestRequest request = new()
        {
            Route = $"{Endpoints.CHANNELS}/{channelId}",
            Url = new($"{Endpoints.CHANNELS}/{channelId}"),
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordMessage> GetMessageAsync
    (
        ulong channelId,
        ulong messageId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordMessage ret = this.PrepareMessage(JObject.Parse(res.Response!));

        return ret;
    }

    internal async ValueTask<DiscordMessage> CreateMessageAsync
    (
        ulong channelId,
        string? content,
        IEnumerable<DiscordEmbed>? embeds,
        ulong? replyMessageId,
        bool mentionReply,
        bool failOnInvalidReply,
        bool suppressNotifications
    )
    {
        if (content != null && content.Length > 2000)
        {
            throw new ArgumentException("Message content length cannot exceed 2000 characters.");
        }

        if (!embeds?.Any() ?? true)
        {
            if (content == null)
            {
                throw new ArgumentException("You must specify message content or an embed.");
            }

            if (content.Length == 0)
            {
                throw new ArgumentException("Message content must not be empty.");
            }
        }

        if (embeds is not null)
        {
            foreach (DiscordEmbed embed in embeds)
            {
                if (embed.Title?.Length > 256)
                {
                    throw new ArgumentException("Embed title length must not exceed 256 characters.");
                }

                if (embed.Description?.Length > 4096)
                {
                    throw new ArgumentException("Embed description length must not exceed 4096 characters.");
                }

                if (embed.Fields?.Count > 25)
                {
                    throw new ArgumentException("Embed field count must not exceed 25.");
                }

                if (embed.Fields is not null)
                {
                    foreach (DiscordEmbedField field in embed.Fields)
                    {
                        if (field.Name.Length > 256)
                        {
                            throw new ArgumentException("Embed field name length must not exceed 256 characters.");
                        }

                        if (field.Value.Length > 1024)
                        {
                            throw new ArgumentException("Embed field value length must not exceed 1024 characters.");
                        }
                    }
                }

                if (embed.Footer?.Text.Length > 2048)
                {
                    throw new ArgumentException("Embed footer text length must not exceed 2048 characters.");
                }

                if (embed.Author?.Name.Length > 256)
                {
                    throw new ArgumentException("Embed author name length must not exceed 256 characters.");
                }

                int totalCharacter = 0;
                totalCharacter += embed.Title?.Length ?? 0;
                totalCharacter += embed.Description?.Length ?? 0;
                totalCharacter += embed.Fields?.Sum(xf => xf.Name.Length + xf.Value.Length) ?? 0;
                totalCharacter += embed.Footer?.Text.Length ?? 0;
                totalCharacter += embed.Author?.Name.Length ?? 0;
                if (totalCharacter > 6000)
                {
                    throw new ArgumentException("Embed total length must not exceed 6000 characters.");
                }

                if (embed.Timestamp != null)
                {
                    embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
                }
            }
        }

        RestChannelMessageCreatePayload pld = new()
        {
            HasContent = content != null,
            Content = content,
            IsTTS = false,
            HasEmbed = embeds?.Any() ?? false,
            Embeds = embeds,
            Flags = suppressNotifications ? MessageFlags.SupressNotifications : 0,
        };

        if (replyMessageId != null)
        {
            pld.MessageReference = new InternalDiscordMessageReference
            {
                MessageId = replyMessageId,
                FailIfNotExists = failOnInvalidReply
            };
        }

        if (replyMessageId != null)
        {
            pld.Mentions = new DiscordMentions(Mentions.All, true, mentionReply);
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordMessage ret = this.PrepareMessage(JObject.Parse(res.Response!));

        return ret;
    }

    internal async ValueTask<DiscordMessage> CreateMessageAsync
    (
        ulong channelId,
        DiscordMessageBuilder builder
    )
    {
        builder.Validate();

        if (builder.Embeds != null)
        {
            foreach (DiscordEmbed embed in builder.Embeds)
            {
                if (embed?.Timestamp != null)
                {
                    embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
                }
            }
        }

        RestChannelMessageCreatePayload pld = new()
        {
            HasContent = builder.Content != null,
            Content = builder.Content,
            StickersIds = builder._stickers?.Where(s => s != null).Select(s => s.Id).ToArray(),
            IsTTS = builder.IsTTS,
            HasEmbed = builder.Embeds != null,
            Embeds = builder.Embeds,
            Components = builder.Components
        };

        if (builder.ReplyId != null)
        {
            pld.MessageReference = new InternalDiscordMessageReference { MessageId = builder.ReplyId, FailIfNotExists = builder.FailOnInvalidReply };
        }

        pld.Mentions = new DiscordMentions(builder.Mentions ?? Mentions.None, builder.Mentions?.Any() ?? false, builder.MentionOnReply);

        if (builder.Files.Count == 0)
        {
            string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}";
            string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}";

            RestRequest request = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Post,
                Payload = DiscordJson.SerializeObject(pld)
            };

            RestResponse res = await this._rest.ExecuteRequestAsync(request);

            DiscordMessage ret = this.PrepareMessage(JObject.Parse(res.Response!));

            return ret;
        }
        else
        {
            Dictionary<string, string> values = new()
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}";
            string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}";

            MultipartRestRequest request = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Post,
                Values = values,
                Files = builder.Files
            };

            RestResponse res;
            try
            {
                res = await this._rest.ExecuteRequestAsync(request);
            }
            finally
            {
                builder.ResetFileStreamPositions();
            }

            return this.PrepareMessage(JObject.Parse(res.Response!));
        }
    }

    internal async ValueTask<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guildId)
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.CHANNELS}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.CHANNELS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordChannel> channelsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response!)!
            .Select
            (
                xc =>
                {
                    xc.Discord = this._discord!;
                    return xc;
                }
            );

        foreach (DiscordChannel? ret in channelsRaw)
        {
            foreach (DiscordOverwrite xo in ret._permissionOverwrites)
            {
                xo.Discord = this._discord!;
                xo._channel_id = ret.Id;
            }
        }

        return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channelsRaw));
    }

    internal async ValueTask<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync
    (
        ulong channelId,
        int limit,
        ulong? before = null,
        ulong? after = null,
        ulong? around = null
    )
    {
        QueryUriBuilder url = new($"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}");
        if (around is not null)
        {
            url.AddParameter("around", around?.ToString(CultureInfo.InvariantCulture));
        }

        if (before is not null)
        {
            url.AddParameter("before", before?.ToString(CultureInfo.InvariantCulture));
        }

        if (after is not null)
        {
            url.AddParameter("after", after?.ToString(CultureInfo.InvariantCulture));
        }

        if (limit > 0)
        {
            url.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        JArray msgsRaw = JArray.Parse(res.Response!);
        List<DiscordMessage> msgs = new();
        foreach (JToken xj in msgsRaw)
        {
            msgs.Add(this.PrepareMessage(xj));
        }

        return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
    }

    internal async ValueTask<DiscordMessage> GetChannelMessageAsync
    (
        ulong channelId,
        ulong messageId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordMessage ret = this.PrepareMessage(JObject.Parse(res.Response!));

        return ret;
    }

    internal async ValueTask<DiscordMessage> EditMessageAsync
    (
        ulong channelId,
        ulong messageId,
        Optional<string> content = default,
        Optional<IEnumerable<DiscordEmbed>> embeds = default,
        Optional<IEnumerable<IMention>> mentions = default,
        IReadOnlyList<DiscordActionRowComponent>? components = null,
        IReadOnlyCollection<DiscordMessageFile>? files = null,
        MessageFlags? flags = null,
        IEnumerable<DiscordAttachment>? attachments = null
    )
    {
        if (embeds.HasValue && embeds.Value != null)
        {
            foreach (DiscordEmbed embed in embeds.Value)
            {
                if (embed.Timestamp != null)
                {
                    embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
                }
            }
        }

        RestChannelMessageEditPayload pld = new()
        {
            HasContent = content.HasValue,
            Content = content.HasValue ? (string)content : null,
            HasEmbed = embeds.HasValue && (embeds.Value?.Any() ?? false),
            Embeds = embeds.HasValue && (embeds.Value?.Any() ?? false) ? embeds.Value : null,
            Components = components,
            Flags = flags,
            Attachments = attachments,
            Mentions = mentions.HasValue
                ? new DiscordMentions
                (
                    mentions.Value ?? Mentions.None,
                    false,
                    mentions.Value?.OfType<RepliedUserMention>().Any() ?? false
                )
                : null
        };

        string payload = DiscordJson.SerializeObject(pld);

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}";

        RestResponse res;

        if (files is not null)
        {
            MultipartRestRequest request = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Patch,
                Values = new Dictionary<string, string>()
                {
                    ["payload_json"] = payload
                },
                Files = (IReadOnlyList<DiscordMessageFile>)files
            };

            res = await this._rest.ExecuteRequestAsync(request);
        }
        else
        {
            RestRequest request = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Patch,
                Payload = payload
            };

            res = await this._rest.ExecuteRequestAsync(request);
        }

        DiscordMessage ret = this.PrepareMessage(JObject.Parse(res.Response!));

        if (files is not null)
        {
            foreach (DiscordMessageFile? file in files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo!.Value;
            }
        }

        return ret;
    }

    internal async ValueTask DeleteMessageAsync
    (
        ulong channelId,
        ulong messageId,
        string reason
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask DeleteMessagesAsync
    (
        ulong channelId,
        IEnumerable<ulong> messageIds,
        string reason
    )
    {
        RestChannelMessageBulkDeletePayload pld = new()
        {
            Messages = messageIds
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{Endpoints.BULK_DELETE}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{Endpoints.BULK_DELETE}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.INVITES}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.INVITES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordInvite> invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response!)!
            .Select
            (
                xi =>
                {
                    xi.Discord = this._discord!;
                    return xi;
                }
            );

        return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
    }

    internal async ValueTask<DiscordInvite> CreateChannelInviteAsync
    (
        ulong channelId,
        int maxAge,
        int maxUses,
        bool temporary,
        bool unique,
        string reason,
        InviteTargetType? targetType = null,
        ulong? targetUserId = null,
        ulong? targetApplicationId = null
    )
    {
        RestChannelInviteCreatePayload pld = new()
        {
            MaxAge = maxAge,
            MaxUses = maxUses,
            Temporary = temporary,
            Unique = unique,
            TargetType = targetType,
            TargetUserId = targetUserId,
            TargetApplicationId = targetApplicationId
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.INVITES}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.INVITES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordInvite ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask DeleteChannelPermissionAsync
    (
        ulong channelId,
        ulong overwriteId,
        string reason
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PERMISSIONS}/:overwrite_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PERMISSIONS}/{overwriteId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask EditChannelPermissionsAsync
    (
        ulong channelId,
        ulong overwriteId,
        Permissions allow,
        Permissions deny,
        string type,
        string? reason = null
    )
    {
        RestChannelPermissionEditPayload pld = new()
        {
            Type = type,
            Allow = allow & PermissionMethods.FULL_PERMS,
            Deny = deny & PermissionMethods.FULL_PERMS
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PERMISSIONS}/:overwrite_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PERMISSIONS}/{overwriteId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask TriggerTypingAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.TYPING}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.TYPING}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post
        };
        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PINS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PINS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        JArray msgsRaw = JArray.Parse(res.Response!);
        List<DiscordMessage> msgs = new();
        foreach (JToken xj in msgsRaw)
        {
            msgs.Add(this.PrepareMessage(xj));
        }

        return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
    }

    internal async ValueTask PinMessageAsync
    (
        ulong channelId,
        ulong messageId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PINS}/:message_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PINS}/{messageId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask UnpinMessageAsync
    (
        ulong channelId,
        ulong messageId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PINS}/:message_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.PINS}/{messageId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };
        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask AddGroupDmRecipientAsync
    (
        ulong channelId,
        ulong userId,
        string accessToken,
        string nickname
    )
    {
        RestChannelGroupDmRecipientAddPayload pld = new()
        {
            AccessToken = accessToken,
            Nickname = nickname
        };

        string route = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CHANNELS}/{channelId}/{Endpoints.RECIPIENTS}/:user_id";
        string url = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CHANNELS}/{channelId}/{Endpoints.RECIPIENTS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put,
            Payload = DiscordJson.SerializeObject(pld)
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask RemoveGroupDmRecipientAsync
    (
        ulong channelId,
        ulong userId
    )
    {
        string route = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CHANNELS}/{channelId}/{Endpoints.RECIPIENTS}/:user_id";
        string url = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CHANNELS}/{channelId}/{Endpoints.RECIPIENTS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };
        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordDmChannel> CreateGroupDmAsync
    (
        IEnumerable<string> accessTokens,
        IDictionary<ulong, string> nicks
    )
    {
        RestUserGroupDmCreatePayload pld = new()
        {
            AccessTokens = accessTokens,
            Nicknames = nicks
        };

        string route = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CHANNELS}";
        string url = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CHANNELS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordDmChannel ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordDmChannel> CreateDmAsync
    (
        ulong recipientId
    )
    {
        RestUserDmCreatePayload pld = new()
        {
            Recipient = recipientId
        };

        string route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
        string url = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CHANNELS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordDmChannel ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response!)!;
        ret.Discord = this._discord!;

        if (this._discord is DiscordClient dc)
        {
            _ = dc._privateChannels.TryAdd(ret.Id, ret);
        }

        return ret;
    }

    internal async ValueTask<DiscordFollowedChannel> FollowChannelAsync
    (
        ulong channelId,
        ulong webhookChannelId
    )
    {
        FollowedChannelAddPayload pld = new()
        {
            WebhookChannelId = webhookChannelId
        };

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.FOLLOWERS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.FOLLOWERS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordFollowedChannel>(res.Response!)!;
    }

    internal async ValueTask<DiscordMessage> CrosspostMessageAsync
    (
        ulong channelId,
        ulong messageId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.CROSSPOST}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.CROSSPOST}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;
    }

    internal async ValueTask<DiscordStageInstance> CreateStageInstanceAsync
    (
        ulong channelId,
        string topic,
        PrivacyLevel? privacyLevel = null,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestCreateStageInstancePayload pld = new()
        {
            ChannelId = channelId,
            Topic = topic,
            PrivacyLevel = privacyLevel
        };

        string route = $"{Endpoints.STAGE_INSTANCES}";
        string url = $"{Endpoints.STAGE_INSTANCES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        DiscordStageInstance stage = JsonConvert.DeserializeObject<DiscordStageInstance>(response.Response!)!;
        stage.Discord = this._discord!;

        return stage;
    }

    internal async ValueTask<DiscordStageInstance> GetStageInstanceAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.STAGE_INSTANCES}/{channelId}";
        string url = $"{Endpoints.STAGE_INSTANCES}/{channelId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        DiscordStageInstance stage = JsonConvert.DeserializeObject<DiscordStageInstance>(response.Response!)!;
        stage.Discord = this._discord!;

        return stage;
    }

    internal async ValueTask<DiscordStageInstance> ModifyStageInstanceAsync
    (
        ulong channelId,
        Optional<string> topic = default,
        Optional<PrivacyLevel> privacyLevel = default,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestModifyStageInstancePayload pld = new()
        {
            Topic = topic,
            PrivacyLevel = privacyLevel
        };

        string route = $"{Endpoints.STAGE_INSTANCES}/{channelId}";
        string url = $"{Endpoints.STAGE_INSTANCES}/{channelId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);
        DiscordStageInstance stage = JsonConvert.DeserializeObject<DiscordStageInstance>(response.Response!)!;
        stage.Discord = this._discord!;

        return stage;
    }

    internal async ValueTask BecomeStageInstanceSpeakerAsync
    (
        ulong guildId,
        ulong id,
        ulong? userId = null,
        DateTime? timestamp = null,
        bool? suppress = null
    )
    {
        Dictionary<string, string> headers = new();

        RestBecomeStageSpeakerInstancePayload pld = new()
        {
            Suppress = suppress,
            ChannelId = id,
            RequestToSpeakTimestamp = timestamp
        };

        string user = userId?.ToString() ?? "@me";
        string route = $"/guilds/{guildId}/{Endpoints.VOICE_STATES}/{(userId is null ? "@me" : ":user_id")}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.VOICE_STATES}/{user}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask DeleteStageInstanceAsync
    (
        ulong channelId,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.STAGE_INSTANCES}/{channelId}";
        string url = $"{Endpoints.STAGE_INSTANCES}/{channelId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    #endregion

    #region Threads

    internal async ValueTask<DiscordThreadChannel> CreateThreadFromMessageAsync
    (
        ulong channelId,
        ulong messageId,
        string name,
        AutoArchiveDuration archiveAfter,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestThreadCreatePayload payload = new()
        {
            Name = name,
            ArchiveAfter = archiveAfter
        };

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.THREADS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.THREADS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(payload),
            Headers = headers
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        DiscordThreadChannel thread = JsonConvert.DeserializeObject<DiscordThreadChannel>(response.Response!)!;
        thread.Discord = this._discord!;

        return thread;
    }

    internal async ValueTask<DiscordThreadChannel> CreateThreadAsync
    (
        ulong channelId,
        string name,
        AutoArchiveDuration archiveAfter,
        ChannelType type,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestThreadCreatePayload payload = new()
        {
            Name = name,
            ArchiveAfter = archiveAfter,
            Type = type
        };

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(payload),
            Headers = headers
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        DiscordThreadChannel thread = JsonConvert.DeserializeObject<DiscordThreadChannel>(response.Response!)!;
        thread.Discord = this._discord!;

        return thread;
    }

    internal async ValueTask JoinThreadAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/{Endpoints.ME}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/{Endpoints.ME}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask LeaveThreadAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/{Endpoints.ME}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/{Endpoints.ME}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordThreadChannelMember> GetThreadMemberAsync
    (
        ulong channelId,
        ulong userId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/:user_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        DiscordThreadChannelMember ret = JsonConvert.DeserializeObject<DiscordThreadChannelMember>(response.Response!)!;

        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask AddThreadMemberAsync
    (
        ulong channelId,
        ulong userId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/:user_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask RemoveThreadMemberAsync
    (
        ulong channelId,
        ulong userId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/:user_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordThreadChannelMember>> ListThreadMembersAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREAD_MEMBERS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        List<DiscordThreadChannelMember> threadMembers = JsonConvert.DeserializeObject<List<DiscordThreadChannelMember>>(response.Response!)!;

        foreach (DiscordThreadChannelMember member in threadMembers)
        {
            member.Discord = this._discord!;
        }

        return new ReadOnlyCollection<DiscordThreadChannelMember>(threadMembers);
    }

    internal async ValueTask<ThreadQueryResult> ListActiveThreadsAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.THREADS}/{Endpoints.ACTIVE}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.THREADS}/{Endpoints.ACTIVE}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        ThreadQueryResult result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response!)!;
        result.HasMore = false;

        foreach (DiscordThreadChannel thread in result.Threads)
        {
            thread.Discord = this._discord!;
        }

        foreach (DiscordThreadChannelMember member in result.Members)
        {
            member.Discord = this._discord!;
            member._guild_id = guildId;
            DiscordThreadChannel? thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
            if (thread is not null)
            {
                thread.CurrentMember = member;
            }
        }

        return result;
    }

    internal async ValueTask<ThreadQueryResult> ListPublicArchivedThreadsAsync
    (
        ulong guildId,
        ulong channelId,
        string before,
        int limit
    )
    {
        QueryUriBuilder queryParams = new($"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}/{Endpoints.ARCHIVED}/{Endpoints.PUBLIC}");
        if (before != null)
        {
            queryParams.AddParameter("before", before?.ToString(CultureInfo.InvariantCulture));
        }

        if (limit > 0)
        {
            queryParams.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}/{Endpoints.ARCHIVED}/{Endpoints.PUBLIC}";

        RestRequest request = new()
        {
            Route = route,
            Url = queryParams.Build(),
            Method = HttpMethod.Get,

        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        ThreadQueryResult result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response!)!;

        foreach (DiscordThreadChannel thread in result.Threads)
        {
            thread.Discord = this._discord!;
        }

        foreach (DiscordThreadChannelMember member in result.Members)
        {
            member.Discord = this._discord!;
            member._guild_id = guildId;
            DiscordThreadChannel? thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
            if (thread is not null)
            {
                thread.CurrentMember = member;
            }
        }

        return result;
    }

    internal async ValueTask<ThreadQueryResult> ListPrivateArchivedThreadsAsync
    (
        ulong guildId,
        ulong channelId,
        int limit,
        string? before = null
    )
    {
        QueryUriBuilder queryParams = new($"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}/{Endpoints.ARCHIVED}/{Endpoints.PRIVATE}");
        if (before is not null)
        {
            queryParams.AddParameter("before", before?.ToString(CultureInfo.InvariantCulture));
        }

        if (limit > 0)
        {
            queryParams.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}/{Endpoints.ARCHIVED}/{Endpoints.PRIVATE}";

        RestRequest request = new()
        {
            Route = route,
            Url = queryParams.Build(),
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        ThreadQueryResult result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response!)!;

        foreach (DiscordThreadChannel thread in result.Threads)
        {
            thread.Discord = this._discord!;
        }

        foreach (DiscordThreadChannelMember member in result.Members)
        {
            member.Discord = this._discord!;
            member._guild_id = guildId;
            DiscordThreadChannel? thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
            if (thread is not null)
            {
                thread.CurrentMember = member;
            }
        }

        return result;
    }

    internal async ValueTask<ThreadQueryResult> ListJoinedPrivateArchivedThreadsAsync
    (
        ulong guildId,
        ulong channelId,
        int limit,
        ulong? before = null
    )
    {
        QueryUriBuilder queryParams = new($"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}/{Endpoints.ARCHIVED}/{Endpoints.PRIVATE}/{Endpoints.ME}");
        if (before is not null)
        {
            queryParams.AddParameter("before", before?.ToString(CultureInfo.InvariantCulture));
        }

        if (limit > 0)
        {
            queryParams.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.THREADS}/{Endpoints.ARCHIVED}/{Endpoints.PUBLIC}";

        RestRequest request = new()
        {
            Route = route,
            Url = queryParams.Build(),
            Method = HttpMethod.Get
        };

        RestResponse response = await this._rest.ExecuteRequestAsync(request);

        ThreadQueryResult result = JsonConvert.DeserializeObject<ThreadQueryResult>(response.Response!)!;

        foreach (DiscordThreadChannel thread in result.Threads)
        {
            thread.Discord = this._discord!;
        }

        foreach (DiscordThreadChannelMember member in result.Members)
        {
            member.Discord = this._discord!;
            member._guild_id = guildId;
            DiscordThreadChannel? thread = result.Threads.SingleOrDefault(x => x.Id == member.ThreadId);
            if (thread is not null)
            {
                thread.CurrentMember = member;
            }
        }

        return result;
    }

    #endregion

    #region Member
    internal ValueTask<DiscordUser> GetCurrentUserAsync()
        => this.GetUserAsync("@me");

    internal ValueTask<DiscordUser> GetUserAsync(ulong userId)
        => this.GetUserAsync(userId.ToString(CultureInfo.InvariantCulture));

    internal async ValueTask<DiscordUser> GetUserAsync(string userId)
    {
        string route = $"{Endpoints.USERS}/:user_id";
        string url = $"{Endpoints.USERS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        TransportUser userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response!)!;
        DiscordUser user = new(userRaw)
        {
            Discord = this._discord!
        };

        return user;
    }

    internal async ValueTask<DiscordMember> GetGuildMemberAsync
    (
        ulong guildId,
        ulong userId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/:user_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        TransportMember tm = JsonConvert.DeserializeObject<TransportMember>(res.Response!)!;

        DiscordUser usr = new(tm.User)
        {
            Discord = this._discord!
        };
        _ = this._discord!.UpdateUserCache(usr);

        return new DiscordMember(tm)
        {
            Discord = this._discord,
            _guild_id = guildId
        };
    }

    internal async ValueTask RemoveGuildMemberAsync
    (
        ulong guildId,
        ulong userId,
        string? reason = null
    )
    {
        string url = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{userId}");
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/:user_id";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = string.IsNullOrWhiteSpace(reason)
                ? null
                : new Dictionary<string, string>
                {
                    [REASON_HEADER_NAME] = reason
                }
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<TransportUser> ModifyCurrentUserAsync
    (
        string username,
        Optional<string> base64Avatar = default
    )
    {
        RestUserUpdateCurrentPayload pld = new()
        {
            Username = username,
            AvatarBase64 = base64Avatar.HasValue ? base64Avatar.Value : null,
            AvatarSet = base64Avatar.HasValue
        };

        string route = $"{Endpoints.USERS}/{Endpoints.ME}";
        string url = $"{Endpoints.USERS}/{Endpoints.ME}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        TransportUser userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response!)!;

        return userRaw;
    }

    internal async ValueTask<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync
    (
        int limit = 100,
        ulong? before = null,
        ulong? after = null
    )
    {
        string route = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.GUILDS}";
        QueryUriBuilder url = new($"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.GUILDS}");
        url.AddParameter($"limit", limit.ToString(CultureInfo.InvariantCulture));

        if (before != null)
        {
            url.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (after != null)
        {
            url.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));
        }

        RestRequest request = new()
        {
            Route = route,
            Url = url.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        if (this._discord is DiscordClient)
        {
            IEnumerable<RestUserGuild> guildsRaw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response!)!;
            IEnumerable<DiscordGuild> guilds = guildsRaw.Select
            (
                xug => (this._discord as DiscordClient)?._guilds[xug.Id]
            )
            .Where(static guild => guild is not null)!;
            return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(guilds));
        }
        else
        {
            return new ReadOnlyCollection<DiscordGuild>(JsonConvert.DeserializeObject<List<DiscordGuild>>(res.Response!)!);
        }
    }

    internal async ValueTask ModifyGuildMemberAsync
    (
        ulong guildId,
        ulong userId,
        Optional<string> nick = default,
        Optional<IEnumerable<ulong>> roleIds = default,
        Optional<bool> mute = default,
        Optional<bool> deaf = default,
        Optional<ulong?> voiceChannelId = default,
        Optional<DateTimeOffset?> communicationDisabledUntil = default,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestGuildMemberModifyPayload pld = new()
        {
            Nickname = nick,
            RoleIds = roleIds,
            Deafen = deaf,
            Mute = mute,
            VoiceChannelId = voiceChannelId,
            CommunicationDisabledUntil = communicationDisabledUntil
        };

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/:user_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask ModifyCurrentMemberAsync
    (
        ulong guildId,
        string nick,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestGuildMemberModifyPayload pld = new()
        {
            Nickname = nick
        };

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{Endpoints.ME}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.MEMBERS}/{Endpoints.ME}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }
    #endregion

    #region Roles
    internal async ValueTask<IReadOnlyList<DiscordRole>> GetGuildRolesAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordRole> rolesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response!)!
            .Select
            (
                xr =>
                {
                    xr.Discord = this._discord!;
                    xr._guild_id = guildId;
                    return xr;
                }
            );

        return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(rolesRaw));
    }

    internal async ValueTask<DiscordGuild> GetGuildAsync
    (
        ulong guildId,
        bool? withCounts
    )
    {
        QueryUriBuilder urlparams = new($"{Endpoints.GUILDS}/{guildId}");
        if (withCounts.HasValue)
        {
            urlparams.AddParameter("with_counts", withCounts?.ToString());
        }

        string route = $"{Endpoints.GUILDS}/{guildId}";

        RestRequest request = new()
        {
            Route = route,
            Url = urlparams.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        JObject json = JObject.Parse(res.Response!);
        JArray rawMembers = (JArray)json["members"]!;
        DiscordGuild guildRest = json.ToDiscordObject<DiscordGuild>();
        foreach (DiscordRole role in guildRest._roles.Values)
        {
            role._guild_id = guildRest.Id;
        }

        if (this._discord is DiscordClient discordClient)
        {
            await discordClient.OnGuildUpdateEventAsync(guildRest, rawMembers);
            return discordClient._guilds[guildRest.Id];
        }
        else
        {
            guildRest.Discord = this._discord!;
            return guildRest;
        }
    }

    internal async ValueTask<DiscordRole> ModifyGuildRoleAsync
    (
        ulong guildId,
        ulong roleId,
        string? name = null,
        Permissions? permissions = null,
        int? color = null,
        bool? hoist = null,
        bool? mentionable = null,
        Stream? icon = null,
        string? emoji = null,
        string? reason = null
    )
    {
        string? image = null;

        if (icon != null)
        {
            using ImageTool it = new(icon);
            image = it.GetBase64();
        }

        RestGuildRolePayload pld = new()
        {
            Name = name,
            Permissions = permissions & PermissionMethods.FULL_PERMS,
            Color = color,
            Hoist = hoist,
            Mentionable = mentionable,
            Emoji = emoji,
            Icon = image
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}/:role_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}/{roleId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordRole ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response!)!;
        ret.Discord = this._discord!;
        ret._guild_id = guildId;

        return ret;
    }

    internal async ValueTask DeleteRoleAsync
    (
        ulong guildId,
        ulong roleId,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}/:role_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}/{roleId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordRole> CreateGuildRoleAsync
    (
        ulong guildId,
        string name,
        Permissions? permissions = null,
        int? color = null,
        bool? hoist = null,
        bool? mentionable = null,
        Stream? icon = null,
        string? emoji = null,
        string? reason = null
    )
    {
        string? image = null;

        if (icon != null)
        {
            using ImageTool it = new(icon);
            image = it.GetBase64();
        }

        RestGuildRolePayload pld = new()
        {
            Name = name,
            Permissions = permissions & PermissionMethods.FULL_PERMS,
            Color = color,
            Hoist = hoist,
            Mentionable = mentionable,
            Emoji = emoji,
            Icon = image
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.ROLES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordRole ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response!)!;
        ret.Discord = this._discord!;
        ret._guild_id = guildId;

        return ret;
    }
    #endregion

    #region Prune
    internal async ValueTask<int> GetGuildPruneCountAsync
    (
        ulong guildId,
        int days,
        IEnumerable<ulong>? includeRoles = null
    )
    {
        if (days < 0 || days > 30)
        {
            throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));
        }

        QueryUriBuilder urlparams = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.PRUNE}");
        urlparams.AddParameter("days", days.ToString(CultureInfo.InvariantCulture));

        StringBuilder sb = new();

        if (includeRoles is not null)
        {
            ulong[] roleArray = includeRoles.ToArray();
            int roleArrayCount = roleArray.Length;

            for (int i = 0; i < roleArrayCount; i++)
            {
                sb.Append($"&include_roles={roleArray[i]}");
            }
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.PRUNE}";

        RestRequest request = new()
        {
            Route = route,
            Url = urlparams.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        RestGuildPruneResultPayload pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response!)!;

        return pruned.Pruned!.Value;
    }

    internal async ValueTask<int?> BeginGuildPruneAsync
    (
        ulong guildId,
        int days,
        bool computePruneCount,
        IEnumerable<ulong>? includeRoles = null,
        string? reason = null
    )
    {
        if (days < 0 || days > 30)
        {
            throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));
        }

        QueryUriBuilder urlparams = new($"{Endpoints.GUILDS}/{guildId}/{Endpoints.PRUNE}");
        urlparams.AddParameter("days", days.ToString(CultureInfo.InvariantCulture));
        urlparams.AddParameter("compute_prune_count", computePruneCount.ToString());

        StringBuilder sb = new();

        if (includeRoles is not null)
        {
            foreach (ulong id in includeRoles)
            {
                sb.Append($"&include_roles={id}");
            }
        }

        Dictionary<string, string> headers = new();
        if (string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason!;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.PRUNE}";

        RestRequest request = new()
        {
            Route = route,
            Url = urlparams.Build() + sb.ToString(),
            Method = HttpMethod.Post,
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        RestGuildPruneResultPayload pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response!)!;

        return pruned.Pruned;
    }
    #endregion

    #region GuildVarious
    internal async ValueTask<DiscordGuildTemplate> GetTemplateAsync
    (
        string code
    )
    {
        string route = $"{Endpoints.GUILDS}/{Endpoints.TEMPLATES}/:code";
        string url = $"{Endpoints.GUILDS}/{Endpoints.TEMPLATES}/{code}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordGuildTemplate templatesRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response!)!;

        return templatesRaw;
    }

    internal async ValueTask<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordIntegration> integrationsRaw =
            JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response!)!
            .Select
            (
                xi =>
                {
                    xi.Discord = this._discord!;
                    return xi;
                }
            );

        return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrationsRaw));
    }

    internal async ValueTask<DiscordGuildPreview> GetGuildPreviewAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.PREVIEW}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.PREVIEW}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordGuildPreview ret = JsonConvert.DeserializeObject<DiscordGuildPreview>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordIntegration> CreateGuildIntegrationAsync
    (
        ulong guildId,
        string type,
        ulong id
    )
    {
        RestGuildIntegrationAttachPayload pld = new()
        {
            Type = type,
            Id = id
        };

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordIntegration ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordIntegration> ModifyGuildIntegrationAsync
    (
        ulong guildId,
        ulong integrationId,
        int expireBehaviour,
        int expireGracePeriod,
        bool enableEmoticons
    )
    {
        RestGuildIntegrationModifyPayload pld = new()
        {
            ExpireBehavior = expireBehaviour,
            ExpireGracePeriod = expireGracePeriod,
            EnableEmoticons = enableEmoticons
        };

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}/:integration_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}/{integrationId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordIntegration ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask DeleteGuildIntegrationAsync
    (
        ulong guildId,
        ulong integrationId,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason!;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}/:integration_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}/{integrationId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask SyncGuildIntegrationAsync
    (
        ulong guildId,
        ulong integrationId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}/:integration_id/{Endpoints.SYNC}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INTEGRATIONS}/{integrationId}/{Endpoints.SYNC}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.REGIONS}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.REGIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordVoiceRegion> regionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response!)!;

        return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regionsRaw));
    }

    internal async ValueTask<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INVITES}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.INVITES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordInvite> invitesRaw =
            JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response!)!
            .Select
            (
                xi =>
                {
                    xi.Discord = this._discord!;
                    return xi;
                }
            );

        return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
    }
    #endregion

    #region Invite
    internal async ValueTask<DiscordInvite> GetInviteAsync
    (
        string inviteCode,
        bool? withCounts = null,
        bool? withExpiration = null
    )
    {
        Dictionary<string, string> urlparams = new();
        if (withCounts.HasValue)
        {
            urlparams["with_counts"] = withCounts?.ToString()!;
            urlparams["with_expiration"] = withExpiration?.ToString()!;
        }

        string route = $"{Endpoints.INVITES}/:invite_code";
        string url = $"{Endpoints.INVITES}/{inviteCode}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordInvite ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordInvite> DeleteInviteAsync
    (
        string inviteCode,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.INVITES}/:invite_code";
        string url = $"{Endpoints.INVITES}/{inviteCode}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordInvite ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }
    #endregion

    #region Connections
    internal async ValueTask<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
    {
        string route = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CONNECTIONS}";
        string url = $"{Endpoints.USERS}/{Endpoints.ME}/{Endpoints.CONNECTIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordConnection> connectionsRaw =
            JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response!)!
            .Select
            (
                xc =>
                {
                    xc.Discord = this._discord!;
                    return xc;
                }
            );

        return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connectionsRaw));
    }
    #endregion

    #region Voice
    internal async ValueTask<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
    {
        string route = $"{Endpoints.VOICE}/{Endpoints.REGIONS}";
        string url = $"{Endpoints.VOICE}/{Endpoints.REGIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordVoiceRegion> regions =
            JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response!)!;

        return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
    }
    #endregion

    #region Webhooks
    internal async ValueTask<DiscordWebhook> CreateWebhookAsync
    (
        ulong channelId,
        string name,
        Optional<string> base64Avatar = default,
        string? reason = null
    )
    {
        RestWebhookPayload pld = new()
        {
            Name = name,
            AvatarBase64 = base64Avatar.HasValue ? base64Avatar.Value : null,
            AvatarSet = base64Avatar.HasValue
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.WEBHOOKS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.WEBHOOKS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordWebhook ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response!)!;
        ret.Discord = this._discord!;
        ret.ApiClient = this;

        return ret;
    }

    internal async ValueTask<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync
    (
        ulong channelId
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.WEBHOOKS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.WEBHOOKS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordWebhook> webhooksRaw =
            JsonConvert
                .DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response!)!
                .Select
                (
                    xw =>
                    {
                        xw.Discord = this._discord!;
                        xw.ApiClient = this;
                        return xw;
                    }
                );

        return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
    }

    internal async ValueTask<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WEBHOOKS}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.WEBHOOKS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordWebhook> webhooksRaw =
            JsonConvert
                .DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response!)!
                .Select
                (
                    xw =>
                    {
                        xw.Discord = this._discord!;
                        xw.ApiClient = this;
                        return xw;
                    }
                );

        return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
    }

    internal async ValueTask<DiscordWebhook> GetWebhookAsync
    (
        ulong webhookId
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/{webhookId}";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordWebhook ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response!)!;
        ret.Discord = this._discord!;
        ret.ApiClient = this;

        return ret;
    }

    // Auth header not required
    internal async ValueTask<DiscordWebhook> GetWebhookWithTokenAsync
    (
        ulong webhookId,
        string webhookToken
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            IsExemptFromGlobalLimit = true,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordWebhook ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response!)!;
        ret.Token = webhookToken;
        ret.Id = webhookId;
        ret.Discord = this._discord!;
        ret.ApiClient = this;

        return ret;
    }

    internal async ValueTask<DiscordMessage> GetWebhookMessageAsync
    (
        ulong webhookId,
        string webhookToken,
        ulong messageId
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token/{Endpoints.MESSAGES}/:message_id";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}/{Endpoints.MESSAGES}/{messageId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            IsExemptFromGlobalLimit = true,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordMessage ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;
        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<DiscordWebhook> ModifyWebhookAsync
    (
        ulong webhookId,
        ulong channelId,
        string? name = null,
        Optional<string> base64Avatar = default,
        string? reason = null
    )
    {
        RestWebhookPayload pld = new()
        {
            Name = name,
            AvatarBase64 = base64Avatar.HasValue ? base64Avatar.Value : null,
            AvatarSet = base64Avatar.HasValue,
            ChannelId = channelId
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.WEBHOOKS}/{webhookId}";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordWebhook ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response!)!;
        ret.Discord = this._discord!;
        ret.ApiClient = this;

        return ret;
    }

    internal async ValueTask<DiscordWebhook> ModifyWebhookAsync
    (
        ulong webhookId,
        string webhookToken,
        string? name = null,
        string? base64Avatar = null,
        string? reason = null
    )
    {
        RestWebhookPayload pld = new()
        {
            Name = name,
            AvatarBase64 = base64Avatar
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            IsExemptFromGlobalLimit = true,
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordWebhook ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response!)!;
        ret.Discord = this._discord!;
        ret.ApiClient = this;

        return ret;
    }

    internal async ValueTask DeleteWebhookAsync
    (
        ulong webhookId,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.WEBHOOKS}/{webhookId}";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask DeleteWebhookAsync
    (
        ulong webhookId,
        string webhookToken,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            IsExemptFromGlobalLimit = true,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordMessage> ExecuteWebhookAsync
    (
        ulong webhookId,
        string webhookToken,
        DiscordWebhookBuilder builder
    )
    {
        builder.Validate();

        if (builder.Embeds != null)
        {
            foreach (DiscordEmbed embed in builder.Embeds)
            {
                if (embed.Timestamp != null)
                {
                    embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
                }
            }
        }

        Dictionary<string, string> values = new();
        RestWebhookExecutePayload pld = new()
        {
            Content = builder.Content,
            Username = builder.Username.HasValue ? builder.Username.Value : null,
            AvatarUrl = builder.AvatarUrl.HasValue ? builder.AvatarUrl.Value : null,
            IsTTS = builder.IsTTS,
            Embeds = builder.Embeds,
            Components = builder.Components,
        };

        if (builder.Mentions != null)
        {
            pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());
        }

        if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTTS == true || builder.Mentions != null)
        {
            values["payload_json"] = DiscordJson.SerializeObject(pld);
        }

        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token";
        QueryUriBuilder url = new($"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}");
        url.AddParameter("wait", "true");

        if (builder.ThreadId.HasValue)
        {
            url.AddParameter("thread_id", builder.ThreadId.Value.ToString());
        }

        MultipartRestRequest request = new()
        {
            Route = route,
            Url = url.Build(),
            Method = HttpMethod.Post,
            Values = values,
            Files = builder.Files,
            IsExemptFromGlobalLimit = true
        };

        RestResponse res;
        try
        {
            res = await this._rest.ExecuteRequestAsync(request);
        }
        finally
        {
            builder.ResetFileStreamPositions();
        }
        DiscordMessage ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;

        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<DiscordMessage> ExecuteWebhookSlackAsync
    (
        ulong webhookId,
        string webhookToken,
        string jsonPayload
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token/{Endpoints.SLACK}";
        QueryUriBuilder url = new($"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}/{Endpoints.SLACK}");

        RestRequest request = new()
        {
            Route = route,
            Url = url.Build(),
            Method = HttpMethod.Post,
            Payload = jsonPayload,
            IsExemptFromGlobalLimit = true
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordMessage ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;
        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<DiscordMessage> ExecuteWebhookGithubAsync
    (
        ulong webhookId,
        string webhookToken,
        string jsonPayload
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token{Endpoints.GITHUB}";
        QueryUriBuilder url = new($"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}{Endpoints.GITHUB}");
        url.AddParameter("wait", "true");

        RestRequest request = new()
        {
            Route = route,
            Url = url.Build(),
            Method = HttpMethod.Post,
            Payload = jsonPayload,
            IsExemptFromGlobalLimit = true
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordMessage ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;
        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<DiscordMessage> EditWebhookMessageAsync
    (
        ulong webhookId,
        string webhookToken,
        ulong messageId,
        DiscordWebhookBuilder builder,
        IEnumerable<DiscordAttachment>? attachments = null
    )
    {
        builder.Validate(true);

        DiscordMentions? mentions = builder.Mentions != null ? new DiscordMentions(builder.Mentions, builder.Mentions.Any()) : null;

        RestWebhookMessageEditPayload pld = new()
        {
            Content = builder.Content,
            Embeds = builder.Embeds,
            Mentions = mentions,
            Components = builder.Components,
            Attachments = attachments
        };

        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token/{Endpoints.MESSAGES}/:message_id";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}/{Endpoints.MESSAGES}/{messageId}";

        Dictionary<string, string> values = new()
        {
            ["payload_json"] = DiscordJson.SerializeObject(pld)
        };

        MultipartRestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Values = values,
            Files = builder.Files,
            IsExemptFromGlobalLimit = true
        };

        RestResponse res;
        try
        {
            res = await this._rest.ExecuteRequestAsync(request);
        }
        finally
        {
            builder.ResetFileStreamPositions();
        }

        return this.PrepareMessage(JObject.Parse(res.Response!));
    }

    internal async ValueTask DeleteWebhookMessageAsync
    (
        ulong webhookId,
        string webhookToken,
        ulong messageId
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/{webhookId}/:webhook_token/{Endpoints.MESSAGES}/:message_id";
        string url = $"{Endpoints.WEBHOOKS}/{webhookId}/{webhookToken}/{Endpoints.MESSAGES}/{messageId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            IsExemptFromGlobalLimit = true
        };

        await this._rest.ExecuteRequestAsync(request);
    }
    #endregion

    #region Reactions
    internal async ValueTask CreateReactionAsync
    (
        ulong channelId,
        ulong messageId,
        string emoji
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.REACTIONS}/:emoji/{Endpoints.ME}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.REACTIONS}/{emoji}/{Endpoints.ME}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask DeleteOwnReactionAsync
    (
        ulong channelId,
        ulong messageId,
        string emoji
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.REACTIONS}/:emoji/{Endpoints.ME}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.REACTIONS}/{emoji}/{Endpoints.ME}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask DeleteUserReactionAsync
    (
        ulong channelId,
        ulong messageId,
        ulong userId,
        string emoji,
        string reason
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.REACTIONS}/:emoji/:user_id";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.REACTIONS}/{emoji}/{userId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordUser>> GetReactionsAsync
    (
        ulong channelId,
        ulong messageId,
        string emoji,
        ulong? afterId = null,
        int limit = 25
    )
    {
        QueryUriBuilder urlparams = new($"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.REACTIONS}/{emoji}");
        if (afterId.HasValue)
        {
            urlparams.AddParameter("after", afterId.Value.ToString(CultureInfo.InvariantCulture));
        }

        urlparams.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.REACTIONS}/:emoji";

        RestRequest request = new()
        {
            Route = route,
            Url = urlparams.Build(),
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<TransportUser> usersRaw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response!)!;
        List<DiscordUser> users = new();
        foreach (TransportUser xr in usersRaw)
        {
            DiscordUser usr = new(xr)
            {
                Discord = this._discord!
            };
            usr = this._discord!.UpdateUserCache(usr);

            users.Add(usr);
        }

        return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(users));
    }

    internal async ValueTask DeleteAllReactionsAsync
    (
        ulong channelId,
        ulong messageId,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.REACTIONS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.REACTIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask DeleteReactionsEmojiAsync
    (
        ulong channelId,
        ulong messageId,
        string emoji
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/:message_id/{Endpoints.REACTIONS}/:emoji";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.MESSAGES}/{messageId}/{Endpoints.REACTIONS}/{emoji}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };

        await this._rest.ExecuteRequestAsync(request);
    }
    #endregion

    #region Emoji
    internal async ValueTask<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<JObject> emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response!)!;

        this._discord!.Guilds.TryGetValue(guildId, out DiscordGuild? guild);
        Dictionary<ulong, DiscordUser> users = new();
        List<DiscordGuildEmoji> emojis = new();
        foreach (JObject rawEmoji in emojisRaw)
        {
            DiscordGuildEmoji discordGuildEmoji = rawEmoji.ToDiscordObject<DiscordGuildEmoji>();

            if (guild is not null)
            {
                discordGuildEmoji.Guild = guild;
            }

            TransportUser? rawUser = rawEmoji["user"]?.ToDiscordObject<TransportUser>();
            if (rawUser != null)
            {
                if (!users.ContainsKey(rawUser.Id))
                {
                    DiscordUser user = guild is not null && guild.Members.TryGetValue(rawUser.Id, out DiscordMember? member) ? member : new DiscordUser(rawUser);
                    users[user.Id] = user;
                }

                discordGuildEmoji.User = users[rawUser.Id];
            }

            emojis.Add(discordGuildEmoji);
        }

        return new ReadOnlyCollection<DiscordGuildEmoji>(emojis);
    }

    internal async ValueTask<DiscordGuildEmoji> GetGuildEmojiAsync
    (
        ulong guildId,
        ulong emojiId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}/:emoji_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}/{emojiId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        this._discord!.Guilds.TryGetValue(guildId, out DiscordGuild? guild);

        JObject emojiRaw = JObject.Parse(res.Response!);
        DiscordGuildEmoji emoji = emojiRaw.ToDiscordObject<DiscordGuildEmoji>();

        if (guild is not null)
        {
            emoji.Guild = guild;
        }

        TransportUser? rawUser = emojiRaw["user"]?.ToDiscordObject<TransportUser>();
        if (rawUser != null)
        {
            emoji.User = guild is not null && guild.Members.TryGetValue(rawUser.Id, out DiscordMember? member) ? member : new DiscordUser(rawUser);
        }

        return emoji;
    }

    internal async ValueTask<DiscordGuildEmoji> CreateGuildEmojiAsync
    (
        ulong guildId,
        string name,
        string imageb64,
        IEnumerable<ulong>? roles = null,
        string? reason = null
    )
    {
        RestGuildEmojiCreatePayload pld = new()
        {
            Name = name,
            ImageB64 = imageb64,
            Roles = roles?.ToArray()
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        this._discord!.Guilds.TryGetValue(guildId, out DiscordGuild? guild);

        JObject emojiRaw = JObject.Parse(res.Response!);
        DiscordGuildEmoji emoji = emojiRaw.ToDiscordObject<DiscordGuildEmoji>();

        if (guild is not null)
        {
            emoji.Guild = guild;
        }

        TransportUser? rawUser = emojiRaw["user"]?.ToDiscordObject<TransportUser>();
        emoji.User = rawUser != null
            ? guild is not null && guild.Members.TryGetValue(rawUser.Id, out DiscordMember? member) ? member : new DiscordUser(rawUser)
            : this._discord.CurrentUser;

        return emoji;
    }

    internal async ValueTask<DiscordGuildEmoji> ModifyGuildEmojiAsync
    (
        ulong guildId,
        ulong emojiId,
        string? name = null,
        IEnumerable<ulong>? roles = null,
        string? reason = null
    )
    {
        RestGuildEmojiModifyPayload pld = new()
        {
            Name = name,
            Roles = roles?.ToArray()
        };

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}/:emoji_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}/{emojiId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld),
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        this._discord!.Guilds.TryGetValue(guildId, out DiscordGuild? guild);

        JObject emojiRaw = JObject.Parse(res.Response!);
        DiscordGuildEmoji emoji = emojiRaw.ToDiscordObject<DiscordGuildEmoji>();

        if (guild is not null)
        {
            emoji.Guild = guild;
        }

        TransportUser? rawUser = emojiRaw["user"]?.ToDiscordObject<TransportUser>();
        if (rawUser != null)
        {
            emoji.User = guild is not null && guild.Members.TryGetValue(rawUser.Id, out DiscordMember? member) ? member : new DiscordUser(rawUser);
        }

        return emoji;
    }

    internal async ValueTask DeleteGuildEmojiAsync
    (
        ulong guildId,
        ulong emojiId,
        string? reason = null
    )
    {
        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}/:emoji_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.EMOJIS}/{emojiId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }
    #endregion

    #region Application Commands
    internal async ValueTask<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync
    (
        ulong applicationId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.COMMANDS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.COMMANDS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordApplicationCommand> ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response!)!;
        foreach (DiscordApplicationCommand app in ret)
        {
            app.Discord = this._discord!;
        }

        return ret.ToList();
    }

    internal async ValueTask<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync
    (
        ulong applicationId,
        IEnumerable<DiscordApplicationCommand> commands
    )
    {
        List<RestApplicationCommandCreatePayload> pld = new();
        foreach (DiscordApplicationCommand command in commands)
        {
            pld.Add(new RestApplicationCommandCreatePayload
            {
                Type = command.Type,
                Name = command.Name,
                Description = command.Description,
                Options = command.Options,
                DefaultPermission = command.DefaultPermission,
                NameLocalizations = command.NameLocalizations,
                DescriptionLocalizations = command.DescriptionLocalizations,
                AllowDMUsage = command.AllowDMUsage,
                DefaultMemberPermissions = command.DefaultMemberPermissions,
                NSFW = command.NSFW
            });
        }

        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.COMMANDS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.COMMANDS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordApplicationCommand> ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response!)!;
        foreach (DiscordApplicationCommand app in ret)
        {
            app.Discord = this._discord!;
        }

        return ret.ToList();
    }

    internal async ValueTask<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync
    (
        ulong applicationId,
        DiscordApplicationCommand command
    )
    {
        RestApplicationCommandCreatePayload pld = new()
        {
            Type = command.Type,
            Name = command.Name,
            Description = command.Description,
            Options = command.Options,
            DefaultPermission = command.DefaultPermission,
            NameLocalizations = command.NameLocalizations,
            DescriptionLocalizations = command.DescriptionLocalizations,
            AllowDMUsage = command.AllowDMUsage,
            DefaultMemberPermissions = command.DefaultMemberPermissions,
            NSFW = command.NSFW
        };

        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.COMMANDS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.COMMANDS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordApplicationCommand ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordApplicationCommand> GetGlobalApplicationCommandAsync
    (
        ulong applicationId,
        ulong commandId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.COMMANDS}/:command_id";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.COMMANDS}/{commandId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordApplicationCommand ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordApplicationCommand> EditGlobalApplicationCommandAsync
    (
        ulong applicationId,
        ulong commandId,
        Optional<string> name = default,
        Optional<string> description = default,
        Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options = default,
        Optional<bool?> defaultPermission = default,
        Optional<bool?> nsfw = default,
        IReadOnlyDictionary<string, string>? nameLocalizations = null,
        IReadOnlyDictionary<string, string>? descriptionLocalizations = null,
        Optional<bool> allowDmUsage = default,
        Optional<Permissions?> defaultMemberPermissions = default
    )
    {
        RestApplicationCommandEditPayload pld = new()
        {
            Name = name,
            Description = description,
            Options = options,
            DefaultPermission = defaultPermission,
            NameLocalizations = nameLocalizations,
            DescriptionLocalizations = descriptionLocalizations,
            AllowDMUsage = allowDmUsage,
            DefaultMemberPermissions = defaultMemberPermissions,
            NSFW = nsfw,
        };

        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.COMMANDS}/:command_id";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.COMMANDS}/{commandId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordApplicationCommand ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask DeleteGlobalApplicationCommandAsync
    (
        ulong applicationId,
        ulong commandId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.COMMANDS}/:command_id";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.COMMANDS}/{commandId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync
    (
        ulong applicationId,
        ulong guildId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordApplicationCommand> ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response!)!;
        foreach (DiscordApplicationCommand app in ret)
        {
            app.Discord = this._discord!;
        }

        return ret.ToList();
    }

    internal async ValueTask<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync
    (
        ulong applicationId,
        ulong guildId,
        IEnumerable<DiscordApplicationCommand> commands
    )
    {
        List<RestApplicationCommandCreatePayload> pld = new();
        foreach (DiscordApplicationCommand command in commands)
        {
            pld.Add(new RestApplicationCommandCreatePayload
            {
                Type = command.Type,
                Name = command.Name,
                Description = command.Description,
                Options = command.Options,
                DefaultPermission = command.DefaultPermission,
                NameLocalizations = command.NameLocalizations,
                DescriptionLocalizations = command.DescriptionLocalizations,
                AllowDMUsage = command.AllowDMUsage,
                DefaultMemberPermissions = command.DefaultMemberPermissions,
                NSFW = command.NSFW
            });
        }

        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordApplicationCommand> ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response!)!;
        foreach (DiscordApplicationCommand app in ret)
        {
            app.Discord = this._discord!;
        }

        return ret.ToList();
    }

    internal async ValueTask<DiscordApplicationCommand> CreateGuildApplicationCommandAsync
    (
        ulong applicationId,
        ulong guildId,
        DiscordApplicationCommand command
    )
    {
        RestApplicationCommandCreatePayload pld = new()
        {
            Type = command.Type,
            Name = command.Name,
            Description = command.Description,
            Options = command.Options,
            DefaultPermission = command.DefaultPermission,
            NameLocalizations = command.NameLocalizations,
            DescriptionLocalizations = command.DescriptionLocalizations,
            AllowDMUsage = command.AllowDMUsage,
            DefaultMemberPermissions = command.DefaultMemberPermissions,
            NSFW = command.NSFW
        };

        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordApplicationCommand ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordApplicationCommand> GetGuildApplicationCommandAsync
    (
        ulong applicationId,
        ulong guildId,
        ulong commandId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}/:command_id";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}/{commandId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordApplicationCommand ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask<DiscordApplicationCommand> EditGuildApplicationCommandAsync
    (
        ulong applicationId,
        ulong guildId,
        ulong commandId,
        Optional<string> name = default,
        Optional<string> description = default,
        Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options = default,
        Optional<bool?> defaultPermission = default,
        Optional<bool?> nsfw = default,
        IReadOnlyDictionary<string, string>? nameLocalizations = null,
        IReadOnlyDictionary<string, string>? descriptionLocalizations = null,
        Optional<bool> allowDmUsage = default,
        Optional<Permissions?> defaultMemberPermissions = default
    )
    {
        RestApplicationCommandEditPayload pld = new()
        {
            Name = name,
            Description = description,
            Options = options,
            DefaultPermission = defaultPermission,
            NameLocalizations = nameLocalizations,
            DescriptionLocalizations = descriptionLocalizations,
            AllowDMUsage = allowDmUsage,
            DefaultMemberPermissions = defaultMemberPermissions,
            NSFW = nsfw
        };

        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}/:command_id";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}/{commandId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        DiscordApplicationCommand ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response!)!;
        ret.Discord = this._discord!;

        return ret;
    }

    internal async ValueTask DeleteGuildApplicationCommandAsync
    (
        ulong applicationId,
        ulong guildId,
        ulong commandId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}/:command_id";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}/{commandId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask CreateInteractionResponseAsync
    (
        ulong interactionId,
        string interactionToken,
        InteractionResponseType type,
        DiscordInteractionResponseBuilder builder
    )
    {
        if (builder?.Embeds != null)
        {
            foreach (DiscordEmbed embed in builder.Embeds)
            {
                if (embed.Timestamp != null)
                {
                    embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
                }
            }
        }

        RestInteractionResponsePayload pld = new()
        {
            Type = type,
            Data = builder is not null
            ? new DiscordInteractionApplicationCommandCallbackData
            {
                Content = builder.Content,
                Title = builder.Title,
                CustomId = builder.CustomId,
                Embeds = builder.Embeds,
                IsTTS = builder.IsTTS,
                Mentions = new DiscordMentions(builder.Mentions ?? Mentions.All, builder.Mentions?.Any() ?? false),
                Flags = builder.Flags,
                Components = builder.Components,
                Choices = builder.Choices
            }
            : null
        };

        Dictionary<string, string> values = new();

        if (builder != null)
        {
            if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTTS == true || builder.Mentions != null)
            {
                values["payload_json"] = DiscordJson.SerializeObject(pld);
            }
        }

        string route = $"{Endpoints.INTERACTIONS}/{interactionId}/:interaction_token/{Endpoints.CALLBACK}";
        string url = $"{Endpoints.INTERACTIONS}/{interactionId}/{interactionToken}/{Endpoints.CALLBACK}";

        if (builder is not null)
        {
            MultipartRestRequest request = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Post,
                Values = values,
                Files = builder.Files,
                IsExemptFromGlobalLimit = true
            };

            try
            {
                await this._rest.ExecuteRequestAsync(request);
            }
            finally
            {
                builder.ResetFileStreamPositions();
            }
        }
        else
        {
            RestRequest request = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Post,
                Payload = DiscordJson.SerializeObject(pld)
            };

            await this._rest.ExecuteRequestAsync(request);
        }
    }

    internal async ValueTask<DiscordMessage> GetOriginalInteractionResponseAsync
    (
        ulong applicationId,
        string interactionToken
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/:application_id/{interactionToken}/{Endpoints.MESSAGES}/{Endpoints.ORIGINAL}";
        string url = $"{Endpoints.WEBHOOKS}/{applicationId}/{interactionToken}/{Endpoints.MESSAGES}/{Endpoints.ORIGINAL}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get,
            IsExemptFromGlobalLimit = true
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordMessage ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;

        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<DiscordMessage> EditOriginalInteractionResponseAsync
    (
        ulong applicationId,
        string interactionToken,
        DiscordWebhookBuilder builder,
        IEnumerable<DiscordAttachment> attachments
    )
    {
        {
            builder.Validate(true);

            DiscordMentions? mentions = builder.Mentions != null ? new DiscordMentions(builder.Mentions, builder.Mentions.Any()) : null;

            RestWebhookMessageEditPayload pld = new()
            {
                Content = builder.Content,
                Embeds = builder.Embeds,
                Mentions = mentions,
                Components = builder.Components,
                Attachments = attachments
            };

            string route = $"{Endpoints.WEBHOOKS}/:application_id/{interactionToken}/{Endpoints.MESSAGES}/@original";
            string url = $"{Endpoints.WEBHOOKS}/{applicationId}/{interactionToken}/{Endpoints.MESSAGES}/@original";

            Dictionary<string, string> values = new()
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            MultipartRestRequest request = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Patch,
                Values = values,
                Files = builder.Files,
                IsExemptFromGlobalLimit = true
            };

            RestResponse res = await this._rest.ExecuteRequestAsync(request);

            DiscordMessage ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;
            ret.Discord = this._discord!;

            foreach (DiscordMessageFile? file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo!.Value;
            }

            return ret;
        }
    }

    internal async ValueTask DeleteOriginalInteractionResponseAsync
    (
        ulong applicationId,
        string interactionToken
    )
    {
        string route = $"{Endpoints.WEBHOOKS}/:application_id/{interactionToken}/{Endpoints.MESSAGES}/@original";
        string url = $"{Endpoints.WEBHOOKS}/{applicationId}/{interactionToken}/{Endpoints.MESSAGES}/@original";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            IsExemptFromGlobalLimit = true
        };

        await this._rest.ExecuteRequestAsync(request);
    }

    internal async ValueTask<DiscordMessage> CreateFollowupMessageAsync
    (
        ulong applicationId,
        string interactionToken,
        DiscordFollowupMessageBuilder builder
    )
    {
        builder.Validate();

        if (builder.Embeds != null)
        {
            foreach (DiscordEmbed embed in builder.Embeds)
            {
                if (embed.Timestamp != null)
                {
                    embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();
                }
            }
        }

        Dictionary<string, string> values = new();
        RestFollowupMessageCreatePayload pld = new()
        {
            Content = builder.Content,
            IsTTS = builder.IsTTS,
            Embeds = builder.Embeds,
            Flags = builder._flags,
            Components = builder.Components
        };

        if (builder.Mentions != null)
        {
            pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());
        }

        if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTTS == true || builder.Mentions != null)
        {
            values["payload_json"] = DiscordJson.SerializeObject(pld);
        }

        string route = $"{Endpoints.WEBHOOKS}/:application_id/{interactionToken}";
        string url = $"{Endpoints.WEBHOOKS}/{applicationId}/{interactionToken}";

        MultipartRestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Values = values,
            Files = builder.Files,
            IsExemptFromGlobalLimit = true
        };

        RestResponse res;
        try
        {
            res = await this._rest.ExecuteRequestAsync(request);
        }
        finally
        {
            builder.ResetFileStreamPositions();
        }
        DiscordMessage ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response!)!;

        ret.Discord = this._discord!;
        return ret;
    }

    internal ValueTask<DiscordMessage> GetFollowupMessageAsync
    (
        ulong applicationId,
        string interactionToken,
        ulong messageId
    )
        => this.GetWebhookMessageAsync(applicationId, interactionToken, messageId);

    internal ValueTask<DiscordMessage> EditFollowupMessageAsync
    (
        ulong applicationId,
        string interactionToken,
        ulong messageId,
        DiscordWebhookBuilder builder,
        IEnumerable<DiscordAttachment> attachments
    ) =>
        this.EditWebhookMessageAsync(applicationId, interactionToken, messageId, builder, attachments);

    internal ValueTask DeleteFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId)
        => this.DeleteWebhookMessageAsync(applicationId, interactionToken, messageId);

    internal async ValueTask<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> GetGuildApplicationCommandPermissionsAsync
    (
        ulong applicationId,
        ulong guildId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}/{Endpoints.PERMISSIONS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}/{Endpoints.PERMISSIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        IEnumerable<DiscordGuildApplicationCommandPermissions> ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermissions>>(res.Response!)!;

        foreach (DiscordGuildApplicationCommandPermissions perm in ret)
        {
            perm.Discord = this._discord!;
        }

        return ret.ToList();
    }

    internal async ValueTask<DiscordGuildApplicationCommandPermissions> GetApplicationCommandPermissionsAsync
    (
        ulong applicationId,
        ulong guildId,
        ulong commandId
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}/:command_id/{Endpoints.PERMISSIONS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}/{commandId}/{Endpoints.PERMISSIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordGuildApplicationCommandPermissions ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermissions>(res.Response!)!;

        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<DiscordGuildApplicationCommandPermissions> EditApplicationCommandPermissionsAsync
    (
        ulong applicationId,
        ulong guildId,
        ulong commandId,
        IEnumerable<DiscordApplicationCommandPermission> permissions
    )
    {

        RestEditApplicationCommandPermissionsPayload pld = new()
        {
            Permissions = permissions
        };

        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}/:command_id/{Endpoints.PERMISSIONS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}/{commandId}/{Endpoints.PERMISSIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put,
            Payload = DiscordJson.SerializeObject(pld)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordGuildApplicationCommandPermissions ret =
            JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermissions>(res.Response!)!;

        ret.Discord = this._discord!;
        return ret;
    }

    internal async ValueTask<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> BatchEditApplicationCommandPermissionsAsync
    (
        ulong applicationId,
        ulong guildId,
        IEnumerable<DiscordGuildApplicationCommandPermissions> permissions
    )
    {
        string route = $"{Endpoints.APPLICATIONS}/:application_id/{Endpoints.GUILDS}/:guild_id/{Endpoints.COMMANDS}/{Endpoints.PERMISSIONS}";
        string url = $"{Endpoints.APPLICATIONS}/{applicationId}/{Endpoints.GUILDS}/{guildId}/{Endpoints.COMMANDS}/{Endpoints.PERMISSIONS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Put,
            Payload = DiscordJson.SerializeObject(permissions)
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        IEnumerable<DiscordGuildApplicationCommandPermissions> ret =
            JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermissions>>(res.Response!)!;

        foreach (DiscordGuildApplicationCommandPermissions perm in ret)
        {
            perm.Discord = this._discord!;
        }

        return ret.ToList();
    }
    #endregion

    #region Misc
    internal ValueTask<TransportApplication> GetCurrentApplicationInfoAsync()
        => this.GetApplicationInfoAsync("@me");

    internal ValueTask<TransportApplication> GetApplicationInfoAsync
    (
        ulong applicationId
    )
        => this.GetApplicationInfoAsync(applicationId.ToString(CultureInfo.InvariantCulture));

    private async ValueTask<TransportApplication> GetApplicationInfoAsync
    (
        string applicationId
    )
    {
        string route = $"{Endpoints.OAUTH2}/{Endpoints.APPLICATIONS}/:application_id";
        string url = $"{Endpoints.OAUTH2}/{Endpoints.APPLICATIONS}/{applicationId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        return JsonConvert.DeserializeObject<TransportApplication>(res.Response!)!;
    }

    internal async ValueTask<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync
    (
        DiscordApplication application
     )
    {
        string route = $"{Endpoints.OAUTH2}/{Endpoints.APPLICATIONS}/:application_id/{Endpoints.ASSETS}";
        string url = $"{Endpoints.OAUTH2}/{Endpoints.APPLICATIONS}/{application.Id}/{Endpoints.ASSETS}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        IEnumerable<DiscordApplicationAsset> assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response!)!;
        foreach (DiscordApplicationAsset asset in assets)
        {
            asset.Discord = application.Discord;
            asset.Application = application;
        }

        return new ReadOnlyCollection<DiscordApplicationAsset>(new List<DiscordApplicationAsset>(assets));
    }

    internal async ValueTask<GatewayInfo> GetGatewayInfoAsync()
    {
        Dictionary<string, string> headers = new();
        string route = Endpoints.GATEWAY + "/";
        if (this._discord!.Configuration.TokenType == TokenType.Bot)
        {
            route += Endpoints.BOT;
        }

        string url = route;

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get,
            Headers = headers
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);

        GatewayInfo info = JObject.Parse(res.Response!).ToDiscordObject<GatewayInfo>();
        info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.ResetAfterInternal);
        return info;
    }
    #endregion

    public async ValueTask<DiscordForumPostStarter> CreateForumPostAsync
    (
        ulong channelId,
        string name,
        DiscordMessageBuilder message,
        AutoArchiveDuration? autoArchiveDuration = null,
        int? rateLimitPerUser = null,
        IEnumerable<ulong>? appliedTags = null
    )
    {
        string route = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}";
        string url = $"{Endpoints.CHANNELS}/{channelId}/{Endpoints.THREADS}";

        RestForumPostCreatePayload pld = new()
        {
            Name = name,
            ArchiveAfter = autoArchiveDuration,
            RateLimitPerUser = rateLimitPerUser,
            Message = new RestChannelMessageCreatePayload
            {
                Content = message.Content,
                HasContent = !string.IsNullOrWhiteSpace(message.Content),
                Embeds = message.Embeds,
                HasEmbed = message.Embeds.Count > 0,
                Mentions = new DiscordMentions(message.Mentions, message.Mentions.Any()),
                Components = message.Components,
                StickersIds = message.Stickers?.Select(s => s.Id) ?? Array.Empty<ulong>(),
            },
            AppliedTags = appliedTags
        };

        JObject ret;
        RestResponse res;
        if (message.Files.Count is 0)
        {
            RestRequest req = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Post,
                Payload = DiscordJson.SerializeObject(pld)
            };

            res = await this._rest.ExecuteRequestAsync(req);
            ret = JObject.Parse(res.Response!);
        }
        else
        {
            Dictionary<string, string> values = new()
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            MultipartRestRequest req = new()
            {
                Route = route,
                Url = url,
                Method = HttpMethod.Post,
                Values = values,
                Files = message.Files
            };

            res = await this._rest.ExecuteRequestAsync(req);
            ret = JObject.Parse(res.Response!);
        }

        JToken? msgToken = ret["message"];
        ret.Remove("message");

        DiscordMessage msg = this.PrepareMessage(msgToken!);
        // We know the return type; deserialize directly.
        DiscordThreadChannel chn = ret.ToDiscordObject<DiscordThreadChannel>();
        chn.Discord = this._discord!;

        return new DiscordForumPostStarter(chn, msg);
    }

    /// <summary>
    /// Internal method to create an auto-moderation rule in a guild.
    /// </summary>
    /// <param name="guildId">The id of the guild where the rule will be created.</param>
    /// <param name="name">The rule name.</param>
    /// <param name="eventType">The Discord event that will trigger the rule.</param>
    /// <param name="triggerType">The rule trigger.</param>
    /// <param name="triggerMetadata">The trigger metadata.</param>
    /// <param name="actions">The actions that will run when a rule is triggered.</param>
    /// <param name="enabled">Whenever the rule is enabled or not.</param>
    /// <param name="exemptRoles">The exempted roles that will not trigger the rule.</param>
    /// <param name="exemptChannels">The exempted channels that will not trigger the rule.</param>
    /// <param name="reason">The reason for audits logs.</param>
    /// <returns>The created rule.</returns>
    internal async ValueTask<DiscordAutoModerationRule> CreateGuildAutoModerationRuleAsync
    (
        ulong guildId,
        string name,
        DiscordRuleEventType eventType,
        DiscordRuleTriggerType triggerType,
        DiscordRuleTriggerMetadata triggerMetadata,
        IReadOnlyList<DiscordAutoModerationAction> actions,
        Optional<bool> enabled = default,
        Optional<IReadOnlyList<DiscordRole>> exemptRoles = default,
        Optional<IReadOnlyList<DiscordChannel>> exemptChannels = default,
        string? reason = null
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}";

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string payload = DiscordJson.SerializeObject(new
        {
            guild_id = guildId,
            name,
            event_type = eventType,
            trigger_type = triggerType,
            trigger_metadata = triggerMetadata,
            actions,
            enabled,
            exempt_roles = exemptRoles.Value.Select(x => x.Id).ToArray(),
            exempt_channels = exemptChannels.Value.Select(x => x.Id).ToArray()
        });

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Post,
            Headers = headers,
            Payload = payload
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordAutoModerationRule rule = JsonConvert.DeserializeObject<DiscordAutoModerationRule>(res.Response!)!;

        return rule;
    }

    /// <summary>
    /// Internal method to get an auto-moderation rule in a guild.
    /// </summary>
    /// <param name="guildId">The guild id where the rule is in.</param>
    /// <param name="ruleId">The rule id.</param>
    /// <returns>The rule found.</returns>
    internal async ValueTask<DiscordAutoModerationRule> GetGuildAutoModerationRuleAsync
    (
        ulong guildId,
        ulong ruleId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}/:rule_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}/{ruleId}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordAutoModerationRule rule = JsonConvert.DeserializeObject<DiscordAutoModerationRule>(res.Response!)!;

        return rule;
    }

    /// <summary>
    /// Internal method to get all auto-moderation rules in a guild.
    /// </summary>
    /// <param name="guildId">The guild id where rules are in.</param>
    /// <returns>The rules found.</returns>
    internal async ValueTask<IReadOnlyList<DiscordAutoModerationRule>> GetGuildAutoModerationRulesAsync
    (
        ulong guildId
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}";

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Get
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        IReadOnlyList<DiscordAutoModerationRule> rules = JsonConvert.DeserializeObject<IReadOnlyList<DiscordAutoModerationRule>>(res.Response!)!;

        return rules;
    }

    /// <summary>
    /// Internal method to modify an auto-moderation rule in a guild.
    /// </summary>
    /// <param name="guildId">The id of the guild where the rule will be modified.</param>
    /// <param name="ruleId">The id of the rule that will be modified.</param>
    /// <param name="name">The rule name.</param>
    /// <param name="eventType">The Discord event that will trigger the rule.</param>
    /// <param name="triggerMetadata">The trigger metadata.</param>
    /// <param name="actions">The actions that will run when a rule is triggered.</param>
    /// <param name="enabled">Whenever the rule is enabled or not.</param>
    /// <param name="exemptRoles">The exempted roles that will not trigger the rule.</param>
    /// <param name="exemptChannels">The exempted channels that will not trigger the rule.</param>
    /// <param name="reason">The reason for audits logs.</param>
    /// <returns>The modified rule.</returns>
    internal async ValueTask<DiscordAutoModerationRule> ModifyGuildAutoModerationRuleAsync
    (
        ulong guildId,
        ulong ruleId,
        Optional<string> name,
        Optional<DiscordRuleEventType> eventType,
        Optional<DiscordRuleTriggerMetadata> triggerMetadata,
        Optional<IReadOnlyList<DiscordAutoModerationAction>> actions,
        Optional<bool> enabled,
        Optional<IReadOnlyList<DiscordRole>> exemptRoles,
        Optional<IReadOnlyList<DiscordChannel>> exemptChannels,
        string? reason = null
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}/:rule_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}/{ruleId}";

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        string payload = DiscordJson.SerializeObject(new
        {
            name,
            event_type = eventType,
            trigger_metadata = triggerMetadata,
            actions,
            enabled,
            exempt_roles = exemptRoles.Value.Select(x => x.Id).ToArray(),
            exempt_channels = exemptChannels.Value.Select(x => x.Id).ToArray()
        });

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Patch,
            Headers = headers,
            Payload = payload
        };

        RestResponse res = await this._rest.ExecuteRequestAsync(request);
        DiscordAutoModerationRule rule = JsonConvert.DeserializeObject<DiscordAutoModerationRule>(res.Response!)!;

        return rule;
    }

    /// <summary>
    /// Internal method to delete an auto-moderation rule in a guild.
    /// </summary>
    /// <param name="guildId">The id of the guild where the rule is in.</param>
    /// <param name="ruleId">The rule id that will be deleted.</param>
    /// <param name="reason">The reason for audits logs.</param>
    internal async ValueTask DeleteGuildAutoModerationRuleAsync
    (
        ulong guildId,
        ulong ruleId,
        string? reason = null
    )
    {
        string route = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}/:rule_id";
        string url = $"{Endpoints.GUILDS}/{guildId}/{Endpoints.AUTO_MODERATION}/{Endpoints.RULES}/{ruleId}";

        Dictionary<string, string> headers = new();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            headers[REASON_HEADER_NAME] = reason;
        }

        RestRequest request = new()
        {
            Route = route,
            Url = url,
            Method = HttpMethod.Delete,
            Headers = headers
        };

        await this._rest.ExecuteRequestAsync(request);
    }
}
