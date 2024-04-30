namespace DSharpPlus.Entities;

using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Constructs ready-to-send webhook requests.
/// </summary>
public sealed class DiscordWebhookBuilder : BaseDiscordMessageBuilder<DiscordWebhookBuilder>
{
    /// <summary>
    /// Username to use for this webhook request.
    /// </summary>
    public Optional<string> Username { get; set; }

    /// <summary>
    /// Avatar url to use for this webhook request.
    /// </summary>
    public Optional<string> AvatarUrl { get; set; }

    /// <summary>
    /// Id of the thread to send the webhook request to.
    /// </summary>
    public ulong? ThreadId { get; set; }

    /// <summary>
    /// Constructs a new empty webhook request builder.
    /// </summary>
    public DiscordWebhookBuilder() { } // I still see no point in initializing collections with empty collections. //

    /// <summary>
    /// Constructs a new webhook request builder based on a previous message builder
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    public DiscordWebhookBuilder(DiscordWebhookBuilder builder) : base(builder)
    {
        Username = builder.Username;
        AvatarUrl = builder.AvatarUrl;
        ThreadId = builder.ThreadId;
    }

    /// <summary>
    /// Copies the common properties from the passed builder.
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    public DiscordWebhookBuilder(IDiscordMessageBuilder builder) : base(builder) { }

    /// <summary>
    /// Sets the username for this webhook builder.
    /// </summary>
    /// <param name="username">Username of the webhook</param>
    public DiscordWebhookBuilder WithUsername(string username)
    {
        Username = username;
        return this;
    }

    /// <summary>
    /// Sets the avatar of this webhook builder from its url.
    /// </summary>
    /// <param name="avatarUrl">Avatar url of the webhook</param>
    public DiscordWebhookBuilder WithAvatarUrl(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        return this;
    }

    /// <summary>
    /// Sets the id of the thread to execute the webhook on.
    /// </summary>
    /// <param name="threadId">The id of the thread</param>
    public DiscordWebhookBuilder WithThreadId(ulong? threadId)
    {
        ThreadId = threadId;
        return this;
    }

    public override void Clear()
    {
        Username = default;
        AvatarUrl = default;
        ThreadId = default;
        base.Clear();
    }

    /// <summary>
    /// Executes a webhook.
    /// </summary>
    /// <param name="webhook">The webhook that should be executed.</param>
    /// <returns>The message sent</returns>
    public async Task<DiscordMessage> SendAsync(DiscordWebhook webhook) => await webhook.ExecuteAsync(this);

    /// <summary>
    /// Sends the modified webhook message.
    /// </summary>
    /// <param name="webhook">The webhook that should be executed.</param>
    /// <param name="message">The message to modify.</param>
    /// <returns>The modified message</returns>
    public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, DiscordMessage message) => await ModifyAsync(webhook, message.Id);
    /// <summary>
    /// Sends the modified webhook message.
    /// </summary>
    /// <param name="webhook">The webhook that should be executed.</param>
    /// <param name="messageId">The id of the message to modify.</param>
    /// <returns>The modified message</returns>
    public async Task<DiscordMessage> ModifyAsync(DiscordWebhook webhook, ulong messageId) => await webhook.EditMessageAsync(messageId, this);

    /// <summary>
    /// Does the validation before we send a the Create/Modify request.
    /// </summary>
    /// <param name="isModify">Tells the method to perform the Modify Validation or Create Validation.</param>
    /// <param name="isFollowup">Tells the method to perform the follow up message validation.</param>
    /// <param name="isInteractionResponse">Tells the method to perform the interaction response validation.</param>
    internal void Validate(bool isModify = false, bool isFollowup = false, bool isInteractionResponse = false)
    {
        if (isModify)
        {
            if (Username.HasValue)
            {
                throw new ArgumentException("You cannot change the username of a message.");
            }

            if (AvatarUrl.HasValue)
            {
                throw new ArgumentException("You cannot change the avatar of a message.");
            }
        }
        else if (isFollowup)
        {
            if (Username.HasValue)
            {
                throw new ArgumentException("You cannot change the username of a follow up message.");
            }

            if (AvatarUrl.HasValue)
            {
                throw new ArgumentException("You cannot change the avatar of a follow up message.");
            }
        }
        else if (isInteractionResponse)
        {
            if (Username.HasValue)
            {
                throw new ArgumentException("You cannot change the username of an interaction response.");
            }

            if (AvatarUrl.HasValue)
            {
                throw new ArgumentException("You cannot change the avatar of an interaction response.");
            }
        }
        else
        {
            if (Files?.Count == 0 && string.IsNullOrEmpty(Content) && !Embeds.Any())
            {
                throw new ArgumentException("You must specify content, an embed, or at least one file.");
            }
        }
    }
}
