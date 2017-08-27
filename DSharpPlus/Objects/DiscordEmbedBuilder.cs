﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
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
                    throw new ArgumentException("Title length cannot exceed 256 characters of length.", nameof(value));
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
                    throw new ArgumentException("Description length cannot exceed 2048 characters of length.", nameof(value));
                this._description = value;
            }
        }
        private string _description;

        /// <summary>
        /// Gets or sets the url for the embed's title.
        /// </summary>
        public string Url
        {
            get { return this._url.ToString(); }
            set
            {
                if (value == null)
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
            get { return this._image_uri.ToString(); }
            set
            {
                if (value == null)
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
            get { return this._thumbnail_uri.ToString(); }
            set
            {
                if (value == null)
                    this._thumbnail_uri = null;
                else
                    this._thumbnail_uri = new Uri(value);
            }
        }
        private Uri _thumbnail_uri;

        /// <summary>
        /// Gets the embed's author.
        /// </summary>
        public DiscordEmbedAuthor Author { get; internal set; }

        /// <summary>
        /// Gets the embed's footer.
        /// </summary>
        public DiscordEmbedFooter Footer { get; internal set; }

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
                this.Author = new DiscordEmbedAuthor
                {
                    IconUrl = original.Author.IconUrl,
                    Name = original.Author.Name,
                    Url = original.Author.Url
                };

            if (original.Footer != null)
                this.Footer = new DiscordEmbedFooter
                {
                    IconUrl = original.Footer.IconUrl,
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
            var _url = url != null ? new Uri(url) : null;
            var _icon_url = icon_url != null ? new Uri(icon_url) : null;

            if (name == null && _url == null && _icon_url == null)
                this.Author = null;
            else
                this.Author = new DiscordEmbedAuthor
                {
                    Name = name,
                    Url = _url,
                    IconUrl = _icon_url
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
         *     if (name == null && url == null && icon_url == null)
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
                throw new ArgumentException("Footer text cannot exceed 2048 characters of length.");

            var _icon_url = icon_url != null ? new Uri(icon_url) : null;

            if (text == null && _icon_url == null)
                this.Footer = null;
            else
                this.Footer = new DiscordEmbedFooter
                {
                    Text = text,
                    IconUrl = _icon_url
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
         *     if (text == null && icon_url == null)
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
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            if (name.Length > 256)
                throw new ArgumentException("Embed field name cannot exceet 256 characters of length.");
            if (value.Length > 1024)
                throw new ArgumentException("Embed field value cannot exceed 1024 characters of length.");

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
                Timestamp = this._timestamp,
                Author = this.Author,
                Footer = this.Footer
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
    }
}
