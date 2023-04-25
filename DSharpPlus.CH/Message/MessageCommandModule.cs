using DSharpPlus.CH.Message.Internals;

namespace DSharpPlus.CH.Message
{
    public abstract class MessageCommandModule
    {
        public DSharpPlus.Entities.DiscordMessage Message { get; set; } = null!;
        public DiscordClient Client { get; set; } = null!;

        internal MessageCommandHandler _handler = null!; // Will be set by the factory.

        protected async Task Post(IMessageCommandModuleResult result)
        {
            await _handler.TurnResultIntoAction(result);
        }

        protected IMessageCommandModuleResult Reply(MessageCommandModuleResult result, bool mention = false)
        {
            if (mention)
                result.Type = MessageCommandModuleResultType.Reply;
            else
                result.Type = MessageCommandModuleResultType.NoMentionReply;
            return result;
        }

        protected IMessageCommandModuleResult FollowUp(MessageCommandModuleResult result)
        {
            result.Type = MessageCommandModuleResultType.FollowUp;
            return result;
        }

        protected IMessageCommandModuleResult Edit(MessageCommandModuleResult result)
        {
            result.Type = MessageCommandModuleResultType.Edit;
            return result;
        }

        protected IMessageCommandModuleResult Empty()
        {
            return new MessageCommandModuleResult
            {
                Type = MessageCommandModuleResultType.Empty
            };
        }

        protected IMessageCommandModuleResult Send(MessageCommandModuleResult result)
        {
            result.Type = MessageCommandModuleResultType.Send;
            return result;
        }
    }
}