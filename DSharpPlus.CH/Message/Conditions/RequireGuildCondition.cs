namespace DSharpPlus.CH.Message.Conditions;

public class RequireGuildCondition : IMessageCondition
{
    public Task<bool> InvokeAsync(MessageContext context)
    {
        if (context.Message.Channel.GuildId is null)
        {
            return Task.FromResult(false);
        }
        else
        {
            return Task.FromResult(true);
        }
    }
}
