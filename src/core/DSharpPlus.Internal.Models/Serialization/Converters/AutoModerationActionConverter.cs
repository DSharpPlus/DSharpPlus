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
/// Provides conversion for <see cref="IAutoModerationAction"/>.
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
                || document.RootElement.TryGetProperty("type", out JsonElement typeElement)
                || !typeElement.TryGetInt32(out int type)
        )
        {
            throw new JsonException("The provided JSON object was malformed.");
        }

        Optional<IAutoModerationActionMetadata> data;

        // these three types have associated metadata, deserialize accordingly
        if
        (
            (DiscordAutoModerationActionType)type is DiscordAutoModerationActionType.BlockMessage
                or DiscordAutoModerationActionType.SendAlertMessage
                or DiscordAutoModerationActionType.Timeout
            && document.RootElement.TryGetProperty("metadata", out JsonElement metadata)
        )
        {
#pragma warning disable IDE0072
            data = (DiscordAutoModerationActionType)type switch
            {
                DiscordAutoModerationActionType.BlockMessage
                    => new(metadata.Deserialize<IBlockMessageActionMetadata>(options)!),
                DiscordAutoModerationActionType.SendAlertMessage
                    => new(metadata.Deserialize<ISendAlertMessageActionMetadata>(options)!),
                DiscordAutoModerationActionType.Timeout
                    => new(metadata.Deserialize<ITimeoutActionMetadata>(options)!),
                _ => Optional<IAutoModerationActionMetadata>.None
            };
#pragma warning restore IDE0072
        }
        // everyone else doesn't have metadata, good job, we made it through
        else
        {
            data = Optional<IAutoModerationActionMetadata>.None;
        }

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

        if (!value.Metadata.TryGetValue(out IAutoModerationActionMetadata? metadata))
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

            default:
                break;
        }

        writer.WriteEndObject();
    }
}
