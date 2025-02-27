//
// SPDX-FileCopyrightText: Copyright (c) 2024 DPlayer234/Vamplay
// SPDX-License-Identifier: MIT
//
// This logic is taken from DPlayer234/Vamplay's command handling infrastructure licensed under MIT:
// https://github.com/DPlayer234/celestia-lib/blob/9747011b63a168b11103988b16aed111572df113/src/CelestiaCS.Lib.Services/Commands/NodeAttributeCollection.cs
//

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using DSharpPlus.Commands.Trees.Metadata;

namespace DSharpPlus.Commands.Trees;

public sealed class NodeMetadataCollection : IReadOnlyList<INodeMetadataItem>
{
    /// <summary> An empty collection of node attributes. </summary>
    public static NodeMetadataCollection Empty { get; } = new();

    private readonly ImmutableArray<INodeMetadataItem> values;

    internal NodeMetadataCollection(ImmutableArray<INodeMetadataItem> values)
    {
        Debug.Assert(!values.IsEmpty);

        this.values = values;
    }

    private NodeMetadataCollection() 
        => this.values = [];

    /// <summary> Gets the number of elements in this collection. </summary>
    public int Count => this.values.Length;

    /// <inheritdoc/>
    public INodeMetadataItem this[int index] => this.values[index];

    /// <summary>
    /// Gets the first attribute of the specified type stored in this collection without accessing the cache.
    /// </summary>
    /// <typeparam name="T"> The attribute type. </typeparam>
    /// <returns> The first attribute of the specified type. </returns>
    public T? Get<T>() where T : class, INodeMetadataItem
    {
        foreach (INodeMetadataItem item in this.values)
        {
            if (item is T)
            {
                return Unsafe.As<T>(item);
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether this collection contains the specified attribute.
    /// </summary>
    /// <param name="item"> The attribute to look for. </param>
    /// <returns> Whether the attribute is contained. </returns>
    public bool Contains(INodeMetadataItem item) => this.values.Contains(item);

    /// <summary>
    /// Determines the index of the specified attribute within this collection.
    /// </summary>
    /// <param name="item"> The attribute to look for. </param>
    /// <returns> The index of the attribute, or -1 if not found. </returns>
    public int IndexOf(INodeMetadataItem item) => this.values.IndexOf(item);

    /// <summary>
    /// Creates a <see cref="NodeMetadataCollection"/> with the specified elements.
    /// </summary>
    /// <param name="source"> The attributes for the collection. </param>
    /// <returns> A matching immutable collection. </returns>
    public static NodeMetadataCollection Create(IEnumerable<INodeMetadataItem> source) => Create([.. source]);

    /// <inheritdoc cref="Create(IEnumerable{INodeMetadataItem})"/>
    public static NodeMetadataCollection Create(ImmutableArray<INodeMetadataItem> source) => source.IsEmpty ? Empty : new NodeMetadataCollection(source);

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public Enumerator GetEnumerator() => new(this);

    IEnumerator<INodeMetadataItem> IEnumerable<INodeMetadataItem>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// An enumerator over the elements in a <see cref="NodeMetadataCollection"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<INodeMetadataItem>
    {
        private ImmutableArray<INodeMetadataItem>.Enumerator enumerator;
        private readonly ImmutableArray<INodeMetadataItem> array;

        internal Enumerator(NodeMetadataCollection self)
        {
            this.enumerator = self.values.GetEnumerator();
            this.array = self.values;
        }

        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public INodeMetadataItem Current => this.enumerator.Current;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {

        }

        /// <inheritdoc cref="IEnumerator.MoveNext"/>
        public bool MoveNext() => this.enumerator.MoveNext();

        public void Reset() => this.enumerator = this.array.GetEnumerator();
    }
}
