namespace DSharpPlus.Commands.Processors.SlashCommands;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

public record SlashCommandContext : CommandContext
{
    public required DiscordInteraction Interaction { get; init; }
    public required IEnumerable<DiscordInteractionDataOption> Options { get; init; }
    public InteractionStatus State { get; protected set; }

    /// <inheritdoc />
    public override async ValueTask RespondAsync(IDiscordMessageBuilder builder)
    {
        if (this.State.HasFlag(InteractionStatus.ResponseSent))
        {
            throw new InvalidOperationException("Cannot respond to an interaction twice. Please use FollowupAsync instead.");
        }

        DiscordInteractionResponseBuilder interactionBuilder = builder is DiscordInteractionResponseBuilder responseBuilder
            ? responseBuilder
            : new DiscordInteractionResponseBuilder(builder);

        // Don't ping anyone if no mentions are explicitly set
        if (interactionBuilder.Mentions?.Count is null or 0)
        {
            interactionBuilder.AddMentions(Mentions.None);
        }

        if (this.State is InteractionStatus.None)
        {
            await this.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, interactionBuilder);
        }
        else if (this.State is InteractionStatus.ResponseDelayed)
        {
            await this.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder(interactionBuilder));
        }

        this.State |= InteractionStatus.ResponseSent;
    }

    /// <inheritdoc />
    public override async ValueTask DeferResponseAsync()
    {
        await this.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        this.State |= InteractionStatus.ResponseDelayed;
    }

    /// <inheritdoc cref="DeferResponseAsync()"/>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public async ValueTask DeferResponseAsync(bool ephemeral)
    {
        await this.Interaction.CreateResponseAsync
        (
            InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(ephemeral)
        );

        this.State |= InteractionStatus.ResponseDelayed;
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> EditResponseAsync(IDiscordMessageBuilder builder)
    {
        if (!this.State.HasFlag(InteractionStatus.ResponseSent) && !this.State.HasFlag(InteractionStatus.ResponseDelayed))
        {
            throw new InvalidOperationException("Cannot edit a response that has not been sent yet.");
        }

        DiscordWebhookBuilder webhookBuilder = builder is DiscordWebhookBuilder messageBuilder
            ? messageBuilder
            : new DiscordWebhookBuilder(builder);

        return await this.Interaction.EditOriginalResponseAsync(webhookBuilder);
    }

    /// <inheritdoc />
    public override async ValueTask DeleteResponseAsync()
    {
        if (!this.State.HasFlag(InteractionStatus.ResponseSent) || !this.State.HasFlag(InteractionStatus.ResponseDelayed))
        {
            throw new InvalidOperationException("Cannot delete a response that has not been sent yet.");
        }

        await this.Interaction.DeleteOriginalResponseAsync();
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage?> GetResponseAsync() => !this.State.HasFlag(InteractionStatus.ResponseSent)
        ? throw new InvalidOperationException("Cannot get a response that has not been properly sent yet.")
        : await this.Interaction.GetOriginalResponseAsync();

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> FollowupAsync(IDiscordMessageBuilder builder)
    {
        if (!this.State.HasFlag(InteractionStatus.ResponseSent))
        {
            throw new InvalidOperationException("Cannot follow up to a response that has not been sent yet.");
        }

        DiscordFollowupMessageBuilder followupBuilder = builder is DiscordFollowupMessageBuilder messageBuilder
            ? messageBuilder
            : new DiscordFollowupMessageBuilder(builder);

        DiscordMessage message = await this.Interaction.CreateFollowupMessageAsync(followupBuilder);
        this._followupMessages.Add(message.Id, message);
        return message;
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder)
    {
        if (!this.State.HasFlag(InteractionStatus.ResponseSent))
        {
            throw new InvalidOperationException("Cannot edit a follow up that has not been sent yet.");
        }

        // Fetch the follow up message if we don't have it cached.
        if (!this._followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            message = await this.Channel.GetMessageAsync(messageId);
        }

        DiscordMessageBuilder editedBuilder = builder is DiscordMessageBuilder messageBuilder
            ? messageBuilder
            : new DiscordMessageBuilder(builder);

        this._followupMessages[messageId] = await message.ModifyAsync(editedBuilder);
        return this._followupMessages[messageId];
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false)
    {
        if (!this.State.HasFlag(InteractionStatus.ResponseSent))
        {
            throw new InvalidOperationException("Cannot get a follow up that has not been sent yet.");
        }

        // Fetch the follow up message if we don't have it cached.
        if (ignoreCache || !this._followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            message = await this.Interaction.GetFollowupMessageAsync(messageId);
            this._followupMessages[messageId] = message;
        }

        return message;
    }

    /// <inheritdoc />
    public override async ValueTask DeleteFollowupAsync(ulong messageId) => await this.Interaction.DeleteFollowupMessageAsync(messageId);

    [Flags]
    public enum InteractionStatus
    {
        None = 0,
        ResponseDelayed = 1 << 0,
        ResponseSent = 1 << 1
    }
}
