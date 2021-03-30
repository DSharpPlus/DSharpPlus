using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an interaction that was invoked.
    /// </summary>
    public sealed class DiscordInteraction : SnowflakeObject
    {
        /// <summary>
        /// Gets the type of interaction invoked. 
        /// </summary>
        [JsonProperty("type")]
        public InteractionType Type { get; internal set; }

        /// <summary>
        /// Gets the command data for this interaction.
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionData Data { get; internal set; }

        /// <summary>
        /// Gets the Id of the guild that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public ulong? GuildId { get; internal set; }

        /// <summary>
        /// Gets the guild that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

        /// <summary>
        /// Gets the Id of the channel that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the channel that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel
            => (this.Discord as DiscordClient).InternalGetCachedChannel(this.ChannelId);

        /// <summary>
        /// Gets the user that invoked this interaction.
        /// <para>This can be cast to a <see cref="DiscordMember"/> if created in a guild.</para>
        /// </summary>
        [JsonIgnore]
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the continuation token for responding to this interaction.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; internal set; }

        /// <summary>
        /// Gets the version number for this interaction type.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; internal set; }

        /// <summary>
        /// Gets the ID of the application that created this interaction.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }

        /// <summary>
        /// Creates a response to this interaction.
        /// </summary>
        /// <param name="type">The type of the response.</param>
        /// <param name="builder">The data, if any, to send.</param>
        public Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder builder = null) =>
            this.Discord.ApiClient.CreateInteractionResponseAsync(this.Id, this.Token, type, builder);

        /// <summary>
        /// Edits the original interaction response.
        /// </summary>
        /// <param name="builder">The webhook builder.</param>
        /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
        public async Task<DiscordMessage> EditOriginalResponseAsync(DiscordWebhookBuilder builder)
        {
            builder.Validate(isInteractionResponse: true);

            return await this.Discord.ApiClient.EditOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token, builder);
        }

        /// <summary>
        /// Deletes the original interaction response.
        /// </summary>>
        public Task DeleteOriginalResponseAsync() =>
            this.Discord.ApiClient.DeleteOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token);

        /// <summary>
        /// Creates a follow up message to this interaction.
        /// </summary>
        /// <param name="builder">The webhook builder.</param>
        /// <returns>The <see cref="DiscordMessage"/> created.</returns>
        public async Task<DiscordMessage> CreateFollowupMessageAsync(DiscordWebhookBuilder builder)
        {
            builder.Validate(isFollowup: true);

            return await this.Discord.ApiClient.CreateFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, builder);
        }

        /// <summary>
        /// Edits a follow up message.
        /// </summary>
        /// <param name="messageId">The id of the follow up message.</param>
        /// <param name="builder">The webhook builder.</param>
        /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
        public async Task<DiscordMessage> EditFollowupMessageAsync(ulong messageId, DiscordWebhookBuilder builder)
        {
            builder.Validate(isFollowup: true);

            return await this.Discord.ApiClient.EditFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId, builder);
        }

        /// <summary>
        /// Deletes a follow up message.
        /// </summary>
        /// <param name="messageId">The id of the follow up message.</param>
        public Task DeleteFollowupMessageAsync(ulong messageId) =>
            this.Discord.ApiClient.DeleteFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId);
    }
}
