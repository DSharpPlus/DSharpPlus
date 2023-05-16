namespace DSharpPlus.UnifiedCommands.Message.Conditions;

public class RequireGuildCondition : IMessageCondition
{
    public Task<bool> InvokeAsync(MessageContext context)
        => Task.FromResult(context.Message.Channel.GuildId is not null);
}
