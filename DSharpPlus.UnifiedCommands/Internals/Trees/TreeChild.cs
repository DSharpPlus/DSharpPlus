namespace DSharpPlus.UnifiedCommands.Internals.Trees;

internal class TreeChild<T> : ITreeChild<T>
{
    public string Key { get; set; } // These shouldn't be used when it's TreeChild
    public T? Value { get; set; }

    public virtual void AddValueAt(T value, ReadOnlySpan<char> span) 
    => throw new Exception();
    public virtual (ITreeChild<T>, int) Traverse(ReadOnlySpan<char> span) 
        => throw new Exception();
    public virtual ITreeChild<T> TraverseOnce(ReadOnlySpan<char> span, IReadOnlyList<(int, int)> positions, ref int location) 
        => throw new Exception();
}