using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// An action which will execute whenever a rule is triggered.
    /// </summary>
    public sealed record InternalAutoModerationAction
    {
        /// <summary>
        /// The type of action.
        /// </summary>
        [JsonPropertyName("type")]
        public InternalAutoModerationActionType Type { get; init; }

        /// <summary>
        /// The additional metadata needed during execution for this specific action type.
        /// </summary>
        /// <remarks>
        /// Can be omitted based on type. See the Associated Action Types column in action metadata to understand which type values require metadata to be set.
        /// </remarks>
        [JsonPropertyName("metadata")]
        public InternalAutoModerationActionMetadata Metadata { get; init; } = null!;
    }
}
