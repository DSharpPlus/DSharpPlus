namespace DSharpPlus.UnifiedCommands.Message.Conditions;

/// <summary>
/// Interface used to built your own conditions. Parameter can contain anything as condition construction uses dependency injection.
/// </summary>
public interface IMessageCondition
{
    public Task<bool> InvokeAsync(MessageContext context);
}
