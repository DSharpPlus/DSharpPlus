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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.Net;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs embeds.
    /// </summary>
    public sealed class DiscordEmbedBuilder
    {
        /// <summary>
        /// Gets or sets the embed's title.
        /// </summary>
        public string Title
        {
            get => this._title;
            set
            {
                if (value != null && value.Length > 256)
                    throw new ArgumentException("Title length cannot exceed 256 characters.", nameof(value));
                this._title = value;
            }
        }
        private string _title;

        /// <summary>
        /// Gets or sets the embed's description.
        /// </summary>
        public string Description
        {
            get => this._description;
            set
            {
                if (value != null && value.Length > 4096)
                    throw new ArgumentException("Description length cannot exceed 4096 characters.", nameof(value));
                this._description = value;
            }
        }
        private string _description;

        /// <summary>
        /// Gets or sets the url for the embed's title.
        /// </summary>
        public string Url
        {
            get => this._url?.ToString();
            set => this._url = string.IsNullOrEmpty(value) ? null : new Uri(value);
        }
        private Uri _url;

        /// <summary>
        /// Gets or sets the embed's color.
        /// </summary>
        public Optional<DiscordColor> Color { get; set; }

        /// <summary>
        /// Gets or sets the embed's timestamp.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the embed's image url.
        /// </summary>
        public string ImageUrl
        {
            get => this._imageUri?.ToString();
            set => this._imageUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        private DiscordUri _imageUri;

        /// <summary>
        /// Gets or sets the embed's author.
        /// </summary>
        public EmbedAuthor Author { get; set; }

        /// <summary>
        /// Gets or sets the embed's footer.
        /// </summary>
        public EmbedFooter Footer { get; set; }

        /// <summary>
        /// Gets or sets the embed's thumbnail.
        /// </summary>
        public EmbedThumbnail Thumbnail { get; set; }

        /// <summary>
        /// Gets the embed's fields.
        /// </summary>
        public IReadOnlyList<DiscordEmbedField> Fields { get; }
        private readonly List<DiscordEmbedField> _fields = new();

        /// <summary>
        /// Constructs a new empty embed builder.
        /// </summary>
        public DiscordEmbedBuilder()
        {
            this.Fields = new ReadOnlyCollection<DiscordEmbedField>(this._fields);
        }

        /// <summary>
        /// Constructs a new embed builder using another embed as prototype.
        /// </summary>
        /// <param name="original">Embed to use as prototype.</param>
        public DiscordEmbedBuilder(DiscordEmbed original)
            : this()
        {
            this.Title = original.Title;
            this.Description = original.Description;
            this.Url = original.Url?.ToString();
            this.ImageUrl = original.Image?.Url?.ToString();
            this.Color = original.Color;
            this.Timestamp = original.Timestamp;

            if (original.Thumbnail != null)
                this.Thumbnail = new EmbedThumbnail
                {
                    Url = original.Thumbnail.Url?.ToString(),
                    Height = original.Thumbnail.Height,
                    Width = original.Thumbnail.Width
                };

            if (original.Author != null)
                this.Author = new EmbedAuthor
                {
                    IconUrl = original.Author.IconUrl?.ToString(),
                    Name = original.Author.Name,
                    Url = original.Author.Url?.ToString()
                };

            if (original.Footer != null)
                this.Footer = new EmbedFooter
                {
                    IconUrl = original.Footer.IconUrl?.ToString(),
                    Text = original.Footer.Text
                };

            if (original.Fields?.Any() == true)
                this._fields.AddRange(original.Fields);

            while (this._fields.Count > 25)
                this._fields.RemoveAt(this._fields.Count - 1);
        }

        /// <summary>
        /// Sets the embed's title.
        /// </summary>
        /// <param name="title">Title to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTitle(string title)
        {
            this.Title = title;
            return this;
        }

        /// <summary>
        /// Sets the embed's description.
        /// </summary>
        /// <param name="description">Description to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithDescription(string description)
        {
            this.Description = description;
            return this;
        }

        /// <summary>
        /// Sets the embed's title url.
        /// </summary>
        /// <param name="url">Title url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithUrl(string url)
        {
            this.Url = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's title url.
        /// </summary>
        /// <param name="url">Title url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithUrl(Uri url)
        {
            this._url = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's color.
        /// </summary>
        /// <param name="color">Embed color to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithColor(DiscordColor color)
        {
            this.Color = color;
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp.
        /// </summary>
        /// <param name="timestamp">Timestamp to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(DateTimeOffset? timestamp)
        {
            this.Timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp.
        /// </summary>
        /// <param name="timestamp">Timestamp to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(DateTime? timestamp)
        {
            this.Timestamp = timestamp == null ? null : (DateTimeOffset?)new DateTimeOffset(timestamp.Value);
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp based on a snowflake.
        /// </summary>
        /// <param name="snowflake">Snowflake to calculate timestamp from.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(ulong snowflake)
        {
            this.Timestamp = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(snowflake >> 22);
            return this;
        }

        /// <summary>
        /// Sets the embed's image url.
        /// </summary>
        /// <param name="url">Image url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithImageUrl(string url)
        {
            this.ImageUrl = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's image url.
        /// </summary>
        /// <param name="url">Image url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithImageUrl(Uri url)
        {
            this._imageUri = new DiscordUri(url);
            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail.
        /// </summary>
        /// <param name="url">Thumbnail url to set.</param>
        /// <param name="height">The height of the thumbnail to set.</param>
        /// <param name="width">The width of the thumbnail to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnail(string url, int height = 0, int width = 0)
        {
            this.Thumbnail = new EmbedThumbnail
            {
                Url = url,
                Height = height,
                Width = width
            };

            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail.
        /// </summary>
        /// <param name="url">Thumbnail url to set.</param>
        /// <param name="height">The height of the thumbnail to set.</param>
        /// <param name="width">The width of the thumbnail to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnail(Uri url, int height = 0, int width = 0)
        {
            this.Thumbnail = new EmbedThumbnail
            {
                _uri = new DiscordUri(url),
                Height = height,
                Width = width
            };

            return this;
        }

        /// <summary>
        /// Sets the embed's author.
        /// </summary>
        /// <param name="name">Author's name.</param>
        /// <param name="url">Author's url.</param>
        /// <param name="iconUrl">Author icon's url.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithAuthor(string name = null, string url = null, string iconUrl = null)
        {
            this.Author = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url) && string.IsNullOrEmpty(iconUrl)
                ? null
                : new EmbedAuthor
                {
                    Name = name,
                    Url = url,
                    IconUrl = iconUrl
                };
            return this;
        }

        /// <summary>
        /// Sets the embed's footer.
        /// </summary>
        /// <param name="text">Footer's text.</param>
        /// <param name="iconUrl">Footer icon's url.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithFooter(string text = null, string iconUrl = null)
        {
            if (text != null && text.Length > 2048)
                throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(text));

            this.Footer = string.IsNullOrEmpty(text) && string.IsNullOrEmpty(iconUrl)
                ? null
                : new EmbedFooter
                {
                    Text = text,
                    IconUrl = iconUrl
                };
            return this;
        }

        /// <summary>
        /// Adds a field to this embed.
        /// </summary>
        /// <param name="name">Name of the field to add.</param>
        /// <param name="value">Value of the field to add.</param>
        /// <param name="inline">Whether the field is to be inline or not.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder AddField(string name, string value, bool inline = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                throw new ArgumentException("Value cannot be empty or whitespace.", nameof(value));
            }

            if (name.Length > 256)
                throw new ArgumentException("Embed field name length cannot exceed 256 characters.");
            if (value.Length > 1024)
                throw new ArgumentException("Embed field value length cannot exceed 1024 characters.");

            if (this._fields.Count >= 25)
                throw new InvalidOperationException("Cannot add more than 25 fields.");

            this._fields.Add(new DiscordEmbedField
            {
                Inline = inline,
                Name = name,
                Value = value
            });
            return this;
        }

        /// <summary>
        /// Removes a field of the specified index from this embed.
        /// </summary>
        /// <param name="index">Index of the field to remove.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder RemoveFieldAt(int index)
        {
            this._fields.RemoveAt(index);
            return this;
        }

        /// <summary>
        /// Removes fields of the specified range from this embed.
        /// </summary>
        /// <param name="index">Index of the first field to remove.</param>
        /// <param name="count">Number of fields to remove.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder RemoveFieldRange(int index, int count)
        {
            this._fields.RemoveRange(index, count);
            return this;
        }

        /// <summary>
        /// Removes all fields from this embed.
        /// </summary>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder ClearFields()
        {
            this._fields.Clear();
            return this;
        }

        /// <summary>
        /// Constructs a new embed from data supplied to this builder.
        /// </summary>
        /// <returns>New discord embed.</returns>
        public DiscordEmbed Build()
        {
            var embed = new DiscordEmbed
            {
                Title = this._title,
                Description = this._description,
                Url = this._url,
                _color = this.Color.IfPresent(e => e.Value),
                Timestamp = this.Timestamp
            };

            if (this.Footer != null)
                embed.Footer = new DiscordEmbedFooter
                {
                    Text = this.Footer.Text,
                    IconUrl = this.Footer._iconUri
                };

            if (this.Author != null)
                embed.Author = new DiscordEmbedAuthor
                {
                    Name = this.Author.Name,
                    Url = this.Author._uri,
                    IconUrl = this.Author._iconUri
                };

            if (this._imageUri != null)
                embed.Image = new DiscordEmbedImage { Url = this._imageUri };
            if (this.Thumbnail != null)
                embed.Thumbnail = new DiscordEmbedThumbnail
                {
                    Url = this.Thumbnail._uri,
                    Height = this.Thumbnail.Height,
                    Width = this.Thumbnail.Width
                };

            embed.Fields = new ReadOnlyCollection<DiscordEmbedField>(new List<DiscordEmbedField>(this._fields)); // copy the list, don't wrap it, prevents mutation

            return embed;
        }

        /// <summary>
        /// Implicitly converts this builder to an embed.
        /// </summary>
        /// <param name="builder">Builder to convert.</param>
        public static implicit operator DiscordEmbed(DiscordEmbedBuilder builder)
            => builder?.Build();

        /// <summary>
        /// Represents an embed author.
        /// </summary>
        public class EmbedAuthor
        {
            /// <summary>
            /// Gets or sets the name of the author.
            /// </summary>
            public string Name
            {
                get => this._name;
                set
                {
                    if (value != null && value.Length > 256)
                        throw new ArgumentException("Author name length cannot exceed 256 characters.", nameof(value));
                    this._name = value;
                }
            }
            private string _name;

            /// <summary>
            /// Gets or sets the Url to which the author's link leads.
            /// </summary>
            public string Url
            {
                get => this._uri?.ToString();
                set => this._uri = string.IsNullOrEmpty(value) ? null : new Uri(value);
            }
            internal Uri _uri;

            /// <summary>
            /// Gets or sets the Author's icon url.
            /// </summary>
            public string IconUrl
            {
                get => this._iconUri?.ToString();
                set => this._iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
            }
            internal DiscordUri _iconUri;
        }

        /// <summary>
        /// Represents an embed footer.
        /// </summary>
        public class EmbedFooter
        {
            /// <summary>
            /// Gets or sets the text of the footer.
            /// </summary>
            public string Text
            {
                get => this._text;
                set
                {
                    if (value != null && value.Length > 2048)
                        throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(value));
                    this._text = value;
                }
            }
            private string _text;

            /// <summary>
            /// Gets or sets the Url
            /// </summary>
            public string IconUrl
            {
                get => this._iconUri?.ToString();
                set => this._iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
            }
            internal DiscordUri _iconUri;
        }

        /// <summary>
        /// Represents an embed thumbnail.
        /// </summary>
        public class EmbedThumbnail
        {
            /// <summary>
            /// Gets or sets the thumbnail's image url.
            /// </summary>
            public string Url
            {
                get => this._uri?.ToString();
                set => this._uri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
            }
            internal DiscordUri _uri;

            /// <summary>
            /// Gets or sets the thumbnail's height.
            /// </summary>
            public int Height
            {
                get => this._height;
                set => this._height = value >= 0 ? value : 0;
            }
            private int _height;

            /// <summary>
            /// Gets or sets the thumbnail's width.
            /// </summary>
            public int Width
            {
                get => this._width;
                set => this._width = value >= 0 ? value : 0;
            }
            private int _width;
        }
    }
}
