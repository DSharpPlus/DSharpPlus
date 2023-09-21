using System;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization;

public class DiscordForumChannelJsonConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new ();

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject job = JObject.Load(reader);
        JToken typeToken;
        bool hasType = job.TryGetValue("type", out typeToken);

        if (!hasType)
        {
            throw new JsonException("Channel object lacks type - this should be reported to library developers");
        }
        
        DiscordChannel channel;
        ChannelType channelType = typeToken.ToObject<ChannelType>();

        if (channelType is ChannelType.GuildForum)
        {
            // Type erasure is almost unheard of in C#, but you never know...
            DiscordForumChannel chn = new DiscordForumChannel();
            serializer.Populate(job.CreateReader(), chn);

            channel = chn;
        }
        // May or not be necessary. Better safe than sorry.
        else if (channelType is ChannelType.NewsThread or ChannelType.PrivateThread or ChannelType.PublicThread)
        {
            DiscordThreadChannel chn = new DiscordThreadChannel();
            serializer.Populate(job.CreateReader(), chn);

            channel = chn;
        }
        else if (channelType is ChannelType.Private)
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
