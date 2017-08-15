using System;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    ///
    /// </summary>
    public class DiscordRole : SnowflakeObject, IEquatable<DiscordRole>
    {
        /// <summary>
        /// Role name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Integer representation of a hexadecimal color code
        /// </summary>
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        internal int _color { get; set; }

        public DiscordColor Color
        {
            get { return new DiscordColor(_color); }
            set { _color = value._color; }
        }

        /// <summary>
        /// Whether this role is pinned
        /// </summary>
        [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
        public bool Hoist { get; internal set; }

        /// <summary>
        /// Position of this role
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; internal set; }

        /// <summary>
        /// Permission bit set
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Permissions { get; internal set; }

        /// <summary>
        /// Whether this role is managed by an integration
        /// </summary>
        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public bool Managed { get; internal set; }

        /// <summary>
        /// Whether this role is mentionable
        /// </summary>
        [JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
        public bool Mentionable { get; internal set; }

        /// <summary>
        /// Mentions the role similar to how a client would, if the role is mentionable
        /// </summary>
        public string Mention => Formatter.Mention(this);
        public PermissionLevel CheckPermission(Permissions permission)
        {
            if ((Permissions & permission) != 0)
                return PermissionLevel.Allowed;
            return PermissionLevel.Unset;
        }

        /// <summary>
        /// Returns a string representation of this role.
        /// </summary>
        /// <returns>String representation of this role.</returns>
        public override string ToString()
        {
            return string.Concat("Role ", this.Id, "; ", this.Name);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordRole"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordRole"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordRole);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordRole"/> is equal to another <see cref="DiscordRole"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordRole"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordRole"/> is equal to this <see cref="DiscordRole"/>.</returns>
        public bool Equals(DiscordRole e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordRole"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordRole"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordRole"/> objects are equal.
        /// </summary>
        /// <param name="e1">First role to compare.</param>
        /// <param name="e2">Second role to compare.</param>
        /// <returns>Whether the two roles are equal.</returns>
        public static bool operator ==(DiscordRole e1, DiscordRole e2)
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
        /// Gets whether the two <see cref="DiscordRole"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First role to compare.</param>
        /// <param name="e2">Second role to compare.</param>
        /// <returns>Whether the two roles are not equal.</returns>
        public static bool operator !=(DiscordRole e1, DiscordRole e2) =>
            !(e1 == e2);
    }
}
