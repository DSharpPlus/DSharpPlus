namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandTree
{
    public MessageCommandMethodData? Data { get; set; }
    public Dictionary<string, MessageCommandTree>? Branches { get; set; }

    public MessageCommandTree(MessageCommandMethodData data) =>
        Data = data;

    public MessageCommandTree() => Branches = new();
}