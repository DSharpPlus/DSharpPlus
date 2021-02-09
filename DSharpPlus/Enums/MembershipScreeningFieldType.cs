using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace DSharpPlus
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MembershipScreeningFieldType
    {
        [EnumMember(Value = "TERMS")]
        Terms
    }
}
