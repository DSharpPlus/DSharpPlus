namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageMethodData
{
    public required MessageModuleData Module { get; set; }
    public required System.Reflection.MethodInfo Method { get; set; }
    public List<MessageParameterData> Parameters { get; set; } = new();
    public bool IsAsync { get; set; } = false;
    public bool ReturnsNothing { get; set; } = false;
}
