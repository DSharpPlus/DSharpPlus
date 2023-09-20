// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogStickerEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected sticker.
    /// </summary>
    public DiscordMessageSticker Target { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's description change.
    /// </summary>
    public PropertyChange<string> DescriptionChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's tags change.
    /// </summary>
    public PropertyChange<string> TagsChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's tags change.
    /// </summary>
    public PropertyChange<string> AssetChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's guild id change.
    /// </summary>
    public PropertyChange<ulong?> GuildIdChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's availability change.
    /// </summary>
    public PropertyChange<bool?> AvailabilityChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's id change.
    /// </summary>
    public PropertyChange<ulong?> IdChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's type change.
    /// </summary>
    public PropertyChange<StickerType?> TypeChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's format change.
    /// </summary>
    public PropertyChange<StickerFormat?> FormatChange { get; internal set; }

    internal DiscordAuditLogStickerEntry() { }
}
