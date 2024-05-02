using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net;

namespace DSharpPlus.Entities;

/// <summary>
/// An abstraction for the different message builders in DSharpPlus.
/// </summary>
public abstract class BaseDiscordMessageBuilder<T> : IDiscordMessageBuilder where T : BaseDiscordMessageBuilder<T>
    // This has got to be the most big brain thing I have ever done with interfaces lmfao
{
    /// <summary>
    /// The contents of this message.
    /// </summary>
    public string? Content
    {
        get => _content;
        set
        {
            if (value != null && value.Length > 2000)
            {
                throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));
            }

            _content = value;
        }
    }
    internal string? _content;

    public DiscordMessageFlags Flags { get; internal set; }

    public T SuppressNotifications()
    {
        Flags |= DiscordMessageFlags.SuppressNotifications;
        return (T)this;
    }

    public bool IsTTS { get; set; }

    /// <summary>
    /// Gets or sets a poll for this message.
    /// </summary>
    public DiscordPollBuilder? Poll { get; set; }

    /// <summary>
    /// Embeds to send on this webhook request.
    /// </summary>
    public IReadOnlyList<DiscordEmbed> Embeds => _embeds;
    internal List<DiscordEmbed> _embeds = [];

    /// <summary>
    /// Files to send on this webhook request.
    /// </summary>
    public IReadOnlyList<DiscordMessageFile> Files => _files;
    internal List<DiscordMessageFile> _files = [];

    /// <summary>
    /// Mentions to send on this webhook request.
    /// </summary>
    public IReadOnlyList<IMention> Mentions => _mentions;
    internal List<IMention> _mentions = [];

    /// <summary>
    /// Components to send on this followup message.
    /// </summary>
    public IReadOnlyList<DiscordActionRowComponent> Components => _components;
    internal List<DiscordActionRowComponent> _components = [];

    /// <summary>
    /// Thou shalt NOT PASS! ⚡
    /// </summary>
    // i'm very proud that we have the actual LOTR quote here, not the movie "you shall not pass"
    internal BaseDiscordMessageBuilder() { }

    /// <summary>
    /// Constructs a new <see cref="BaseDiscordMessageBuilder{T}"/> based on an existing <see cref="IDiscordMessageBuilder"/>.
    /// Existing file streams will have their position reset to 0.
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    protected BaseDiscordMessageBuilder(IDiscordMessageBuilder builder)
    {
        _content = builder.Content;
        _mentions.AddRange([.. builder.Mentions]);
        _embeds.AddRange(builder.Embeds);
        _components.AddRange(builder.Components);
        _files.AddRange(builder.Files);
        IsTTS = builder.IsTTS;
        Poll = builder.Poll;
    }

    /// <summary>
    /// Sets the content of the Message.
    /// </summary>
    /// <param name="content">The content to be set.</param>
    /// <returns>The current builder to be chained.</returns>
    public T WithContent(string content)
    {
        Content = content;
        return (T)this;
    }

    /// <summary>
    /// Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
    /// </summary>
    /// <param name="components">The components to add to the message.</param>
    /// <returns>The current builder to be chained.</returns>
    /// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
    public T AddComponents(params DiscordComponent[] components)
        => AddComponents((IEnumerable<DiscordComponent>)components);

    /// <summary>
    /// Appends several rows of components to the message
    /// </summary>
    /// <param name="components">The rows of components to add, holding up to five each.</param>
    /// <returns></returns>
    public T AddComponents(IEnumerable<DiscordActionRowComponent> components)
    {
        int count = components.TryGetNonEnumeratedCount(out int nonEnumerated) ? nonEnumerated : components.Count();

        if (count + _components.Count > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(components), "The amount of action rows provided exceeds the maximum of five.");
        }

        foreach (DiscordActionRowComponent? ar in components)
        {
            _components.Add(ar);
        }

        return (T)this;
    }

    /// <summary>
    /// Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
    /// </summary>
    /// <param name="components">The components to add to the message.</param>
    /// <returns>The current builder to be chained.</returns>
    /// <exception cref="ArgumentOutOfRangeException">No components were passed.</exception>
    public T AddComponents(IEnumerable<DiscordComponent> components)
    {
        int count = components.TryGetNonEnumeratedCount(out int nonEnumerated) ? nonEnumerated : components.Count();

        if (count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(components), "You must provide at least one component");
        }

        if (count > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(components), "You cannot add more than 5 components per action row!");
        }

        DiscordActionRowComponent comp = new(components);
        _components.Add(comp);

        return (T)this;
    }

    /// <summary>
    /// Sets if the message should be TTS.
    /// </summary>
    /// <param name="isTTS">If TTS should be set.</param>
    /// <returns>The current builder to be chained.</returns>
    public T WithTTS(bool isTTS)
    {
        IsTTS = isTTS;
        return (T)this;
    }

    public T WithPoll(DiscordPollBuilder poll)
    {
        Poll = poll;
        return (T)this;
    }

    /// <summary>
    /// Appends an embed to the current builder.
    /// </summary>
    /// <param name="embed">The embed that should be appended.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddEmbed(DiscordEmbed embed)
    {
        if (embed is null)
        {
            return (T)this; //Providing null embeds will produce a 400 response from Discord.//
        }

        _embeds.Add(embed);
        return (T)this;
    }

    /// <summary>
    /// Appends several embeds to the current builder.
    /// </summary>
    /// <param name="embeds">The embeds that should be appended.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddEmbeds(IEnumerable<DiscordEmbed> embeds)
    {
        _embeds.AddRange(embeds);
        return (T)this;
    }

    /// <summary>
    /// Clears the embeds on the current builder.
    /// </summary>
    /// <returns>The current builder for chaining.</returns>
    public T ClearEmbeds()
    {
        _embeds.Clear();
        return (T)this;
    }

    /// <summary>
    /// Removes the embed at the specified index.
    /// </summary>
    /// <returns>The current builder for chaining.</returns>
    public T RemoveEmbedAt(int index)
    {
        _embeds.RemoveAt(index);
        return (T)this;
    }

    /// <summary>
    /// Removes the specified range of embeds.
    /// </summary>
    /// <param name="index">The starting index of the embeds to remove.</param>
    /// <param name="count">The amount of embeds to remove.</param>
    /// <returns>The current builder for chaining.</returns>
    public T RemoveEmbeds(int index, int count)
    {
        _embeds.RemoveRange(index, count);
        return (T)this;
    }

    /// <summary>
    /// Sets if the message has files to be sent.
    /// </summary>
    /// <param name="fileName">The fileName that the file should be sent as.</param>
    /// <param name="stream">The Stream to the file.</param>
    /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddFile(string fileName, Stream stream, bool resetStreamPosition = false) => AddFile(fileName, stream, resetStreamPosition ? AddFileOptions.ResetStream : AddFileOptions.None);

    /// <summary>
    /// Sets if the message has files to be sent.
    /// </summary>
    /// <param name="stream">The Stream to the file.</param>
    /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddFile(FileStream stream, bool resetStreamPosition = false) => AddFile(stream, resetStreamPosition ? AddFileOptions.ResetStream : AddFileOptions.None);

    /// <summary>
    /// Sets if the message has files to be sent.
    /// </summary>
    /// <param name="files">The Files that should be sent.</param>
    /// <param name="resetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddFiles(IDictionary<string, Stream> files, bool resetStreamPosition = false) => AddFiles(files, resetStreamPosition ? AddFileOptions.ResetStream : AddFileOptions.None);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="fileName">Name of the file to attach.</param>
    /// <param name="stream">Stream containing said file's contents.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file stream.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddFile(string fileName, Stream stream, AddFileOptions fileOptions)
    {
        if (Files.Count >= 10)
        {
            throw new ArgumentException("Cannot send more than 10 files with a single message.");
        }

        if (_files.Any(x => x.FileName == fileName))
        {
            throw new ArgumentException("A file with that filename already exists");
        }

        stream = ResolveStream(stream, fileOptions);
        long? resetPosition = fileOptions.HasFlag(AddFileOptions.ResetStream) ? stream.Position : null;
        _files.Add(new DiscordMessageFile(fileName, stream, resetPosition, fileOptions: fileOptions));

        return (T)this;
    }

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="stream">FileStream pointing to the file to attach.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file stream.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddFile(FileStream stream, AddFileOptions fileOptions) => AddFile(stream.Name, stream, fileOptions);

    /// <summary>
    /// Attaches multiple files to this message.
    /// </summary>
    /// <param name="files">Dictionary of files to add, where <see cref="string"/> is a file name and <see cref="Stream"/> is a stream containing the file's contents.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file streams.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddFiles(IDictionary<string, Stream> files, AddFileOptions fileOptions)
    {
        if (Files.Count + files.Count > 10)
        {
            throw new ArgumentException("Cannot send more than 10 files with a single message.");
        }

        foreach (KeyValuePair<string, Stream> file in files)
        {
            if (_files.Any(x => x.FileName == file.Key))
            {
                throw new ArgumentException("A File with that filename already exists");
            }

            Stream stream = ResolveStream(file.Value, fileOptions);
            long? resetPosition = fileOptions.HasFlag(AddFileOptions.ResetStream) ? stream.Position : null;
            _files.Add(new DiscordMessageFile(file.Key, stream, resetPosition, fileOptions: fileOptions));
        }

        return (T)this;
    }

    public T AddFiles(IEnumerable<DiscordMessageFile> files)
    {
        _files.AddRange(files);
        return (T)this;
    }

    /// <summary>
    /// Adds the mention to the mentions to parse, etc. with the interaction response.
    /// </summary>
    /// <param name="mention">Mention to add.</param>
    public T AddMention(IMention mention)
    {
        _mentions.Add(mention);
        return (T)this;
    }

    /// <summary>
    /// Adds the mentions to the mentions to parse, etc. with the interaction response.
    /// </summary>
    /// <param name="mentions">Mentions to add.</param>
    public T AddMentions(IEnumerable<IMention> mentions)
    {
        _mentions.AddRange(mentions);
        return (T)this;
    }

    /// <summary>
    /// Clears all message components on this builder.
    /// </summary>
    public virtual void ClearComponents()
        => _components.Clear();

    /// <summary>
    /// Allows for clearing the Message Builder so that it can be used again to send a new message.
    /// </summary>
    public virtual void Clear()
    {
        Content = "";
        _embeds.Clear();
        IsTTS = false;
        _mentions.Clear();
        _files.Clear();
        _components.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // We don't bother to fully implement the dispose pattern
        // since deriving from this type outside this assembly is unusual.

        foreach (DiscordMessageFile file in _files)
        {
            if (file.FileOptions.HasFlag(AddFileOptions.CloseStream))
            {
                if (file.Stream is RequestStreamWrapper wrapper)
                {
                    wrapper.UnderlyingStream.Dispose();
                }
                else
                {
                    file.Stream.Dispose();
                }
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (DiscordMessageFile file in _files)
        {
            if (file.FileOptions.HasFlag(AddFileOptions.CloseStream))
            {
                if (file.Stream is RequestStreamWrapper wrapper)
                {
                    await wrapper.UnderlyingStream.DisposeAsync();
                }
                else
                {
                    await file.Stream.DisposeAsync();
                }
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Helper method to reset stream positions used several times by the API client.
    /// </summary>
    internal void ResetFileStreamPositions()
    {
        foreach (DiscordMessageFile file in _files)
        {
            if (file.ResetPositionTo is long pos)
            {
                file.Stream.Seek(pos, SeekOrigin.Begin);
            }
        }
    }

    /// <summary>
    /// Helper method to resolve stream copies depending on the file mode parameter.
    /// </summary>
    private static Stream ResolveStream(Stream stream, AddFileOptions fileOptions)
    {
        if (!fileOptions.HasFlag(AddFileOptions.CopyStream))
        {
            return new RequestStreamWrapper(stream);
        }

        Stream originalStream = stream;
        MemoryStream newStream = new();
        originalStream.CopyTo(newStream);
        newStream.Position = 0;
        if (fileOptions.HasFlag(AddFileOptions.CloseStream))
        {
            originalStream.Dispose();
        }

        return newStream;
    }

    IDiscordMessageBuilder IDiscordMessageBuilder.SuppressNotifications() => SuppressNotifications();
    IDiscordMessageBuilder IDiscordMessageBuilder.WithContent(string content) => WithContent(content);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddComponents(params DiscordComponent[] components) => AddComponents(components);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddComponents(IEnumerable<DiscordComponent> components) => AddComponents(components);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddComponents(IEnumerable<DiscordActionRowComponent> components) => AddComponents(components);
    IDiscordMessageBuilder IDiscordMessageBuilder.WithTTS(bool isTTS) => WithTTS(isTTS);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddEmbed(DiscordEmbed embed) => AddEmbed(embed);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddEmbeds(IEnumerable<DiscordEmbed> embeds) => AddEmbeds(embeds);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFile(string fileName, Stream stream, bool resetStream) => AddFile(fileName, stream, resetStream);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFile(FileStream stream, bool resetStream) => AddFile(stream, resetStream);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFiles(IDictionary<string, Stream> files, bool resetStreams) => AddFiles(files, resetStreams);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFiles(IEnumerable<DiscordMessageFile> files) => AddFiles(files);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFile(string fileName, Stream stream, AddFileOptions fileOptions) => AddFile(fileName, stream, fileOptions);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFile(FileStream stream, AddFileOptions fileOptions) => AddFile(stream, fileOptions);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFiles(IDictionary<string, Stream> files, AddFileOptions fileOptions) => AddFiles(files, fileOptions);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddMention(IMention mention) => AddMention(mention);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddMentions(IEnumerable<IMention> mentions) => AddMentions(mentions);
}

/// <summary>
/// Additional flags for files added to a message builder.
/// </summary>
[Flags]
public enum AddFileOptions
{
    /// <summary>
    /// No additional behavior. The stream will read to completion and is left at that position after sending.
    /// </summary>
    None = 0,

    /// <summary>
    /// Resets the stream to its original position after sending.
    /// </summary>
    ResetStream = 0x1,

    /// <summary>
    /// Closes the stream upon disposal of the message builder.
    /// </summary>
    /// <remarks>
    /// Streams will not be disposed upon sending. Disposal of the message builder is necessary.
    /// </remarks>
    CloseStream = 0x2,

    /// <summary>
    /// Immediately reads the stream to completion and copies its contents to an in-memory stream.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this incurs an additional memory overhead and may perform synchronous I/O and should only be used if the source stream cannot be kept open any longer.
    /// </para>
    /// <para>
    /// If specified together with <see cref="CloseStream"/>, the stream will closed immediately after the copy.
    /// </para>
    /// </remarks>
    CopyStream = 0x4,
}

/// <summary>
/// Base interface for any discord message builder.
/// </summary>
public interface IDiscordMessageBuilder : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Getter / setter for message content.
    /// </summary>
    string? Content { get; set; }

    /// <summary>
    /// Whether this message will play as a text-to-speech message.
    /// </summary>
    bool IsTTS { get; set; }

    /// <summary>
    /// Gets or sets a poll for this message.
    /// </summary>
    DiscordPollBuilder? Poll { get; set; }

    /// <summary>
    /// All embeds on this message.
    /// </summary>
    IReadOnlyList<DiscordEmbed> Embeds { get; }

    /// <summary>
    /// All files on this message.
    /// </summary>
    IReadOnlyList<DiscordMessageFile> Files { get; }

    /// <summary>
    /// All components on this message.
    /// </summary>
    IReadOnlyList<DiscordActionRowComponent> Components { get; }

    /// <summary>
    /// All allowed mentions on this message.
    /// </summary>
    IReadOnlyList<IMention> Mentions { get; }

    DiscordMessageFlags Flags { get; }

    /// <summary>
    /// Adds content to this message
    /// </summary>
    /// <param name="content">Message content to use</param>
    /// <returns></returns>
    IDiscordMessageBuilder WithContent(string content);

    /// <summary>
    /// Adds components to this message. Each call should append to a new row.
    /// </summary>
    /// <param name="components">Components to add.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddComponents(params DiscordComponent[] components);

    /// <summary>
    /// Adds components to this message. Each call should append to a new row.
    /// </summary>
    /// <param name="components">Components to add.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddComponents(IEnumerable<DiscordComponent> components);

    /// <summary>
    /// Adds an action row component to this message.
    /// </summary>
    /// <param name="components">Action row to add to this message. Should contain child components.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddComponents(IEnumerable<DiscordActionRowComponent> components);

    /// <summary>
    /// Sets whether this message should play as a text-to-speech message.
    /// </summary>
    /// <param name="isTTS"></param>
    /// <returns></returns>
    IDiscordMessageBuilder WithTTS(bool isTTS);

    /// <summary>
    /// Adds an embed to this message.
    /// </summary>
    /// <param name="embed">Embed to add.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddEmbed(DiscordEmbed embed);

    /// <summary>
    /// Adds multiple embeds to this message.
    /// </summary>
    /// <param name="embeds">Collection of embeds to add.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> embeds);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="fileName">Name of the file to attach.</param>
    /// <param name="stream">Stream containing said file's contents.</param>
    /// <param name="resetStream">Whether to reset the stream to position 0 after sending.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddFile(string fileName, Stream stream, bool resetStream = false);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="stream">FileStream pointing to the file to attach.</param>
    /// <param name="resetStream">Whether to reset the stream position to 0 after sending.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddFile(FileStream stream, bool resetStream = false);

    /// <summary>
    /// Attaches multiple files to this message.
    /// </summary>
    /// <param name="files">Dictionary of files to add, where <see cref="string"/> is a file name and <see cref="Stream"/> is a stream containing the file's contents.</param>
    /// <param name="resetStreams">Whether to reset all stream positions to 0 after sending.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddFiles(IDictionary<string, Stream> files, bool resetStreams = false);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="fileName">Name of the file to attach.</param>
    /// <param name="stream">Stream containing said file's contents.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file stream.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddFile(string fileName, Stream stream, AddFileOptions fileOptions);

    /// <summary>
    /// Attaches a file to this message.
    /// </summary>
    /// <param name="stream">FileStream pointing to the file to attach.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file stream.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddFile(FileStream stream, AddFileOptions fileOptions);

    /// <summary>
    /// Attaches multiple files to this message.
    /// </summary>
    /// <param name="files">Dictionary of files to add, where <see cref="string"/> is a file name and <see cref="Stream"/> is a stream containing the file's contents.</param>
    /// <param name="fileOptions">Additional flags for the handling of the file streams.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddFiles(IDictionary<string, Stream> files, AddFileOptions fileOptions);

    /// <summary>
    /// Attaches previously used files to this file stream.
    /// </summary>
    /// <param name="files">Previously attached files to reattach</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddFiles(IEnumerable<DiscordMessageFile> files);

    /// <summary>
    /// Adds an allowed mention to this message.
    /// </summary>
    /// <param name="mention">Mention to allow in this message.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddMention(IMention mention);

    /// <summary>
    /// Adds multiple allowed mentions to this message.
    /// </summary>
    /// <param name="mentions">Collection of mentions to allow in this message.</param>
    /// <returns></returns>
    IDiscordMessageBuilder AddMentions(IEnumerable<IMention> mentions);

    /// <summary>
    /// Applies <see cref="DiscordMessageFlags.SuppressNotifications"/> to the message.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// As per <see cref="DiscordMessageFlags.SuppressNotifications"/>, this does not change the message's allowed mentions
    /// (controlled by <see cref="AddMentions"/>), but instead prevents a mention from triggering a push notification.
    /// </remarks>
    IDiscordMessageBuilder SuppressNotifications();

    /// <summary>
    /// Clears all components attached to this builder.
    /// </summary>
    void ClearComponents();

    /// <summary>
    /// Clears this builder.
    /// </summary>
    void Clear();
}

/*
* Zǎoshang hǎo zhōngguó xiànzài wǒ yǒu BING CHILLING 🥶🍦
* wǒ hěn xǐhuān BING CHILLING 🥶🍦
*/
