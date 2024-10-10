using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying an auto moderation rule.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class AutoModerationRuleEditModel : BaseEditModel
{
    /// <summary>
    /// The new rule name.
    /// </summary>
    public Optional<string> Name { get; set; }

    /// <summary>
    /// The new rule event type.
    /// </summary>
    public Optional<DiscordRuleEventType> EventType { get; set; }

    /// <summary>
    /// The new rule trigger metadata.
    /// </summary>
    public Optional<DiscordRuleTriggerMetadata> TriggerMetadata { get; set; }

    /// <summary>
    /// The new rule actions.
    /// </summary>
    public Optional<List<DiscordAutoModerationAction>> Actions { get; set; }

    /// <summary>
    /// The new rule status.
    /// </summary>
    public Optional<bool> Enable { get; set; }

    /// <summary>
    /// The new rule exempt roles.
    /// </summary>
    public Optional<List<DiscordRole>> ExemptRoles { get; set; }

    /// <summary>
    /// The new rule exempt channels.
    /// </summary>
    public Optional<List<DiscordChannel>> ExemptChannels { get; set; }
}
