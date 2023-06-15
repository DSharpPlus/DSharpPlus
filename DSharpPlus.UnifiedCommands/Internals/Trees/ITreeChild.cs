namespace DSharpPlus.UnifiedCommands.Internals.Trees;

internal interface ITreeChild<T>
{
    internal (ITreeChild<T>, int) Traverse(ReadOnlySpan<char> span, int depth = 0);

    internal void AddValueAt(T value, ReadOnlySpan<char> span);


    public string? Key { get; set; }

    /// <summary>
    /// The value of the tree parent
    /// </summary>
    /// <remarks>
    /// This should never be null unless it as a <see cref="ITreeParent{T}">ITreeParent</see>!!
    /// </remarks>
    public T? Value { get; set; }
}
