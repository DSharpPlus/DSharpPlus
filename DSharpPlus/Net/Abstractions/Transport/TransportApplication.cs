using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal sealed class TransportApplication
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
    public ulong Id { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
    public string Name { get; set; }

    [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
    public string IconHash { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
    public string Description { get; set; }
    
    [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
    public IList<string> RpcOrigins { get; set; }

    [JsonProperty("bot_public", NullValueHandling = NullValueHandling.Include)]
    public bool IsPublicBot { get; set; }

    [JsonProperty("bot_require_code_grant", NullValueHandling = NullValueHandling.Include)]
    public bool BotRequiresCodeGrant { get; set; }
    
    [JsonProperty("bot")]
    public TransportUser? Bot { get; set; }

    [JsonProperty("terms_of_service_url", NullValueHandling = NullValueHandling.Ignore)]
    public string? TermsOfServiceUrl { get; set; }

    [JsonProperty("privacy_policy_url", NullValueHandling = NullValueHandling.Ignore)]
    public string? PrivacyPolicyUrl { get; set; }

    [JsonProperty("owner", NullValueHandling = NullValueHandling.Include)]
    public TransportUser? Owner { get; set; }
    
    [JsonProperty("verify_key", NullValueHandling = NullValueHandling.Include)]
    public string VerifyKey { get; set; }

    [JsonProperty("team", NullValueHandling = NullValueHandling.Include)]
    public TransportTeam? Team { get; set; }
    
    [JsonProperty("guild_id")]
    public Optional<ulong> GuildId { get; set; }
    
    [JsonProperty("guild_id")]
    public DiscordGuild? Guild { get; set; }

    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordApplicationFlags? Flags { get; set; }
    
    [JsonProperty("approximate_guild_count")]
    public int? ApproximateGuildCount { get; set; }

    internal TransportApplication() { }
}
