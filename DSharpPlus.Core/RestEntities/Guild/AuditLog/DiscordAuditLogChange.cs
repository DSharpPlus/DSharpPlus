using Newtonsoft.Json;

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
        [JsonProperty("new_value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<object?> NewValue { get; init; }

        /// <summary>
        /// Old value of the key.
        /// </summary>
        [JsonProperty("old_value", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<object?> OldValue { get; init; }

        /// <summary>
        /// Name of audit log change key.
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string? Key { get; init; }
    }
}
