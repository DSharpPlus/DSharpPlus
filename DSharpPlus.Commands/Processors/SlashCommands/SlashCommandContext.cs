using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

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
    public virtual ValueTask<DiscordMessage> RespondAsync(string content, bool ephemeral)
        => RespondAsync(new DiscordInteractionResponseBuilder().WithContent(content).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.RespondAsync(DiscordEmbed)" />
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask<DiscordMessage> RespondAsync(DiscordEmbed embed, bool ephemeral)
        => RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.RespondAsync(string, DiscordEmbed)" />
    /// <param name="content">Content to send in the response.</param>
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask<DiscordMessage> RespondAsync(string content, DiscordEmbed embed, bool ephemeral)
        => RespondAsync(new DiscordInteractionResponseBuilder().WithContent(content).AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> RespondAsync(IDiscordMessageBuilder builder)
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
            return await this.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, interactionBuilder);
        }
        else if (this.Interaction.ResponseState is DiscordInteractionResponseState.Deferred)
        {
            return await this.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder(interactionBuilder));
        }

        throw new UnreachableException("Invalid interaction state - if the interaction was already replied to, it should have been caught earlier.");
    }

    /// <summary>
    /// Respond to the command with a Modal.
    /// </summary>
    /// <param name="builder">Builder which is used to build the modal.</param>
    /// <exception cref="InvalidOperationException">Thrown when the interaction response state is not <see cref="DiscordInteractionResponseState.Unacknowledged"/></exception>
    /// <exception cref="ArgumentException">Thrown when the response builder is not valid</exception>
    public async ValueTask RespondWithModalAsync(DiscordInteractionResponseBuilder builder)
    {
        if (this.Interaction.ResponseState is not DiscordInteractionResponseState.Unacknowledged)
        {
            throw new InvalidOperationException("Cannot respond to an interaction twice. Please use FollowupAsync instead.");
        }

        if (string.IsNullOrWhiteSpace(builder.CustomId))
        {
            throw new ArgumentException("Modal response has to have a custom id");
        }

        if (builder.Components.Any(x => x.Components.Any(y => y is not DiscordTextInputComponent)))
        {
            throw new ArgumentException("Modals currently only support TextInputComponents");
        }

        await this.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, builder);
    }

    /// <inheritdoc />
    public override ValueTask DeferResponseAsync() => DeferResponseAsync(false);

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
    public virtual ValueTask<DiscordMessage> FollowupAsync(string content, bool ephemeral)
        => FollowupAsync(new DiscordFollowupMessageBuilder().WithContent(content).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.FollowupAsync(DiscordEmbed)" />
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask<DiscordMessage> FollowupAsync(DiscordEmbed embed, bool ephemeral)
        => FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.FollowupAsync(string, DiscordEmbed)" />
    /// <param name="content">Content to send in the response.</param>
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask<DiscordMessage> FollowupAsync(string content, DiscordEmbed embed, bool ephemeral)
        => FollowupAsync(new DiscordFollowupMessageBuilder().WithContent(content).AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> FollowupAsync(IDiscordMessageBuilder builder)
    {
        DiscordFollowupMessageBuilder followupBuilder = builder is DiscordFollowupMessageBuilder messageBuilder
            ? messageBuilder
            : new DiscordFollowupMessageBuilder(builder);

        DiscordMessage message = await this.Interaction.CreateFollowupMessageAsync(followupBuilder);
        this.followupMessages.Add(message.Id, message);
        return message;
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder)
    {
        DiscordWebhookBuilder editedBuilder = builder as DiscordWebhookBuilder ?? new DiscordWebhookBuilder(builder);

        this.followupMessages[messageId] = await this.Interaction.EditFollowupMessageAsync(messageId, editedBuilder);
        return this.followupMessages[messageId];
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false)
    {
        // Fetch the follow up message if we don't have it cached.
        if (ignoreCache || !this.followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            message = await this.Interaction.GetFollowupMessageAsync(messageId);
            this.followupMessages[messageId] = message;
        }

        return message;
    }

    /// <inheritdoc />
    public override async ValueTask DeleteFollowupAsync(ulong messageId) => await this.Interaction.DeleteFollowupMessageAsync(messageId);
}
