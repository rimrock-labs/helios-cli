namespace Rimrock.Helios.Analysis
{
    using System;
    using System.Collections.Generic;
    using Rimrock.Helios.Analysis.Analyzers;

    /// <summary>
    /// Analysis context class.
    /// </summary>
    public class AnalysisContext
    {
        /// <summary>
        /// Gets the working directory.
        /// </summary>
        public required string WorkingDirectory { get; init; }

        /// <summary>
        /// Gets the path of the trace being analyzed.
        /// </summary>
        public required string TracePath { get; init; }

        /// <summary>
        /// Gets the symbol store.
        /// </summary>
        public required SymbolStore Symbols { get; init; }

        /// <summary>
        /// Gets the enabled analyzers.
        /// </summary>
        public List<IDataAnalyzer> Analyzers { get; } = new();

        /// <summary>
        /// Gets the output format types.
        /// </summary>
        public required IReadOnlySet<Type> OutputFormats { get; init; }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        public required IReadOnlySet<string> Tags { get; init; }
    }
}