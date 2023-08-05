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
        /// Gets or sets the working directory.
        /// </summary>
        public required string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the path of the trace being analyzed.
        /// </summary>
        public required string TracePath { get; set; }

        /// <summary>
        /// Gets or sets the symbol store.
        /// </summary>
        public required SymbolStore Symbols { get; set; }

        /// <summary>
        /// Gets or sets the enabled analyzers.
        /// </summary>
        public required IReadOnlyList<IDataAnalyzer> Analyzers { get; set; }

        /// <summary>
        /// Gets or sets the views.
        /// </summary>
        public required IReadOnlySet<Type> Views { get; set; }
    }
}