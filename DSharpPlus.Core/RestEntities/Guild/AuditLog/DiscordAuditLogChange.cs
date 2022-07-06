using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    /// <remarks>
    /// If <see cref="NewValue"/> is not present in the change object, while <see cref="OldValue"/> is, that means the property that was changed has been reset, or set to null
    /// </remarks>
    public sealed record DiscordAuditLogChange
    {
        /// <summary>
        /// New value of the key.
        /// </summary>
        [JsonPropertyName("new_value")]
        public Optional<object?> NewValue { get; init; }

        /// <summary>
        /// Old value of the key.
        /// </summary>
        [JsonPropertyName("old_value")]
        public Optional<object?> OldValue { get; init; }

        /// <summary>
        /// Name of audit log change key.
        /// </summary>
        [JsonPropertyName("key")]
        public string? Key { get; init; }
    }
}
