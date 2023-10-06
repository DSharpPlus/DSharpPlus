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
    public ApplicationCommandOptionType Type { get; internal set; }

    /// <summary>
    /// If this is an autocomplete option: Whether this option is currently active.
    /// </summary>
    [JsonProperty("focused")]
    public bool Focused { get; internal set; }

    [JsonProperty("value")]
    internal string InternalValue { get; set; }

    /// <summary>
    /// Gets the value of this interaction parameter.
    /// <para>This can be cast to a <see langword="long"/>, <see langword="bool"></see>, <see langword="string"></see>, <see langword="double"></see> or <see langword="ulong"/> depending on the <see cref="Type"/></para>
    /// </summary>
    [JsonIgnore]
    public object Value => this.Type switch
    {
        ApplicationCommandOptionType.Boolean => bool.Parse(this.InternalValue),
        ApplicationCommandOptionType.Integer => long.Parse(this.InternalValue),
        ApplicationCommandOptionType.String => this.InternalValue,
        ApplicationCommandOptionType.Channel => ulong.Parse(this.InternalValue),
        ApplicationCommandOptionType.User => ulong.Parse(this.InternalValue),
        ApplicationCommandOptionType.Role => ulong.Parse(this.InternalValue),
        ApplicationCommandOptionType.Mentionable => ulong.Parse(this.InternalValue),
        ApplicationCommandOptionType.Number => double.Parse(this.InternalValue, CultureInfo.InvariantCulture),
        ApplicationCommandOptionType.Attachment => ulong.Parse(this.InternalValue, CultureInfo.InvariantCulture),
        _ => this.InternalValue,
    };

    /// <summary>
    /// Gets the additional parameters if this parameter is a subcommand.
    /// </summary>
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
}
