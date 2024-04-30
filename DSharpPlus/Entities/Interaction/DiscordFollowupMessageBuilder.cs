namespace DSharpPlus.Entities;
using System;
using System.Linq;

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
        => IsEphemeral ? (int?)(Flags | DiscordMessageFlags.Ephemeral) : null;

    /// <summary>
    /// Constructs a new followup message builder
    /// </summary>
    public DiscordFollowupMessageBuilder() { }

    public DiscordFollowupMessageBuilder(DiscordFollowupMessageBuilder builder) : base(builder) => IsEphemeral = builder.IsEphemeral;

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
        IsEphemeral = ephemeral;
        return this;
    }

    /// <summary>
    /// Allows for clearing the Followup Message builder so that it can be used again to send a new message.
    /// </summary>
    public override void Clear()
    {
        IsEphemeral = false;

        base.Clear();
    }

    internal void Validate()
    {
        if (Files?.Count == 0 && string.IsNullOrEmpty(Content) && !Embeds.Any())
        {
            throw new ArgumentException("You must specify content, an embed, or at least one file.");
        }
    }
}
