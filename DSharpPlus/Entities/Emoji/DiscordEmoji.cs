using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Caching;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

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
    public IReadOnlyList<ulong> Roles => this._roles;

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    internal List<ulong> _roles;

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
            return this.Id == 0
                ? throw new InvalidOperationException("Cannot get URL of unicode emojis.")
                : this.IsAnimated
                ? $"https://cdn.discordapp.com/emojis/{this.Id.ToString(CultureInfo.InvariantCulture)}.gif"
                : $"https://cdn.discordapp.com/emojis/{this.Id.ToString(CultureInfo.InvariantCulture)}.png";
        }
    }

    /// <summary>
    /// Gets whether the emoji is available for use.
    /// An emoji may not be available due to loss of server boost.
    /// </summary>
    [JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsAvailable { get; internal set; }

    internal DiscordEmoji() { }

    /// <summary>
    /// Gets emoji's name in non-Unicode format (eg. :thinking: instead of the Unicode representation of the emoji).
    /// </summary>
    public string GetDiscordName()
    {
        DiscordNameLookup.TryGetValue(this.Name, out string? name);

        return name ?? $":{this.Name}:";
    }

    /// <summary>
    /// Returns a string representation of this emoji.
    /// </summary>
    /// <returns>String representation of this emoji.</returns>
    public override string ToString()
    {
        return this.Id != 0
            ? this.IsAnimated
                ? $"<a:{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}>"
                : $"<:{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}>"
            : this.Name;
    }

    /// <summary>
    /// Checks whether this <see cref="DiscordEmoji"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordEmoji"/>.</returns>
    public override bool Equals(object obj) => this.Equals(obj as DiscordEmoji);

    /// <summary>
    /// Checks whether this <see cref="DiscordEmoji"/> is equal to another <see cref="DiscordEmoji"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordEmoji"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordEmoji"/> is equal to this <see cref="DiscordEmoji"/>.</returns>
    public bool Equals(DiscordEmoji e) => e is null ? false : ReferenceEquals(this, e) || (this.Id == e.Id && (this.Id != 0 || this.Name == e.Name));

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordEmoji"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordEmoji"/>.</returns>
    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + this.Id.GetHashCode();
        hash = (hash * 7) + this.Name.GetHashCode();

        return hash;
    }

    internal string ToReactionString()
        => this.Id != 0 ? $"{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}" : this.Name;

    /// <summary>
    /// Gets whether the two <see cref="DiscordEmoji"/> objects are equal.
    /// </summary>
    /// <param name="e1">First emoji to compare.</param>
    /// <param name="e2">Second emoji to compare.</param>
    /// <returns>Whether the two emoji are equal.</returns>
    public static bool operator ==(DiscordEmoji e1, DiscordEmoji e2)
    {
        object? o1 = e1 as object;
        object? o2 = e2 as object;

        return (o1 != null ^ o2 == null)
            && ((o1 == null && o2 == null) || (e1.Id == e2.Id && (e1.Id != 0 || e1.Name == e2.Name)));
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
    /// Checks whether specified unicode entity is a valid unicode emoji.
    /// </summary>
    /// <param name="unicodeEntity">Entity to check.</param>
    /// <returns>Whether it's a valid emoji.</returns>
    public static bool IsValidUnicode(string unicodeEntity)
        => DiscordNameLookup.ContainsKey(unicodeEntity);

    /// <summary>
    /// Creates an emoji object from a unicode entity.
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="unicodeEntity">Unicode entity to create the object from.</param>
    /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
    public static DiscordEmoji FromUnicode(BaseDiscordClient client, string unicodeEntity)
    {
        return !IsValidUnicode(unicodeEntity)
            ? throw new ArgumentException("Specified unicode entity is not a valid unicode emoji.", nameof(unicodeEntity))
            : new DiscordEmoji { Name = unicodeEntity, Discord = client };
    }

    /// <summary>
    /// Creates an emoji object from a unicode entity.
    /// </summary>
    /// <param name="unicodeEntity">Unicode entity to create the object from.</param>
    /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
    public static DiscordEmoji FromUnicode(string unicodeEntity)
        => FromUnicode(null, unicodeEntity);

    /// <summary>
    /// Attempts to create an emoji object from a unicode entity.
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="unicodeEntity">Unicode entity to create the object from.</param>
    /// <param name="emoji">Resulting <see cref="DiscordEmoji"/> object.</param>
    /// <returns>Whether the operation was successful.</returns>
    public static bool TryFromUnicode(BaseDiscordClient client, string unicodeEntity, out DiscordEmoji emoji)
    {
        // this is a round-trip operation because of FE0F inconsistencies.
        // through this, the inconsistency is normalized.

        emoji = null;
        if (!DiscordNameLookup.TryGetValue(unicodeEntity, out string? discordName))
        {
            return false;
        }

        if (!UnicodeEmojis.TryGetValue(discordName, out unicodeEntity))
        {
            return false;
        }

        emoji = new DiscordEmoji { Name = unicodeEntity, Discord = client };
        return true;
    }

    /// <summary>
    /// Attempts to create an emoji object from a unicode entity.
    /// </summary>
    /// <param name="unicodeEntity">Unicode entity to create the object from.</param>
    /// <param name="emoji">Resulting <see cref="DiscordEmoji"/> object.</param>
    /// <returns>Whether the operation was successful.</returns>
    public static bool TryFromUnicode(string unicodeEntity, out DiscordEmoji emoji)
        => TryFromUnicode(null, unicodeEntity, out emoji);

    /// <summary>
    /// Tries to get an emoji from a guild by its ID.
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="id">Id of the emote.</param>
    /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
    public static async ValueTask<DiscordEmoji?> TryFromGuildEmoteAsync(BaseDiscordClient client, ulong id)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        foreach (ulong guildId in client._guildIds)
        {
            DiscordGuild? guild = await client.Cache.TryGetGuildAsync(guildId);
            if (guild is not null && guild.Emojis.TryGetValue(id, out DiscordEmoji? emoji))
            {
                return emoji;
            }
        }

        return null;
    }  //TODO: This is a behavior change, anounce in the PR

    /// <summary>
    /// Creates an emoji object from emote name that includes colons (eg. :thinking:). This method also supports
    /// skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), as well as guild emoji
    /// (still specified by :name:).
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
    /// <param name="includeGuilds">Should guild emojis be included in the search.</param>
    /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
    public static async ValueTask<DiscordEmoji> FromNameAsync(BaseDiscordClient client, string name, bool includeGuilds = true)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        
        if (name.Length < 2 || name[0] != ':' || name[name.Length - 1] != ':')
        {
            throw new ArgumentException("Invalid emoji name specified. Ensure the emoji name starts and ends with ':'", nameof(name));
        }

        if (UnicodeEmojis.TryGetValue(name, out string? unicodeEntity))
        {
            return new DiscordEmoji { Discord = client, Name = unicodeEntity };
        }
        else if (includeGuilds)
        {
            name = name.Substring(1, name.Length - 2); // remove colons
            foreach (ulong guildId in client._guildIds)
            {
                DiscordGuild? guild = await client.Cache.TryGetGuildAsync(guildId);
                DiscordEmoji? emoji = guild?.Emojis.Values.FirstOrDefault(emoji => emoji.Name == name);
                if (emoji is not null)
                {
                    return emoji;
                }
            }
        }

        throw new ArgumentException("Invalid emoji name specified.", nameof(name));
    }

    /// <summary>
    /// Attempts to create an emoji object from emote name that includes colons (eg. :thinking:). This method also
    /// supports skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), as well as guild
    /// emoji (still specified by :name:).
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
    /// <param name="includeGuilds">Should guild emojis be included in the search.</param>
    /// <param name="emoji">Resulting <see cref="DiscordEmoji"/> object.</param>
    /// <returns>Whether the operation was successful.</returns>
    public static async ValueTask<DiscordEmoji?> TryFromNameAsync(BaseDiscordClient client, string name, bool includeGuilds = true)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        
        // Checks if the emoji name is invalid
        if (name.Length < 2 || name[0] != ':' || name[name.Length - 1] != ':')
        {
            return null; // invalid name
        }

        if (UnicodeEmojis.TryGetValue(name, out string? unicodeEntity))
        {
            return new DiscordEmoji { Discord = client, Name = unicodeEntity };
        }
        else if (includeGuilds)
        {
            name = name.Substring(1, name.Length - 2); // remove colons
            foreach (ulong guildId in client._guildIds)
            {
                DiscordGuild? guild = await client.Cache.TryGetGuildAsync(guildId);
                DiscordEmoji? emoji = guild?.Emojis.Values.FirstOrDefault(emoji => emoji.Name == name);
                if (emoji is not null)
                {
                    return emoji;
                }
            }
        }
        
        return null;
    }
}
