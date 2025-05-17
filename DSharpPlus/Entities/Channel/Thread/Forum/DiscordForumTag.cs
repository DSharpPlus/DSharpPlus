using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public sealed class DiscordForumTag : SnowflakeObject
{
    /// <summary>
    /// Gets the name of this tag.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets whether this tag is moderated. Moderated tags can only be applied by users with the <see cref="DiscordPermission.ManageThreads"/> permission.
    /// </summary>
    [JsonProperty("moderated")]
    public bool Moderated { get; internal set; }

    /// <summary>
    /// Gets the Id of the emoji for this tag, if applicable.
    /// </summary>
    [JsonProperty("emoji_id")]
    public ulong? EmojiId { get; internal set; }

    /// <summary>
    /// Gets the unicode emoji for this tag, if applicable.
    /// </summary>
    [JsonProperty("emoji_name")]
    public string EmojiName { get; internal set; }
}

public class DiscordForumTagBuilder
{
    [JsonProperty("name"), SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "This is used by JSON.NET.")]
    private string name;

    [JsonProperty("moderated"), SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "This is used by JSON.NET.")]
    private bool moderated;

    [JsonProperty("emoji_id"), SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "This is used by JSON.NET.")]
    private ulong? emojiId;

    [JsonProperty("emoji_name"), SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "This is used by JSON.NET.")]
    private string emojiName;

    public static DiscordForumTagBuilder FromTag(DiscordForumTag tag)
    {
        DiscordForumTagBuilder builder = new()
        {
            name = tag.Name,
            moderated = tag.Moderated,
            emojiId = tag.EmojiId,
            emojiName = tag.EmojiName
        };
        return builder;
    }

    /// <summary>
    /// Sets the name of this tag.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    /// <summary>
    /// Sets this tag to be moderated (as in, it can only be set by users with the <see cref="DiscordPermission.ManageThreads"/> permission).
    /// </summary>
    /// <param name="moderated">Whether the tag is moderated.</param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder IsModerated(bool moderated = true)
    {
        this.moderated = moderated;
        return this;
    }

    /// <summary>
    /// Sets the emoji ID for this tag (which will overwrite the emoji name).
    /// </summary>
    /// <param name="emojiId"></param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder WithEmojiId(ulong? emojiId)
    {
        this.emojiId = emojiId;
        this.emojiName = null;
        return this;
    }

    /// <summary>
    /// Sets the emoji for this tag.
    /// </summary>
    /// <param name="emoji">The emoji to use.</param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder WithEmoji(DiscordEmoji emoji)
    {
        this.emojiId = emoji.Id;
        this.emojiName = emoji.Name;
        return this;
    }

    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder WithEmojiName(string emojiName)
    {
        this.emojiId = null;
        this.emojiName = emojiName;
        return this;
    }
}
