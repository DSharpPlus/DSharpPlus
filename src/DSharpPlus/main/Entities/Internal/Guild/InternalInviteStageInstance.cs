using System;
using System.Collections.Generic;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// This is deprecated.
/// </summary>
[Obsolete("This is deprecated.", false)]
public sealed record InternalInviteStageInstance
{
    /// <summary>
    /// The members speaking in the stage instance.
    /// </summary>
    public required IReadOnlyList<InternalGuildMember> Members { get; init; } 

    /// <summary>
    /// The number of users in the stage instance.
    /// </summary>
    public required int ParticipantCount { get; init; }

    /// <summary>
    /// The number of users speaking in the stage instance.
    /// </summary>
    public required int SpeakerCount { get; init; }

    /// <summary>
    /// The topic of the stage instance (1-120 characters).
    /// </summary>
    public required string Topic { get; init; } 
}
