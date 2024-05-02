using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

public class AutoModerationRuleEditModel : BaseEditModel
{
    /// <summary>
    /// The new rule name.
    /// </summary>
    public Optional<string> Name { internal get; set; }

    /// <summary>
    /// The new rule event type.
    /// </summary>
    public Optional<DiscordRuleEventType> EventType { internal get; set; }

    /// <summary>
    /// The new rule trigger metadata.
    /// </summary>
    public Optional<DiscordRuleTriggerMetadata> TriggerMetadata { internal get; set; }

    /// <summary>
    /// The new rule actions.
    /// </summary>
    public Optional<IReadOnlyList<DiscordAutoModerationAction>> Actions { internal get; set; }

    /// <summary>
    /// The new rule status.
    /// </summary>
    public Optional<bool> Enable { internal get; set; }

    /// <summary>
    /// The new rule exempt roles.
    /// </summary>
    public Optional<IReadOnlyList<DiscordRole>> ExemptRoles { internal get; set; }

    /// <summary>
    /// The new rule exempt channels.
    /// </summary>
    public Optional<IReadOnlyList<DiscordChannel>> ExemptChannels { internal get; set; }
}
