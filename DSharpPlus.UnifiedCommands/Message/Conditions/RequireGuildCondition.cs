namespace DSharpPlus.UnifiedCommands.Message.Conditions;

public class RequireGuildCondition : IMessageCondition
{
    public ValueTask<bool> InvokeAsync(MessageContext context)
        => ValueTask.FromResult(context.Message.Channel.GuildId is not null);
}
