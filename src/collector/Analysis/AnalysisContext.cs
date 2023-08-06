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
        public required IReadOnlyList<IDataAnalyzer> Analyzers { get; init; }

        /// <summary>
        /// Gets the views.
        /// </summary>
        public required IReadOnlySet<Type> Views { get; init; }
    }
}