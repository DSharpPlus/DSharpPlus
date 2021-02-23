using System.IO;

namespace DSharpPlus.Entities
{
    public struct DiscordMessageFile
    {
        public DiscordMessageFile(string fileName, Stream stream, bool wasUserStream)
        {
            this.FileName = fileName;
            this.Stream = stream;
            this.WasUserStream = wasUserStream;
        }

        public string FileName { get; set; }
        public Stream Stream { get; set; }
        internal bool WasUserStream { get; set; }
    }
}
