using DSharpPlus.Caching;

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogStickerEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected sticker.
    /// </summary>
    public CachedEntity<ulong, DiscordMessageSticker> Target { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's name change.
    /// </summary>
    public PropertyChange<string>? NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's description change.
    /// </summary>
    public PropertyChange<string>? DescriptionChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's tags change.
    /// </summary>
    public PropertyChange<string>? TagsChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's tags change.
    /// </summary>
    public PropertyChange<string>? AssetChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's guild id change.
    /// </summary>
    public PropertyChange<ulong>? GuildIdChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's availability change.
    /// </summary>
    public PropertyChange<bool>? AvailabilityChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's id change.
    /// </summary>
    public PropertyChange<ulong>? IdChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's type change.
    /// </summary>
    public PropertyChange<StickerType>? TypeChange { get; internal set; }

    /// <summary>
    /// Gets the description of sticker's format change.
    /// </summary>
    public PropertyChange<StickerFormat>? FormatChange { get; internal set; }

    internal DiscordAuditLogStickerEntry() { }
}
