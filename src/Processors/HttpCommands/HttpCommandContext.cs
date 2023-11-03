using System;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public record HttpCommandContext : SlashCommandContext
    {
        public required HttpListenerContext HttpContext { get; init; }

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
    }
}
