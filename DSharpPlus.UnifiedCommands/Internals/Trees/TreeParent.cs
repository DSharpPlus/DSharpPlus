namespace DSharpPlus.UnifiedCommands.Internals.Trees;

internal class TreeParent<T> : TreeChild<T>, ITreeParent<T>
{
    public TreeParent(string key) => this.Key = key;

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
                    ITreeChild<T> child = currentChild.List[ii];
                    if (spanPart.Equals(child.Key.AsSpan(), StringComparison.Ordinal))
                    {
                        found = true;
                        if (child is ITreeParent<T> p)
                        {
                            currentChild = p;
                        }
                        else
                        {
                            TreeParent<T> parent = new(child.Key);
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

        TreeChild<T> childWithValue = new()
        {
            Key = span[last..span.Length].ToString(),
            Value = value
        };
        currentChild.List.Add(childWithValue);
    }

    public override (ITreeChild<T>, int) Traverse(ReadOnlySpan<char> span)
    {
        List<(int, int)> positions = new();

        int last = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ' ')
            {
                positions.Add(new(last, i));
                last = i + 1;
            }
        }
        positions.Add((last, span.Length));

        int location = 0;
        ITreeChild<T> treeChild = TraverseOnce(span, positions.AsReadOnly(), ref location);

        return (treeChild, location);
    }

    public override ITreeChild<T> TraverseOnce(ReadOnlySpan<char> span, IReadOnlyList<(int, int)> positions, ref int location)
    {
        (int min, int max) = positions[location];
        ReadOnlySpan<char> key = span[min..max];

        Console.WriteLine($"Looking for key {key}");
        foreach (ITreeChild<T> item in List)
        {
            Console.WriteLine($"Item key is {item.Key}");
            if (item is ITreeParent<T> p && p.List.Count != 0)
            {
                Console.WriteLine($"First node in the item is {p.List[0].Key} and contains {p.List.Count} childs");
            }

            if (!key.Equals(item.Key.AsSpan(), StringComparison.Ordinal))
            {
                continue;
            }

            try
            {
                location++;
                return (positions.Count - 1) == location
                    ? item
                    : item is not ITreeParent<T>
                    ? item
                    : item.TraverseOnce(span, positions, ref location);
            }
            catch (KeyNotFoundException)
            {
                return item;
            }
        }

        throw new KeyNotFoundException("Couldn't find the item.");
    }
}