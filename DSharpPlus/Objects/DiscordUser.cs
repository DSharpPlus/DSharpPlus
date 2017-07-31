using System;
using DSharpPlus.Objects.Transport;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a Discord user.
    /// </summary>
    public class DiscordUser : SnowflakeObject, IEquatable<DiscordUser>
    {
        internal DiscordUser() { }
        internal DiscordUser(TransportUser transport)
        {
            this.Id = transport.Id;
            this.Username = transport.Username;
            this.DiscriminatorInt = transport.DiscriminatorInt;
            this.AvatarHash = transport.AvatarHash;
            this.IsBot = transport.IsBot;
            this.MfaEnabled = transport.MfaEnabled;
            this.Verified = transport.Verified;
            this.Email = transport.Email;
        }

        /// <summary>
        /// Gets this user's username.
        /// </summary>
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; internal set; }

        /// <summary>
        /// Gets the user's 4-digit tag.
        /// </summary>
        [JsonIgnore]
        public string Discriminator => this.DiscriminatorInt.ToString("0000");
        [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
        internal int DiscriminatorInt { get; set; }

        /// <summary>
        /// Gets the user's avatar hash.
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarHash { get; internal set; }

        /// <summary>
        /// Gets the user's avatar URL.
        /// </summary>
        [JsonIgnore]
        public string AvatarUrl => !string.IsNullOrWhiteSpace(this.AvatarHash) ? (AvatarHash.StartsWith("a_") ? $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.gif?size=1024" : $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png?size=1024") : this.DefaultAvatarUrl;

        /// <summary>
        /// Gets the URL of default avatar for this user.
        /// </summary>
        [JsonIgnore]
        public string DefaultAvatarUrl => $"https://cdn.discordapp.com/embed/avatars/{this.DiscriminatorInt % 5}.png?size=1024";

        /// <summary>
        /// Gets whether the user is a bot.
        /// </summary>
        [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsBot { get; internal set; }

        /// <summary>
        /// Gets whether the user has multi-factor authentication enabled.
        /// </summary>
        [JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MfaEnabled { get; internal set; }

        /// <summary>
        /// Gets whether the user is verified.
        /// </summary>
        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Verified { get; internal set; }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; internal set; }

        /// <summary>
        /// Gets the user's mention string.
        /// </summary>
        [JsonIgnore]
        public string Mention => Formatter.Mention(this, this is DiscordMember);

        /// <summary>
        /// Gets this user's presence.
        /// </summary>
        [JsonIgnore]
        public DiscordPresence Presence => this.Discord.Presences.ContainsKey(this.Id) ? this.Discord.Presences[this.Id] : null;

        /// <summary>
        /// Returns a string representation of this user.
        /// </summary>
        /// <returns>String representation of this user.</returns>
        public override string ToString()
        {
            return string.Concat("Member ", this.Id, "; ", this.Username, "#", this.Discriminator);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordUser"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordUser"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordUser);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordUser"/> is equal to another <see cref="DiscordUser"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordUser"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordUser"/> is equal to this <see cref="DiscordUser"/>.</returns>
        public bool Equals(DiscordUser e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordUser"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordUser"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordUser"/> objects are equal.
        /// </summary>
        /// <param name="e1">First user to compare.</param>
        /// <param name="e2">Second user to compare.</param>
        /// <returns>Whether the two users are equal.</returns>
        public static bool operator ==(DiscordUser e1, DiscordUser e2)
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
        /// Gets whether the two <see cref="DiscordUser"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First user to compare.</param>
        /// <param name="e2">Second user to compare.</param>
        /// <returns>Whether the two users are not equal.</returns>
        public static bool operator !=(DiscordUser e1, DiscordUser e2) =>
            !(e1 == e2);
    }
}
