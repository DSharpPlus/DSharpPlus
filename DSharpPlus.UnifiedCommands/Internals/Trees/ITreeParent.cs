namespace DSharpPlus.UnifiedCommands.Internals.Trees;

internal interface ITreeParent<T> : ITreeChild<T>
{
    public List<ITreeChild<T>> List { get; set; }
}
