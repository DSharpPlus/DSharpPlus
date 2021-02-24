using System.IO;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the File that should be sent to Discord from the <see cref="DiscordMessageBuilder"/>.
    /// </summary>
    public class DiscordMessageFile
    {
        internal DiscordMessageFile(string fileName, Stream stream, bool isDisposedInternally)
        {
            this.FileName = fileName;
            this.Stream = stream;
            this.IsDisposedInternally = isDisposedInternally;
        }

        /// <summary>
        /// Gets the FileName of the File.
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// Gets the stream of the File.
        /// </summary>
        public Stream Stream { get; internal set; }

        /// <summary>
        /// Gets if the stream should be disposed by the library.
        /// </summary>
        internal bool IsDisposedInternally { get; set; }
    }
}
