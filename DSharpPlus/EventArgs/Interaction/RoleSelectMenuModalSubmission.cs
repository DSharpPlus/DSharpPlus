using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about a role select menu submitted through a modal.
/// </summary>
public sealed class RoleSelectMenuModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.RoleSelect;

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <summary>
    /// The snowflake identifiers of the roles submitted.
    /// </summary>
    public IReadOnlyList<ulong> Ids { get; internal set; }

    internal RoleSelectMenuModalSubmission(string customId, IReadOnlyList<ulong> ids)
    {
        this.CustomId = customId;
        this.Ids = ids;
    }
}
