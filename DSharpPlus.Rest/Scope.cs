using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Rest
{
#if !NETSTANDARD1_1
    [Serializable]
#endif
    //[JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum Scope
    {
        [EnumMember(Value = "bot")]
        bot,
        [EnumMember(Value = "connections")]
        connections,
        [EnumMember(Value = "email")]
        email,
        [EnumMember(Value = "identify")]
        identify,
        [EnumMember(Value = "guilds")]
        guilds,
        [EnumMember(Value = "guilds.join")]
        guilds_join,
        [EnumMember(Value = "gdm.join")]
        gdm_join,
        [EnumMember(Value = "messages.read")]
        messages_read,
        [EnumMember(Value = "rpc")]
        rpc,
        [EnumMember(Value = "rpc.api")]
        rpc_api,
        [EnumMember(Value = "rpc.notifications.read")]
        rpc_notifications_read,
        [EnumMember(Value = "webhook.incoming")]
        webhook_incoming,
    }
}