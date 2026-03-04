using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DSharpPlus.Voice.MemoryServices.Collections;

internal sealed class SynchronizedList<T> : IReadOnlyList<T>
{
    private readonly List<T> backing = [];
    private readonly Lock @lock = new();

    public void Add(T item)
    {
        lock (this.@lock)
        {
            this.backing.Add(item);
        }
    }

    public void AddRange(params IEnumerable<T> items)
    {
        lock (this.@lock)
        {
            this.backing.AddRange(items);
        }
    }

    public void Remove(T item)
    {
        lock (this.@lock)
        {
            this.backing.Remove(item);
        }
    }

    public T[] ToArray()
    {
        lock (this.@lock)
        {
            return [.. this.backing];
        }
    }

    public T this[int index]
    {
        get
        {
            lock (this.@lock)
            {
                return this.backing[index];
            }
        }
    }

    public int Count => this.backing.Count;

    public IEnumerator<T> GetEnumerator()
    {
        lock (this.@lock)
        {
            return this.backing.GetEnumerator();
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}