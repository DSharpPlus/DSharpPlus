using System;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization;

public class DiscordForumChannelJsonConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new();

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);
        bool hasType = jObject.TryGetValue("type", out JToken? typeToken);

        if (!hasType)
        {
            throw new JsonException("Channel object lacks type - this should be reported to library developers");
        }

        DiscordChannel channel;
        DiscordChannelType channelType = typeToken!.ToObject<DiscordChannelType>();

        if (channelType is DiscordChannelType.GuildForum)
        {
            // Type erasure is almost unheard of in C#, but you never know...
            DiscordForumChannel forumChannel = new();
            serializer.Populate(jObject.CreateReader(), forumChannel);

            channel = forumChannel;
        }
        // May or not be necessary. Better safe than sorry.
        else if (channelType is DiscordChannelType.NewsThread or DiscordChannelType.PrivateThread or DiscordChannelType.PublicThread)
        {
            DiscordThreadChannel threadChannel = new();
            serializer.Populate(jObject.CreateReader(), threadChannel);

            channel = threadChannel;
        }
        else if (channelType is DiscordChannelType.Private or DiscordChannelType.Group)
        {
            channel = new DiscordDmChannel();
            serializer.Populate(jObject.CreateReader(), channel);
        }
        else
        {
            channel = new DiscordChannel();
            serializer.Populate(jObject.CreateReader(), channel);
        }

        return channel;
    }

    public override bool CanConvert(Type objectType) => objectType.IsAssignableFrom(typeof(DiscordChannel));
}
