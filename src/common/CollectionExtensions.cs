namespace System.Collections.Generic
{
    /// <summary>
    /// Collection extensions class.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds range of values to set.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="set">The set.</param>
        /// <param name="values">The values.</param>
        public static void AddRange<TValue>(this ISet<TValue> set, IEnumerable<TValue> values)
        {
            foreach (TValue value in values)
            {
                set.Add(value);
            }
        }
    }
}