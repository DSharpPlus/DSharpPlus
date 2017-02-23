using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordOverwrite : SnowflakeObject
    {
        /// <summary>
        /// Either "role" or "member"
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }
        /// <summary>
        /// Termission bit set
        /// </summary>
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        public Permission Allow;
        /// <summary>
        /// Permission bit set
        /// </summary>
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public Permission Deny;

        public PermissionLevel CheckPermission(Permission permission)
        {
            if ((Allow & permission) != 0)
                return PermissionLevel.Allowed;
            if ((Deny & permission) != 0)
                return PermissionLevel.Denied;
            return PermissionLevel.Unset;
        }

        public void DenyPermission(Permission p) { Deny = DiscordClient.InternalAddPermission(Deny, p); }
        public void UndenyPermission(Permission p) { Deny = DiscordClient.InternalRemovePermission(Deny, p); }
        public void AllowPermission(Permission p) { Allow = DiscordClient.InternalAddPermission(Allow, p); }
        public void UnallowPermission(Permission p) { Allow = DiscordClient.InternalRemovePermission(Allow, p); }
    }
}
