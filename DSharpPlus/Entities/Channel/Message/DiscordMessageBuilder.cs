using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Entities;

/// <summary>
/// Constructs a Message to be sent.
/// </summary>
public sealed class DiscordMessageBuilder : BaseDiscordMessageBuilder<DiscordMessageBuilder>
{
    /// <summary>
    /// Gets or sets the embed for the builder. This will always set the builder to have one embed.
    /// </summary>
    [Obsolete("Use the features for manipulating multiple embeds instead.", true, DiagnosticId = "DSP1001")]
    public DiscordEmbed? Embed
    {
        get => this.embeds.Count > 0 ? this.embeds[0] : null;
        set
        {
            this.embeds.Clear();
            if (value != null)
            {
                this.embeds.Add(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the sticker for the builder. This will always set the builder to have one sticker.
    /// </summary>
    [Obsolete("Use the features for manipulating multiple stickers instead.", true, DiagnosticId = "DSP1002")]
    public DiscordMessageSticker? Sticker
    {
        get => this.stickers.Count > 0 ? this.stickers[0] : null;
        set
        {
            this.stickers.Clear();
            if (value != null)
            {
                this.stickers.Add(value);
            }
        }
    }

    /// <summary>
    /// The stickers to attach to the message.
    /// </summary>
    public IReadOnlyList<DiscordMessageSticker> Stickers => this.stickers;
    internal List<DiscordMessageSticker> stickers = [];

    /// <summary>
    /// Gets the Reply Message ID.
    /// </summary>
    public ulong? ReplyId { get; private set; } = null;

    /// <summary>
    /// Gets if the Reply should mention the user.
    /// </summary>
    public bool MentionOnReply { get; private set; } = false;

    /// <summary>
    /// Gets if the Reply will error if the Reply Message Id does not reference a valid message.
    /// <para>If set to false, invalid replies are send as a regular message.</para>
    /// <para>Defaults to false.</para>
    /// </summary>
    public bool FailOnInvalidReply { get; set; }

    /// <summary>
    /// Constructs a new discord message builder
    /// </summary>
    public DiscordMessageBuilder() { }

    /// <summary>
    /// Constructs a new discord message builder based on a previous builder.
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    public DiscordMessageBuilder(DiscordMessageBuilder builder) : base(builder)
    {
        this.stickers = builder.stickers;
        this.ReplyId = builder.ReplyId;
        this.MentionOnReply = builder.MentionOnReply;
        this.FailOnInvalidReply = builder.FailOnInvalidReply;
    }

    /// <summary>
    /// Copies the common properties from the passed builder.
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    public DiscordMessageBuilder(IDiscordMessageBuilder builder) : base(builder) { }

    /// <summary>
    /// Constructs a new discord message builder based on the passed message.
    /// </summary>
    /// <param name="baseMessage">The message to copy.</param>
    public DiscordMessageBuilder(DiscordMessage baseMessage)
    {
        this.IsTTS = baseMessage.IsTTS;
        this.Poll = baseMessage.Poll == null ? null : new DiscordPollBuilder(baseMessage.Poll);
        this.ReplyId = baseMessage.ReferencedMessage?.Id;
        this.components = [.. baseMessage.Components];
        this.content = baseMessage.Content;
        this.embeds = [.. baseMessage.Embeds];
        this.stickers = [.. baseMessage.Stickers];
        this.mentions = [];

        if (baseMessage.mentionedUsers != null)
        {
            foreach (DiscordUser user in baseMessage.mentionedUsers)
            {
                this.mentions.Add(new UserMention(user.Id));
            }
        }

        // Unsure about mentionedRoleIds
        if (baseMessage.mentionedRoles != null)
        {
            foreach (DiscordRole role in baseMessage.mentionedRoles)
            {
                this.mentions.Add(new RoleMention(role.Id));
            }
        }
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="DiscordMessage"/> object.
    /// </summary>
    /// <param name="discordmessageJson">The JSON string representing a Discord embed.</param>
    /// <returns>A <see cref="DiscordMessage"/> object deserialized from the JSON string.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided JSON is invalid or does not match the <see cref="DiscordMessage"/> structure.</exception>
    public static DiscordMessage fromJson(string discordmessageJson)
    {
        DiscordMessage? discordMessage = JsonConvert.DeserializeObject<DiscordMessage>(discordmessageJson, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        return discordMessage ?? throw new ArgumentException("The provided JSON is invalid or does not match the DiscordMessage structure.");
    }

    /// <summary>
    /// Serializes a <see cref="DiscordMessage"/> object into a JSON string.
    /// </summary>
    /// <param name="discordMessage">The <see cref="DiscordMessage"/> object to serialize.</param>
    /// <returns>A JSON string representing the <see cref="DiscordMessage"/> object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided <see cref="DiscordMessage"/> object is null.</exception>
    public static string ToJson(DiscordMessage discordMessage)
    {
        return discordMessage == null
            ? throw new ArgumentNullException(nameof(discordMessage), "The provided DiscordMessage object cannot be null.")
            : JsonConvert.SerializeObject(discordMessage, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
    }

    /// <summary>
    /// Serializes a <see cref="DiscordMessage"/> object into a JSON string using the specified <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="discordMessage">The <see cref="DiscordMessage"/> object to serialize.</param>
    /// <param name="jsonSerializerSettings">The <see cref="JsonSerializerSettings"/> to use for serialization.</param>
    /// <returns>A JSON string representing the <see cref="DiscordMessage"/> object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided <see cref="DiscordMessage"/> object is null.</exception>
    public static string ToJson(DiscordMessage discordMessage, JsonSerializerSettings jsonSerializerSettings)
    {
        return discordMessage == null
            ? throw new ArgumentNullException(nameof(discordMessage), "The provided DiscordMessage object cannot be null.")
            : JsonConvert.SerializeObject(discordMessage, jsonSerializerSettings);
    }

    /// <summary>
    /// Adds a sticker to the message. Sticker must be from current guild.
    /// </summary>
    /// <param name="sticker">The sticker to add.</param>
    /// <returns>The current builder to be chained.</returns>
    [Obsolete("Use the features for manipulating multiple stickers instead.", true, DiagnosticId = "DSP1002")]
    public DiscordMessageBuilder WithSticker(DiscordMessageSticker sticker)
    {
        this.Sticker = sticker;
        return this;
    }

    /// <summary>
    /// Adds a sticker to the message. Sticker must be from current guild.
    /// </summary>
    /// <param name="stickers">The sticker to add.</param>
    /// <returns>The current builder to be chained.</returns>
    public DiscordMessageBuilder WithStickers(IEnumerable<DiscordMessageSticker> stickers)
    {
        this.stickers = stickers.ToList();
        return this;
    }

    /// <summary>
    /// Sets the embed for the current builder.
    /// </summary>
    /// <param name="embed">The embed that should be set.</param>
    /// <returns>The current builder to be chained.</returns>
    [Obsolete("Use the features for manipulating multiple embeds instead.", true, DiagnosticId = "DSP1001")]
    public DiscordMessageBuilder WithEmbed(DiscordEmbed embed)
    {
        if (embed == null)
        {
            return this;
        }

        this.Embed = embed;
        return this;
    }

    /// <summary>
    /// Sets if the message has allowed mentions.
    /// </summary>
    /// <param name="allowedMention">The allowed Mention that should be sent.</param>
    /// <returns>The current builder to be chained.</returns>
    public DiscordMessageBuilder WithAllowedMention(IMention allowedMention)
        => AddMention(allowedMention);

    /// <summary>
    /// Sets if the message has allowed mentions.
    /// </summary>
    /// <param name="allowedMentions">The allowed Mentions that should be sent.</param>
    /// <returns>The current builder to be chained.</returns>
    public DiscordMessageBuilder WithAllowedMentions(IEnumerable<IMention> allowedMentions)
        => AddMentions(allowedMentions);

    /// <summary>
    /// Sets if the message is a reply
    /// </summary>
    /// <param name="messageId">The ID of the message to reply to.</param>
    /// <param name="mention">If we should mention the user in the reply.</param>
    /// <param name="failOnInvalidReply">Whether sending a reply that references an invalid message should be </param>
    /// <returns>The current builder to be chained.</returns>
    public DiscordMessageBuilder WithReply(ulong? messageId, bool mention = false, bool failOnInvalidReply = false)
    {
        this.ReplyId = messageId;
        this.MentionOnReply = mention;
        this.FailOnInvalidReply = failOnInvalidReply;

        if (mention)
        {
            this.mentions ??= [];
            this.mentions.Add(new RepliedUserMention());
        }

        return this;
    }

    /// <summary>
    /// Sends the Message to a specific channel
    /// </summary>
    /// <param name="channel">The channel the message should be sent to.</param>
    /// <returns>The current builder to be chained.</returns>
    public Task<DiscordMessage> SendAsync(DiscordChannel channel) => channel.SendMessageAsync(this);

    /// <summary>
    /// Sends the modified message.
    /// <para>Note: Message replies cannot be modified. To clear the reply, simply pass <see langword="null"/> to <see cref="WithReply"/>.</para>
    /// </summary>
    /// <param name="msg">The original Message to modify.</param>
    /// <returns>The current builder to be chained.</returns>
    public Task<DiscordMessage> ModifyAsync(DiscordMessage msg) => msg.ModifyAsync(this);

    /// <summary>
    /// Does the validation before we send a the Create/Modify request.
    /// </summary>
    internal void Validate()
    {
        if (this.embeds.Count > 10)
        {
            throw new ArgumentException("A message can only have up to 10 embeds.");
        }

        if (this.Poll == null && this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && (!this.Embeds?.Any() ?? true) && (!this.Stickers?.Any() ?? true))
        {
            throw new ArgumentException("You must specify content, an embed, a sticker, a poll, or at least one file.");
        }

        if (this.Components.Count > 5)
        {
            throw new InvalidOperationException("You can only have 5 action rows per message.");
        }

        if (this.Components.Any(c => c.Components.Count > 5))
        {
            throw new InvalidOperationException("Action rows can only have 5 components");
        }

        if (this.Stickers?.Count > 3)
        {
            throw new InvalidOperationException("You can only have 3 stickers per message.");
        }
    }
}
