namespace Rimrock.Helios.Analysis.Analyzers
{
    using Microsoft.Diagnostics.Tracing;

    /// <summary>
    /// Data analyzer interface.
    /// </summary>
    public interface IDataAnalyzer
    {
        /// <summary>
        /// Called at the start of analysis.
        /// </summary>
        /// <param name="context">The context.</param>
        void OnStart(AnalysisContext context);

        /// <summary>
        /// Called when new data is available.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="data">The data.</param>
        void OnData(AnalysisContext context, TraceEvent data);

        /// <summary>
        /// Called at the end of analysis.
        /// </summary>
        /// <param name="context">The context.</param>
        void OnEnd(AnalysisContext context);
    }
}