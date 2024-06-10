using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Abstractions;

/// <summary>
/// Represents data for identify payload's shard info.
/// </summary>
[JsonConverter(typeof(ShardInfoConverter))]
public sealed class ShardInfo
{
    /// <summary>
    /// Gets or sets this client's shard id.
    /// </summary>
    public int ShardId { get; set; }

    /// <summary>
    /// Gets or sets the total shard count for this token.
    /// </summary>
    public int ShardCount { get; set; }
}

internal sealed class ShardInfoConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        ShardInfo info = (ShardInfo)value;
        int[] obj = [info.ShardId, info.ShardCount];

        serializer.Serialize(writer, obj);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JArray arr = ReadArrayObject(reader, serializer);

        return new ShardInfo
        {
            ShardId = (int)arr[0],
            ShardCount = (int)arr[1],
        };
    }

    private static JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer)
    {
        return serializer.Deserialize<JToken>(reader) is not JArray arr || arr.Count != 2
            ? throw new JsonSerializationException("Expected array of length 2")
            : arr;
    }

    public override bool CanConvert(Type objectType) => objectType == typeof(ShardInfo);
}
