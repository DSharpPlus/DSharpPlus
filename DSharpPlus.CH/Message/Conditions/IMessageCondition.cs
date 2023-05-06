namespace DSharpPlus.CH.Message.Conditions;

public interface IMessageCondition
{
    public Task<bool> InvokeAsync(MessageContext context);
}
