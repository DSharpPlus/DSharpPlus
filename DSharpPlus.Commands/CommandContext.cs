namespace DSharpPlus.Commands;

using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Represents a base context for application command contexts.
/// </summary>
public abstract record CommandContext : AbstractContext
{
    /// <summary>
    /// The command arguments.
    /// </summary>
    public required IReadOnlyDictionary<CommandParameter, object?> Arguments { get; init; }

    /// <summary>
    /// The followup messages sent from this interaction.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordMessage> FollowupMessages => this._followupMessages;
    protected Dictionary<ulong, DiscordMessage> _followupMessages = [];

    /// <inheritdoc cref="RespondAsync(string, DiscordEmbed)"/>
    public virtual ValueTask RespondAsync(string content) => this.RespondAsync(new DiscordMessageBuilder().WithContent(content));

    /// <inheritdoc cref="RespondAsync(string, DiscordEmbed)"/>
    public virtual ValueTask RespondAsync(DiscordEmbed embed) => this.RespondAsync(new DiscordMessageBuilder().AddEmbed(embed));

    /// <summary>
    /// Creates a response to this interaction.
    /// <para>You must create a response within 3 seconds of this interaction being executed; if the command has the potential to take more than 3 seconds, use <see cref="DeferResponseAsync"/> at the start, and edit the response later.</para>
    /// </summary>
    /// <param name="content">Content to send in the response.</param>
    /// <param name="embed">Embed to send in the response.</param>
    public virtual ValueTask RespondAsync(string content, DiscordEmbed embed) => this.RespondAsync(new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));

    /// <inheritdoc cref="RespondAsync(string, DiscordEmbed)"/>
    /// <param name="builder">The message builder.</param>
    public abstract ValueTask RespondAsync(IDiscordMessageBuilder builder);


    /// <inheritdoc cref="EditResponseAsync(string, DiscordEmbed)"/>
    public virtual ValueTask<DiscordMessage> EditResponseAsync(string content) => this.EditResponseAsync(new DiscordMessageBuilder().WithContent(content));

    /// <inheritdoc cref="EditResponseAsync(string, DiscordEmbed)"/>
    public virtual ValueTask<DiscordMessage> EditResponseAsync(DiscordEmbed embed) => this.EditResponseAsync(new DiscordMessageBuilder().AddEmbed(embed));

    /// <summary>
    /// Edits the response.
    /// </summary>
    /// <param name="content">Content to send in the response.</param>
    /// <param name="embed">Embed to send in the response.</param>
    public virtual ValueTask<DiscordMessage> EditResponseAsync(string content, DiscordEmbed embed)
        => this.EditResponseAsync(new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));

    /// <inheritdoc cref="EditResponseAsync(string, DiscordEmbed)"/>
    /// <param name="builder">The message builder.</param>
    public abstract ValueTask<DiscordMessage> EditResponseAsync(IDiscordMessageBuilder builder);

    /// <summary>
    /// Gets the sent response.
    /// </summary>
    /// <returns>The sent response.</returns>
    public abstract ValueTask<DiscordMessage?> GetResponseAsync();

    /// <summary>
    /// Creates a deferred response to this interaction.
    /// </summary>
    public abstract ValueTask DeferResponseAsync();

    /// <summary>
    /// Deletes the sent response.
    /// </summary>
    public abstract ValueTask DeleteResponseAsync();

    /// <inheritdoc cref="FollowupAsync(string, DiscordEmbed)"/>
    public virtual ValueTask<DiscordMessage> FollowupAsync(string content) => this.FollowupAsync(new DiscordMessageBuilder().WithContent(content));

    /// <inheritdoc cref="FollowupAsync(string, DiscordEmbed)"/>
    public virtual ValueTask<DiscordMessage> FollowupAsync(DiscordEmbed embed) => this.FollowupAsync(new DiscordMessageBuilder().AddEmbed(embed));

    /// <summary>
    /// Creates a followup message to the interaction.
    /// </summary>
    /// <param name="content">Content to send in the followup message.</param>
    /// <param name="embed">Embed to send in the followup message.</param>
    /// <returns>The created message.</returns>
    public virtual ValueTask<DiscordMessage> FollowupAsync(string content, DiscordEmbed embed)
        => this.FollowupAsync(new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));

    /// <inheritdoc cref="FollowupAsync(string, DiscordEmbed)"/>
    /// <param name="builder">The followup message to be sent.</param>
    public abstract ValueTask<DiscordMessage> FollowupAsync(IDiscordMessageBuilder builder);

    /// <inheritdoc cref="EditFollowupAsync(ulong, string, DiscordEmbed)"/>
    public virtual ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, string content)
        => this.EditFollowupAsync(messageId, new DiscordMessageBuilder().WithContent(content));

    /// <inheritdoc cref="EditFollowupAsync(ulong, string, DiscordEmbed)"/>
    public virtual ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, DiscordEmbed embed)
        => this.EditFollowupAsync(messageId, new DiscordMessageBuilder().AddEmbed(embed));

    /// <summary>
    /// Edits a followup message.
    /// </summary>
    /// <param name="messageId">The id of the followup message to edit.</param>
    /// <param name="content">Content to send in the followup message.</param>
    /// <param name="embed">Embed to send in the followup message.</param>
    /// <returns>The edited message.</returns>
    public virtual ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, string content, DiscordEmbed embed)
        => this.EditFollowupAsync(messageId, new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));

    /// <inheritdoc cref="EditFollowupAsync(ulong, string, DiscordEmbed)"/>
    /// <param name="messageId">The id of the followup message to edit.</param>
    /// <param name="builder">The message builder.</param>
    public abstract ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder);

    /// <summary>
    /// Gets a sent followup message from this interaction.
    /// </summary>
    /// <param name="messageId">The id of the followup message to edit.</param>
    /// <param name="ignoreCache">Whether to ignore the cache and fetch the message from Discord.</param>
    /// <returns>The message.</returns>
    public abstract ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false);

    /// <summary>
    /// Deletes a followup message sent from this interaction.
    /// </summary>
    /// <param name="messageId">The id of the followup message to delete.</param>
    public abstract ValueTask DeleteFollowupAsync(ulong messageId);

    /// <summary>
    /// Cast this context to a different one.
    /// </summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <returns>This context as T.</returns>
    public T As<T>() where T : CommandContext => (T)this;
}
