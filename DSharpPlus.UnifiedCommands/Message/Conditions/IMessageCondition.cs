namespace DSharpPlus.UnifiedCommands.Message.Conditions;

public interface IMessageCondition
{
    public Task<bool> InvokeAsync(MessageContext context);
}
