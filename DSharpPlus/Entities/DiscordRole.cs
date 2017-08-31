using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a discord role, to which users can be assigned.
    /// </summary>
    public class DiscordRole : SnowflakeObject, IEquatable<DiscordRole>
    {
        /// <summary>
        /// Gets the name of this role.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the color of this role.
        /// </summary>
        [JsonIgnore]
        public DiscordColor Color => new DiscordColor(this._color);
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        internal int _color;

        /// <summary>
        /// Gets whether this role is hoisted.
        /// </summary>
        [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsHoisted { get; internal set; }

        /// <summary>
        /// Gets the position of this role in the role hierarchy.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; internal set; }

        /// <summary>
        /// Gets the permissions set for this role.
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Permissions { get; internal set; }

        /// <summary>
        /// Gets whether this role is managed by an integration.
        /// </summary>
        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsManaged { get; internal set; }

        /// <summary>
        /// Gets whether this role is mentionable.
        /// </summary>
        [JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsMentionable { get; internal set; }

        /// <summary>
        /// Gets a mention string for this role. If the role is mentionable, this string will mention all the users that belong to this role.
        /// </summary>
        public string Mention => Formatter.Mention(this);

        /// <summary>
        /// Checks whether this role has specific permissions.
        /// </summary>
        /// <param name="permission">Permissions to check for.</param>
        /// <returns>Whether the permissions are allowed or not.</returns>
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
