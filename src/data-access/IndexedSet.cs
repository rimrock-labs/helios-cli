namespace Rimrock.Helios.DataAccess;

using System.Collections.Generic;

/// <summary>
/// Indexed set class.
/// </summary>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class IndexedSet<TValue>
    where TValue : notnull
{
    private readonly Dictionary<TValue, int> index;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedSet{TValue}"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    public IndexedSet(IEqualityComparer<TValue>? comparer = null)
    {
        this.index = new Dictionary<TValue, int>(comparer);
    }

    /// <summary>
    /// Gets the value index.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The index.</returns>
    public int GetIndex(TValue value)
    {
        if (!this.index.TryGetValue(value, out int index))
        {
            this.index[value] = index = this.index.Count;
        }

        return index;
    }
}