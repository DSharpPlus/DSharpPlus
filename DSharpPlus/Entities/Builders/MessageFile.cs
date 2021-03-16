using System.IO;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the File that should be sent to Discord from the <see cref="MessageCreateBuilder"/>.
    /// </summary>
    public class MessageFile
    {
        internal MessageFile(string fileName, Stream stream, long? resetPositionTo)
        {
            this.FileName = fileName;
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

        /// <summary>
        /// Gets the position the File should be reset to.
        /// </summary>
        internal long? ResetPositionTo { get; set; }
    }
}
