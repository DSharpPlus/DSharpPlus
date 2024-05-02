using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents parameters for interaction commands.
/// </summary>
public sealed class DiscordInteractionDataOption
{
    /// <summary>
    /// Gets the name of this interaction parameter.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the type of this interaction parameter.
    /// </summary>
    [JsonProperty("type")]
    public DiscordApplicationCommandOptionType Type { get; internal set; }

    /// <summary>
    /// If this is an autocomplete option: Whether this option is currently active.
    /// </summary>
    [JsonProperty("focused")]
    public bool Focused { get; internal set; }

    [JsonProperty("value")]
    public string RawValue { get; internal set; }

    /// <summary>
    /// Gets the value of this interaction parameter.
    /// <para>This can be cast to a <see langword="long"/>, <see langword="bool"></see>, <see langword="string"></see>, <see langword="double"></see> or <see langword="ulong"/> depending on the <see cref="Type"/></para>
    /// </summary>
    [JsonIgnore]
    public object Value => Type switch
    {
        DiscordApplicationCommandOptionType.Boolean => bool.Parse(RawValue),
        DiscordApplicationCommandOptionType.Integer => long.Parse(RawValue),
        DiscordApplicationCommandOptionType.String => RawValue,
        DiscordApplicationCommandOptionType.Channel => ulong.Parse(RawValue),
        DiscordApplicationCommandOptionType.User => ulong.Parse(RawValue),
        DiscordApplicationCommandOptionType.Role => ulong.Parse(RawValue),
        DiscordApplicationCommandOptionType.Mentionable => ulong.Parse(RawValue),
        DiscordApplicationCommandOptionType.Number => double.Parse(RawValue, CultureInfo.InvariantCulture),
        DiscordApplicationCommandOptionType.Attachment => ulong.Parse(RawValue, CultureInfo.InvariantCulture),
        _ => RawValue,
    };

    /// <summary>
    /// Gets the additional parameters if this parameter is a subcommand.
    /// </summary>
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
}
