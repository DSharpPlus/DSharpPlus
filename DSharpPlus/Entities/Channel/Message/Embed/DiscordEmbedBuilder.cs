namespace DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.Net;

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
    private string? _title;

    /// <summary>
    /// Gets or sets the embed's description.
    /// </summary>
    public string? Description
    {
        get => _description;
        set
        {
            if (value != null && value.Length > 4096)
            {
                throw new ArgumentException("Description length cannot exceed 4096 characters.", nameof(value));
            }

            _description = value;
        }
    }
    private string? _description;

    /// <summary>
    /// Gets or sets the url for the embed's title.
    /// </summary>
    public string? Url
    {
        get => _url?.ToString();
        set => _url = string.IsNullOrEmpty(value) ? null : new Uri(value);
    }
    private Uri? _url;

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
        get => _imageUri?.ToString();
        set => _imageUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
    }
    private DiscordUri? _imageUri;

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
    private readonly List<DiscordEmbedField> _fields = [];

    /// <summary>
    /// Constructs a new empty embed builder.
    /// </summary>
    public DiscordEmbedBuilder() => Fields = new ReadOnlyCollection<DiscordEmbedField>(_fields);

    /// <summary>
    /// Constructs a new embed builder using another embed as prototype.
    /// </summary>
    /// <param name="original">Embed to use as prototype.</param>
    public DiscordEmbedBuilder(DiscordEmbed original)
        : this()
    {
        Title = original.Title;
        Description = original.Description;
        Url = original.Url?.ToString();
        ImageUrl = original.Image?.Url?.ToString();
        Color = original.Color;
        Timestamp = original.Timestamp;

        if (original.Thumbnail != null)
        {
            Thumbnail = new EmbedThumbnail
            {
                Url = original.Thumbnail.Url?.ToString(),
                Height = original.Thumbnail.Height,
                Width = original.Thumbnail.Width
            };
        }

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

        if (original.Fields?.Any() == true)
        {
            _fields.AddRange(original.Fields);
        }

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
        Timestamp = timestamp == null ? null : new DateTimeOffset(timestamp.Value);
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
        _imageUri = new DiscordUri(url);
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
        Thumbnail = new EmbedThumbnail
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
        Thumbnail = new EmbedThumbnail
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
    public DiscordEmbedBuilder WithAuthor(string? name = null, string? url = null, string? iconUrl = null)
    {
        Author = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url) && string.IsNullOrEmpty(iconUrl)
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

        Footer = string.IsNullOrEmpty(text) && string.IsNullOrEmpty(iconUrl)
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
    /// Removes a field of the specified index from this embed.
    /// </summary>
    /// <param name="index">Index of the field to remove.</param>
    /// <returns>This embed builder.</returns>
    public DiscordEmbedBuilder RemoveFieldAt(int index)
    {
        _fields.RemoveAt(index);
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
        _fields.RemoveRange(index, count);
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
        DiscordEmbed embed = new()
        {
            Title = _title,
            Description = _description,
            Url = _url,
            _color = Color is not null ? Optional.FromValue(Color.Value.Value) : Optional.FromNoValue<int>(),
            Timestamp = Timestamp
        };

        if (Footer is not null)
        {
            embed.Footer = new DiscordEmbedFooter
            {
                Text = Footer.Text,
                IconUrl = Footer._iconUri
            };
        }

        if (Author is not null)
        {
            embed.Author = new DiscordEmbedAuthor
            {
                Name = Author.Name,
                Url = Author._uri,
                IconUrl = Author._iconUri
            };
        }

        if (_imageUri is not null)
        {
            embed.Image = new DiscordEmbedImage { Url = _imageUri.Value };
        }

        if (Thumbnail is not null)
        {
            embed.Thumbnail = new DiscordEmbedThumbnail
            {
                Url = Thumbnail._uri,
                Height = Thumbnail.Height,
                Width = Thumbnail.Width
            };
        }

        embed.Fields = new ReadOnlyCollection<DiscordEmbedField>(new List<DiscordEmbedField>(_fields)); // copy the list, don't wrap it, prevents mutation

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
            get => _name;
            set
            {
                if (value != null && value.Length > 256)
                {
                    throw new ArgumentException("Author name length cannot exceed 256 characters.", nameof(value));
                }

                _name = value;
            }
        }
        private string? _name;

        /// <summary>
        /// Gets or sets the Url to which the author's link leads.
        /// </summary>
        public string? Url
        {
            get => _uri?.ToString();
            set => _uri = string.IsNullOrEmpty(value) ? null : new Uri(value);
        }
        internal Uri? _uri;

        /// <summary>
        /// Gets or sets the Author's icon url.
        /// </summary>
        public string? IconUrl
        {
            get => _iconUri?.ToString();
            set => _iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        internal DiscordUri? _iconUri;
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
            get => _text;
            set
            {
                if (value != null && value.Length > 2048)
                {
                    throw new ArgumentException("Footer text length cannot exceed 2048 characters.", nameof(value));
                }

                _text = value;
            }
        }
        private string? _text;

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string? IconUrl
        {
            get => _iconUri?.ToString();
            set => _iconUri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        internal DiscordUri? _iconUri;
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
            get => _uri?.ToString();
            set => _uri = string.IsNullOrEmpty(value) ? null : new DiscordUri(value);
        }
        internal DiscordUri? _uri;

        /// <summary>
        /// Gets or sets the thumbnail's height.
        /// </summary>
        public int Height
        {
            get => _height;
            set => _height = value >= 0 ? value : 0;
        }
        private int _height;

        /// <summary>
        /// Gets or sets the thumbnail's width.
        /// </summary>
        public int Width
        {
            get => _width;
            set => _width = value >= 0 ? value : 0;
        }
        private int _width;
    }
}
