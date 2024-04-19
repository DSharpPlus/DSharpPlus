// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Extensions.Internal.Builders.Errors;
using DSharpPlus.Extensions.Internal.Builders.Implementations;
using DSharpPlus.Internal.Abstractions.Models;

using DSharpPlus.Results;

namespace DSharpPlus.Extensions.Internal.Builders.Interactions;

/// <summary>
/// Represents a <seealso cref="ITextInputComponent"/> under construction. 
/// </summary>
public record struct TextInputComponentBuilder
{
    /// <inheritdoc cref="ITextInputComponent.CustomId"/>
    public string? CustomId { get; set; }

    /// <inheritdoc cref="ITextInputComponent.Style"/>
    public DiscordTextInputStyle? Style { get; set; }

    /// <inheritdoc cref="ITextInputComponent.Label"/>
    public string? Label { get; set; }

    /// <inheritdoc cref="ITextInputComponent.MinLength"/>
    public Optional<int> MinLength { get; set; }

    /// <inheritdoc cref="ITextInputComponent.MaxLength"/>
    public Optional<int> MaxLength { get; set; }

    /// <inheritdoc cref="ITextInputComponent.Required"/>
    public Optional<bool> Required { get; set; }

    /// <inheritdoc cref="ITextInputComponent.Placeholder"/>
    public Optional<string> Placeholder { get; set; }

    /// <inheritdoc cref="ITextInputComponent.Value"/>
    public Optional<string> Value { get; set; }
}

public static class TextInputComponentBuilderExtensions
{
    /// <summary>
    /// Sets the custom ID of the component for later identification.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder WithCustomId
    (
        ref this TextInputComponentBuilder builder,
        string customId
    )
    {
        builder.CustomId = customId;
        return ref builder;
    }

    /// <summary>
    /// Sets the style of the component.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder WithStyle
    (
        ref this TextInputComponentBuilder builder,
        DiscordTextInputStyle style
    )
    {
        builder.Style = style;
        return ref builder;
    }

    /// <summary>
    /// Sets the label of the component.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder WithLabel
    (
        ref this TextInputComponentBuilder builder,
        string label
    )
    {
        builder.Label = label;
        return ref builder;
    }

    /// <summary>
    /// Sets the minimum length of the text the user is required to put in.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder WithMinLength
    (
        ref this TextInputComponentBuilder builder,
        int minLength
    )
    {
        builder.MinLength = minLength;
        return ref builder;
    }

    /// <summary>
    /// Sets the maximum length of the text the user is required to put in.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder WithMaxLength
    (
        ref this TextInputComponentBuilder builder,
        int maxLength
    )
    {
        builder.MaxLength = maxLength;
        return ref builder;
    }

    /// <summary>
    /// Sets whether this input component is required to be filled.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder SetRequired
    (
        ref this TextInputComponentBuilder builder,
        bool required = true
    )
    {
        builder.Required = required;
        return ref builder;
    }

    /// <summary>
    /// Sets the placeholder value of this component.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder WithPlaceholder
    (
        ref this TextInputComponentBuilder builder,
        string placeholder
    )
    {
        builder.Placeholder = placeholder;
        return ref builder;
    }

    /// <summary>
    /// Sets the default value of this component.
    /// </summary>
    /// <returns>The component builder for chaining.</returns>
    public static ref TextInputComponentBuilder WithValue
    (
        ref this TextInputComponentBuilder builder,
        string value
    )
    {
        builder.Value = value;
        return ref builder;
    }

    /// <summary>
    /// Validates whether the builder can be converted into a legal component.
    /// </summary>
    public static Result Validate(ref this TextInputComponentBuilder builder)
    {
        List<(string, string)> errors = [];

        if (builder.CustomId is null)
        {
            errors.Add((nameof(builder.CustomId), "The custom ID of a text input component must be provided."));
        }
        else if (builder.CustomId.Length > 100)
        {
            errors.Add((nameof(builder.CustomId), "A custom ID cannot exceed 100 characters in length."));
        }

        if (builder.Style is null)
        {
            errors.Add((nameof(builder.Style), "The style of a text input component must be provided."));
        }

        if (builder.Label is null)
        {
            errors.Add((nameof(builder.Label), "The label of a text input component must be provided"));
        }
        else if (builder.Label.Length > 45)
        {
            errors.Add((nameof(builder.Label), "A text input component's label cannot exceed 45 characters in length."));
        }

        if (builder.MinLength.HasValue && builder.MinLength.Value is not > 0 and < 4000)
        {
            errors.Add((nameof(builder.MinLength), "The minimum length must be between 0 and 4000."));
        }

        if (builder.MaxLength.HasValue && builder.MaxLength.Value is not > 1 and < 4000)
        {
            errors.Add((nameof(builder.MaxLength), "The maximum length must be between 1 and 4000."));
        }

        if (builder.Placeholder.HasValue && builder.Placeholder.Value!.Length > 100)
        {
            errors.Add((nameof(builder.Placeholder), "A placeholder cannot exceed 100 characters in length."));
        }

        if (builder.Value.HasValue && builder.Value.Value!.Length > 4000)
        {
            errors.Add((nameof(builder.Value), "The default value cannot exceed 4000 characters in length."));
        }

        if (errors is not [])
        {
            return new BuilderValidationError
            (
                "Some component fields were invalid. See the attached dictionary for further information.",
                errors.ToArray()
            );
        }
        else
        {
            return Result.Success;
        }
    }

    /// <summary>
    /// Builds the text input component. This does not enforce validity, past enforcing all fields are present.
    /// </summary>
    public static ITextInputComponent Build(ref this TextInputComponentBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder.CustomId);
        ArgumentNullException.ThrowIfNull(builder.Label);
        if (!builder.Style.HasValue)
        {
            throw new ArgumentNullException(nameof(builder.Style));
        }

        return new BuiltTextInputComponent
        {
            Type = DiscordMessageComponentType.TextInput,
            CustomId = builder.CustomId,
            Style = builder.Style.Value,
            Label = builder.Label,
            MinLength = builder.MinLength,
            MaxLength = builder.MaxLength,
            Required = builder.Required,
            Value = builder.Value,
            Placeholder = builder.Placeholder
        };
    }
}
