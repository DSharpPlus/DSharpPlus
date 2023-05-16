using System.Collections.Concurrent;

namespace DSharpPlus.UnifiedCommands.Message.Conditions;

public class CooldownCondition : IMessageCondition
{
    public static ConcurrentDictionary<string, DateTimeOffset> Cooldown { get; set; } =
        new(); // VERY INEFFICIENT. CHANGE ASAP
    // This will hold expired times forever.

    public Task<bool> InvokeAsync(MessageContext context)
    {
        CooldownAttribute? attribute = context.Data.GetMetadata<CooldownAttribute>();
        if (attribute is null)
        {
            return Task.FromResult(true);
        }

        string key = $"{context.Data.Name}/{context.Message.Channel.Id}/{context.Message.Author.Id}";
        if (Cooldown.TryGetValue(key, out DateTimeOffset date))
        {
            if (date > DateTime.Now)
            {
                return Task.FromResult(false);
            }

            Cooldown.TryRemove(new KeyValuePair<string, DateTimeOffset>(key, date));
        }

        Cooldown.TryAdd(key, DateTimeOffset.Now.Add(attribute.Cooldown));
        return Task.FromResult(true);
    }
}
