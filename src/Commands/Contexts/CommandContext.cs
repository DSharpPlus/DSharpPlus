using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Commands
{
    public abstract record CommandContext
    {
        public required DiscordUser User { get; init; }
        public required DiscordChannel Channel { get; init; }
        public required CommandAllExtension Extension { get; init; }
        public required Command Command { get; init; }
        public required IReadOnlyDictionary<CommandArgument, object?> Arguments { get; init; }

        public IReadOnlyDictionary<ulong, DiscordMessage> FollowupMessages => _followupMessages;
        protected Dictionary<ulong, DiscordMessage> _followupMessages = new();

        public DiscordGuild? Guild => Channel.Guild;
        public DiscordMember? Member => User as DiscordMember;
        public DiscordClient Client => Extension.Client;

        public virtual ValueTask RespondAsync(string content) => RespondAsync(new DiscordMessageBuilder().WithContent(content));
        public virtual ValueTask RespondAsync(DiscordEmbed embed) => RespondAsync(new DiscordMessageBuilder().WithEmbed(embed));
        public virtual ValueTask RespondAsync(string content, DiscordEmbed embed) => RespondAsync(new DiscordMessageBuilder().WithContent(content).WithEmbed(embed));
        public abstract ValueTask RespondAsync(IDiscordMessageBuilder builder);

        public virtual ValueTask EditResponseAsync(string content) => EditResponseAsync(new DiscordMessageBuilder().WithContent(content));
        public virtual ValueTask EditResponseAsync(DiscordEmbed embed) => EditResponseAsync(new DiscordMessageBuilder().WithEmbed(embed));
        public virtual ValueTask EditResponseAsync(string content, DiscordEmbed embed) => EditResponseAsync(new DiscordMessageBuilder().WithContent(content).WithEmbed(embed));
        public abstract ValueTask EditResponseAsync(IDiscordMessageBuilder builder);

        public abstract ValueTask<DiscordMessage?> GetResponseAsync();
        public abstract ValueTask DelayResponseAsync();
        public abstract ValueTask DeleteResponseAsync();

        public virtual ValueTask FollowupAsync(string content) => FollowupAsync(new DiscordMessageBuilder().WithContent(content));
        public virtual ValueTask FollowupAsync(DiscordEmbed embed) => FollowupAsync(new DiscordMessageBuilder().WithEmbed(embed));
        public virtual ValueTask FollowupAsync(string content, DiscordEmbed embed) => FollowupAsync(new DiscordMessageBuilder().WithContent(content).WithEmbed(embed));
        public abstract ValueTask FollowupAsync(IDiscordMessageBuilder builder);

        public virtual ValueTask EditFollowupAsync(ulong messageId, string content) => EditFollowupAsync(messageId, new DiscordMessageBuilder().WithContent(content));
        public virtual ValueTask EditFollowupAsync(ulong messageId, DiscordEmbed embed) => EditFollowupAsync(messageId, new DiscordMessageBuilder().WithEmbed(embed));
        public virtual ValueTask EditFollowupAsync(ulong messageId, string content, DiscordEmbed embed) => EditFollowupAsync(messageId, new DiscordMessageBuilder().WithContent(content).WithEmbed(embed));
        public abstract ValueTask EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder);

        public abstract ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false);
        public abstract ValueTask DeleteFollowupAsync(ulong messageId);
    }
}
