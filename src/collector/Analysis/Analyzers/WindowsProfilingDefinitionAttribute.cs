namespace Rimrock.Helios.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Rimrock.Helios.Analysis.Analyzers;

    /// <summary>
    /// Windows profiling definition attribute class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WindowsProfilingDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Gets the Windows ETW Kernel events required.
        /// </summary>
        public required string[] KernelEvents { get; init; }

        /// <summary>
        /// Gets the Windows CLR events required.
        /// </summary>
        public required string[] ClrEvents { get; init; }

        /// <summary>
        /// Gets the kernel events from the specified analyzers.
        /// </summary>
        /// <param name="analyzerTypes">The analyzer types.</param>
        /// <returns>The kernel event set.</returns>
        public static IReadOnlySet<string> GetKernelEvents(IEnumerable<Type> analyzerTypes)
        {
            HashSet<string> events = new(StringComparer.OrdinalIgnoreCase);
            foreach (Type type in analyzerTypes)
            {
                WindowsProfilingDefinitionAttribute? attribute = type.GetCustomAttribute<WindowsProfilingDefinitionAttribute>();
                if (attribute != null)
                {
                    foreach (string value in attribute.KernelEvents)
                    {
                        events.Add(value);
                    }
                }
            }

            return events;
        }

        /// <summary>
        /// Gets the clr events from the specified analyzers.
        /// </summary>
        /// <param name="analyzerTypes">The analyzer types.</param>
        /// <returns>The clr event set.</returns>
        public static IReadOnlySet<string> GetClrEvents(IEnumerable<Type> analyzerTypes)
        {
            HashSet<string> events = new(StringComparer.OrdinalIgnoreCase);
            foreach (Type type in analyzerTypes)
            {
                WindowsProfilingDefinitionAttribute? attribute = type.GetCustomAttribute<WindowsProfilingDefinitionAttribute>();
                if (attribute != null)
                {
                    foreach (string value in attribute.ClrEvents)
                    {
                        events.Add(value);
                    }
                }
            }

            return events;
        }
    }
}