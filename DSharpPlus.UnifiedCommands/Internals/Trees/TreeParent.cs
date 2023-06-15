namespace DSharpPlus.UnifiedCommands.Internals.Trees;

internal class TreeParent<T> : TreeChild<T>, ITreeParent<T>
{
    public TreeParent(string? key = null) => this.Key = key;

    public List<ITreeChild<T>> List { get; set; } = new();

    public override void AddValueAt(T value, ReadOnlySpan<char> span)
    {
        ITreeParent<T> currentChild = this;
        int last = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ' ')
            {
                bool found = false;
                ReadOnlySpan<char> spanPart = span[last..i];
                for (int ii = 0; ii < currentChild.List.Count; ii++)
                {
                    // itChild = iteration child
                    ITreeChild<T> itChild = currentChild.List[ii];
                    if (spanPart.Equals(itChild.Key.AsSpan(), StringComparison.Ordinal))
                    {
                        found = true;
                        if (itChild is ITreeParent<T> p)
                        {
                            currentChild = p;
                        }
                        else
                        {
                            TreeParent<T> parent = new(itChild.Key);
                            currentChild.List[ii] = parent;
                            currentChild = parent;
                        }
                        break;
                    }
                    continue;
                }

                if (!found)
                {
                    TreeParent<T> parent = new(spanPart.ToString());
                    currentChild.List.Add(parent);
                    currentChild = parent;
                }
                last = i + 1;
            }
        }

        ITreeChild<T>? child = null;
        int childIt = 0;
        for (int i = 0; i < currentChild.List.Count; i++)
        {
            ITreeChild<T> candidateChild = currentChild.List[i];
            if (span[last..].Equals(candidateChild.Key.AsSpan(), StringComparison.Ordinal))
            {
                child = candidateChild;
                childIt = i;
                break;
            }
        }

        // This is here to allow multiple values for one key if that key already exist
        TreeChild<T> childWithValue = new()
        {
            Key = span[last..].ToString(),
            Value = value
        };
        if (child is null)
        {
            currentChild.List.Add(childWithValue);
        }
        else
        {
            TreeParent<T> parent = new(child.Key);
            childWithValue.Key = null;
            child.Key = null;
            parent.List.Add(child);
            parent.List.Add(childWithValue);
            currentChild.List[childIt] = parent;
        }
    }

    public override (ITreeChild<T>, int) Traverse(ReadOnlySpan<char> span, int depth = 0)
    {
        int space = span.IndexOf(' ');

        ReadOnlySpan<char> key;
        if (space == -1)
        {
            key = span;
            span = default;
        }
        else
        {
            key = span[..space];
            span = span[(space + 1)..];
        }

        foreach (ITreeChild<T> item in List)
        {
            if (!key.Equals(item.Key.AsSpan(), StringComparison.Ordinal))
            {
                continue;
            }

            depth += 1;
            return space == -1 || item is not ITreeParent<T>
                ? (item, depth)
                : item.Traverse(span, depth);
        }

        throw new KeyNotFoundException("Couldn't find the item.");
    }
}