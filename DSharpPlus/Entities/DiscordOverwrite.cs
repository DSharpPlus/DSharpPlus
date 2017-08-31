using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a permission overwrite for a channel.
    /// </summary>
    public class DiscordOverwrite : SnowflakeObject
    {
        /// <summary>
        /// Gets the type of the overwrite. Either "role" or "member".
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }

        /// <summary>
        /// Gets the allowed permission set.
        /// </summary>
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Allow { get; internal set; }

        /// <summary>
        /// Gets the denied permission set.
        /// </summary>
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Deny { get; internal set; }

        /// <summary>
        /// Checks whether given permissions are allowed, denied, or not set.
        /// </summary>
        /// <param name="permission">Permissions to check.</param>
        /// <returns>Whether given permissions are allowed, denied, or not set.</returns>
        public PermissionLevel CheckPermission(Permissions permission)
        {
            if ((Allow & permission) != 0)
                return PermissionLevel.Allowed;
            if ((Deny & permission) != 0)
                return PermissionLevel.Denied;
            return PermissionLevel.Unset;
        }
    }
}
