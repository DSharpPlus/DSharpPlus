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
            get => _title;
            set
            {
                if (value != null && value.Length > 256)
                {
                    throw new ArgumentException("Title length cannot exceed 256 characters.", nameof(value));
                }

                _title = value;
            }
        }
        private string _title;

        /// <summary>
        /// Gets or sets the embed's description.
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (value != null && value.Length > 2048)
                {
                    throw new ArgumentException("Description length cannot exceed 2048 characters.", nameof(value));
                }

                _description = value;
            }
        }
        private string _description;

        /// <summary>
        /// Gets or sets the url for the embed's title.
        /// </summary>
        public string Url
        {
            get => _url?.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _url = null;
                }
                else
                {
                    _url = new Uri(value);
                }
            }
        }
        private Uri _url;

        /// <summary>
        /// Gets or sets the embed's color.
        /// </summary>
        public DiscordColor Color
        {
            get => _color;
            set => _color = value;
        }
        private DiscordColor _color;

        /// <summary>
        /// Gets or sets the embed's timestamp.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get => _timestamp;
            set => _timestamp = value;
        }
        private DateTimeOffset? _timestamp;

        /// <summary>
        /// Gets or sets the embed's image url.
        /// </summary>
        public string ImageUrl
        {
            get => _imageUri?.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _imageUri = null;
                }
                else
                {
                    _imageUri = new Uri(value);
                }
            }
        }
        private Uri _imageUri;

        /// <summary>
        /// Gets or sets the thumbnail's image url.
        /// </summary>
        public string ThumbnailUrl
        {
            get => _thumbnailUri?.ToString();
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _thumbnailUri = null;
                }
                else
                {
                    _thumbnailUri = new Uri(value);
                }
            }
        }
        private Uri _thumbnailUri;

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
            Fields = new ReadOnlyCollection<DiscordEmbedField>(_fields);
        }

        /// <summary>
        /// Constructs a new embed builder using another embed as prototype.
        /// </summary>
        /// <param name="original">Embed to use as prototype.</param>
        public DiscordEmbedBuilder(DiscordEmbed original)
        {
            Title = original.Title;
            Description = original.Description;
            Url = original.Url?.ToString();
            Color = original.Color;
            Timestamp = original.Timestamp;

            if (original.Author != null)
            {
                Author = new EmbedAuthor
                {
                    IconUrl = original.Author.IconUrl?.ToString(),
                    Name = original.Author.Name,
                    Url = original.Author.Url?.ToString()
                };
            }

            if (original.Footer != null)
            {
                Footer = new EmbedFooter
                {
                    IconUrl = original.Footer.IconUrl?.ToString(),
                    Text = original.Footer.Text
                };
            }

            _fields = original.Fields?.ToList() ?? new List<DiscordEmbedField>();
            Fields = new ReadOnlyCollection<DiscordEmbedField>(_fields);

            while (_fields.Count > 25)
            {
                _fields.RemoveAt(_fields.Count - 1);
            }
        }

        /// <summary>
        /// Sets the embed's title.
        /// </summary>
        /// <param name="title">Title to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Sets the embed's description.
        /// </summary>
        /// <param name="description">Description to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Sets the embed's title url.
        /// </summary>
        /// <param name="url">Title url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithUrl(string url)
        {
            Url = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's title url.
        /// </summary>
        /// <param name="url">Title url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithUrl(Uri url)
        {
            _url = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's color.
        /// </summary>
        /// <param name="color">Embed color to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithColor(DiscordColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp.
        /// </summary>
        /// <param name="timestamp">Timestamp to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(DateTimeOffset? timestamp)
        {
            Timestamp = timestamp;
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
            {
                Timestamp = null;
            }
            else
            {
                Timestamp = new DateTimeOffset(timestamp.Value);
            }
            return this;
        }

        /// <summary>
        /// Sets the embed's timestamp based on a snowflake.
        /// </summary>
        /// <param name="snowflake">Snowflake to calculate timestamp from.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithTimestamp(ulong snowflake)
        {
            Timestamp = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(snowflake >> 22);
            return this;
        }

        /// <summary>
        /// Sets the embed's image url.
        /// </summary>
        /// <param name="url">Image url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithImageUrl(string url)
        {
            ImageUrl = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's image url.
        /// </summary>
        /// <param name="url">Image url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithImageUrl(Uri url)
        {
            _imageUri = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail url.
        /// </summary>
        /// <param name="url">Thumbnail url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnailUrl(string url)
        {
            ThumbnailUrl = url;
            return this;
        }

        /// <summary>
        /// Sets the embed's thumbnail url.
        /// </summary>
        /// <param name="url">Thumbnail url to set.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithThumbnailUrl(Uri url)
        {
            _thumbnailUri = url;
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
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url) && string.IsNullOrEmpty(iconUrl))
            {
                Author = null;
            }
            else
            {
                Author = new EmbedAuthor
                {
                    Name = name,
                    Url = url,
                    IconUrl = iconUrl
                };
            }
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
        /// <param name="iconUrl">Footer icon's url.</param>
        /// <returns>This embed builder.</returns>
        public DiscordEmbedBuilder WithFooter(string text = null, string iconUrl = null)
        {
            if (text != null && text.Length > 2048)
            {
                throw new ArgumentException("Footer text length cannot exceed 2048 characters.");
            }

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(iconUrl))
            {
                Footer = null;
            }
            else
            {
                Footer = new EmbedFooter
                {
                    Text = text,
                    IconUrl = iconUrl
                };
            }
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
                {
                    throw new ArgumentNullException(nameof(name));
                }

                throw new ArgumentException($"{nameof(name)} cannot be empty or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                throw new ArgumentException($"{nameof(value)} cannot be empty or whitespace.");
            }

            if (name.Length > 256)
            {
                throw new ArgumentException("Embed field name length cannot exceed 256 characters.");
            }
            if (value.Length > 1024)
            {
                throw new ArgumentException("Embed field value length cannot exceed 1024 characters.");
            }

            if (_fields.Count >= 25)
            {
                throw new InvalidOperationException("Cannot add more than 25 fields.");
            }

            _fields.Add(new DiscordEmbedField
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
            _fields.Clear();
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
                Title = _title,
                Description = _description,
                Url = _url,
                _color = _color.Value,
                Timestamp = _timestamp
            };

            if (Footer != null)
            {
                embed.Footer = new DiscordEmbedFooter
                {
                    Text = Footer.Text,
                    IconUrl = Footer.IconUrl != null ? new Uri(Footer.IconUrl) : null
                };
            }

            if (Author != null)
            {
                embed.Author = new DiscordEmbedAuthor
                {
                    Name = Author.Name,
                    Url = Author.Url != null ? new Uri(Author.Url) : null,
                    IconUrl = Author.IconUrl != null ? new Uri(Author.IconUrl) : null
                };
            }

            if (_imageUri != null)
            {
                embed.Image = new DiscordEmbedImage { Url = _imageUri };
            }
            if (_thumbnailUri != null)
            {
                embed.Thumbnail = new DiscordEmbedThumbnail { Url = _thumbnailUri };
            }

            if (_fields.Any())
            {
                embed.Fields = new ReadOnlyCollection<DiscordEmbedField>(new List<DiscordEmbedField>(_fields)); // copy the list, don't wrap it, prevents mutation
            }

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
                get => _name;
                set
                {
                    if (value != null && value.Length > 256)
                    {
                        throw new ArgumentException("Author name length cannot exceed 256 characters.");
                    }

                    _name = value;
                }
            }
            private string _name;

            /// <summary>
            /// Gets or sets the Url to which the author's link leads.
            /// </summary>
            public string Url
            {
                get => _uri?.ToString();
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        _uri = null;
                    }
                    else
                    {
                        _uri = new Uri(value);
                    }
                }
            }
            private Uri _uri;

            /// <summary>
            /// Gets or sets the Author's icon url.
            /// </summary>
            public string IconUrl
            {
                get => _iconUri?.ToString();
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        _iconUri = null;
                    }
                    else
                    {
                        _iconUri = new Uri(value);
                    }
                }
            }
            private Uri _iconUri;
        }

        public class EmbedFooter
        {
            /// <summary>
            /// Gets or sets the text of the footer.
            /// </summary>
            public string Text
            {
                get => _text;
                set
                {
                    if (value != null && value.Length > 2048)
                    {
                        throw new ArgumentException("Footer text length cannot exceed 2048 characters.");
                    }

                    _text = value;
                }
            }
            private string _text;

            /// <summary>
            /// Gets or sets the Url 
            /// </summary>
            public string IconUrl
            {
                get => _iconUri?.ToString();
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        _iconUri = null;
                    }
                    else
                    {
                        _iconUri = new Uri(value);
                    }
                }
            }
            private Uri _iconUri;
        }
    }
}
