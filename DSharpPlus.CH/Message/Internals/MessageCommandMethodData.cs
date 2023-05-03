namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandMethodData
{
    public required MessageCommandModuleData Module { get; set; }
    public required System.Reflection.MethodInfo Method { get; set; }
    public List<MessageCommandParameterData> Parameters { get; set; } = new();
    public bool IsAsync { get; set; } = false;
    public bool ReturnsNothing { get; set; } = false;
}
