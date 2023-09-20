using System.Collections.Generic;
using Newtonsoft.Json;
namespace DSharpPlus.EventArgs
{
    public class ApplicationCommandPermissionsUpdatedEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// The Id of the guild the command was updated for.
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// The Id of the command that was updated.
        /// </summary>
        [JsonProperty("id")]
        public ulong CommandId { get; internal set; }

        /// <summary>
        /// The Id of the application the command was updated for.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }

        /// <summary>
        /// The new permissions for the command.
        /// </summary>
        [JsonProperty("permissions")]
        public IReadOnlyList<ApplicationCommandPermissionUpdate> NewPermissions { get; internal set; }
    }

    public class ApplicationCommandPermissionUpdate
    {
        /// <summary>
        /// The Id of the entity this permission is for.
        /// </summary>
        [JsonProperty("id")]
        public ulong Id { get; internal set; }

        /// <summary>
        /// Whether the role/user/channel [or anyone in the channel/with the role] is allowed to use the command.
        /// </summary>
        [JsonProperty("permission")]
        public bool Allow { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandPermissionType Type { get; internal set; }
    }
}
