namespace DSharpPlus.CH.Message.Permission
{
    public class PermissionMiddleware : IMessageMiddleware
    {
        private NextDelegate _next;

        public PermissionMiddleware(NextDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(MessageContext context)
        {
            var metadata = context.Data.GetMetadata<MessagePermissionAttribute>();
            if (metadata is null)
            {
                await _next(context);
                return;
            }
            if (context.Message.Channel.GuildId is null)
            {
                var msgBuilder = new Entities.DiscordMessageBuilder();
                msgBuilder.WithReply(context.Message.Id);
                msgBuilder.WithContent("This command can only be used in a guild.");
                await context.Message.Channel.SendMessageAsync(msgBuilder);
            }

            var member = await context.Message.Channel.Guild.GetMemberAsync(context.Message.Author.Id);

            if ((member.Permissions & metadata.Permissions) != 0 || (member.Permissions & Permissions.Administrator) != 0)
            {
                await _next(context);
            }
            else
            {
                var msgBuilder = new Entities.DiscordMessageBuilder();
                msgBuilder.WithContent("You do not have enough permissions to use this command.");
                msgBuilder.WithReply(context.Message.Id);
                await context.Message.Channel.SendMessageAsync(msgBuilder);
                return;
            }
        }
    }
}