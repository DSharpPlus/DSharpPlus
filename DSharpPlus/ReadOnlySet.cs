
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DSharpPlus;
/// <summary>
/// Read-only view of a given <see cref="ISet{T}"/>.
/// </summary>
/// <typeparam name="T">Type of the items in the set.</typeparam>
internal readonly struct ReadOnlySet<T> : IReadOnlyCollection<T>
{
    private readonly ISet<T> _underlyingSet;

    /// <summary>
    /// Creates a new read-only view of the given set.
    /// </summary>
    /// <param name="sourceSet">Set to create a view over.</param>
    public ReadOnlySet(ISet<T> sourceSet) => _underlyingSet = sourceSet;

    /// <summary>
    /// Gets the number of items in the underlying set.
    /// </summary>
    public int Count => _underlyingSet.Count;

    /// <summary>
    /// Returns an enumerator that iterates through this set view.
    /// </summary>
    /// <returns>Enumerator for the underlying set.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator()
        => _underlyingSet.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through this set view.
    /// </summary>
    /// <returns>Enumerator for the underlying set.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
        => (_underlyingSet as IEnumerable).GetEnumerator();
}
