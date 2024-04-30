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
        get => _embeds.Count > 0 ? _embeds[0] : null;
        set
        {
            _embeds.Clear();
            if (value != null)
            {
                _embeds.Add(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the sticker for the builder. This will always set the builder to have one sticker.
    /// </summary>
    [Obsolete("Use the features for manipulating multiple stickers instead.", true, DiagnosticId = "DSP1002")]
    public DiscordMessageSticker? Sticker
    {
        get => _stickers.Count > 0 ? _stickers[0] : null;
        set
        {
            _stickers.Clear();
            if (value != null)
            {
                _stickers.Add(value);
            }
        }
    }

    /// <summary>
    /// The stickers to attach to the message.
    /// </summary>
    public IReadOnlyList<DiscordMessageSticker> Stickers => _stickers;
    internal List<DiscordMessageSticker> _stickers = [];

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
        _stickers = builder._stickers;
        ReplyId = builder.ReplyId;
        MentionOnReply = builder.MentionOnReply;
        FailOnInvalidReply = builder.FailOnInvalidReply;
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
        IsTTS = baseMessage.IsTTS;
        Poll = baseMessage.Poll == null ? null : new DiscordPollBuilder(baseMessage.Poll);
        ReplyId = baseMessage.ReferencedMessage?.Id;
        _components = [.. baseMessage.Components];
        _content = baseMessage.Content;
        _embeds = [.. baseMessage.Embeds];
        _stickers = [.. baseMessage.Stickers];
        _mentions = [];

        if (baseMessage._mentionedUsers != null)
        {
            foreach (DiscordUser user in baseMessage._mentionedUsers)
            {
                _mentions.Add(new UserMention(user.Id));
            }
        }

        // Unsure about mentionedRoleIds
        if (baseMessage._mentionedRoles != null)
        {
            foreach (DiscordRole role in baseMessage._mentionedRoles)
            {
                _mentions.Add(new RoleMention(role.Id));
            }
        }
    }

    /// <summary>
    /// Adds a sticker to the message. Sticker must be from current guild.
    /// </summary>
    /// <param name="sticker">The sticker to add.</param>
    /// <returns>The current builder to be chained.</returns>
    [Obsolete("Use the features for manipulating multiple stickers instead.", true, DiagnosticId = "DSP1002")]
    public DiscordMessageBuilder WithSticker(DiscordMessageSticker sticker)
    {
        Sticker = sticker;
        return this;
    }

    /// <summary>
    /// Adds a sticker to the message. Sticker must be from current guild.
    /// </summary>
    /// <param name="stickers">The sticker to add.</param>
    /// <returns>The current builder to be chained.</returns>
    public DiscordMessageBuilder WithStickers(IEnumerable<DiscordMessageSticker> stickers)
    {
        _stickers = stickers.ToList();
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

        Embed = embed;
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
        ReplyId = messageId;
        MentionOnReply = mention;
        FailOnInvalidReply = failOnInvalidReply;

        if (mention)
        {
            _mentions ??= [];
            _mentions.Add(new RepliedUserMention());
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
        if (_embeds.Count > 10)
        {
            throw new ArgumentException("A message can only have up to 10 embeds.");
        }

        if (Poll == null && Files?.Count == 0 && string.IsNullOrEmpty(Content) && (!Embeds?.Any() ?? true) && (!Stickers?.Any() ?? true))
        {
            throw new ArgumentException("You must specify content, an embed, a sticker, a poll, or at least one file.");
        }

        if (Components.Count > 5)
        {
            throw new InvalidOperationException("You can only have 5 action rows per message.");
        }

        if (Components.Any(c => c.Components.Count > 5))
        {
            throw new InvalidOperationException("Action rows can only have 5 components");
        }

        if (Stickers?.Count > 3)
        {
            throw new InvalidOperationException("You can only have 3 stickers per message.");
        }
    }
}
