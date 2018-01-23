using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    public class DiscordOverwriteBuilder
    {
        public Permissions Allowed;
        public Permissions Denied;
        public OverwriteType Type;
        public ulong Id;

        public DiscordOverwriteBuilder()
        {

        }

        /// <summary>
        /// Allows a permission for this overwrite.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public DiscordOverwriteBuilder Allow(Permissions permission)
        {
            Allowed = permission | Allowed;
            return this;
        }

        /// <summary>
        /// Denies a permission for this overwrite.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public DiscordOverwriteBuilder Deny(Permissions permission)
        {
            Denied = permission | Denied;
            return this;
        }

        /// <summary>
        /// Sets the ID of this overwrite.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public DiscordOverwriteBuilder ForId(ulong Id)
        {
            this.Id = Id;
            return this;
        }

        /// <summary>
        /// Sets the Overwrite type of this overwrite.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public DiscordOverwriteBuilder WithType(OverwriteType Type)
        {
            this.Type = Type;
            return this;
        }

        /// <summary>
        /// Builds this DiscordOverwrite.
        /// </summary>
        /// <returns>Use this object for creation of new overwrites.</returns>
        internal DiscordRestOverwrite Build()
        {
            return new DiscordRestOverwrite()
            {
                Allow = this.Allowed,
                Deny = this.Denied,
                Id = this.Id,
                Type = this.Type,
            };
        }
    }

    internal struct DiscordRestOverwrite
    {
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        internal Permissions Allow { get; set; }

        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        internal Permissions Deny { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        internal OverwriteType Type { get; set; }
    }
}
