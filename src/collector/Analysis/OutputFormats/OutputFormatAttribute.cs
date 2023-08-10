namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Output format attribute class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class OutputFormatAttribute : Attribute
    {
        private static readonly Dictionary<string, Type> InternalViews = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Get name of format.
        /// </summary>
        /// <param name="type">The output format type.</param>
        /// <returns>The name.</returns>
        /// <exception cref="InvalidOperationException">Thrown when target type is not decorated with <see cref="OutputFormatAttribute"/>.</exception>
        public static string GetName(Type type) =>
            type.GetCustomAttribute<OutputFormatAttribute>()?.Name ?? throw new InvalidOperationException("Specified type is missing output format attribute.");

        /// <summary>
        /// Gets the views declared in this assembly.
        /// </summary>
        /// <returns>The views.</returns>
        public static IReadOnlyDictionary<string, Type> GetViews()
        {
            if (InternalViews.Count == 0)
            {
                IEnumerable<Type> viewTypes = Assembly.GetExecutingAssembly().GetTypes().Where(_ => _.IsAssignableFrom(typeof(IOutputFormat)) && !_.IsAbstract);
                foreach (Type viewType in viewTypes)
                {
                    OutputFormatAttribute? attribute = viewType.GetCustomAttribute<OutputFormatAttribute>();
                    if (attribute != null)
                    {
                        InternalViews.Add(attribute.Name, viewType);
                    }
                }
            }

            return InternalViews;
        }

        /// <summary>
        /// Gets view types by name.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>The view types.</returns>
        public static IEnumerable<Type> GetViewsByName(IEnumerable<string>? names)
        {
            if (names != null)
            {
                var views = GetViews();
                foreach (string name in names)
                {
                    if (views.TryGetValue(name, out Type? type))
                    {
                        yield return type;
                    }
                    else
                    {
                        type = Type.GetType(name, false, true);
                        if (type != null)
                        {
                            InternalViews.Add(type.Name, type);
                            yield return type;
                        }
                    }
                }
            }
        }
    }
}