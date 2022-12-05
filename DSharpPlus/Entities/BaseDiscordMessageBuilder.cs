// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.IO;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Interface that provides abstractions for the various message builder types in DSharpPlus,
    /// allowing re-use of code.
    /// </summary>
    public abstract class BaseDiscordMessageBuilder<T> : IDiscordMessageBuilder where T : BaseDiscordMessageBuilder<T>
        // This has got to be the most big brain thing I have ever done with interfaces lmfao
    {
        public abstract string Content { get; set; }
        public abstract bool IsTTS { get; set; }
        public abstract IReadOnlyList<DiscordEmbed> Embeds { get; }
        public abstract IReadOnlyList<DiscordMessageFile> Files { get; }
        public abstract IReadOnlyList<DiscordActionRowComponent> Components { get; }
        public abstract IReadOnlyList<IMention> Mentions { get; }

        /// <summary>
        /// You shall not pass ⚡
        /// </summary>
        internal BaseDiscordMessageBuilder(){}

        /// <summary>
        /// Adds content to this message
        /// </summary>
        /// <param name="content">Message content to use</param>
        /// <returns></returns>
        public abstract T WithContent(string content);

        /// <summary>
        /// Adds components to this message. Each call should append to a new row.
        /// </summary>
        /// <param name="components">Components to add.</param>
        /// <returns></returns>
        public abstract T AddComponents(params DiscordComponent[] components);

        /// <summary>
        /// Adds components to this message. Each call should append to a new row.
        /// </summary>
        /// <param name="components">Components to add.</param>
        /// <returns></returns>
        public abstract T AddComponents(IEnumerable<DiscordComponent> components);

        /// <summary>
        /// Adds an action row component to this message.
        /// </summary>
        /// <param name="components">Action row to add to this message. Should contain child components.</param>
        /// <returns></returns>
        public abstract T AddComponents(IEnumerable<DiscordActionRowComponent> components);

        /// <summary>
        /// Sets whether this message should play as a text-to-speech message.
        /// </summary>
        /// <param name="isTTS"></param>
        /// <returns></returns>
        public abstract T WithTTS(bool isTTS);

        /// <summary>
        /// Adds an embed to this message.
        /// </summary>
        /// <param name="embed">Embed to add.</param>
        /// <returns></returns>
        public abstract T AddEmbed(DiscordEmbed embed);

        /// <summary>
        /// Adds multiple embeds to this message.
        /// </summary>
        /// <param name="embeds">Collection of embeds to add.</param>
        /// <returns></returns>
        public abstract T AddEmbeds(IEnumerable<DiscordEmbed> embeds);

        /// <summary>
        /// Attaches a file to this message.
        /// </summary>
        /// <param name="fileName">Name of the file to attach.</param>
        /// <param name="stream">Stream containing said file's contents.</param>
        /// <param name="resetStream">Whether to reset the stream to position 0 after sending.</param>
        /// <returns></returns>
        public abstract T AddFile(string fileName, Stream stream, bool resetStream = false);

        /// <summary>
        /// Attaches a file to this message.
        /// </summary>
        /// <param name="stream">FileStream pointiong to the file to attach.</param>
        /// <param name="resetStream">Whether to reset the stream position to 0 after sending.</param>
        /// <returns></returns>
        public abstract T AddFile(FileStream stream, bool resetStream = false);

        /// <summary>
        /// Attaches multiple files to this message.
        /// </summary>
        /// <param name="files">Dictionary of files to add, where <see cref="string"/> is a file name and <see cref="Stream"/> is a stream containing the file's contents.</param>
        /// <param name="resetStreams">Whether to reset all stream positions to 0 after sending.</param>
        /// <returns></returns>
        public abstract T AddFiles(IDictionary<string, Stream> files, bool resetStreams = false);

        /// <summary>
        /// Adds an allowed mention to this message.
        /// </summary>
        /// <param name="mention">Mention to allow in this message.</param>
        /// <returns></returns>
        public abstract T AddMention(IMention mention);

        /// <summary>
        /// Adds multiple allowed mentions to this message.
        /// </summary>
        /// <param name="mentions">Collection of mentions to allow in this message.</param>
        /// <returns></returns>
        public abstract T AddMentions(IEnumerable<IMention> mentions);

        /// <summary>
        /// Clears all components attached to this builder.
        /// </summary>
        public abstract void ClearComponents();

        /// <summary>
        /// Clears this builder.
        /// </summary>
        public abstract void Clear();


        IDiscordMessageBuilder IDiscordMessageBuilder.WithContent(string content) => this.WithContent(content);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddComponents(params DiscordComponent[] components) => this.AddComponents(components);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddComponents(IEnumerable<DiscordComponent> components) => this.AddComponents(components);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddComponents(IEnumerable<DiscordActionRowComponent> components) => this.AddComponents(components);
        IDiscordMessageBuilder IDiscordMessageBuilder.WithTTS(bool isTTS) => this.WithTTS(isTTS);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddEmbed(DiscordEmbed embed) => this.AddEmbed(embed);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddEmbeds(IEnumerable<DiscordEmbed> embeds) => this.AddEmbeds(embeds);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddFile(string fileName, Stream stream, bool resetStream) => this.AddFile(fileName, stream, resetStream);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddFile(FileStream stream, bool resetStream) => this.AddFile(stream, resetStream);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddFiles(IDictionary<string, Stream> files, bool resetStreams) => this.AddFiles(files, resetStreams);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddMention(IMention mention) => this.AddMention(mention);
        IDiscordMessageBuilder IDiscordMessageBuilder.AddMentions(IEnumerable<IMention> mentions) => this.AddMentions(mentions);
    }

    public interface IDiscordMessageBuilder
    {
        /// <summary>
        /// Getter / setter for message content.
        /// </summary>
        string Content { get; set; }

        /// <summary>
        /// Whether this message will play as a text-to-speech message.
        /// </summary>
        bool IsTTS { get; set; }

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
        /// <param name="stream">FileStream pointiong to the file to attach.</param>
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
        /// Clears all components attached to this builder.
        /// </summary>
        void ClearComponents();

        /// <summary>
        /// Clears this builder.
        /// </summary>
        void Clear();
    }
}

/*
 * Zǎoshang hǎo zhōngguó xiànzài wǒ yǒu BING CHILLING 🥶🍦
 * wǒ hěn xǐhuān BING CHILLING 🥶🍦
 */
