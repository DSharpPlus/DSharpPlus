using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a permission for a application command.
    /// </summary>
    public class DiscordApplicationCommandPermission
    {
        /// <summary>
        /// The id of the role or the user this permission is for.
        /// </summary>
        [JsonProperty("id")]
        public ulong Id { get; internal set; }

        /// <summary>
        /// Gets the type of the permission.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandPermissionType Type { get; internal set; }

        /// <summary>
        /// Gets whether the command is enabled for the role or user.
        /// </summary>
        [JsonProperty("permission")]
        public bool Permission { get; internal set; }

        /// <summary>
        /// Represents a permission for a application command.
        /// </summary>
        /// <param name="role">The role to construct the permission for.</param>
        /// <param name="permission">Whether the command should be enabled for the role.</param>
        public DiscordApplicationCommandPermission(DiscordRole role, bool permission)
        {
            this.Id = role.Id;
            this.Type = ApplicationCommandPermissionType.Role;
            this.Permission = permission;
        }

        /// <summary>
        /// Represents a permission for a application command.
        /// </summary>
        /// <param name="member">The member to construct the permission for.</param>
        /// <param name="permission">Whether the command should be enabled for the role.</param>
        public DiscordApplicationCommandPermission(DiscordMember member, bool permission)
        {
            this.Id = member.Id;
            this.Type = ApplicationCommandPermissionType.User;
            this.Permission = permission;
        }

        internal DiscordApplicationCommandPermission() { }
    }
}
