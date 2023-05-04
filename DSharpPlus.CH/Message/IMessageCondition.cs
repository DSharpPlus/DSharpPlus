namespace DSharpPlus.CH.Message;

public interface IMessageCondition
{
    public Task<bool> InvokeAsync(MessageContext context);
}
