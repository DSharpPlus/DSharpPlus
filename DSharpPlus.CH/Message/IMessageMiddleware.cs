namespace DSharpPlus.CH.Message;

public interface IMessageMiddleware
{
    public Task<bool> InvokeAsync(MessageContext context);
}
