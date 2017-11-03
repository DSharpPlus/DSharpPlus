using Newtonsoft.Json;
using System.Threading.Tasks;

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
        public OverwriteType Type { get; internal set; }

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

        [JsonIgnore]
        internal ulong _channel_id;

        #region Methods
        /// <summary>
        /// Deletes this channel overwrite.
        /// </summary>
        /// <param name="reason">Reason as to why this overwrite gets deleted.</param>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) => this.Discord.ApiClient.DeleteChannelPermissionAsync(this._channel_id, this.Id, reason);

        /// <summary>
        /// Updates this channel overwrite.
        /// </summary>
        /// <param name="allowed">Permissions that are allowed.</param>
        /// <param name="denied">Permissions that are denied.</param>
        /// <param name="reason">Reason as to why you made this change.</param>
        /// <returns></returns>
        public Task UpdateAsync(Permissions allowed, Permissions denied, string reason = null)
            => this.Discord.ApiClient.EditChannelPermissionsAsync(this._channel_id, this.Id, allowed, denied, this.Type.ToString(), reason);
        #endregion

        internal DiscordOverwrite() { }

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
