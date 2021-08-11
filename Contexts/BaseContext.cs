using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Respresents a base context for application command contexts.
    /// </summary>
    public class BaseContext
    {
        /// <summary>
        /// Gets the interaction that was created.
        /// </summary>
        public DiscordInteraction Interaction { get; internal set; }

        /// <summary>
        /// Gets the client for this interaction.
        /// </summary>
        public DiscordClient Client { get; internal set; }

        /// <summary>
        /// Gets the guild this interaction was executed in.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the channel this interaction was executed in.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the user which executed this interaction.
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the member which executed this interaction, or null if the command is in a DM.
        /// </summary>
        public DiscordMember Member
            => this.User is DiscordMember member ? member : null;

        /// <summary>
        /// Gets the slash command module this interaction was created in.
        /// </summary>
        public SlashCommandsExtension SlashCommandsExtension { get; internal set; }

        /// <summary>
        /// Gets the token for this interaction.
        /// </summary>
        public string Token { get; internal set; }

        /// <summary>
        /// Gets the id for this interaction.
        /// </summary>
        public ulong InteractionId { get; internal set; }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string CommandName { get; internal set; }

        /// <summary>
        /// Gets the type of this interaction.
        /// </summary>
        public ApplicationCommandType Type { get; internal set;}

        /// <summary>
        /// <para>Gets the service provider.</para>
        /// <para>This allows passing data around without resorting to static members.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IServiceProvider Services { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

        /// <summary>
        /// Creates a response to this interaction.
        /// <para>You must create a response within 3 seconds of this interaction being executed; if the command has the potential to take more than 3 seconds, create a <see cref="InteractionResponseType.DeferredChannelMessageWithSource"/> at the start, and edit the response later.</para>
        /// </summary>
        /// <param name="type">The type of the response.</param>
        /// <param name="builder">The data to be sent, if any.</param>
        /// <returns></returns>
        public Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder builder = null)
            => this.Interaction.CreateResponseAsync(type, builder);

        /// <summary>
        /// Edits the interaction response.
        /// </summary>
        /// <param name="builder">The data to edit the response with.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns></returns>
        public Task<DiscordMessage> EditResponseAsync(DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
            => this.Interaction.EditOriginalResponseAsync(builder, attachments);

        /// <summary>
        /// Deletes the interaction response.
        /// </summary>
        /// <returns></returns>
        public Task DeleteResponseAsync()
            => this.Interaction.DeleteOriginalResponseAsync();

        /// <summary>
        /// Creates a follow up message to the interaction.
        /// </summary>
        /// <param name="builder">The message to be sent, in the form of a webhook.</param>
        /// <returns>The created message.</returns>
        public Task<DiscordMessage> FollowUpAsync(DiscordFollowupMessageBuilder builder)
            => this.Interaction.CreateFollowupMessageAsync(builder);

        /// <summary>
        /// Edits a followup message.
        /// </summary>
        /// <param name="followupMessageId">The id of the followup message to edit.</param>
        /// <param name="builder">The webhook builder.</param>
        /// <param name="attachments">Attached files to keep.</param>
        /// <returns></returns>
        public Task<DiscordMessage> EditFollowupAsync(ulong followupMessageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment> attachments = default)
            => this.Interaction.EditFollowupMessageAsync(followupMessageId, builder, attachments);

        /// <summary>
        /// Deletes a followup message.
        /// </summary>
        /// <param name="followupMessageId">The id of the followup message to delete.</param>
        /// <returns></returns>
        public Task DeleteFollowupAsync(ulong followupMessageId)
            => this.Interaction.DeleteFollowupMessageAsync(followupMessageId);

        /// <summary>
        /// Gets the original interaction response.
        /// </summary>
        /// <returns>The original interaction response.</returns>
        public Task<DiscordMessage> GetOriginalResponseAsync()
             => this.Interaction.GetOriginalResponseAsync();
    }
}
