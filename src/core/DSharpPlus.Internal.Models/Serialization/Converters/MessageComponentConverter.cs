// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models.Serialization.Converters;

/// <summary>
/// Converts between message components and JSON.
/// </summary>
public class MessageComponentConverter : JsonConverter<IInteractiveComponent>
{
    /// <inheritdoc/>
    public override IInteractiveComponent? Read
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
            || document.RootElement.TryGetProperty("type", out JsonElement property)
            || !property.TryGetInt32(out int type)
        )
        {
            throw new JsonException("The provided JSON object was malformed.");
        }

        IInteractiveComponent? component = (DiscordMessageComponentType)type switch
        {
            DiscordMessageComponentType.ActionRow
                => throw new JsonException("Invalid JSON structure: expected an interactive component, not an action row."),

            DiscordMessageComponentType.Button
                => document.Deserialize<IButtonComponent>(options),

            DiscordMessageComponentType.StringSelect
                => document.Deserialize<IStringSelectComponent>(options),

            DiscordMessageComponentType.TextInput
                => document.Deserialize<ITextInputComponent>(options),

            DiscordMessageComponentType.UserSelect
                => document.Deserialize<IUserSelectComponent>(options),

            DiscordMessageComponentType.RoleSelect
                => document.Deserialize<IRoleSelectComponent>(options),

            DiscordMessageComponentType.MentionableSelect
                => document.Deserialize<IMentionableSelectComponent>(options),

            DiscordMessageComponentType.ChannelSelect
                => document.Deserialize<IChannelSelectComponent>(options),

            _ => throw new JsonException("Unknown component type.")
        };

        document.Dispose();
        return component;
    }

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer,
        IInteractiveComponent value,
        JsonSerializerOptions options
    )
    {
        switch (value)
        {
            case IButtonComponent button:
                JsonSerializer.Serialize(writer, button, options);
                break;

            case IStringSelectComponent stringSelect:
                JsonSerializer.Serialize(writer, stringSelect, options);
                break;

            case ITextInputComponent text:
                JsonSerializer.Serialize(writer, text, options);
                break;

            case IUserSelectComponent user:
                JsonSerializer.Serialize(writer, user, options);
                break;

            case IRoleSelectComponent role:
                JsonSerializer.Serialize(writer, role, options);
                break;

            case IMentionableSelectComponent mentionable:
                JsonSerializer.Serialize(writer, mentionable, options);
                break;

            case IChannelSelectComponent channel:
                JsonSerializer.Serialize(writer, channel, options);
                break;

            default:
                throw new InvalidOperationException($"The component {value} could not be serialized.");
        }
    }
}
