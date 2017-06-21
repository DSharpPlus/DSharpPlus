using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmoji : SnowflakeObject
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
    }
}
