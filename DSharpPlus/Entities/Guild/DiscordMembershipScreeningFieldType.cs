namespace DSharpPlus.Entities;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Represents a membership screening field type
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum DiscordMembershipScreeningFieldType
{
    /// <summary>
    /// Specifies the server rules
    /// </summary>
    [EnumMember(Value = "TERMS")]
    Terms
}
