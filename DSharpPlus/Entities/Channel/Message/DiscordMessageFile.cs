
using System.IO;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents files that should be sent to Discord as part of a <seealso cref="DiscordMessageBuilder"/>.
/// </summary>
public record struct DiscordMessageFile
{
    public DiscordMessageFile
    (
        string fileName,
        Stream stream,
        long? resetPositionTo = null,
        string? fileType = null,
        string? contentType = null,
        AddFileOptions fileOptions = AddFileOptions.None
    )
    {
        FileName = fileName ?? "file";
        FileType = fileType;
        ContentType = contentType;
        FileOptions = fileOptions;
        Stream = stream;
        ResetPositionTo = resetPositionTo;
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
}
