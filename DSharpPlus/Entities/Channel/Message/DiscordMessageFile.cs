using System.IO;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the File that should be sent to Discord from the <see cref="DiscordMessageBuilder"/>.
    /// </summary>
    public class DiscordMessageFile
    {
        internal DiscordMessageFile(string fileName, Stream stream, long? resetPositionTo, string fileType = null, string contentType = null)
        {
            this.FileName = fileName ?? "file";
            this.FileType = fileType;
            this.ContentType = contentType;
            this.Stream = stream;
            this.ResetPositionTo = resetPositionTo;
        }

        /// <summary>
        /// Gets the FileName of the File.
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// Gets the stream of the File.
        /// </summary>
        public Stream Stream { get; internal set; }

        internal string FileType { get; set; }

        internal string ContentType { get; set; }

        /// <summary>
        /// Gets the position the File should be reset to.
        /// </summary>
        internal long? ResetPositionTo { get; set; }
    }
}
