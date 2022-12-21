using System;
using System.Collections.Generic;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// This is deprecated.
    /// </summary>
    [Obsolete("This is deprecated.", false)]
    public sealed record InternalInviteStageInstance
    {
        /// <summary>
        /// The members speaking in the stage instance.
        /// </summary>
        public IReadOnlyList<InternalGuildMember> Members { get; init; } = Array.Empty<InternalGuildMember>();

        /// <summary>
        /// The number of users in the stage instance.
        /// </summary>
        public int ParticipantCount { get; init; }

        /// <summary>
        /// The number of users speaking in the stage instance.
        /// </summary>
        public int SpeakerCount { get; init; }

        /// <summary>
        /// The topic of the stage instance (1-120 characters).
        /// </summary>
        public string Topic { get; init; } = null!;
    }
}
