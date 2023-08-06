namespace Rimrock.Helios.Analysis
{
    using System;

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
    }
}