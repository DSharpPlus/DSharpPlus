
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents a field in a guild's membership screening form
/// </summary>
public class DiscordGuildMembershipScreeningField
{
    /// <summary>
    /// Gets the type of the field.
    /// </summary>
    [JsonProperty("field_type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMembershipScreeningFieldType Type { get; internal set; }

    /// <summary>
    /// Gets the title of the field.
    /// </summary>
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string Label { get; internal set; }

    /// <summary>
    /// Gets the list of rules
    /// </summary>
    [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Values { get; internal set; }

    /// <summary>
    /// Gets whether the user has to fill out this field
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsRequired { get; internal set; }

    public DiscordGuildMembershipScreeningField(DiscordMembershipScreeningFieldType type, string label, IEnumerable<string> values, bool required = true)
    {
        Type = type;
        Label = label;
        Values = values.ToList();
        IsRequired = required;
    }

    internal DiscordGuildMembershipScreeningField() { }
}
