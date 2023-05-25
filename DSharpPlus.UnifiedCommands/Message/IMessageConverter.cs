namespace DSharpPlus.UnifiedCommands.Message;

public interface IMessageConverter<T>
{
    public T ConvertValue(DiscordClient client, string? message);

    public ValueTask<T> ConvertValueAsync(DiscordClient client, string? message)
        => ValueTask.FromResult(ConvertValue(client, message));
}
