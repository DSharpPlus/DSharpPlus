// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System;
using System.Collections.Generic;

using DSharpPlus.Extensions.Internal.Toolbox.Errors;
using DSharpPlus.Extensions.Internal.Toolbox.Implementations;
using DSharpPlus.Internal.Abstractions.Models;

using DSharpPlus.Results;

namespace DSharpPlus.Extensions.Internal.Toolbox.Builders.Messages;

/// <summary>
/// Represents an <see cref="IEmbed"/> currently under construction.
/// </summary>
public record struct EmbedBuilder
{
    /// <inheritdoc cref="IEmbed.Title"/>
    public Optional<string> Title { get; set; }

    /// <inheritdoc cref="IEmbed.Description"/>
    public Optional<string> Description { get; set; }

    /// <inheritdoc cref="IEmbed.Color"/> 
    public Optional<int> Color { get; set; }

    /// <inheritdoc cref="IEmbed.Url"/>
    public Optional<string> Url { get; set; }

    /// <inheritdoc cref="IEmbed.Timestamp"/>
    public Optional<DateTimeOffset> Timestamp { get; set; }

    /// <inheritdoc cref="IEmbed.Footer"/>
    public Optional<IEmbedFooter> Footer { get; set; }

    /// <inheritdoc cref="IEmbed.Author"/>
    public Optional<IEmbedAuthor> Author { get; set; }

    /// <inheritdoc cref="IEmbed.Fields"/>
    public Optional<IReadOnlyList<IEmbedField>> Fields { get; set; }
}

public static class EmbedBuilderExtensions
{
    /// <summary>
    /// Sets the title of the embed to the specified string.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithTitle(ref this EmbedBuilder builder, string title)
    {
        builder.Title = title;
        return ref builder;
    }

    /// <summary>
    /// Sets the description of the embed to the specified string.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithDescription(ref this EmbedBuilder builder, string description)
    {
        builder.Description = description;
        return ref builder;
    }

    /// <summary>
    /// Sets the sidebar color of the embed to the specified color code.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithColor(ref this EmbedBuilder builder, int color)
    {
        builder.Color = color;
        return ref builder;
    }

    /// <summary>
    /// Sets the url of the embed to the specified link.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithUrl(ref this EmbedBuilder builder, string url)
    {
        builder.Url = url;
        return ref builder;
    }

    /// <summary>
    /// Sets the url of the embed to the specified link.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithUrl(ref this EmbedBuilder builder, Uri url)
    {
        builder.Url = url.AbsoluteUri;
        return ref builder;
    }

    /// <summary>
    /// Sets the timestamp of the embed to the specified value.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithTimestamp(ref this EmbedBuilder builder, DateTimeOffset timestamp)
    {
        builder.Timestamp = timestamp;
        return ref builder;
    }

    /// <summary>
    /// Sets the footer of the embed to the specified value.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithFooter(ref this EmbedBuilder builder, IEmbedFooter footer)
    {
        builder.Footer = new(footer);
        return ref builder;
    }

    /// <summary>
    /// Sets the author of the embed to the specified value.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder WithAuthor(ref this EmbedBuilder builder, IEmbedAuthor author)
    {
        builder.Author = new(author);
        return ref builder;
    }

    /// <summary>
    /// Adds a field to the embed builder.
    /// </summary>
    /// <returns>The embed builder for chaining.</returns>
    public static ref EmbedBuilder AddField(ref this EmbedBuilder builder, IEmbedField field)
    {
        builder.Fields = builder.Fields.MapOr<Optional<IReadOnlyList<IEmbedField>>>
        (
            transformation: fields => new([.. fields, field]),
            value: new([field])
        );

        return ref builder;
    }

    /// <summary>
    /// Verifies whether the embed builder can be transformed into a valid embed.
    /// </summary>
    public static Result Validate(ref this EmbedBuilder builder)
    {
        int totalCount = 0;
        List<(string, string)> errors = [];

        if (builder.Title.TryGetNonNullValue(out string? title))
        {
            if (title.Length > 256)
            {
                errors.Add((nameof(EmbedBuilder.Title), "The length of the title cannot exceed 256 characters."));
            }

            totalCount += title.Length;
        }

        if (builder.Description.TryGetNonNullValue(out string? desc))
        {
            if (desc.Length > 256)
            {
                errors.Add((nameof(EmbedBuilder.Description), "The length of the description cannot exceed 4096 characters."));
            }

            totalCount += desc.Length;
        }

        if (builder.Color.TryGetNonNullValue(out int color))
        {
            if (color is < 0x000000 or > 0xFFFFFF)
            {
                errors.Add((nameof(EmbedBuilder.Color), "The color code must be between 0x000000 and 0xFFFFFF."));
            }
        }

        if (builder.Footer.TryGetNonNullValue(out IEmbedFooter? footer))
        {
            if (footer.Text.Length > 2048)
            {
                errors.Add((nameof(EmbedBuilder.Footer), "The length of the footer text cannot exceed 2048 characters."));
            }

            totalCount += footer.Text.Length;
        }

        if (builder.Author.TryGetNonNullValue(out IEmbedAuthor? author))
        {
            if (author.Name.Length > 256)
            {
                errors.Add((nameof(EmbedBuilder.Author), "The length of the author name cannot exceed 256 characters."));
            }

            totalCount += author.Name.Length;
        }

        if (builder.Fields.TryGetNonNullValue(out IReadOnlyList<IEmbedField>? fields))
        {
            if (fields.Count > 25)
            {
                errors.Add((nameof(EmbedBuilder.Fields), "There cannot be more than 25 fields in an embed."));
            }

            for (int i = 0; i < 25; i++)
            {
                if (fields[i].Name.Length > 256)
                {
                    errors.Add(($"Fields[{i}]", "The title of a field cannot exceed 256 characters."));
                }

                if (fields[i].Value.Length > 1024)
                {
                    errors.Add(($"Fields[{i}]", "The length of a field value cannot exceed 1024 characters."));
                }

                totalCount += fields[i].Name.Length + fields[i].Value.Length;
            }
        }

        if (totalCount > 6000 && errors is not [])
        {
            return new BuilderValidationError
            (
                "The total length of the embed exceeded 6000 characters, and some embed fields were invalid.",
                errors.ToArray()
            );
        }
        else if (totalCount > 6000)
        {
            return new BuilderValidationError("The total length of the embed cannot exceed 6000 characters.");
        }
        else if (errors is not [])
        {
            return new BuilderValidationError
            (
                "Some embed fields were invalid. See the attached dictionary for further information.",
                errors.ToArray()
            );
        }
        else
        {
            return Result.Success;
        }
    }

    /// <summary>
    /// Builds the embed. This does not enforce validity.
    /// </summary>
    public static IEmbed Build(ref this EmbedBuilder builder)
    {
        return new BuiltEmbed
        {
            Title = builder.Title,
            Description = builder.Description,
            Color = builder.Color,
            Url = builder.Url,
            Timestamp = builder.Timestamp,
            Footer = builder.Footer,
            Author = builder.Author,
            Fields = builder.Fields
        };
    }
}
