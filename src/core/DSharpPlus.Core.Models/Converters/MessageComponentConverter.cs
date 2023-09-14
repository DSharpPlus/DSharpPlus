// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Models.Converters;

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
                => JsonSerializer.Deserialize<IButtonComponent>(document, options),
            
            DiscordMessageComponentType.StringSelect
                => JsonSerializer.Deserialize<IStringSelectComponent>(document, options),
            
            DiscordMessageComponentType.TextInput
                => JsonSerializer.Deserialize<ITextInputComponent>(document, options),
            
            DiscordMessageComponentType.UserSelect
                => JsonSerializer.Deserialize<IUserSelectComponent>(document, options),
            
            DiscordMessageComponentType.RoleSelect
                => JsonSerializer.Deserialize<IRoleSelectComponent>(document, options),
            
            DiscordMessageComponentType.MentionableSelect
                => JsonSerializer.Deserialize<IMentionableSelectComponent>(document, options),
            
            DiscordMessageComponentType.ChannelSelect
                => JsonSerializer.Deserialize<IChannelSelectComponent>(document, options),
            
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
