// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents embedded content in a message.
/// </summary>
public interface IEmbed
{
    /// <summary>
    /// The title of this embed.
    /// </summary>
    public Optional<string> Title { get; }

    /// <summary>
    /// The type of this embed, always <c>rich</c> for bot/webhook embeds.
    /// </summary>
    public Optional<string> Type { get; }

    /// <summary>
    /// The main content field of this embed.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// The url of this embed.
    /// </summary>
    public Optional<string> Url { get; }

    /// <summary>
    /// The timestamp of this embed content.
    /// </summary>
    public Optional<DateTimeOffset> Timestamp { get; }

    /// <summary>
    /// The color code for the event sidebar.
    /// </summary>
    public Optional<int> Color { get; }

    /// <summary>
    /// The embed footer.
    /// </summary>
    public Optional<IEmbedFooter> Footer { get; }

    /// <summary>
    /// The embed image.
    /// </summary>
    public Optional<IEmbedImage> Image { get; }

    /// <summary>
    /// The embed thumbnail.
    /// </summary>
    public Optional<IEmbedThumbnail> Thumbnail { get; }

    /// <summary>
    /// The embed video.
    /// </summary>
    public Optional<IEmbedVideo> Video { get; }

    /// <summary>
    /// The embed provider.
    /// </summary>
    public Optional<IEmbedProvider> Provider { get; }

    /// <summary>
    /// The embed author.
    /// </summary>
    public Optional<IEmbedAuthor> Author { get; }

    /// <summary>
    /// Up to 25 embed fields.
    /// </summary>
    public Optional<IReadOnlyList<IEmbedField>> Fields { get; }
}
