using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs a Message to be sent.
    /// </summary>
    public sealed class DiscordMessageBuilder
    {
        /// <summary>
        /// Gets the Message to be sent.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the Embed to be sent.
        /// </summary>
        public DiscordEmbed Embed { get; private set; }

        /// <summary>
        /// Gets if the message should be TTS.
        /// </summary>
        public bool IsTTS { get; private set; } = false;

        /// <summary>
        /// Gets the Allowed Mentions for the message to be sent. 
        /// </summary>
        public IEnumerable<IMention> Mentions { get; private set; }

        /// <summary>
        /// Gets the Files to be sent in the Message.
        /// </summary>
        public Dictionary<string, Stream> Files { get; private set; } = new Dictionary<string, Stream>();

        /// <summary>
        /// Sets the Content of the Message.
        /// </summary>
        /// <param name="content">The content to be set.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithContent(string content)
        {
            this.Content = content;
            return this;
        } 

        /// <summary>
        /// Sets if the message should be TTS.
        /// </summary>
        /// <param name="isTTS">If TTS should be set.</param>
        /// <returns></returns>
        public DiscordMessageBuilder HasTTS(bool isTTS)
        {
            this.IsTTS = isTTS;
            return this;
        }

        /// <summary>
        /// Sets if the message will have an Embed.
        /// </summary>
        /// <param name="embed">The embed that should be sent.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithEmbed(DiscordEmbed embed)
        {
            this.Embed = embed;
            return this;
        }

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMentions">The allowed Mentions that should be sent.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithAllowedMentions(IEnumerable<IMention> allowedMentions)
        {
            this.Mentions = allowedMentions;
            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="files">The Files that should be sent.</param>
        /// <returns></returns>
        public DiscordMessageBuilder WithFiles(Dictionary<string, Stream> files)
        {
            this.Files = files;
            return this;
        }

        /// <summary>
        /// Sends the Message to a specific channel
        /// </summary>
        /// <param name="channel">The channel the message should be sent to.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendAsync(DiscordChannel channel)
        {
            return await channel.SendMessageAsync(this);
        }

        /// <summary>
        /// Sends the modified message.
        /// </summary>
        /// <param name="msg">The original Message to modify.</param>
        /// <returns></returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordMessage msg)
        {
            return await msg.ModifyAsync(this);
        }
    }
}
