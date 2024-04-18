namespace DSharpPlus.Commands.Processors.SlashCommands;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

/// <summary>
/// Represents a base context for slash command contexts.
/// </summary>
public record SlashCommandContext : CommandContext
{
    public required DiscordInteraction Interaction { get; init; }
    public required IEnumerable<DiscordInteractionDataOption> Options { get; init; }

    /// <inheritdoc cref="CommandContext.RespondAsync(string)" />
    /// <param name="content">Content to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask RespondAsync(string content, bool ephemeral) => this.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(content).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.RespondAsync(DiscordEmbed)" />
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask RespondAsync(DiscordEmbed embed, bool ephemeral) => this.RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.RespondAsync(string, DiscordEmbed)" />
    /// <param name="content">Content to send in the response.</param>
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask RespondAsync(string content, DiscordEmbed embed, bool ephemeral) => this.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(content).AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc />
    public override async ValueTask RespondAsync(IDiscordMessageBuilder builder)
    {
        if (this.Interaction.ResponseState is DiscordInteractionResponseState.Replied)
        {
            throw new InvalidOperationException("Cannot respond to an interaction twice. Please use FollowupAsync instead.");
        }

        DiscordInteractionResponseBuilder interactionBuilder = builder as DiscordInteractionResponseBuilder ?? new(builder);
        
        // Don't ping anyone if no mentions are explicitly set
        if (interactionBuilder.Mentions.Count is 0)
        {
            interactionBuilder.AddMentions(Mentions.None);
        }

        if (this.Interaction.ResponseState is DiscordInteractionResponseState.Unacknowledged)
        {
            await this.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, interactionBuilder);
        }
        else if (this.Interaction.ResponseState is DiscordInteractionResponseState.Deferred)
        {
            await this.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder(interactionBuilder));
        }
    }

    /// <inheritdoc />
    public override ValueTask DeferResponseAsync() => this.DeferResponseAsync(false);

    /// <inheritdoc cref="DeferResponseAsync()"/>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public async ValueTask DeferResponseAsync(bool ephemeral) => await this.Interaction.DeferAsync(ephemeral);

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> EditResponseAsync(IDiscordMessageBuilder builder)
        => await this.Interaction.EditOriginalResponseAsync(builder as DiscordWebhookBuilder ?? new(builder));

    /// <inheritdoc />
    public override async ValueTask DeleteResponseAsync() => await this.Interaction.DeleteOriginalResponseAsync();

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage?> GetResponseAsync() => await this.Interaction.GetOriginalResponseAsync();

    /// <inheritdoc cref="CommandContext.FollowupAsync(string)" />
    /// <param name="content">Content to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask<DiscordMessage> FollowupAsync(string content, bool ephemeral) => this.FollowupAsync(new DiscordFollowupMessageBuilder().WithContent(content).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.FollowupAsync(DiscordEmbed)" />
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask<DiscordMessage> FollowupAsync(DiscordEmbed embed, bool ephemeral) => this.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.FollowupAsync(string, DiscordEmbed)" />
    /// <param name="content">Content to send in the response.</param>
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask<DiscordMessage> FollowupAsync(string content, DiscordEmbed embed, bool ephemeral) => this.FollowupAsync(new DiscordFollowupMessageBuilder().WithContent(content).AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> FollowupAsync(IDiscordMessageBuilder builder)
    {
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
}
