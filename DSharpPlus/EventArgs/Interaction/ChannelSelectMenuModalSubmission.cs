using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about a channel select menu submitted through a modal.
/// </summary>
public sealed class ChannelSelectMenuModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.ChannelSelect;

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <summary>
    /// The snowflake identifiers of the channels submitted.
    /// </summary>
    public IReadOnlyList<ulong> Ids { get; internal set; }

    internal ChannelSelectMenuModalSubmission(string customId, IReadOnlyList<ulong> ids)
    {
        this.CustomId = customId;
        this.Ids = ids;
    }
}
