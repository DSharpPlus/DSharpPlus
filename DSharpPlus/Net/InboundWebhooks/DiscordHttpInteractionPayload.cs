using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.InboundWebhooks;

/// <summary>
/// Represents a transport-time wrapper for a pre-deserialized interaction and the raw data it was spawned from.
/// </summary>
/// <param name="ProtoInteraction">A partly deserialized interaction, as far as is possible to deserialize.</param>
/// <param name="Data">The data it was spawned from.</param>
public readonly record struct DiscordHttpInteractionPayload(DiscordHttpInteraction ProtoInteraction, JObject Data);
