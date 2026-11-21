using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace DSharpPlus.Entities;

/// <summary>
/// Base interface for any discord message builder.
/// </summary>
public interface IDiscordMessageBuilder
{
    /// <summary>
    /// Getter / setter for message content.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Whether this message will play as a text-to-speech message.
    /// </summary>
    public bool IsTTS { get; set; }

    /// <summary>
    /// Gets or sets a poll for this message.
    /// </summary>
    public DiscordPollBuilder? Poll { get; set; }

    /// <summary>
    /// All embeds on this message.
    /// </summary>
    public IReadOnlyList<DiscordEmbed> Embeds { get; }

    /// <summary>
    /// All files on this message.
    /// </summary>
    public IReadOnlyList<DiscordFile> Files { get; }

    /// <summary>
    /// All components on this message.
    /// </summary>
    public IReadOnlyList<DiscordComponent> Components { get; }

    /// <summary>
    /// All allowed mentions on this message.
    /// </summary>
    public IReadOnlyList<IMention> Mentions { get; }

    public DiscordMessageFlags Flags { get; }

    /// <summary>
    /// Adds content to this message
    /// </summary>
    /// <param name="content">Message content to use</param>
    /// <returns></returns>
    public IDiscordMessageBuilder WithContent(string content);

    /// <summary>
    /// Enables support for V2 components; messages with the V2 flag cannot be downgraded.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    public IDiscordMessageBuilder EnableV2Components();

    /// <summary>
    /// Disables V2 components IF this builder does not currently contain illegal components.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">The builder contains V2 components and cannot be downgraded.</exception>
    /// <remarks>This method only disables the V2 components flag; the message originally associated with this builder cannot be downgraded, and this method only exists for convenience.</remarks>
    public IDiscordMessageBuilder DisableV2Components();

    /// <summary>
    /// Adds a raw action row.
    /// </summary>
    /// <param name="component">The select menu to add, if possible.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddActionRowComponent(DiscordActionRowComponent component);

    /// <summary>
    /// Adds a new action row with the given component.
    /// </summary>
    /// <param name="selectMenu">The select menu to add, if possible.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddActionRowComponent(DiscordSelectComponent selectMenu);

    /// <summary>
    /// Adds buttons to the builder.
    /// </summary>
    /// <param name="buttons">The buttons to add to the message. They will automatically be chunked into separate action rows as necessary.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddActionRowComponent(params IEnumerable<DiscordButtonComponent> buttons);

    /// <summary>
    /// Adds a media gallery to this builder.
    /// </summary>
    /// <param name="galleryItems">The items to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddMediaGalleryComponent(params IEnumerable<DiscordMediaGalleryItem> galleryItems);

    /// <summary>
    /// Adds a section component to the builder.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddSectionComponent(DiscordSectionComponent section);

    /// <summary>
    /// Adds a text display to this builder.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddTextDisplayComponent(DiscordTextDisplayComponent component);

    /// <summary>
    /// Adds a text display to this builder.
    /// </summary>
    /// <param name="content"></param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddTextDisplayComponent(string content);

    /// <summary>
    /// Adds a separator component to this builder.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddSeparatorComponent(DiscordSeparatorComponent component);

    /// <summary>
    /// Adds a file component to this builder.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddFileComponent(DiscordFileComponent component);
    
    /// <summary>
    /// Adds a container component to this builder.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public IDiscordMessageBuilder AddContainerComponent(DiscordContainerComponent component);

    /// <summary>
    /// Adds an arbitrary DiscordComponent to this message builder. This does not guarantee type safety or correctness.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IDiscordMessageBuilder AddArbitraryComponent(DiscordComponent component);

    /// <summary>
    /// Sets whether this message should play as a text-to-speech message.
    /// </summary>
    /// <param name="isTTS"></param>
    /// <returns></returns>
    public IDiscordMessageBuilder WithTTS(bool isTTS);

    /// <summary>
    /// Adds an embed to this message.
    /// </summary>
    /// <param name="embed">Embed to add.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddEmbed(DiscordEmbed embed);

    /// <summary>
    /// Adds multiple embeds to this message.
    /// </summary>
    /// <param name="embeds">Collection of embeds to add.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="fileName">Name of the file to attach.</param>
    /// <param name="stream">Stream containing said file's contents.</param>
    /// <param name="resetStream">Whether to reset the stream to position 0 after sending.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddFile(string fileName, Stream stream, bool resetStream = false);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="stream">FileStream pointing to the file to attach.</param>
    /// <param name="resetStream">Whether to reset the stream position to 0 after sending.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddFile(FileStream stream, bool resetStream = false);

    /// <summary>
    /// Attaches multiple files to this message.
    /// </summary>
    /// <param name="files">Dictionary of files to add, where <see cref="string"/> is a file name and <see cref="Stream"/> is a stream containing the file's contents.</param>
    /// <param name="resetStreams">Whether to reset all stream positions to 0 after sending.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddFiles(IDictionary<string, Stream> files, bool resetStreams = false);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="fileName">Name of the file to attach.</param>
    /// <param name="stream">Stream containing said file's contents.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file stream.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddFile(string fileName, Stream stream, AddFileOptions fileOptions);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="stream">FileStream pointing to the file to attach.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file stream.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddFile(FileStream stream, AddFileOptions fileOptions);

    /// <summary>
    /// Attaches multiple files to this message.
    /// </summary>
    /// <param name="files">Dictionary of files to add, where <see cref="string"/> is a file name and <see cref="Stream"/> is a stream containing the file's contents.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file streams.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddFiles(IDictionary<string, Stream> files, AddFileOptions fileOptions);

    /// <summary>
    /// Attaches previously used files to this file stream.
    /// </summary>
    /// <param name="files">Previously attached files to reattach</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddFiles(IEnumerable<DiscordFile> files);

    /// <summary>
    /// Adds an allowed mention to this message.
    /// </summary>
    /// <param name="mention">Mention to allow in this message.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddMention(IMention mention);

    /// <summary>
    /// Adds multiple allowed mentions to this message.
    /// </summary>
    /// <param name="mentions">Collection of mentions to allow in this message.</param>
    /// <returns></returns>
    public IDiscordMessageBuilder AddMentions(IEnumerable<IMention> mentions);

    /// <summary>
    /// Applies <see cref="DiscordMessageFlags.SuppressNotifications"/> to the message.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// As per <see cref="DiscordMessageFlags.SuppressNotifications"/>, this does not change the message's allowed mentions
    /// (controlled by <see cref="AddMentions"/>), but instead prevents a mention from triggering a push notification.
    /// </remarks>
    public IDiscordMessageBuilder SuppressNotifications();

    /// <summary>
    /// Clears all components attached to this builder.
    /// </summary>
    public void ClearComponents();

    /// <summary>
    /// Clears this builder.
    /// </summary>
    public void Clear();

    public IDiscordMessageBuilder SuppressEmbeds();
}
