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
        public Permissions Allow { get; set; }
        /// <summary>
        /// Permission bit set
        /// </summary>
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Deny { get; set; }

        public PermissionLevel CheckPermission(Permissions permission)
        {
            if ((Allow & permission) != 0)
                return PermissionLevel.Allowed;
            if ((Deny & permission) != 0)
                return PermissionLevel.Denied;
            return PermissionLevel.Unset;
        }

        public void DenyPermission(Permissions p) { Deny = DiscordClient.InternalAddPermission(Deny, p); }
        public void UndenyPermission(Permissions p) { Deny = DiscordClient.InternalRemovePermission(Deny, p); }
        public void AllowPermission(Permissions p) { Allow = DiscordClient.InternalAddPermission(Allow, p); }
        public void UnallowPermission(Permissions p) { Allow = DiscordClient.InternalRemovePermission(Allow, p); }
    }
}
