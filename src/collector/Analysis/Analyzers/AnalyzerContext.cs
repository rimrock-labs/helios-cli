namespace Rimrock.Helios.Analysis.Analyzers
{
    /// <summary>
    /// Analyzer context class.
    /// </summary>
    public class AnalyzerContext
    {
        /// <summary>
        /// Gets the analyzer name.
        /// </summary>
        public required string AnalyzerName { get; init; }
    }
}