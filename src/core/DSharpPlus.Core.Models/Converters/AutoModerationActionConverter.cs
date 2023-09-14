// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Models.Converters;

/// <summary>
/// Provides conversion for <seealso cref="IAutoModerationAction"/>.
/// </summary>
public class AutoModerationActionConverter : JsonConverter<IAutoModerationAction>
{
    /// <inheritdoc/>
    public override IAutoModerationAction? Read
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
            || document.RootElement.TryGetProperty("metadata", out JsonElement metadata)
        )
        {
            throw new JsonException("The provided JSON object was malformed.");
        }

        Optional<IAutoModerationActionMetadata> data = (DiscordAutoModerationActionType)type switch
        {
            DiscordAutoModerationActionType.BlockMessage
                => new(JsonSerializer.Deserialize<IBlockMessageActionMetadata>(metadata, options)!),
            DiscordAutoModerationActionType.SendAlertMessage
                => new(JsonSerializer.Deserialize<ISendAlertMessageActionMetadata>(metadata, options)!),
            DiscordAutoModerationActionType.Timeout
                => new(JsonSerializer.Deserialize<ITimeoutActionMetadata>(metadata, options)!),
            _ => new()
        };

        document.Dispose();

        return new AutoModerationAction
        {
            Type = (DiscordAutoModerationActionType)type,
            Metadata = data
        };
    }

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer, 
        IAutoModerationAction value, 
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WritePropertyName("type");
        writer.WriteNumberValue((int)value.Type);

        if (!value.Metadata.TryGet(out IAutoModerationActionMetadata? metadata))
        {
            writer.WriteEndObject();
            return;
        }

        writer.WritePropertyName("metadata");

        switch (metadata)
        {
            case IBlockMessageActionMetadata block:
                JsonSerializer.Serialize(writer, block, options);
                break;

            case ISendAlertMessageActionMetadata alert:
                JsonSerializer.Serialize(writer, alert, options);
                break;

            case ITimeoutActionMetadata timeout:
                JsonSerializer.Serialize(writer, timeout, options);
                break;
        }

        writer.WriteEndObject();
    }
}
