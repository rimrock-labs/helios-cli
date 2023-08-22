namespace System.Linq
{
    using System.Collections.Generic;

    /// <summary>
    /// Enumerable extensions class.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Iterates the collection.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="collection">The collection.</param>
        public static void Iterate<TValue>(this IEnumerable<TValue> collection)
        {
            foreach (TValue value in collection)
            {
            }
        }
    }
}