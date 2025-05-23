// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models.Serialization.Converters;

public class ComponentConverter : JsonConverter<IComponent>
{
    /// <inheritdoc/>
    public override IComponent? Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("There was no JSON object found.");
        }

        if
        (
            !JsonDocument.TryParseValue(ref reader, out JsonDocument? document)
            || !document.RootElement.TryGetProperty("type", out JsonElement property)
            || !property.TryGetInt32(out int type)
        )
        {
            throw new JsonException("The provided JSON object was malformed.");
        }

        IComponent? component = (DiscordMessageComponentType)type switch
        {
            DiscordMessageComponentType.ActionRow => document.Deserialize<IActionRowComponent>(options),
            DiscordMessageComponentType.Button => document.Deserialize<IButtonComponent>(options),
            DiscordMessageComponentType.StringSelect => document.Deserialize<IStringSelectComponent>(options),
            DiscordMessageComponentType.TextInput => document.Deserialize<ITextInputComponent>(options),
            DiscordMessageComponentType.UserSelect => document.Deserialize<IUserSelectComponent>(options),
            DiscordMessageComponentType.RoleSelect => document.Deserialize<IRoleSelectComponent>(options),
            DiscordMessageComponentType.MentionableSelect => document.Deserialize<IMentionableSelectComponent>(options),
            DiscordMessageComponentType.ChannelSelect => document.Deserialize<IChannelSelectComponent>(options),
            DiscordMessageComponentType.Section => document.Deserialize<ISectionComponent>(options),
            DiscordMessageComponentType.TextDisplay => document.Deserialize<ITextDisplayComponent>(options),
            DiscordMessageComponentType.Thumbnail => document.Deserialize<IThumbnailComponent>(options),
            DiscordMessageComponentType.MediaGallery => document.Deserialize<IMediaGalleryComponent>(options),
            DiscordMessageComponentType.File => document.Deserialize<IFileComponent>(options),
            DiscordMessageComponentType.Separator => document.Deserialize<ISeparatorComponent>(options),
            DiscordMessageComponentType.Container => document.Deserialize<IContainerComponent>(options),

            _ => new UnknownComponent
            {
                Type = (DiscordMessageComponentType)type,
                RawPayload = JsonSerializer.Serialize(document)
            }
        };

        document.Dispose();
        return component;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IComponent value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, (object)value, options);
}
