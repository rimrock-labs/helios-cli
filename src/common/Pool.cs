namespace Rimrock.Helios.Common;

using System;
using System.Collections.Generic;

/// <summary>
/// Object pool class.
/// </summary>
/// <typeparam name="TObject">The object type.</typeparam>
public static class Pool<TObject>
    where TObject : class, new()
{
    private static readonly Queue<TObject> InternalPool = new();

    /// <summary>
    /// Borrows an item from the pool.
    /// </summary>
    /// <param name="creator">The creator delegate.</param>
    /// <returns>The instance.</returns>
    public static TObject Borrow(Func<TObject>? creator = null)
    {
        if (!InternalPool.TryDequeue(out TObject? instance))
        {
            instance = creator?.Invoke() ?? new TObject();
        }

        return instance;
    }

    /// <summary>
    /// Returns an item to the pool.
    /// </summary>
    /// <param name="instance">The instance.</param>
    public static void Return(ref TObject? instance)
    {
        if (instance != default)
        {
            InternalPool.Enqueue(instance);
            instance = default;
        }
    }

    /// <summary>
    /// Clears the pool.
    /// </summary>
    public static void Clear() =>
        InternalPool.Clear();
}