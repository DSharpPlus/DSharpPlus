using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Constructs embeds.
    /// </summary>
    public class DiscordEmbedBuilder
    {
        /// <summary>
        /// Gets or sets the embed's title.
        /// </summary>
        public string Title
        {
            get { return this._title; }
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
            get { return this._description; }
            set
            {
                if (value != null && value.Length > 2048)
                    throw new ArgumentException("Description length cannot exceed 2048 characters.", nameof(value));
                this._description = value;
            }
        }
        private string _description;

        /// <summary>
        /// Gets or sets the url for the embed's title.
        /// </summary>
        public string Url
        {
            get { return this._url?.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this._url = null;
                else
                    this._url = new Uri(value);
            }
        }
        private Uri _url;

        /// <summary>
        /// Gets or sets the embed's color.
        /// </summary>
        public DiscordColor Color
        {
            get { return this._color; }
            set { this._color = value; }
        }
        private DiscordColor _color;

        /// <summary>
        /// Gets or sets the embed's timestamp.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get { return this._timestamp; }
            set { this._timestamp = value; }
        }
        private DateTimeOffset? _timestamp;

        /// <summary>
        /// Gets or sets the embed's image url.
        /// </summary>
        public string ImageUrl
        {
            get { return this._image_uri?.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this._image_uri = null;
                else
                    this._image_uri = new Uri(value);
            }
        }
        private Uri _image_uri;

        /// <summary>
        /// Gets or sets the thumbnail's image url.
        /// </summary>
        public string ThumbnailUrl
        {
            get { return this._thumbnail_uri?.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this._thumbnail_uri = null;
                else
                    this._thumbnail_uri = new Uri(value);
            }
        }
        private Uri _thumbnail_uri;

        /// <summary>
        /// Gets the embed's author.
        /// </summary>
        public EmbedAuthor Author { get; set; }

        /// <summary>
        /// Gets the embed's footer.
        /// </summary>
        public EmbedFooter Footer { get; set; }

        /// <summary>
        /// Gets the embed's fields.
        /// </summary>
        public IReadOnlyList<DiscordEmbedField> Fields { get; }
        private List<DiscordEmbedField> _fields;

        /// <summary>
        /// Constructs a new empty embed builder.
        /// </summary>
        public DiscordEmbedBuilder()
        {
            _fields = new List<DiscordEmbedField>();
            this.Fields = new ReadOnlyCollection<DiscordEmbedField>(this._fields);
        }

        /// <summary>
        /// Constructs a new embed builder using another embed as prototype.
        /// </summary>
        /// <param name="original">Embed to use as prototype.</param>
        public DiscordEmbedBuilder(DiscordEmbed original)
        {
            this.Title = original.Title;
            this.Description = original.Description;
            this.Url = original.Url?.ToString();
            this.Color = original.Color;
            this.Timestamp = original.Timestamp;

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

            this._fields = original.Fields?.ToList() ?? new List<DiscordEmbedField>();
            this.Fields = new ReadOnlyCollection<DiscordEmbedField>(this._fields);

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
            if (timestamp == null)
                this.Timestamp = null;
            else
                this.Timestamp = new DateTimeOffset(timestamp.Value);
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
            this._image_uri = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail url.
        /// </summary>
        /// <param name="url">Thumbnail url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnailUrl(string url)
        {
            this.ThumbnailUrl = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail url.
        /// </summary>
        /// <param name="url">Thumbnail url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnailUrl(Uri url)
        {
            this._thumbnail_uri = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's author.
        /// </summary>
        /// <param name="name">Author's name.</param>
        /// <param name="url">Author's url.</param>
        /// <param name="icon_url">Author icon's url.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithAuthor(string name = null, string url = null, string icon_url = null)
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url) && string.IsNullOrEmpty(icon_url))
                this.Author = null;
            else
                this.Author = new EmbedAuthor
                {
                    Name = name,
                    Url = url,
                    IconUrl = icon_url
                };
            return this;
        }

        /* Disabled for the time being, cause ambiguous calls.
         * /// <summary>
         * /// Sets the embed's author.
         * /// </summary>
         * /// <param name="name">Author's name.</param>
         * /// <param name="url">Author's url.</param>
         * /// <param name="icon_url">Author icon's url.</param>
         * /// <returns>This embed builder.</returns>
         * public DiscordEmbedBuilder WithAuthor(string name = null, Uri url = null, Uri icon_url = null)
         * {
         *     if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url) && string.IsNullOrEmpty(icon_url))
         *         this.Author = null;
         *     else
         *         this.Author = new DiscordEmbedAuthor
         *         {
         *             Name = name,
         *             Url = url,
         *             IconUrl = icon_url
         *         };
         *     return this;
         * }
         */

        /// <summary>
        /// Sets the embed's footer.
        /// </summary>
        /// <param name="text">Footer's text.</param>
        /// <param name="icon_url">Footer icon's url.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithFooter(string text = null, string icon_url = null)
        {
            if (text != null && text.Length > 2048)
                throw new ArgumentException("Footer text length cannot exceed 2048 characters.");

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(icon_url))
                this.Footer = null;
            else
                this.Footer = new EmbedFooter
                {
                    Text = text,
                    IconUrl = icon_url
                };
            return this;
        }

        /* Disabled for the time being, cause ambiguous calls.
         * /// <summary>
         * /// Sets the embed's footer.
         * /// </summary>
         * /// <param name="text">Footer's text.</param>
         * /// <param name="icon_url">Footer icon's url.</param>
         * /// <returns>This embed builder.</returns>
         * public DiscordEmbedBuilder WithFooter(string text = null, Uri icon_url = null)
         * {
         *     if (text != null && text.Length > 2048)
         *         throw new ArgumentException("Footer text cannot exceed 2048 characters of length.");
         * 
         *     if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(icon_url))
         *         this.Footer = null;
         *     else
         *         this.Footer = new DiscordEmbedFooter
         *         {
         *             Text = text,
         *             IconUrl = icon_url
         *         };
         *     return this;
         * }
         */

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
                else
                    throw new ArgumentException($"{nameof(name)} cannot be empty or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                else
                    throw new ArgumentException($"{nameof(value)} cannot be empty or whitespace.");
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
                _color = this._color.Value,
                Timestamp = this._timestamp
            };

            if (this.Footer != null)
                embed.Footer = new DiscordEmbedFooter
                {
                    Text = this.Footer.Text,
                    IconUrl = this.Footer.IconUrl != null ? new Uri(this.Footer.IconUrl) : null
                };

            if (this.Author != null)
                embed.Author = new DiscordEmbedAuthor
                {
                    Name = this.Author.Name,
                    Url = this.Author.Url != null ? new Uri(this.Author.Url) : null,
                    IconUrl = this.Author.IconUrl != null ? new Uri(this.Author.IconUrl) : null
                };

            if (this._image_uri != null)
                embed.Image = new DiscordEmbedImage { Url = this._image_uri };
            if (this._thumbnail_uri != null)
                embed.Thumbnail = new DiscordEmbedThumbnail { Url = this._thumbnail_uri };

            if (this._fields.Any())
                embed.Fields = new ReadOnlyCollection<DiscordEmbedField>(new List<DiscordEmbedField>(this._fields)); // copy the list, don't wrap it, prevents mutation

            return embed;
        }

        /// <summary>
        /// Implicitly converts this builder to an embed.
        /// </summary>
        /// <param name="builder">Builder to convert.</param>
        public static implicit operator DiscordEmbed(DiscordEmbedBuilder builder) =>
            builder?.Build();

        /// <summary>
        /// Implicitly converts this builder to an optional embed.
        /// </summary>
        /// <param name="builder">Builder to convert.</param>
        public static implicit operator Optional<DiscordEmbed>(DiscordEmbedBuilder builder) =>
            builder?.Build();

        public class EmbedAuthor
        {
            /// <summary>
            /// Gets or sets the name of the author.
            /// </summary>
            public string Name
            {
                get { return this._name; }
                set
                {
                    if (value != null && value.Length > 256)
                        throw new ArgumentException("Author name length cannot exceed 256 characters.");
                    this._name = value;
                }
            }
            private string _name;

            /// <summary>
            /// Gets or sets the Url to which the author's link leads.
            /// </summary>
            public string Url
            {
                get { return this._uri?.ToString(); }
                set
                {
                    if (string.IsNullOrEmpty(value))
                        this._uri = null;
                    else
                        this._uri = new Uri(value);
                }
            }
            private Uri _uri;

            /// <summary>
            /// Gets or sets the Author's icon url.
            /// </summary>
            public string IconUrl
            {
                get { return this._icon_uri?.ToString(); }
                set
                {
                    if (string.IsNullOrEmpty(value))
                        this._icon_uri = null;
                    else
                        this._icon_uri = new Uri(value);
                }
            }
            private Uri _icon_uri;
        }

        public class EmbedFooter
        {
            /// <summary>
            /// Gets or sets the text of the footer.
            /// </summary>
            public string Text
            {
                get { return this._text; }
                set
                {
                    if (value != null && value.Length > 2048)
                        throw new ArgumentException("Footer text length cannot exceed 2048 characters.");
                    this._text = value;
                }
            }
            private string _text;

            /// <summary>
            /// Gets or sets the Url 
            /// </summary>
            public string IconUrl
            {
                get { return this._icon_uri?.ToString(); }
                set
                {
                    if (string.IsNullOrEmpty(value))
                        this._icon_uri = null;
                    else
                        this._icon_uri = new Uri(value);
                }
            }
            private Uri _icon_uri;
        }
    }
}
