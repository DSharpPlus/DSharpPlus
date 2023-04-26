using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Permission;

public class PermissionMiddleware : IMessageMiddleware
{
    private NextDelegate _next;

    public PermissionMiddleware(NextDelegate next) => _next = next;

    public async Task InvokeAsync(MessageContext context)
    {
        MessagePermissionAttribute? metadata = context.Data.GetMetadata<MessagePermissionAttribute>();
        if (metadata is null)
        {
            await _next(context);
            return;
        }

        if (context.Message.Channel.GuildId is null)
        {
            DiscordMessageBuilder msgBuilder = new();
            msgBuilder.WithReply(context.Message.Id);
            msgBuilder.WithContent("This command can only be used in a guild.");
            await context.Message.Channel.SendMessageAsync(msgBuilder);
        }

        DiscordMember member = await context.Message.Channel.Guild.GetMemberAsync(context.Message.Author.Id);

        if ((member.Permissions & metadata.Permissions) != 0 || (member.Permissions & Permissions.Administrator) != 0)
        {
            await _next(context);
        }
        else
        {
            DiscordMessageBuilder msgBuilder = new();
            msgBuilder.WithContent("You do not have enough permissions to use this command.");
            msgBuilder.WithReply(context.Message.Id);
            await context.Message.Channel.SendMessageAsync(msgBuilder);
        }
    }
}
