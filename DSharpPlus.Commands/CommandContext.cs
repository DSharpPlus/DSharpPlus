namespace DSharpPlus.Commands;

using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

public abstract record CommandContext : AbstractContext
{
    public required IReadOnlyDictionary<CommandParameter, object?> Arguments { get; init; }

    public IReadOnlyDictionary<ulong, DiscordMessage> FollowupMessages => this._followupMessages;
    protected Dictionary<ulong, DiscordMessage> _followupMessages = [];

    public virtual ValueTask RespondAsync(string content) => this.RespondAsync(new DiscordMessageBuilder().WithContent(content));
    public virtual ValueTask RespondAsync(DiscordEmbed embed) => this.RespondAsync(new DiscordMessageBuilder().AddEmbed(embed));
    public virtual ValueTask RespondAsync(string content, DiscordEmbed embed) => this.RespondAsync(new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));
    public abstract ValueTask RespondAsync(IDiscordMessageBuilder builder);

    public virtual ValueTask<DiscordMessage> EditResponseAsync(string content) => this.EditResponseAsync(new DiscordMessageBuilder().WithContent(content));
    public virtual ValueTask<DiscordMessage> EditResponseAsync(DiscordEmbed embed) => this.EditResponseAsync(new DiscordMessageBuilder().AddEmbed(embed));
    public virtual ValueTask<DiscordMessage> EditResponseAsync(string content, DiscordEmbed embed) => this.EditResponseAsync(new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));
    public abstract ValueTask<DiscordMessage> EditResponseAsync(IDiscordMessageBuilder builder);

    public abstract ValueTask<DiscordMessage?> GetResponseAsync();
    public abstract ValueTask DeferResponseAsync();
    public abstract ValueTask DeleteResponseAsync();

    public virtual ValueTask<DiscordMessage> FollowupAsync(string content) => this.FollowupAsync(new DiscordMessageBuilder().WithContent(content));
    public virtual ValueTask<DiscordMessage> FollowupAsync(DiscordEmbed embed) => this.FollowupAsync(new DiscordMessageBuilder().AddEmbed(embed));
    public virtual ValueTask<DiscordMessage> FollowupAsync(string content, DiscordEmbed embed) => this.FollowupAsync(new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));
    public abstract ValueTask<DiscordMessage> FollowupAsync(IDiscordMessageBuilder builder);

    public virtual ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, string content) => this.EditFollowupAsync(messageId, new DiscordMessageBuilder().WithContent(content));
    public virtual ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, DiscordEmbed embed) => this.EditFollowupAsync(messageId, new DiscordMessageBuilder().AddEmbed(embed));
    public virtual ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, string content, DiscordEmbed embed) => this.EditFollowupAsync(messageId, new DiscordMessageBuilder().WithContent(content).AddEmbed(embed));
    public abstract ValueTask<DiscordMessage> EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder);

    public abstract ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false);
    public abstract ValueTask DeleteFollowupAsync(ulong messageId);

    public T As<T>() where T : CommandContext => (T)this;
}
