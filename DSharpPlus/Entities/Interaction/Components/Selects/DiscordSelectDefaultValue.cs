using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordSelectDefaultValue
{
    [JsonProperty("id")]
    public ulong Id { get; internal set; }
    
    [JsonProperty("type")]
    public string Type { get; internal set; }
    
    public DiscordSelectDefaultValue(ulong id, DiscordSelectDefaultValueType type)
    {
        Id = id;
        Type = type switch
        {
            DiscordSelectDefaultValueType.Channel => "channel",
            DiscordSelectDefaultValueType.User => "user",
            DiscordSelectDefaultValueType.Role => "role",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
