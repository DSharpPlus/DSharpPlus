using System;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization;

internal sealed class DiscordComponentJsonConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        JObject job = JObject.Load(reader);
        DiscordComponentType? type = job["type"]?.ToDiscordObject<DiscordComponentType>() ?? throw new ArgumentException($"Value {reader} does not have a component type specifier");
        DiscordComponent cmp = type switch
        {
            DiscordComponentType.ActionRow => new DiscordActionRowComponent(),
            DiscordComponentType.Button when (int)job["style"] is 5 => new DiscordLinkButtonComponent(),
            DiscordComponentType.Button => new DiscordButtonComponent(),
            DiscordComponentType.StringSelect => new DiscordSelectComponent(),
            DiscordComponentType.FormInput => new DiscordTextInputComponent(),
            DiscordComponentType.UserSelect => new DiscordUserSelectComponent(),
            DiscordComponentType.RoleSelect => new DiscordRoleSelectComponent(),
            DiscordComponentType.MentionableSelect => new DiscordMentionableSelectComponent(),
            DiscordComponentType.ChannelSelect => new DiscordChannelSelectComponent(),
            DiscordComponentType.Section => new DiscordSectionComponent(),
            DiscordComponentType.TextDisplay => new DiscordTextDisplayComponent(),
            DiscordComponentType.Thumbnail => new DiscordThumbnailComponent(),
            DiscordComponentType.MediaGallery => new DiscordMediaGalleryComponent(),
            DiscordComponentType.Separator => new DiscordSeparatorComponent(),
            DiscordComponentType.File => new DiscordFileComponent(),
            DiscordComponentType.Container => new DiscordContainerComponent(),
            _ => new DiscordComponent() { Type = type.Value }
        };

        // Populate the existing component with the values in the JObject. This avoids a recursive JsonConverter loop
        using JsonReader jreader = job.CreateReader();
        serializer.Populate(jreader, cmp);

        return cmp;
    }

    public override bool CanConvert(Type objectType) => typeof(DiscordComponent).IsAssignableFrom(objectType);
}
