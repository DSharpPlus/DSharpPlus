using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        get;
        set
        {
            if (value != null && value.Length > 2000)
            {
                throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));
            }

            SetIfV2Disabled(ref field, value, nameof(Content));
        }
    }

    public DiscordMessageFlags Flags { get; internal set; }

    public T SuppressNotifications()
    {
        this.Flags |= DiscordMessageFlags.SuppressNotifications;
        return (T)this;
    }

    /// <summary>
    /// Enables support for V2 components; messages with the V2 flag cannot be downgraded.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    public T EnableV2Components()
    {
        bool isAnyContentSet = this.Content is not null || this.Embeds is not [];

        if (isAnyContentSet)
        {
            throw new InvalidOperationException("Content and embeds are not supported with Components V2. Please call .Clear first.");
        }
        
        this.Flags |= DiscordMessageFlags.IsComponentsV2;
        return (T)this;
    }
    
    /// <summary>
    /// Disables V2 components IF this builder does not currently contain illegal components.
    /// </summary>
    /// <exception cref="InvalidOperationException">The builder contains V2 components and cannot be downgraded.</exception>
    /// <remarks>This method only disables the V2 components flag; the message originally associated with this builder cannot be downgraded, and this method only exists for convenience.</remarks>
    public T DisableV2Components()
    {
        if (this.components.Any(c => c is not DiscordActionRowComponent))
        {
            throw new InvalidOperationException
            (
                "This builder cannot contain V2 components when disabling V2 component support. Call ClearComponents() first."
            );
        }

        this.Flags &= ~DiscordMessageFlags.IsComponentsV2;
        
        return (T)this;
    }

    public bool IsTTS { get; set; }

    /// <summary>
    /// Gets or sets a poll for this message.
    /// </summary>
    public DiscordPollBuilder? Poll { get; set => SetIfV2Disabled(ref field, value, nameof(Poll)); }

    /// <summary>
    /// Embeds to send on this webhook request.
    /// </summary>
    public IReadOnlyList<DiscordEmbed> Embeds => this.embeds;
    internal List<DiscordEmbed> embeds = [];

    /// <summary>
    /// Files to send on this webhook request.
    /// </summary>
    public IReadOnlyList<DiscordMessageFile> Files => this.files;
    internal List<DiscordMessageFile> files = [];

    /// <summary>
    /// Mentions to send on this webhook request.
    /// </summary>
    public IReadOnlyList<IMention> Mentions => this.mentions;
    internal List<IMention> mentions = [];

    /// <summary>
    /// Components to send on this message.
    /// </summary>
    public IReadOnlyList<DiscordComponent> Components => this.components;
    internal List<DiscordComponent> components = [];
    
    /// <summary>
    /// Components, filtered for only action rows.
    /// </summary>
    public IReadOnlyList<DiscordActionRowComponent>? ComponentActionRows
        => this.Components?.Where(x => x is DiscordActionRowComponent).Cast<DiscordActionRowComponent>().ToList();

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
        this.Content = builder.Content;
        this.mentions.AddRange([.. builder.Mentions]);
        this.embeds.AddRange(builder.Embeds);
        this.components.AddRange(builder.Components);
        this.files.AddRange(builder.Files);
        this.IsTTS = builder.IsTTS;
        this.Poll = builder.Poll;
        this.Flags = builder.Flags;
    }

    /// <summary>
    /// Sets the content of the Message.
    /// </summary>
    /// <param name="content">The content to be set.</param>
    /// <returns>The current builder to be chained.</returns>
    public T WithContent(string content)
    {
        ThrowIfV2Enabled();
        this.Content = content;
        return (T)this;
    }

    public T AddActionRowComponent
    (
        DiscordActionRowComponent component
    )
    {
        EnsureSufficientSpaceForComponent(component);
        this.components.Add(component);

        return (T)this;
    }

    /// <summary>
    /// Adds a new action row with the given component.
    /// </summary>
    /// <param name="selectMenu">The select menu to add, if possible.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddActionRowComponent
    (
        DiscordSelectComponent selectMenu
    )
    {
        DiscordActionRowComponent component = new DiscordActionRowComponent([selectMenu]);
        
        EnsureSufficientSpaceForComponent(component);
        
        this.components.Add(component);
        return (T)this;
    }

    /// <summary>
    /// Adds buttons to the builder.
    /// </summary>
    /// <param name="buttons">The buttons to add to the message. They will automatically be chunked into separate action rows as necessary.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddActionRowComponent
    (
        params IEnumerable<DiscordButtonComponent> buttons
    )
    {
        IEnumerable<IEnumerable<DiscordButtonComponent>> components = buttons.Chunk(5);

        foreach (IEnumerable<DiscordButtonComponent> component in components)
        {
            DiscordActionRowComponent componentComponent = new(component);
            
            EnsureSufficientSpaceForComponent(componentComponent);
            this.components.Add(componentComponent);
        }
        
        return (T)this;
    }

    /// <summary>
    /// Adds a media gallery to this builder.
    /// </summary>
    /// <param name="galleryItems">The items to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddMediaGalleryComponent
    (
        params IEnumerable<DiscordMediaGalleryItem> galleryItems
    )
    {
        int itemCount = galleryItems.TryGetNonEnumeratedCount(out int fastCount) ? fastCount : galleryItems.Count();

        if (itemCount is 0)
        {
            throw new InvalidOperationException("At least one item must be added to the media gallery.");
        }

        if (itemCount > 10)
        {
            throw new InvalidOperationException("At most 10 items can be added to the media gallery.");
        }
        
        DiscordMediaGalleryComponent component = new(galleryItems);
        
        EnsureSufficientSpaceForComponent(component);
        this.components.Add(component);
        
        return (T)this;
    }

    /// <summary>
    /// Adds a section component to the builder.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddSectionComponent
    (
        DiscordSectionComponent section
    )
    {
        EnsureSufficientSpaceForComponent(section);
        this.components.Add(section);
        
        return (T)this;
    }

    /// <summary>
    /// Adds a text display to this builder.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddTextDisplayComponent
    (
        DiscordTextDisplayComponent component
    )
    {
        EnsureSufficientSpaceForComponent(component);
        this.components.Add(component);
        
        return (T)this;
    }

    /// <summary>
    /// Adds a text display to this builder.
    /// </summary>
    /// <param name="content"></param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddTextDisplayComponent
    (
        string content
    )
    {
        DiscordTextDisplayComponent component = new(content);
        
        EnsureSufficientSpaceForComponent(component);
        this.components.Add(component);
        
        return (T)this;
    }

    /// <summary>
    /// Adds a separator component to this builder.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddSeparatorComponent
    (
        DiscordSeparatorComponent component
    )
    {
        EnsureSufficientSpaceForComponent(component);
        this.components.Add(component);
        
        return (T)this;
    }

    /// <summary>
    /// Adds a file component to this builder.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddFileComponent
    (
        DiscordFileComponent component
    )
    {
        EnsureSufficientSpaceForComponent(component);
        this.components.Add(component);
        
        return (T)this;
    }

    
    /// <summary>
    /// Adds a container component to this builder.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient slots to support the component.</exception>
    public T AddContainerComponent
    (
        DiscordContainerComponent component
    )
    {
        EnsureSufficientSpaceForComponent(component);
        this.components.Add(component);
        
        return (T)this;
    }

    /// <summary>
    /// Sets if the message should be TTS.
    /// </summary>
    /// <param name="isTTS">If TTS should be set.</param>
    /// <returns>The current builder to be chained.</returns>
    public T WithTTS(bool isTTS)
    {
        this.IsTTS = isTTS;
        return (T)this;
    }

    public T WithPoll(DiscordPollBuilder poll)
    {
        ThrowIfV2Enabled();
        this.Poll = poll;
        return (T)this;
    }

    /// <summary>
    /// Appends an embed to the current builder.
    /// </summary>
    /// <param name="embed">The embed that should be appended.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddEmbed(DiscordEmbed embed)
    {
        ThrowIfV2Enabled();
        if (embed is null)
        {
            return (T)this; //Providing null embeds will produce a 400 response from Discord.//
        }

        this.embeds.Add(embed);
        return (T)this;
    }

    /// <summary>
    /// Appends several embeds to the current builder.
    /// </summary>
    /// <param name="embeds">The embeds that should be appended.</param>
    /// <returns>The current builder to be chained.</returns>
    public T AddEmbeds(IEnumerable<DiscordEmbed> embeds)
    {
        ThrowIfV2Enabled();
        this.embeds.AddRange(embeds);
        return (T)this;
    }

    /// <summary>
    /// Clears the embeds on the current builder.
    /// </summary>
    /// <returns>The current builder for chaining.</returns>
    public T ClearEmbeds()
    {
        this.embeds.Clear();
        return (T)this;
    }

    /// <summary>
    /// Removes the embed at the specified index.
    /// </summary>
    /// <returns>The current builder for chaining.</returns>
    public T RemoveEmbedAt(int index)
    {
        this.embeds.RemoveAt(index);
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
        this.embeds.RemoveRange(index, count);
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
        if (this.Files.Count >= 10)
        {
            throw new ArgumentException("Cannot send more than 10 files with a single message.");
        }

        if (this.files.Any(x => x.FileName == fileName))
        {
            throw new ArgumentException("A file with that filename already exists");
        }

        stream = ResolveStream(stream, fileOptions);
        long? resetPosition = fileOptions.HasFlag(AddFileOptions.ResetStream) ? stream.Position : null;
        this.files.Add(new DiscordMessageFile(fileName, stream, resetPosition, fileOptions: fileOptions));

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
        if (this.Files.Count + files.Count > 10)
        {
            throw new ArgumentException("Cannot send more than 10 files with a single message.");
        }

        foreach (KeyValuePair<string, Stream> file in files)
        {
            if (this.files.Any(x => x.FileName == file.Key))
            {
                throw new ArgumentException("A File with that filename already exists");
            }

            Stream stream = ResolveStream(file.Value, fileOptions);
            long? resetPosition = fileOptions.HasFlag(AddFileOptions.ResetStream) ? stream.Position : null;
            this.files.Add(new DiscordMessageFile(file.Key, stream, resetPosition, fileOptions: fileOptions));
        }

        return (T)this;
    }

    public T AddFiles(IEnumerable<DiscordMessageFile> files)
    {
        this.files.AddRange(files);
        return (T)this;
    }

    /// <summary>
    /// Adds the mention to the mentions to parse, etc. with the interaction response.
    /// </summary>
    /// <param name="mention">Mention to add.</param>
    public T AddMention(IMention mention)
    {
        this.mentions.Add(mention);
        return (T)this;
    }

    /// <summary>
    /// Adds the mentions to the mentions to parse, etc. with the interaction response.
    /// </summary>
    /// <param name="mentions">Mentions to add.</param>
    public T AddMentions(IEnumerable<IMention> mentions)
    {
        this.mentions.AddRange(mentions);
        return (T)this;
    }

    /// <summary>
    /// Clears all message components on this builder.
    /// </summary>
    public virtual void ClearComponents()
        => this.components.Clear();

    /// <summary>
    /// Allows for clearing the Message Builder so that it can be used again to send a new message.
    /// </summary>
    public virtual void Clear()
    {
        this.Content = "";
        this.embeds.Clear();
        this.IsTTS = false;
        this.mentions.Clear();
        this.files.Clear();
        this.components.Clear();
        this.Flags = default;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // We don't bother to fully implement the dispose pattern
        // since deriving from this type outside this assembly is unusual.

        foreach (DiscordMessageFile file in this.files)
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
        foreach (DiscordMessageFile file in this.files)
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
        foreach (DiscordMessageFile file in this.files)
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

    private void EnsureSufficientSpaceForComponent
    (
        DiscordComponent component
    )
    {
        int maxTopComponents = this.Flags.HasFlag(DiscordMessageFlags.IsComponentsV2) ? 10 : 5;
        int maxAllComponents = this.Flags.HasFlag(DiscordMessageFlags.IsComponentsV2) ? 30 : 25;
        
        int allComponentCount = this.Components.Sum
        (
            c =>
            {
                return c switch
                {
                    DiscordActionRowComponent arc => 1 + arc.Components.Count,
                    DiscordSectionComponent section => 2 + section.Components.Count,
                    DiscordContainerComponent container => 1 + container.Components.Sum
                    (
                      nc => nc switch
                        {
                            DiscordActionRowComponent narc => narc.Components.Count + 1,
                            DiscordSectionComponent narc => narc.Components.Count + 2,
                            _ => 1,
                        }
                    ),
                   _ => 1
                };
            }
        );

        int requiredSpaceForComponent = component switch
        {
            DiscordActionRowComponent arc => arc.Components.Count + 1, // Action row + components
            DiscordSectionComponent section => section.Components.Count + 2, // Section + Accessory
            DiscordContainerComponent container => container.Components.Sum
            (
                nc => nc switch
                {
                    DiscordActionRowComponent narc => narc.Components.Count + 1,
                    DiscordSectionComponent narc => narc.Components.Count + 2,
                    _ => 1,
                }
            ),
            _ => 1,
        };

        if (this.Components.Count + 1 >= maxTopComponents)
        {
            throw new InvalidOperationException($"Too many top-level components! Maximum allowed is {maxTopComponents}.");
        }

        if (allComponentCount + requiredSpaceForComponent > maxAllComponents)
        {
            throw new InvalidOperationException($"Too many components! Maximum allowed is {maxAllComponents}; {component.GetType().Name} requires {requiredSpaceForComponent} slots.");
        }
    }

    private void SetIfV2Disabled<TField>(ref TField field, TField value, string fieldName)
    {
        if (this.Flags.HasFlag(DiscordMessageFlags.IsComponentsV2))
        {
            throw new ArgumentException("This field cannot be set when V2 components is enabled.", fieldName);
        }

        field = value;
    }

    private void ThrowIfV2Enabled([CallerMemberName] string caller = "")
    {
        if (this.Flags.HasFlag(DiscordMessageFlags.IsComponentsV2))
        {
            throw new InvalidOperationException($"{caller} cannot be called when V2 components is enabled.");
        }
    }
    
    IDiscordMessageBuilder IDiscordMessageBuilder.AddActionRowComponent(DiscordActionRowComponent component) => this.AddActionRowComponent(component);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddActionRowComponent(DiscordSelectComponent selectMenu) => this.AddActionRowComponent(selectMenu);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddActionRowComponent(params IEnumerable<DiscordButtonComponent> buttons) => this.AddActionRowComponent(buttons);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddMediaGalleryComponent(params IEnumerable<DiscordMediaGalleryItem> galleryItems) => this.AddMediaGalleryComponent(galleryItems);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddSectionComponent(DiscordSectionComponent section) => this.AddSectionComponent(section);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddTextDisplayComponent(DiscordTextDisplayComponent component) => this.AddTextDisplayComponent(component);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddTextDisplayComponent(string content) => this.AddTextDisplayComponent(content);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddSeparatorComponent(DiscordSeparatorComponent component) => this.AddSeparatorComponent(component);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddFileComponent(DiscordFileComponent component) => this.AddFileComponent(component);
    IDiscordMessageBuilder IDiscordMessageBuilder.AddContainerComponent(DiscordContainerComponent component) => this.AddContainerComponent(component);
    
    IDiscordMessageBuilder IDiscordMessageBuilder.SuppressNotifications() => SuppressNotifications();
    IDiscordMessageBuilder IDiscordMessageBuilder.WithContent(string content) => WithContent(content);
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
    public IReadOnlyList<DiscordMessageFile> Files { get; }

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
    public IDiscordMessageBuilder AddFiles(IEnumerable<DiscordMessageFile> files);

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
}

/*
* Zǎoshang hǎo zhōngguó xiànzài wǒ yǒu BING CHILLING 🥶🍦
* wǒ hěn xǐhuān BING CHILLING 🥶🍦
*/
