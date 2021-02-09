using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a membership screening field type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MembershipScreeningFieldType
    {
        /// <summary>
        /// Specifies the server rules
        /// </summary>
        [EnumMember(Value = "TERMS")]
        Terms
    }
}
