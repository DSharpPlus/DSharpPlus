using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message.Conditions;

public class PermissionConditionConfiguration
{
    public Func<MessageContext, DiscordMessageBuilder>? MessageFunc;
    public bool SendAMessage = true;
}
