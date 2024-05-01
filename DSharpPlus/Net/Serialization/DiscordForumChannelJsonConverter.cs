namespace DSharpPlus.Net.Serialization;

using System;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DiscordForumChannelJsonConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new();

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject job = JObject.Load(reader);
        bool hasType = job.TryGetValue("type", out JToken typeToken);

        if (!hasType)
        {
            throw new JsonException("Channel object lacks type - this should be reported to library developers");
        }

        DiscordChannel channel;
        DiscordChannelType channelType = typeToken.ToObject<DiscordChannelType>();

        if (channelType is DiscordChannelType.GuildForum)
        {
            // Type erasure is almost unheard of in C#, but you never know...
            DiscordForumChannel chn = new();
            serializer.Populate(job.CreateReader(), chn);

            channel = chn;
        }
        // May or not be necessary. Better safe than sorry.
        else if (channelType is DiscordChannelType.NewsThread or DiscordChannelType.PrivateThread or DiscordChannelType.PublicThread)
        {
            DiscordThreadChannel chn = new();
            serializer.Populate(job.CreateReader(), chn);

            channel = chn;
        }
        else if (channelType is DiscordChannelType.Private)
        {
            channel = new DiscordDmChannel();
            serializer.Populate(job.CreateReader(), channel);
        }
        else
        {
            channel = new DiscordChannel();
            serializer.Populate(job.CreateReader(), channel);
        }

        return channel;
    }

    public override bool CanConvert(Type objectType) => objectType.IsAssignableFrom(typeof(DiscordChannel));
}
