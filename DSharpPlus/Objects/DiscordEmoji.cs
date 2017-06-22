using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DiscordEmoji : SnowflakeObject
    {
        /// <summary>
        /// Emoji Name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }
        /// <summary>
        /// Roles this emoji is active for
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<ulong> Roles { get; internal set; }
        /// <summary>
        /// Whether this emoji must be wrapped in colons
        /// </summary>
        [JsonProperty("require_colons", NullValueHandling = NullValueHandling.Ignore)]
        public bool RequireColons { get; internal set; }
        /// <summary>
        /// Whether this emoji is managed
        /// </summary>
        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public bool Managed { get; internal set; }

        /// <summary>
        /// Returns a string representation of this emoji.
        /// </summary>
        /// <returns>String representation of this emoji.</returns>
        public override string ToString()
        {
            if (this.Id != 0)
                return $"<:{this.Name}:{this.Id}>";
            return this.Name;
        }

        internal string ToReactionString()
        {
            if (this.Id != 0)
                return $"{this.Name}:{this.Id}";
            return this.Name;
        }

        /// <summary>
        /// Creates an emoji object from a unicode entity.
        /// </summary>
        /// <param name="client"><see cref="DiscordClient"/> to attach to the object.</param>
        /// <param name="unicode_entity">Unicode entity to create the object from.</param>
        /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji FromUnicode(DiscordClient client, string unicode_entity)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "Client cannot be null.");

            return new DiscordEmoji { Name = unicode_entity, Discord = client };
        }

        /// <summary>
        /// Creates an emoji object from a guild emote.
        /// </summary>
        /// <param name="client"><see cref="DiscordClient"/> to attach to the object.</param>
        /// <param name="id">Id of the emote.</param>
        /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji FromGuildEmote(DiscordClient client, ulong id)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "Client cannot be null.");

            var ed = client.Guilds.Values.SelectMany(xg => xg.Emojis)
                .ToDictionary(xe => xe.Id, xe => xe);

            if (!ed.ContainsKey(id))
                throw new ArgumentOutOfRangeException(nameof(id), "Given emote was not found.");

            return ed[id];
        }

        /// <summary>
        /// Creates a DiscordEmoji from emote name that includes colons (eg. :thinking:). This method also supports skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), as well as guild emoji (still specified by :name:).
        /// </summary>
        /// <param name="client"><see cref="DiscordClient"/> to attach to the object.</param>
        /// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
        /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji FromName(DiscordClient client, string name)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "Client cannot be null.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Name cannot be empty or null.");

            if (UnicodeEmojis.ContainsKey(name))
                return new DiscordEmoji { Discord = client, Name = UnicodeEmojis[name] };

            var ed = client.Guilds.Values.SelectMany(xg => xg.Emojis)
                .OrderBy(xe => xe.Name)
                .GroupBy(xe => xe.Name)
                .ToDictionary(xg => xg.Key, xg => xg);
            var ek = name.Substring(1, name.Length - 2);

            if (ed.ContainsKey(ek))
                return ed[ek].First();

            throw new ArgumentException(nameof(name), "Invalid emoji name specified.");
        }
    }
}
