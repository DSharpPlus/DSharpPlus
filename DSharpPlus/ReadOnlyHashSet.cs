using System.Collections;
using System.Collections.Generic;

namespace DSharpPlus
{
    /// <summary>
    /// Read-only view of a given <see cref="ISet{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of the items in the set.</typeparam>
    public sealed class ReadOnlySet<T> : IReadOnlyCollection<T>
    {
        private ISet<T> UnderlyingSet { get; }

        /// <summary>
        /// Creates a new read-only view of the given set.
        /// </summary>
        /// <param name="sourceSet">Set to create a view over.</param>
        public ReadOnlySet(ISet<T> sourceSet)
        {
            this.UnderlyingSet = sourceSet;
        }

        /// <summary>
        /// Gets the number of items in the underlying set.
        /// </summary>
        public int Count => this.UnderlyingSet.Count;

        /// <summary>
        /// Returns an enumerator that iterates through this set view.
        /// </summary>
        /// <returns>Enumerator for the underlying set.</returns>
        public IEnumerator<T> GetEnumerator()
            => this.UnderlyingSet.GetEnumerator();
        
        /// <summary>
        /// Returns an enumerator that iterates through this set view.
        /// </summary>
        /// <returns>Enumerator for the underlying set.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => (this.UnderlyingSet as IEnumerable).GetEnumerator();
    }
}
