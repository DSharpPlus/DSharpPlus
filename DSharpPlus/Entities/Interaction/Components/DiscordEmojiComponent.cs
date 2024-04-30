namespace DSharpPlus.Entities;

using System;
using Newtonsoft.Json;

/// <summary>
/// Represents an emoji to add to a component.
/// </summary>
public sealed class DiscordComponentEmoji
{
    /// <summary>
    /// The Id of the emoji to use.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong Id { get; set; }

    /// <summary>
    /// The name of the emoji to use. Ignored if <see cref="Id"/> is set.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    /// <summary>
    /// Constructs a new component emoji to add to a <see cref="DiscordComponent"/>.
    /// </summary>
    public DiscordComponentEmoji() { }

    /// <summary>
    /// Constructs a new component emoji from an emoji Id.
    /// </summary>
    /// <param name="id">The Id of the emoji to use. Any valid emoji Id can be passed.</param>
    public DiscordComponentEmoji(ulong id) => Id = id;

    /// <summary>
    /// Constructs a new component emoji from unicode.
    /// </summary>
    /// <param name="name">The unicode emoji to set.</param>
    public DiscordComponentEmoji(string name)
    {
        if (!DiscordEmoji.IsValidUnicode(name))
        {
            throw new ArgumentException("Only unicode emojis can be passed.");
        }

        Name = name;
    }

    /// <summary>
    /// Constructs a new component emoji from an existing <see cref="DiscordEmoji"/>.
    /// </summary>
    /// <param name="emoji">The emoji to use.</param>
    public DiscordComponentEmoji(DiscordEmoji emoji)
    {
        Id = emoji.Id;
        Name = emoji.Name; // Name is ignored if the Id is present. //
    }
}
