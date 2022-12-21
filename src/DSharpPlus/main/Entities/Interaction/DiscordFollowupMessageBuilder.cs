// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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

using System;
using System.Linq;

namespace DSharpPlus.Entities;

/// <summary>
/// Constructs a followup message to an interaction.
/// </summary>
public sealed class DiscordFollowupMessageBuilder : BaseDiscordMessageBuilder<DiscordFollowupMessageBuilder>
{
    /// <summary>
    /// Whether this followup message should be ephemeral.
    /// </summary>
    public bool IsEphemeral { get; set; }

    internal int? _flags
        => this.IsEphemeral ? 64 : null;

    /// <summary>
    /// Constructs a new followup message builder
    /// </summary>
    public DiscordFollowupMessageBuilder() { }

    public DiscordFollowupMessageBuilder(DiscordFollowupMessageBuilder builder) : base(builder)
    {
        this.IsEphemeral = builder.IsEphemeral;
    }

    /// <summary>
    /// Copies the common properties from the passed builder.
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    public DiscordFollowupMessageBuilder(IDiscordMessageBuilder builder) : base(builder) { }

    /// <summary>
    /// Sets the followup message to be ephemeral.
    /// </summary>
    /// <param name="ephemeral">Ephemeral.</param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordFollowupMessageBuilder AsEphemeral(bool ephemeral = true)
    {
        this.IsEphemeral = ephemeral;
        return this;
    }

    /// <summary>
    /// Allows for clearing the Followup Message builder so that it can be used again to send a new message.
    /// </summary>
    public override void Clear()
    {
        this.IsEphemeral = false;

        base.Clear();
    }

    internal void Validate()
    {
        if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && !this.Embeds.Any())
            throw new ArgumentException("You must specify content, an embed, or at least one file.");
    }
}
