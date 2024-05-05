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

    /// <summary>
    /// Gets the raw value of this interaction parameter.
    /// </summary>
    [JsonProperty("value")]
    public string? RawValue { get; internal set; }

#pragma warning disable IDE0046
    /// <summary>
    /// Gets the value of this interaction parameter.
    /// <para>This can be cast to a <see langword="long"/>, <see langword="bool"></see>, <see langword="string"></see>, <see langword="double"></see> or <see langword="ulong"/> depending on the <see cref="Type"/></para>
    /// </summary>
    [JsonIgnore]
    public object? Value
    {
        get
        {
            if (this.RawValue is null)
            {
                return this.RawValue;
            }

            return this.Type switch
            {
                DiscordApplicationCommandOptionType.Boolean => bool.Parse(this.RawValue),
                DiscordApplicationCommandOptionType.Integer => long.Parse(this.RawValue),
                DiscordApplicationCommandOptionType.String => this.RawValue,
                DiscordApplicationCommandOptionType.Channel => ulong.Parse(this.RawValue),
                DiscordApplicationCommandOptionType.User => ulong.Parse(this.RawValue),
                DiscordApplicationCommandOptionType.Role => ulong.Parse(this.RawValue),
                DiscordApplicationCommandOptionType.Mentionable => ulong.Parse(this.RawValue),
                DiscordApplicationCommandOptionType.Number => double.Parse(this.RawValue, CultureInfo.InvariantCulture),
                DiscordApplicationCommandOptionType.Attachment => ulong.Parse(this.RawValue, CultureInfo.InvariantCulture),
                _ => this.RawValue,
            };
        }
    }
#pragma warning restore IDE0046

    /// <summary>
    /// Gets the additional parameters if this parameter is a subcommand.
    /// </summary>
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
}
