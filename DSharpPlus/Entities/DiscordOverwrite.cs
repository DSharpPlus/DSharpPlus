﻿using Newtonsoft.Json;
using System.Threading.Tasks;
using System;

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
        public Permissions Allowed { get; internal set; }

        /// <summary>
        /// Gets the denied permission set.
        /// </summary>
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Denied { get; internal set; }

        [JsonIgnore]
        internal ulong _channel_id;

        #region Methods
        /// <summary>
        /// Deletes this channel overwrite.
        /// </summary>
        /// <param name="reason">Reason as to why this overwrite gets deleted.</param>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) => Discord.ApiClient.DeleteChannelPermissionAsync(_channel_id, Id, reason);

        /// <summary>
        /// Updates this channel overwrite.
        /// </summary>
        /// <param name="allow">Permissions that are allowed.</param>
        /// <param name="deny">Permissions that are denied.</param>
        /// <param name="reason">Reason as to why you made this change.</param>
        /// <returns></returns>
        public Task UpdateAsync(Permissions? allow = null, Permissions? deny = null, string reason = null)
            => Discord.ApiClient.EditChannelPermissionsAsync(_channel_id, Id, allow ?? Allowed, deny ?? Denied, Type.ToString().ToLowerInvariant(), reason);
        #endregion
        
        /// <summary>
        /// Gets the DiscordMember that is affected by this overwrite.
        /// </summary>
        /// <returns>The DiscordMember that is affected by this overwrite</returns>
        public async Task<DiscordMember> GetMemberAsync()
        {
            if (Type != OverwriteType.Member)
            {
                throw new ArgumentException(nameof(Type), "This overwrite is for a role, not a member.");
            }

            return await (await Discord.ApiClient.GetChannelAsync(_channel_id).ConfigureAwait(false)).Guild.GetMemberAsync(Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the DiscordRole that is affected by this overwrite.
        /// </summary>
        /// <returns>The DiscordRole that is affected by this overwrite</returns>
        public async Task<DiscordRole> GetRoleAsync()
        {
            if (Type != OverwriteType.Role)
            {
                throw new ArgumentException(nameof(Type), "This overwrite is for a member, not a role.");
            }

            return (await Discord.ApiClient.GetChannelAsync(_channel_id).ConfigureAwait(false)).Guild.GetRole(Id);
        }

        internal DiscordOverwrite() { }

        /// <summary>
        /// Checks whether given permissions are allowed, denied, or not set.
        /// </summary>
        /// <param name="permission">Permissions to check.</param>
        /// <returns>Whether given permissions are allowed, denied, or not set.</returns>
        public PermissionLevel CheckPermission(Permissions permission)
        {
            if ((Allowed & permission) != 0)
            {
                return PermissionLevel.Allowed;
            }

            if ((Denied & permission) != 0)
            {
                return PermissionLevel.Denied;
            }

            return PermissionLevel.Unset;
        }
    }
}
