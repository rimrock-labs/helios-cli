namespace Rimrock.Helios.Analysis
{
    using System;

    /// <summary>
    /// Data analyzer attribute class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DataAnalyzerAttribute : Attribute
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public required string Name { get; init; }
    }
}