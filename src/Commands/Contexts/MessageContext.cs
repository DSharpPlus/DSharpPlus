using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Commands.Contexts
{
    public sealed record MessageContext : CommandContext
    {
        public required DiscordMessage Message { get; init; }
        public DiscordMessage? Response { get; private set; }

        /// <inheritdoc />
        public override async ValueTask RespondAsync(IDiscordMessageBuilder builder)
        {
            DiscordMessageBuilder messageBuilder = new(builder);

            // Reply to the message that invoked the command if no reply is set
            if (messageBuilder.ReplyId is null)
            {
                messageBuilder.WithReply(Message.Id);
            }

            // Don't ping anyone if no mentions are explicitly set
            if (messageBuilder.Mentions?.Count is null or 0)
            {
                messageBuilder.WithAllowedMentions(Mentions.None);
            }

            Response = await Channel.SendMessageAsync(messageBuilder);
        }

        /// <inheritdoc />
        public override async ValueTask EditResponseAsync(IDiscordMessageBuilder builder)
        {
            if (Response is null)
            {
                throw new InvalidOperationException("Cannot edit a response that has not been sent yet.");
            }

            Response = await Response.ModifyAsync(new DiscordMessageBuilder(builder));
        }

        /// <inheritdoc />
        public override async ValueTask DeleteResponseAsync()
        {
            if (Response is null)
            {
                throw new InvalidOperationException("Cannot delete a response that has not been sent yet.");
            }

            await Response.DeleteAsync();
        }

        /// <inheritdoc />
        public override ValueTask<DiscordMessage?> GetResponseAsync() => ValueTask.FromResult(Response);

        /// <inheritdoc />
        public override async ValueTask DelayResponseAsync() => await Channel.TriggerTypingAsync();

        public override async ValueTask FollowupAsync(IDiscordMessageBuilder builder)
        {
            if (Response is null)
            {
                throw new InvalidOperationException("Cannot send a followup message before the initial response.");
            }

            DiscordMessageBuilder messageBuilder = new(builder);

            // Reply to the original message if no reply is set, to indicate that this message is related to the command
            if (messageBuilder.ReplyId is null)
            {
                messageBuilder.WithReply(Response.Id);
            }

            // Don't ping anyone if no mentions are explicitly set
            if (messageBuilder.Mentions?.Count is null or 0)
            {
                messageBuilder.WithAllowedMentions(Mentions.None);
            }

            DiscordMessage followup = await Channel.SendMessageAsync(messageBuilder);
            _followupMessages.Add(followup.Id, followup);
        }

        public override async ValueTask EditFollowupAsync(ulong messageId, IDiscordMessageBuilder builder)
        {
            if (Response is null)
            {
                throw new InvalidOperationException("Cannot edit a followup message before the initial response.");
            }

            if (!_followupMessages.TryGetValue(messageId, out DiscordMessage? message))
            {
                throw new InvalidOperationException("Cannot edit a followup message that does not exist.");
            }

            DiscordMessageBuilder messageBuilder = new(builder);
            _followupMessages[messageId] = await message.ModifyAsync(messageBuilder);
        }

        public override async ValueTask<DiscordMessage?> GetFollowupAsync(ulong messageId, bool ignoreCache = false)
        {
            if (Response is null)
            {
                throw new InvalidOperationException("Cannot get a followup message before the initial response.");
            }

            // Fetch the follow up message if we don't have it cached.
            if (ignoreCache || !_followupMessages.TryGetValue(messageId, out DiscordMessage? message))
            {
                message = await Channel.GetMessageAsync(messageId, true);
                _followupMessages[messageId] = message;
            }

            return message;
        }

        public override async ValueTask DeleteFollowupAsync(ulong messageId)
        {
            if (Response is null)
            {
                throw new InvalidOperationException("Cannot delete a followup message before the initial response.");
            }

            if (!_followupMessages.TryGetValue(messageId, out DiscordMessage? message))
            {
                throw new InvalidOperationException("Cannot delete a followup message that does not exist.");
            }

            await message.DeleteAsync();
        }
    }
}
