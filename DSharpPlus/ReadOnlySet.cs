using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DSharpPlus;

/// <summary>
/// Read-only view of a given <see cref="ISet{T}"/>.
/// </summary>
/// <typeparam name="T">Type of the items in the set.</typeparam>
internal readonly struct ReadOnlySet<T> : IReadOnlyList<T>
{
    private readonly ISet<T> underlyingSet;

    /// <summary>
    /// Creates a new read-only view of the given set.
    /// </summary>
    /// <param name="sourceSet">Set to create a view over.</param>
    public ReadOnlySet(ISet<T> sourceSet) => this.underlyingSet = sourceSet;

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    public T this[int index] => this.underlyingSet.ElementAt(index);

    /// <summary>
    /// Gets the number of items in the underlying set.
    /// </summary>
    public int Count => this.underlyingSet.Count;

    /// <summary>
    /// Returns an enumerator that iterates through this set view.
    /// </summary>
    /// <returns>Enumerator for the underlying set.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator()
        => this.underlyingSet.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through this set view.
    /// </summary>
    /// <returns>Enumerator for the underlying set.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
        => (this.underlyingSet as IEnumerable).GetEnumerator();
}
