using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about a user select menu submitted through a modal.
/// </summary>
public sealed class UserSelectMenuModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.UserSelect;

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <summary>
    /// The snowflake identifiers of the users submitted.
    /// </summary>
    public IReadOnlyList<ulong> Ids { get; internal set; }

    internal UserSelectMenuModalSubmission(string customId, IReadOnlyList<ulong> ids)
    {
        this.CustomId = customId;
        this.Ids = ids;
    }
}
