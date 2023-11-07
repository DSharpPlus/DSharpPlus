using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public record SlashCommandContext : CommandContext
    {
        public required DiscordInteraction Interaction { get; init; }
        public required IEnumerable<DiscordInteractionDataOption> Options { get; init; }
        public InteractionState State { get; protected set; }

        /// <inheritdoc />
        public override async ValueTask RespondAsync(IDiscordMessageBuilder builder)
        {
            if (State.HasFlag(InteractionState.ResponseSent))
            {
                throw new InvalidOperationException("Cannot respond to an interaction twice. Please use FollowupAsync instead.");
            }

            DiscordInteractionResponseBuilder interactionBuilder = new(builder);

            // Don't ping anyone if no mentions are explicitly set
            if (interactionBuilder.Mentions?.Count is null or 0)
            {
                interactionBuilder.AddMentions(Mentions.None);
            }

            if (State is InteractionState.None)
            {
                await Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, interactionBuilder);
            }
            else if (State is InteractionState.ResponseDelayed)
            {
                await Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder(interactionBuilder));
            }

            State |= InteractionState.ResponseSent;
        }

        /// <inheritdoc />
        public override async ValueTask DelayResponseAsync()
        {
            await Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            State |= InteractionState.ResponseDelayed;
        }

        /// <inheritdoc />
        public override async ValueTask EditResponseAsync(IDiscordMessageBuilder builder)
        {
            if (!State.HasFlag(InteractionState.ResponseSent) && !State.HasFlag(InteractionState.ResponseDelayed))
            {
                throw new InvalidOperationException("Cannot edit a response that has not been sent yet.");
            }

            DiscordWebhookBuilder webhookBuilder = new(builder);
            await Interaction.EditOriginalResponseAsync(webhookBuilder);
        }

        /// <inheritdoc />
        public override async ValueTask DeleteResponseAsync()
        {
            if (!State.HasFlag(InteractionState.ResponseSent) || !State.HasFlag(InteractionState.ResponseDelayed))
            {
                throw new InvalidOperationException("Cannot delete a response that has not been sent yet.");
            }

            await Interaction.DeleteOriginalResponseAsync();
        }

        /// <inheritdoc />
        public override async ValueTask<DiscordMessage?> GetResponseAsync() => !State.HasFlag(InteractionState.ResponseSent)
            ? throw new InvalidOperationException("Cannot get a response that has not been properly sent yet.")
            : await Interaction.GetOriginalResponseAsync();

        /// <inheritdoc />
        public override async ValueTask FollowupAsync(IDiscordMessageBuilder builder)
        {
            if (!State.HasFlag(InteractionState.ResponseSent))
            {
                throw new InvalidOperationException("Cannot follow up to a response that has not been sent yet.");
            }

            DiscordFollowupMessageBuilder followupBuilder = new(builder);
            DiscordMessage message = await Interaction.CreateFollowupMessageAsync(followupBuilder);

            _followupMessages.Add(message.Id, message);
        }

        /// <inheritdoc />
        public override async ValueTask EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder)
        {
            if (!State.HasFlag(InteractionState.ResponseSent))
            {
                throw new InvalidOperationException("Cannot edit a follow up that has not been sent yet.");
            }

            // Fetch the follow up message if we don't have it cached.
            if (!_followupMessages.TryGetValue(messageId, out DiscordMessage? message))
            {
                message = await Channel.GetMessageAsync(messageId);
            }

            DiscordMessageBuilder messageBuilder = new(builder);
            _followupMessages[messageId] = await message.ModifyAsync(messageBuilder);
        }

        /// <inheritdoc />
        public override async ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false)
        {
            if (!State.HasFlag(InteractionState.ResponseSent))
            {
                throw new InvalidOperationException("Cannot get a follow up that has not been sent yet.");
            }

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

        [Flags]
        public enum InteractionState
        {
            None = 0,
            ResponseDelayed = 1 << 0,
            ResponseSent = 1 << 1
        }
    }
}
