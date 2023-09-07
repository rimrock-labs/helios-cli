namespace Rimrock.Helios.DataAccess;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Index set factory class.
/// </summary>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class IndexedSetFactory<TValue> : IEnumerable<IndexedSet<TValue>>
    where TValue : notnull
{
    private readonly Dictionary<string, IndexedSet<TValue>> sets;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedSetFactory{TValue}"/> class.
    /// </summary>
    public IndexedSetFactory()
    {
        this.sets = new Dictionary<string, IndexedSet<TValue>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates an index.
    /// </summary>
    /// <param name="name">The index name.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>The index.</returns>
    public IndexedSet<TValue> GetOrCreate(string name, IEqualityComparer<TValue>? comparer = null)
    {
        if (!this.sets.TryGetValue(name, out var set))
        {
            this.sets[name] = set = new IndexedSet<TValue>(comparer);
        }

        return set;
    }

    /// <inheritdoc />
    public IEnumerator<IndexedSet<TValue>> GetEnumerator() =>
        this.sets.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();
}