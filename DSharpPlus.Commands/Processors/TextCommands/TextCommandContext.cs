using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.TextCommands;

public sealed record TextCommandContext : CommandContext
{
    public required DiscordMessage Message { get; init; }
    public required int PrefixLength { internal get; init; }
    public string? Prefix => this.Message.Content?[..this.PrefixLength];
    public DiscordMessage? Response { get; private set; }
    public bool Delayed { get; private set; }

    /// <inheritdoc />
    public override async ValueTask RespondAsync(IDiscordMessageBuilder builder)
    {
        DiscordMessageBuilder messageBuilder = new(builder);

        // Reply to the message that invoked the command if no reply is set
        if (messageBuilder.ReplyId is null)
        {
            messageBuilder.WithReply(this.Message.Id);
        }

        // Don't ping anyone if no mentions are explicitly set
        if (messageBuilder.Mentions?.Count is null or 0)
        {
            messageBuilder.WithAllowedMentions(Mentions.None);
        }

        this.Response = await this.Channel.SendMessageAsync(messageBuilder);
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> EditResponseAsync(IDiscordMessageBuilder builder)
    {
        if (this.Response is not null)
        {
            this.Response = await this.Response.ModifyAsync(new DiscordMessageBuilder(builder));
        }
        else if (this.Delayed)
        {
            await RespondAsync(builder);
        }
        else
        {
            throw new InvalidOperationException("Cannot edit a response that has not been sent yet.");
        }

        return this.Response!;
    }

    /// <inheritdoc />
    public override async ValueTask DeleteResponseAsync()
    {
        if (this.Response is null)
        {
            throw new InvalidOperationException("Cannot delete a response that has not been sent yet.");
        }

        await this.Response.DeleteAsync();
    }

    /// <inheritdoc />
    public override ValueTask<DiscordMessage?> GetResponseAsync() => ValueTask.FromResult(this.Response);

    /// <inheritdoc />
    public override async ValueTask DeferResponseAsync()
    {
        await this.Channel.TriggerTypingAsync();
        this.Delayed = true;
    }

    public override async ValueTask<DiscordMessage> FollowupAsync(IDiscordMessageBuilder builder)
    {
        if (this.Response is null)
        {
            throw new InvalidOperationException("Cannot send a followup message before the initial response.");
        }

        DiscordMessageBuilder messageBuilder = new(builder);

        // Reply to the original message if no reply is set, to indicate that this message is related to the command
        if (messageBuilder.ReplyId is null)
        {
            messageBuilder.WithReply(this.Response.Id);
        }

        // Don't ping anyone if no mentions are explicitly set
        if (messageBuilder.Mentions?.Count is null or 0)
        {
            messageBuilder.WithAllowedMentions(Mentions.None);
        }

        DiscordMessage followup = await this.Channel.SendMessageAsync(messageBuilder);
        this.followupMessages.Add(followup.Id, followup);
        return followup;
    }

    public override async ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder)
    {
        if (this.Response is null)
        {
            throw new InvalidOperationException("Cannot edit a followup message before the initial response.");
        }

        if (!this.followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            throw new InvalidOperationException("Cannot edit a followup message that does not exist.");
        }

        DiscordMessageBuilder messageBuilder = new(builder);
        this.followupMessages[messageId] = await message.ModifyAsync(messageBuilder);
        return this.followupMessages[messageId];
    }

    public override async ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false)
    {
        if (this.Response is null)
        {
            throw new InvalidOperationException("Cannot get a followup message before the initial response.");
        }

        // Fetch the follow up message if we don't have it cached.
        if (ignoreCache || !this.followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            message = await this.Channel.GetMessageAsync(messageId, true);
            this.followupMessages[messageId] = message;
        }

        return message;
    }

    public override async ValueTask DeleteFollowupAsync(ulong messageId)
    {
        if (this.Response is null)
        {
            throw new InvalidOperationException("Cannot delete a followup message before the initial response.");
        }

        if (!this.followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            throw new InvalidOperationException("Cannot delete a followup message that does not exist.");
        }

        await message.DeleteAsync();
    }
}
