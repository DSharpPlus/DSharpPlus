using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message.Conditions;

public class PermissionCondition : IMessageCondition
{
    public static PermissionConditionConfiguration Configuration = null!;

    public async Task<bool> InvokeAsync(MessageContext context)
    {
        MessagePermissionAttribute? metadata = context.Data.GetMetadata<MessagePermissionAttribute>();
        if (metadata is null)
        {
            return true;
        }

        if (context.Message.Channel.GuildId is null)
        {
            return false;
        }

        DiscordMember member = await context.Message.Channel.Guild.GetMemberAsync(context.Message.Author.Id);

        if (member.Permissions.HasPermission(metadata.Permissions))
        {
            return true;
        }
        else
        {
            if (Configuration.MessageFunc is not null && Configuration.SendAMessage)
            {
                await context.Message.Channel.SendMessageAsync(Configuration.MessageFunc(context));
            }
            else if (!Configuration.SendAMessage)
            {
            }
            return false;
        }
    }
}
