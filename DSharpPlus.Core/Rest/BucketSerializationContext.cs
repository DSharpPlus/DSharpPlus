using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Rest
{
    // use JSON source generation for ratelimit buckets. those need to be as fast as possible.
    // this is named Context to make calls look cleaner and more readable.

    [JsonSerializable(typeof(RatelimitBucket), TypeInfoPropertyName = "Context")]
    internal partial class BucketSerializationContext : JsonSerializerContext
    {
    }
}
