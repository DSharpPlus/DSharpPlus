using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    /// <summary>
    /// Represents an OAuth2 application.
    /// </summary>
    public class DiscordApplication : SnowflakeObject, IEquatable<DiscordApplication>
    {
        /// <summary>
        /// Gets the application's description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the application's icon.
        /// </summary>
        public string Icon => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/app-icons/{this.Id}/{this.IconHash}.png?size=1024" : null;
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        internal string IconHash { get; set; }

        /// <summary>
        /// Gets the application's name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the application's allowed RPC origins.
        /// </summary>
        [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> RpcOrigins { get; internal set; }

        /// <summary>
        /// Gets the application's flags.
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public int Flags { get; internal set; }

        /// <summary>
        /// Gets the application's owner.
        /// </summary>
        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Owner { get; internal set; }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplication"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordApplication"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordApplication);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplication"/> is equal to another <see cref="DiscordApplication"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordApplication"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordApplication"/> is equal to this <see cref="DiscordApplication"/>.</returns>
        public bool Equals(DiscordApplication e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordApplication"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordApplication"/> objects are equal.
        /// </summary>
        /// <param name="e1">First application to compare.</param>
        /// <param name="e2">Second application to compare.</param>
        /// <returns>Whether the two applications are equal.</returns>
        public static bool operator ==(DiscordApplication e1, DiscordApplication e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordApplication"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First application to compare.</param>
        /// <param name="e2">Second application to compare.</param>
        /// <returns>Whether the two applications are not equal.</returns>
        public static bool operator !=(DiscordApplication e1, DiscordApplication e2) =>
            !(e1 == e2);
    }
}
