using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord emoji.
    /// </summary>
    public partial class DiscordEmoji : SnowflakeObject, IEquatable<DiscordEmoji>
    {
        /// <summary>
        /// Gets the name of this emoji.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets IDs the roles this emoji is enabled for.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<ulong> Roles => this._rolesLazy.Value;

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal List<ulong> _roles;
        private Lazy<IReadOnlyList<ulong>> _rolesLazy;

        /// <summary>
        /// Gets whether this emoji requires colons to use.
        /// </summary>
        [JsonProperty("require_colons")]
        public bool RequiresColons { get; internal set; }

        /// <summary>
        /// Gets whether this emoji is managed by an integration.
        /// </summary>
        [JsonProperty("managed")]
        public bool IsManaged { get; internal set; }

        /// <summary>
        /// Gets whether this emoji is animated.
        /// </summary>
        [JsonProperty("animated")]
        public bool IsAnimated { get; internal set; }

        /// <summary>
        /// Gets the image URL of this emoji.
        /// </summary>
        [JsonIgnore]
        public string Url
        {
            get
            {
                if (this.Id == 0)
                    throw new InvalidOperationException("Cannot get URL of unicode emojis.");

                if (this.IsAnimated)
                    return $"https://cdn.discordapp.com/emojis/{this.Id.ToString(CultureInfo.InvariantCulture)}.gif";

                return $"https://cdn.discordapp.com/emojis/{this.Id.ToString(CultureInfo.InvariantCulture)}.png";
            }
        }

        internal DiscordEmoji()
        {
            this._rolesLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._roles));
        }

        /// <summary>
        /// Gets emoji's name in non-Unicode format (eg. :thinking: instead of the Unicode representation of the emoji).
        /// </summary>
        public string GetDiscordName()
        {
            DiscordNameLookup.TryGetValue(this.Name, out var name);

            if (name == null)
                return $":{ this.Name }:";

            return name;
        }

        /// <summary>
        /// Returns a string representation of this emoji.
        /// </summary>
        /// <returns>String representation of this emoji.</returns>
        public override string ToString()
        {
            if (this.Id != 0)
            {
                if (this.IsAnimated)
                    return $"<a:{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}>";
                else
                    return $"<:{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}>";
            }

            return this.Name;
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordEmoji"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordEmoji"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordEmoji);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordEmoji"/> is equal to another <see cref="DiscordEmoji"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordEmoji"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordEmoji"/> is equal to this <see cref="DiscordEmoji"/>.</returns>
        public bool Equals(DiscordEmoji e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id && this.Name == e.Name;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordEmoji"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordEmoji"/>.</returns>
        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + this.Id.GetHashCode();
            hash = (hash * 7) + this.Name.GetHashCode();

            return hash;
        }

        internal string ToReactionString()
        {
            if (this.Id != 0)
                return $"{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}";
            return this.Name;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordEmoji"/> objects are equal.
        /// </summary>
        /// <param name="e1">First emoji to compare.</param>
        /// <param name="e2">Second emoji to compare.</param>
        /// <returns>Whether the two emoji are equal.</returns>
        public static bool operator ==(DiscordEmoji e1, DiscordEmoji e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id && e1.Name == e2.Name;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordEmoji"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First emoji to compare.</param>
        /// <param name="e2">Second emoji to compare.</param>
        /// <returns>Whether the two emoji are not equal.</returns>
        public static bool operator !=(DiscordEmoji e1, DiscordEmoji e2) 
            => !(e1 == e2);

        /// <summary>
        /// Implicitly converts this emoji to its string representation.
        /// </summary>
        /// <param name="e1">Emoji to convert.</param>
        public static implicit operator string(DiscordEmoji e1) 
            => e1.ToString();

        /// <summary>
        /// Creates an emoji object from a unicode entity.
        /// </summary>
        /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
        /// <param name="unicode_entity">Unicode entity to create the object from.</param>
        /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji FromUnicode(BaseDiscordClient client, string unicode_entity)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "Client cannot be null.");

            return new DiscordEmoji { Name = unicode_entity, Discord = client };
        }

        /// <summary>
        /// Creates an emoji object from a unicode entity.
        /// </summary>
        /// <param name="unicode_entity">Unicode entity to create the object from.</param>
        /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji FromUnicode(string unicode_entity)
        {
            return new DiscordEmoji { Name = unicode_entity, Discord = null };
        }

        /// <summary>
        /// Creates an emoji object from a guild emote.
        /// </summary>
        /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
        /// <param name="id">Id of the emote.</param>
        /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji FromGuildEmote(BaseDiscordClient client, ulong id)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "Client cannot be null.");

            foreach (var guild in client.Guilds.Values)
            {
                if (guild.Emojis.TryGetValue(id, out var found))
                    return found;
            }
            
            throw new KeyNotFoundException("Given emote was not found.");
        }

        /// <summary>
        /// Creates a DiscordEmoji from emote name that includes colons (eg. :thinking:). This method also supports skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), as well as guild emoji (still specified by :name:).
        /// </summary>
        /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
        /// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
        /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji FromName(BaseDiscordClient client, string name)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "Client cannot be null.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Name cannot be empty or null.");

            if (UnicodeEmojis.ContainsKey(name))
                return new DiscordEmoji { Discord = client, Name = UnicodeEmojis[name] };

            var allEmojis = client.Guilds.Values.SelectMany(xg => xg.Emojis.Values).OrderBy(xe => xe.Name);
            
            var ek = name.Substring(1, name.Length - 2);
            foreach (var emoji in allEmojis)
                if (emoji.Name == ek)
                    return emoji;

            throw new ArgumentException("Invalid emoji name specified.", nameof(name));
        }
    }
}
