using System;
using System.Collections.Generic;
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
    public virtual ValueTask RespondAsync(string content, bool ephemeral)
        => RespondAsync(new DiscordInteractionResponseBuilder().WithContent(content).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.RespondAsync(DiscordEmbed)" />
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask RespondAsync(DiscordEmbed embed, bool ephemeral)
        => RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc cref="CommandContext.RespondAsync(string, DiscordEmbed)" />
    /// <param name="content">Content to send in the response.</param>
    /// <param name="embed">Embed to send in the response.</param>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public virtual ValueTask RespondAsync(string content, DiscordEmbed embed, bool ephemeral)
        => RespondAsync(new DiscordInteractionResponseBuilder().WithContent(content).AddEmbed(embed).AsEphemeral(ephemeral));

    /// <inheritdoc />
    public override async ValueTask RespondAsync(IDiscordMessageBuilder builder)
    {
        if (Interaction.ResponseState is DiscordInteractionResponseState.Replied)
        {
            throw new InvalidOperationException("Cannot respond to an interaction twice. Please use FollowupAsync instead.");
        }

        DiscordInteractionResponseBuilder interactionBuilder = builder as DiscordInteractionResponseBuilder ?? new(builder);

        // Don't ping anyone if no mentions are explicitly set
        if (interactionBuilder.Mentions.Count is 0)
        {
            interactionBuilder.AddMentions(Mentions.None);
        }

        if (Interaction.ResponseState is DiscordInteractionResponseState.Unacknowledged)
        {
            await Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, interactionBuilder);
        }
        else if (Interaction.ResponseState is DiscordInteractionResponseState.Deferred)
        {
            await Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder(interactionBuilder));
        }
    }

    /// <inheritdoc />
    public override ValueTask DeferResponseAsync() => DeferResponseAsync(false);

    /// <inheritdoc cref="DeferResponseAsync()"/>
    /// <param name="ephemeral">Specifies whether this response should be ephemeral.</param>
    public async ValueTask DeferResponseAsync(bool ephemeral) => await Interaction.DeferAsync(ephemeral);

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> EditResponseAsync(IDiscordMessageBuilder builder)
        => await Interaction.EditOriginalResponseAsync(builder as DiscordWebhookBuilder ?? new(builder));

    /// <inheritdoc />
    public override async ValueTask DeleteResponseAsync() => await Interaction.DeleteOriginalResponseAsync();

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage?> GetResponseAsync() => await Interaction.GetOriginalResponseAsync();

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

        DiscordMessage message = await Interaction.CreateFollowupMessageAsync(followupBuilder);
        _followupMessages.Add(message.Id, message);
        return message;
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder)
    {
        // Fetch the follow up message if we don't have it cached.
        if (!_followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            message = await Channel.GetMessageAsync(messageId);
        }

        DiscordMessageBuilder editedBuilder = builder is DiscordMessageBuilder messageBuilder
            ? messageBuilder
            : new DiscordMessageBuilder(builder);

        _followupMessages[messageId] = await message.ModifyAsync(editedBuilder);
        return _followupMessages[messageId];
    }

    /// <inheritdoc />
    public override async ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false)
    {
        // Fetch the follow up message if we don't have it cached.
        if (ignoreCache || !_followupMessages.TryGetValue(messageId, out DiscordMessage? message))
        {
            message = await Interaction.GetFollowupMessageAsync(messageId);
            _followupMessages[messageId] = message;
        }

        return message;
    }

    /// <inheritdoc />
    public override async ValueTask DeleteFollowupAsync(ulong messageId) => await Interaction.DeleteFollowupMessageAsync(messageId);
}
