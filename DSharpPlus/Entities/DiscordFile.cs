using System.IO;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a file to be sent to Discord.
/// </summary>
public record struct DiscordFile
{
    public DiscordFile
    (
        string fileName,
        Stream stream,
        long? resetPositionTo = null,
        string? fileType = null,
        string? contentType = null,
        AddFileOptions fileOptions = AddFileOptions.None,
        bool streamDisposedByBuilder = false
    )
    {
        this.FileName = fileName ?? "file";
        this.FileType = fileType;
        this.ContentType = contentType;
        this.FileOptions = fileOptions;
        this.Stream = stream;
        this.ResetPositionTo = resetPositionTo;
        this.StreamDisposedByBuilder = streamDisposedByBuilder;
    }

    /// <summary>
    /// Gets the FileName of the File.
    /// </summary>
    public string FileName { get; internal set; }

    /// <summary>
    /// Gets the stream of the File.
    /// </summary>
    public Stream Stream { get; internal set; }

    internal string? FileType { get; set; }

    internal string? ContentType { get; set; }

    /// <summary>
    /// Gets the position the File should be reset to.
    /// </summary>
    internal long? ResetPositionTo { get; set; }
    internal AddFileOptions FileOptions { get; set; }
    /// <summary>
    /// Indicates if the stream should be disposed when used in a request or if a builder handles the disposal
    /// </summary>
    /// <remarks>This has only any effect if <see cref="AddFileOptions.CloseStream"/> is present</remarks>
    internal bool StreamDisposedByBuilder { get; set; }
}
