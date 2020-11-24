using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    public sealed class DiscordMessageBuilder
    {
        /// <summary>
        /// Gets the Message to be sent.
        /// </summary>
        public string Content { get => _content; private set => _content = value; }

        private string _content;

        /// <summary>
        /// Gets the Embed to be sent.
        /// </summary>
        public DiscordEmbedBuilder Embed { get => _embed; private set => _embed = value; }

        private DiscordEmbedBuilder _embed;

        /// <summary>
        /// Gets if the message should be TTS.
        /// </summary>
        public bool IsTTS { get => _isTTS; private set => _isTTS = value; }

        private bool _isTTS = false;

        /// <summary>
        /// Gets the Allowed Mentions for the message to be sent. 
        /// </summary>
        public IEnumerable<IMention> Mentions { get => _mentions; private set => _mentions = value; }

        private IEnumerable<IMention> _mentions;

        /// <summary>
        /// Gets the Files to be sent in the Message.
        /// </summary>
        public Dictionary<string, Stream> Files { get => _files; private set => _files = value; }

        private Dictionary<string, Stream> _files;

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
        /// <param name="embedBuilder"></param>
        /// <returns></returns>
        public DiscordMessageBuilder WithEmbed(DiscordEmbedBuilder embedBuilder)
        {
            this.Embed = embedBuilder;
            return this;
        }

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="allowedMentions"></param>
        /// <returns></returns>
        public DiscordMessageBuilder HasAllowedMentions(IEnumerable<IMention> allowedMentions)
        {
            this.Mentions = allowedMentions;
            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public DiscordMessageBuilder WithFiles(Dictionary<string, Stream> files)
        {
            this.Files = files;
            return this;
        }

        /// <summary>
        /// Sends the Message to a specific channel
        /// </summary>
        /// <param name="channel">The c</param>
        /// <returns></returns>
        public async Task SendMessageToChannel(DiscordChannel channel)
        {
            await channel.SendMessageAsync(this);
        }

    }
}
