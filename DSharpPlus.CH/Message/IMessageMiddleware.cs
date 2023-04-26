namespace DSharpPlus.CH.Message;

public delegate Task NextDelegate(MessageContext context);

public interface IMessageMiddleware
{
    public Task InvokeAsync(MessageContext context);
}
