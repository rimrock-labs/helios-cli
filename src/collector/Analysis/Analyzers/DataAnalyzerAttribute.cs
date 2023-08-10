namespace Rimrock.Helios.Analysis.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Data analyzer attribute class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DataAnalyzerAttribute : Attribute
    {
        private static readonly Dictionary<string, Type> InternalAnalyzers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Get name of format.
        /// </summary>
        /// <param name="type">The output format type.</param>
        /// <returns>The name.</returns>
        /// <exception cref="InvalidOperationException">Thrown when target type is not decorated with <see cref="DataAnalyzerAttribute"/>.</exception>
        public static string GetName(Type type) =>
            type.GetCustomAttribute<DataAnalyzerAttribute>()?.Name ?? throw new InvalidOperationException("Specified type is missing data analyzer attribute.");

        /// <summary>
        /// Gets the analyzers declared in this assembly.
        /// </summary>
        /// <returns>The views.</returns>
        public static IReadOnlyDictionary<string, Type> GetAnalyzers()
        {
            if (InternalAnalyzers.Count == 0)
            {
                IEnumerable<Type> analyzerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(_ => _.IsAssignableTo(typeof(IDataAnalyzer)) && !_.IsAbstract);
                foreach (Type analyzerType in analyzerTypes)
                {
                    DataAnalyzerAttribute? attribute = analyzerType.GetCustomAttribute<DataAnalyzerAttribute>();
                    if (attribute != null)
                    {
                        InternalAnalyzers.Add(attribute.Name, analyzerType);
                    }
                }
            }

            return InternalAnalyzers;
        }

        /// <summary>
        /// Gets analyzer types by name.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>The analyzer types.</returns>
        public static IEnumerable<Type> GetAnalyzersByName(IEnumerable<string>? names)
        {
            if (names != null)
            {
                IReadOnlyDictionary<string, Type> analyzers = GetAnalyzers();
                foreach (string name in names)
                {
                    if (analyzers.TryGetValue(name, out Type? type))
                    {
                        yield return type;
                    }
                    else
                    {
                        type = Type.GetType(name, false, true);
                        if (type != null)
                        {
                            InternalAnalyzers.Add(type.Name, type);
                            yield return type;
                        }
                    }
                }
            }
        }
    }
}