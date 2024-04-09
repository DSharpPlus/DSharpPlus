using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public abstract class DiscordInteractionMetadata : SnowflakeObject
{
    /// <summary>
    /// The name of the invoked command.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; internal set; }

    /// <summary>
    /// Type of interaction.
    /// </summary>
    [JsonProperty("type")]
    public InteractionType Type { get; internal set; }

    /// <summary>
    /// Discord user object for the invoking user, if invoked in a DM.
    /// </summary>
    [JsonIgnore]
    public DiscordUser User => this.Discord.GetCachedOrEmptyUserInternal(this.UserId);

    /// <summary>
    /// User object for the invoking user, if invoked in a DM.
    /// </summary>
    [JsonProperty("user_id")]
    internal ulong UserId { get; set; }

    /// <summary>
    /// Mapping of installation contexts that the interaction was authorized for to related user or guild IDs.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ApplicationIntegrationType, ulong> AuthorizingIntegrationOwners => this._authorizingIntegrationOwners;

    /// <summary>
    /// Mapping of installation contexts that the interaction was authorized for to related user or guild IDs.
    /// </summary>
    [JsonProperty("authorizing_integration_owners", NullValueHandling = NullValueHandling.Ignore)]
    private Dictionary<ApplicationIntegrationType, ulong> _authorizingIntegrationOwners;

    /// <summary>
    /// ID of the original response message, present only on follow-up messages.
    /// </summary>
    [JsonProperty("original_response_message_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? OriginalMessageID { get; internal set; }

    /// <summary>
    /// Creates a new instance of a <see cref="DiscordInteractionMetadata"/>.
    /// </summary>
    internal DiscordInteractionMetadata() { }
}
