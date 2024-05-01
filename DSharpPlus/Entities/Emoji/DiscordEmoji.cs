namespace DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

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
    public IReadOnlyList<ulong> Roles => _rolesLazy.Value;

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    internal List<ulong> _roles;
    private readonly Lazy<IReadOnlyList<ulong>> _rolesLazy;

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
    public string Url => Id == 0
                ? throw new InvalidOperationException("Cannot get URL of unicode emojis.")
                : IsAnimated
                ? $"https://cdn.discordapp.com/emojis/{Id.ToString(CultureInfo.InvariantCulture)}.gif"
                : $"https://cdn.discordapp.com/emojis/{Id.ToString(CultureInfo.InvariantCulture)}.png";

    /// <summary>
    /// Gets whether the emoji is available for use.
    /// An emoji may not be available due to loss of server boost.
    /// </summary>
    [JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsAvailable { get; internal set; }

    internal DiscordEmoji() => _rolesLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(_roles));

    /// <summary>
    /// Gets emoji's name in non-Unicode format (eg. :thinking: instead of the Unicode representation of the emoji).
    /// </summary>
    public string GetDiscordName()
    {
        DiscordNameLookup.TryGetValue(Name, out string? name);

        return name ?? $":{Name}:";
    }

    /// <summary>
    /// Returns a string representation of this emoji.
    /// </summary>
    /// <returns>String representation of this emoji.</returns>
    public override string ToString() => Id != 0
            ? IsAnimated
                ? $"<a:{Name}:{Id.ToString(CultureInfo.InvariantCulture)}>"
                : $"<:{Name}:{Id.ToString(CultureInfo.InvariantCulture)}>"
            : Name;

    /// <summary>
    /// Checks whether this <see cref="DiscordEmoji"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordEmoji"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordEmoji);

    /// <summary>
    /// Checks whether this <see cref="DiscordEmoji"/> is equal to another <see cref="DiscordEmoji"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordEmoji"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordEmoji"/> is equal to this <see cref="DiscordEmoji"/>.</returns>
    public bool Equals(DiscordEmoji e) => e is not null && (ReferenceEquals(this, e) || (Id == e.Id && (Id != 0 || Name == e.Name)));

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordEmoji"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordEmoji"/>.</returns>
    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + Id.GetHashCode();
        hash = (hash * 7) + Name.GetHashCode();

        return hash;
    }

    internal string ToReactionString()
        => Id != 0 ? $"{Name}:{Id.ToString(CultureInfo.InvariantCulture)}" : Name;

    /// <summary>
    /// Gets whether the two <see cref="DiscordEmoji"/> objects are equal.
    /// </summary>
    /// <param name="e1">First emoji to compare.</param>
    /// <param name="e2">Second emoji to compare.</param>
    /// <returns>Whether the two emoji are equal.</returns>
    public static bool operator ==(DiscordEmoji e1, DiscordEmoji e2)
    {
        object? o1 = e1;
        object? o2 = e2;

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
    public static DiscordEmoji FromUnicode(BaseDiscordClient client, string unicodeEntity) => !IsValidUnicode(unicodeEntity)
            ? throw new ArgumentException("Specified unicode entity is not a valid unicode emoji.", nameof(unicodeEntity))
            : new DiscordEmoji { Name = unicodeEntity, Discord = client };

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
    /// Creates an emoji object from a guild emote.
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="id">Id of the emote.</param>
    /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
    public static DiscordEmoji FromGuildEmote(BaseDiscordClient client, ulong id)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }

        foreach (DiscordGuild guild in client.Guilds.Values)
        {
            if (guild.Emojis.TryGetValue(id, out DiscordEmoji? found))
            {
                return found;
            }
        }

        throw new KeyNotFoundException("Given emote was not found.");
    }

    /// <summary>
    /// Attempts to create an emoji object from a guild emote.
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="id">Id of the emote.</param>
    /// <param name="emoji">Resulting <see cref="DiscordEmoji"/> object.</param>
    /// <returns>Whether the operation was successful.</returns>
    public static bool TryFromGuildEmote(BaseDiscordClient client, ulong id, out DiscordEmoji emoji)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }

        foreach (DiscordGuild guild in client.Guilds.Values)
        {
            if (guild.Emojis.TryGetValue(id, out emoji))
            {
                return true;
            }
        }

        emoji = null;
        return false;
    }

    /// <summary>
    /// Creates an emoji object from emote name that includes colons (eg. :thinking:). This method also supports
    /// skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), as well as guild emoji
    /// (still specified by :name:).
    /// </summary>
    /// <param name="client"><see cref="BaseDiscordClient"/> to attach to the object.</param>
    /// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
    /// <param name="includeGuilds">Should guild emojis be included in the search.</param>
    /// <returns>Create <see cref="DiscordEmoji"/> object.</returns>
    public static DiscordEmoji FromName(BaseDiscordClient client, string name, bool includeGuilds = true)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }
        else if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Name cannot be empty or null.");
        }
        else if (name.Length < 2 || name[0] != ':' || name[name.Length - 1] != ':')
        {
            throw new ArgumentException("Invalid emoji name specified. Ensure the emoji name starts and ends with ':'", nameof(name));
        }

        if (UnicodeEmojis.TryGetValue(name, out string? unicodeEntity))
        {
            return new DiscordEmoji { Discord = client, Name = unicodeEntity };
        }
        else if (includeGuilds)
        {
            name = name[1..^1]; // remove colons
            foreach (DiscordGuild guild in client.Guilds.Values)
            {
                DiscordEmoji? found = guild.Emojis.Values.FirstOrDefault(emoji => emoji.Name == name);
                if (found != null)
                {
                    return found;
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
    /// <param name="emoji">Resulting <see cref="DiscordEmoji"/> object.</param>
    /// <returns>Whether the operation was successful.</returns>
    public static bool TryFromName(BaseDiscordClient client, string name, out DiscordEmoji emoji)
        => TryFromName(client, name, true, out emoji);

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
    public static bool TryFromName(BaseDiscordClient client, string name, bool includeGuilds, out DiscordEmoji emoji)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }
        // Checks if the emoji name is null
        else if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name[0] != ':' || name[name.Length - 1] != ':')
        {
            emoji = null;
            return false; // invalid name
        }

        if (UnicodeEmojis.TryGetValue(name, out string? unicodeEntity))
        {
            emoji = new DiscordEmoji { Discord = client, Name = unicodeEntity };
            return true;
        }
        else if (includeGuilds)
        {
            name = name[1..^1]; // remove colons
            foreach (DiscordGuild guild in client.Guilds.Values)
            {
                emoji = guild.Emojis.Values.FirstOrDefault(emoji => emoji.Name == name);
                if (emoji != null)
                {
                    return true;
                }
            }
        }

        emoji = null;
        return false;
    }
}
