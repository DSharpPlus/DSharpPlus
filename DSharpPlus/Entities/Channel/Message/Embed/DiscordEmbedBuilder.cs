using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.Net;

namespace DSharpPlus.Entities;

/// <summary>
/// Constructs embeds.
/// </summary>
public sealed class DiscordEmbedBuilder
{
    /// <summary>
    /// Gets or sets the embed's title.
    /// </summary>
    public string? Title
    {
        get => this.title;
        set
        {
            if (value != null && value.Length > 256)
            {
                throw new ArgumentException("Title length cannot exceed 256 characters.", nameof(value));
            }

            this.title = value;
        }
    }
    private string? title;

    /// <summary>
    /// Gets or sets the embed's description.
    /// </summary>
    public string? Description
    {
        get => this.description;
        set
        {
            if (value != null && value.Length > 4096)
            {
                throw new ArgumentException("Description length cannot exceed 4096 characters.", nameof(value));
            }

            this.description = value;
        }
    }
    private string? description;

    /// <summary>
    /// Gets or sets the url for the embed's title.
    /// </summary>
    public string? Url
    {
        get => this.url?.ToString();
        set => this.url = string.IsNullOrEmpty(value) ? null : new Uri(value);
    }
    private Uri? url;

    /// <summary>
    /// Gets or sets the embed's color.
    /// </summary>
    public DiscordColor? Color { get; set; }

    /// <summary>
    /// Gets or sets the embed's timestamp.
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the embed's image url.
    /// </summary>
    public string? ImageUrl
    {
        get => this.imageUri?.ToString();
        set => this.imageUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
    }
    private DiscordUri? imageUri;

    /// <summary>
    /// Gets or sets the embed's author.
    /// </summary>
    public EmbedAuthor? Author { get; set; }

    /// <summary>
    /// Gets or sets the embed's footer.
    /// </summary>
    public EmbedFooter? Footer { get; set; }

    /// <summary>
    /// Gets or sets the embed's thumbnail.
    /// </summary>
    public EmbedThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Gets the embed's fields.
    /// </summary>
    public IReadOnlyList<DiscordEmbedField> Fields { get; }
    private readonly List<DiscordEmbedField> fields = [];

    /// <summary>
    /// Constructs a new empty embed builder.
    /// </summary>
    public DiscordEmbedBuilder() => this.Fields = new ReadOnlyCollection<DiscordEmbedField>(this.fields);

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
        {
            this.Thumbnail = new EmbedThumbnail
            {
                Url = original.Thumbnail.Url?.ToString(),
                Height = original.Thumbnail.Height,
                Width = original.Thumbnail.Width
            };
        }

        if (original.Author != null)
        {
            this.Author = new EmbedAuthor
            {
                IconUrl = original.Author.IconUrl?.ToString(),
                Name = original.Author.Name,
                Url = original.Author.Url?.ToString()
            };
        }

        if (original.Footer != null)
        {
            this.Footer = new EmbedFooter
            {
                IconUrl = original.Footer.IconUrl?.ToString(),
                Text = original.Footer.Text
            };
        }

        if (original.Fields?.Any() == true)
        {
            this.fields.AddRange(original.Fields);
        }

        while (this.fields.Count > 25)
        {
            this.fields.RemoveAt(this.fields.Count - 1);
        }
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
        this.url = url;
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
        this.Timestamp = timestamp == null ? null : new DateTimeOffset(timestamp.Value);
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
        this.imageUri = new DiscordUri(url);
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
            uri = new DiscordUri(url),
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
    public DiscordEmbedBuilder WithAuthor(string? name = null, string? url = null, string? iconUrl = null)
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
    public DiscordEmbedBuilder WithFooter(string? text = null, string? iconUrl = null)
    {
        if (text is not null && text.Length > 2048)
        {
            throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(text));
        }

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
            ArgumentNullException.ThrowIfNull(name);

            throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            ArgumentNullException.ThrowIfNull(value);

            throw new ArgumentException("Value cannot be empty or whitespace.", nameof(value));
        }

        if (name.Length > 256)
        {
            throw new ArgumentException("Embed field name length cannot exceed 256 characters.");
        }

        if (value.Length > 1024)
        {
            throw new ArgumentException("Embed field value length cannot exceed 1024 characters.");
        }

        if (this.fields.Count >= 25)
        {
            throw new InvalidOperationException("Cannot add more than 25 fields.");
        }

        this.fields.Add(new DiscordEmbedField
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
        this.fields.RemoveAt(index);
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
        this.fields.RemoveRange(index, count);
        return this;
    }

    /// <summary>
    /// Removes all fields from this embed.
    /// </summary>
    /// <returns>This embed builder.</returns>
    public DiscordEmbedBuilder ClearFields()
    {
        this.fields.Clear();
        return this;
    }

    /// <summary>
    /// Constructs a new embed from data supplied to this builder.
    /// </summary>
    /// <returns>New discord embed.</returns>
    public DiscordEmbed Build()
    {
        DiscordEmbed embed = new()
        {
            Title = this.title,
            Description = this.description,
            Url = this.url,
            color = this.Color is not null ? Optional.FromValue(this.Color.Value.Value) : Optional.FromNoValue<int>(),
            Timestamp = this.Timestamp
        };

        if (this.Footer is not null)
        {
            embed.Footer = new DiscordEmbedFooter
            {
                Text = this.Footer.Text,
                IconUrl = this.Footer.iconUri
            };
        }

        if (this.Author is not null)
        {
            embed.Author = new DiscordEmbedAuthor
            {
                Name = this.Author.Name,
                Url = this.Author.uri,
                IconUrl = this.Author.iconUri
            };
        }

        if (this.imageUri is not null)
        {
            embed.Image = new DiscordEmbedImage { Url = this.imageUri.Value };
        }

        if (this.Thumbnail is not null)
        {
            embed.Thumbnail = new DiscordEmbedThumbnail
            {
                Url = this.Thumbnail.uri,
                Height = this.Thumbnail.Height,
                Width = this.Thumbnail.Width
            };
        }

        embed.Fields = new ReadOnlyCollection<DiscordEmbedField>(new List<DiscordEmbedField>(this.fields)); // copy the list, don't wrap it, prevents mutation

        return embed;
    }

    /// <summary>
    /// Implicitly converts this builder to an embed.
    /// </summary>
    /// <param name="builder">Builder to convert.</param>
    public static implicit operator DiscordEmbed(DiscordEmbedBuilder builder)
        => builder.Build();

    /// <summary>
    /// Represents an embed author.
    /// </summary>
    public class EmbedAuthor
    {
        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        public string? Name
        {
            get => this.name;
            set
            {
                if (value != null && value.Length > 256)
                {
                    throw new ArgumentException("Author name length cannot exceed 256 characters.", nameof(value));
                }

                this.name = value;
            }
        }
        private string? name;

        /// <summary>
        /// Gets or sets the Url to which the author's link leads.
        /// </summary>
        public string? Url
        {
            get => this.uri?.ToString();
            set => this.uri = string.IsNullOrEmpty(value) ? null : new Uri(value);
        }
        internal Uri? uri;

        /// <summary>
        /// Gets or sets the Author's icon url.
        /// </summary>
        public string? IconUrl
        {
            get => this.iconUri?.ToString();
            set => this.iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        internal DiscordUri? iconUri;
    }

    /// <summary>
    /// Represents an embed footer.
    /// </summary>
    public class EmbedFooter
    {
        /// <summary>
        /// Gets or sets the text of the footer.
        /// </summary>
        public string? Text
        {
            get => this.text;
            set
            {
                if (value != null && value.Length > 2048)
                {
                    throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(value));
                }

                this.text = value;
            }
        }
        private string? text;

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string? IconUrl
        {
            get => this.iconUri?.ToString();
            set => this.iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        internal DiscordUri? iconUri;
    }

    /// <summary>
    /// Represents an embed thumbnail.
    /// </summary>
    public class EmbedThumbnail
    {
        /// <summary>
        /// Gets or sets the thumbnail's image url.
        /// </summary>
        public string? Url
        {
            get => this.uri?.ToString();
            set => this.uri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        internal DiscordUri? uri;

        /// <summary>
        /// Gets or sets the thumbnail's height.
        /// </summary>
        public int Height
        {
            get => this.height;
            set => this.height = value >= 0 ? value : 0;
        }
        private int height;

        /// <summary>
        /// Gets or sets the thumbnail's width.
        /// </summary>
        public int Width
        {
            get => this.width;
            set => this.width = value >= 0 ? value : 0;
        }
        private int width;
    }
}
