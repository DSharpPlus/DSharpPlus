namespace DSharpPlus.Entities;

using Newtonsoft.Json;

public sealed class DiscordForumTag : SnowflakeObject
{
    /// <summary>
    /// Gets the name of this tag.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets whether this tag is moderated. Moderated tags can only be applied by users with the <see cref="Permissions.ManageThreads"/> permission.
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
    [JsonProperty("name")]
    private string _name;

    [JsonProperty("moderated")]
    private bool _moderated;

    [JsonProperty("emoji_id")]
    private ulong? _emojiId;

    [JsonProperty("emoji_name")]
    private string _emojiName;

    public static DiscordForumTagBuilder FromTag(DiscordForumTag tag)
    {
        DiscordForumTagBuilder builder = new DiscordForumTagBuilder
        {
            _name = tag.Name,
            _moderated = tag.Moderated,
            _emojiId = tag.EmojiId,
            _emojiName = tag.EmojiName
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
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets this tag to be moderated (as in, it can only be set by users with the <see cref="Permissions.ManageThreads"/> permission).
    /// </summary>
    /// <param name="moderated">Whether the tag is moderated.</param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder IsModerated(bool moderated = true)
    {
        _moderated = moderated;
        return this;
    }

    /// <summary>
    /// Sets the emoji ID for this tag (which will overwrite the emoji name).
    /// </summary>
    /// <param name="emojiId"></param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder WithEmojiId(ulong? emojiId)
    {
        _emojiId = emojiId;
        _emojiName = null;
        return this;
    }

    /// <summary>
    /// Sets the emoji for this tag.
    /// </summary>
    /// <param name="emoji">The emoji to use.</param>
    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder WithEmoji(DiscordEmoji emoji)
    {
        _emojiId = emoji.Id;
        _emojiName = emoji.Name;
        return this;
    }

    /// <returns>The builder to chain calls with.</returns>
    public DiscordForumTagBuilder WithEmojiName(string emojiName)
    {
        _emojiId = null;
        _emojiName = emojiName;
        return this;
    }
}
