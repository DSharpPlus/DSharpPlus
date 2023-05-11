namespace DSharpPlus.UnifiedCommands.Message.Conditions;

[AttributeUsage(AttributeTargets.Method)]
public class CooldownAttribute : Attribute
{
    public TimeSpan Cooldown { get; set; }

    public CooldownAttribute(int seconds)
        => Cooldown = TimeSpan.FromSeconds(seconds);
}
