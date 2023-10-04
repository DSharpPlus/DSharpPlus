using System;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace DSharpPlus.Net.Serialization
{
    internal sealed class DiscordComponentJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var job = JObject.Load(reader);
            var type = job["type"]?.ToDiscordObject<ComponentType>();

            if (type == null)
                throw new ArgumentException($"Value {reader} does not have a component type specifier");

            var cmp = type switch
            {
                ComponentType.ActionRow => new DiscordActionRowComponent(),
                ComponentType.Button when (int)job["style"] is 5 => new DiscordLinkButtonComponent(),
                ComponentType.Button => new DiscordButtonComponent(),
                ComponentType.StringSelect => new DiscordSelectComponent(),
                ComponentType.FormInput => new TextInputComponent(),
                ComponentType.UserSelect => new DiscordUserSelectComponent(),
                ComponentType.RoleSelect => new DiscordRoleSelectComponent(),
                ComponentType.MentionableSelect => new DiscordMentionableSelectComponent(),
                ComponentType.ChannelSelect => new DiscordChannelSelectComponent(),
                _ => new DiscordComponent() { Type = type.Value }
            };

            // Populate the existing component with the values in the JObject. This avoids a recursive JsonConverter loop
            using var jreader = job.CreateReader();
            serializer.Populate(jreader, cmp);

            return cmp;
        }

        public override bool CanConvert(Type objectType) => typeof(DiscordComponent).IsAssignableFrom(objectType);
    }
}
