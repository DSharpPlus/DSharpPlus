using System.Collections.Concurrent;

namespace DSharpPlus.UnifiedCommands.Message.Conditions;

public class CooldownCondition : IMessageCondition
{
    public static ConcurrentDictionary<string, DateTimeOffset> Cooldown { get; set; } =
        new(); // TODO: Improve this system so keys aren't stored forever.

    public ValueTask<bool> InvokeAsync(MessageContext context)
    {
        CooldownAttribute? attribute = context.Data.GetMetadata<CooldownAttribute>();
        if (attribute is null)
        {
            return ValueTask.FromResult(true);
        }

        string key = $"{context.Data.Name}/{context.Message.Channel.Id}/{context.Message.Author.Id}";
        if (Cooldown.TryGetValue(key, out DateTimeOffset date))
        {
            if (date > DateTime.Now)
            {
                return ValueTask.FromResult(false);
            }

            Cooldown.TryRemove(new KeyValuePair<string, DateTimeOffset>(key, date));
        }

        Cooldown.TryAdd(key, DateTimeOffset.Now.Add(attribute.Cooldown));
        return ValueTask.FromResult(true);
    }
}
