namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// Tells the register that a parameter should get rest of the arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class RemainingArgumentsAttribute : Attribute
{
}
