// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus.Entities;
using DSharpPlus.Extensions.Internal.Builders.Errors;
using DSharpPlus.Extensions.Internal.Builders.Implementations;
using DSharpPlus.Internal.Abstractions.Models;

using OneOf;

using Remora.Results;

namespace DSharpPlus.Extensions.Internal.Builders.Interactions;

/// <summary>
/// Represents a modal under construction.
/// </summary>
public record struct ModalBuilder
{
    /// <inheritdoc cref="IModalCallbackData.CustomId"/>
    public string? CustomId { get; set; }

    /// <inheritdoc cref="IModalCallbackData.Title"/>
    public string? Title { get; set; }

    /// <inheritdoc cref="IModalCallbackData.Components"/>
    public IReadOnlyList<ITextInputComponent>? Components { get; set; }
}

public static class ModalBuilderExtensions
{
    /// <summary>
    /// Sets the custom ID of the modal.
    /// </summary>
    /// <returns>The modal builder for chaining.</returns>
    public static ref ModalBuilder WithCustomId(ref this ModalBuilder builder, string customId)
    {
        builder.CustomId = customId;
        return ref builder;
    }

    /// <summary>
    /// Sets the title of the modal.
    /// </summary>
    /// <returns>The modal builder for chaining.</returns>
    public static ref ModalBuilder WithTitle(ref this ModalBuilder builder, string title)
    {
        builder.Title = title;
        return ref builder;
    }

    /// <summary>
    /// Adds a text input component to the modal.
    /// </summary>
    /// <returns>The modal builder for chaining.</returns>
    public static ref ModalBuilder AddComponent(ref this ModalBuilder builder, ITextInputComponent component)
    {
        if (builder.Components is null)
        {
            builder.Components = [component];
            return ref builder;
        }

        builder.Components = [..builder.Components, component];
        return ref builder;
    }

    /// <summary>
    /// Verifies whether the modal builder can be transformed into a valid modal.
    /// </summary>
    public static Result Validate(ref this ModalBuilder builder)
    {
        List<(string, string)> errors = [];

        if (builder.CustomId is null)
        {
            errors.Add((nameof(builder.CustomId), "The custom ID of a modal must be provided."));
        }
        else if (builder.CustomId.Length > 100)
        {
            errors.Add((nameof(builder.CustomId), "A custom ID cannot exceed 100 characters in length."));
        }

        if (builder.Title is null)
        {
            errors.Add((nameof(builder.Title), "The title of a modal must be provided."));
        }
        else if (builder.Title.Length > 45)
        {
            errors.Add((nameof(builder.CustomId), "A modal title cannot exceed 45 characters in length."));
        }

        if (builder.Components is null || builder.Components.Count is not >= 1 and <= 5)
        {
            errors.Add((nameof(builder.Components), "There must be between one and five components provided."));
        }

        if (errors is not [])
        {
            return new BuilderValidationError
            (
                "Some modal fields were invalid. See the attached dictionary for further information.",
                errors.ToArray()
            );
        }
        else
        {
            return Result.Success;
        }
    }

    /// <summary>
    /// Builds the modal. This does not enforce validity, past enforcing all fields are present.
    /// </summary>
    public static IInteractionResponse Build(ref this ModalBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder.Title);
        ArgumentNullException.ThrowIfNull(builder.CustomId);
        ArgumentNullException.ThrowIfNull(builder.Components);

        return new BuiltInteractionResponse
        {
            Type = DiscordInteractionCallbackType.Modal,
            Data = OneOf<IAutocompleteCallbackData, IMessageCallbackData, IModalCallbackData>.FromT2
            (
                new BuiltModalCallbackData
                {
                    Title = builder.Title,
                    CustomId = builder.CustomId,
                    Components = builder.Components.Select<ITextInputComponent, IActionRowComponent>
                    (
                        x => 
                        {
                            return new BuiltActionRowComponent
                            {
                                Type = DiscordMessageComponentType.ActionRow,
                                Components = [x]
                            };
                        }
                    ).ToArray()
                }
            )
        };
    }
}
