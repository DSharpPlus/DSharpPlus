namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageTree
{
    public MessageMethodData? Data { get; set; }
    public Dictionary<string, MessageTree>? Branches { get; set; }

    public MessageTree(MessageMethodData data) =>
        Data = data;

    public MessageTree() => Branches = new();
}
