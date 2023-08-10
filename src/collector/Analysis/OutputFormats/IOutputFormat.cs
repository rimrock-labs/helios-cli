namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using Rimrock.Helios.Analysis.Analyzers;

    /// <summary>
    /// View interface.
    /// </summary>
    public interface IOutputFormat
    {
        /// <summary>
        /// Gets the model type.
        /// </summary>
        Type ModelType { get; }

        /// <summary>
        /// Saves this view.
        /// </summary>
        /// <param name="analyzer">The analyzer context.</param>
        /// <param name="context">The context.</param>
        /// <param name="model">The model.</param>
        void Save(AnalyzerContext analyzer, AnalysisContext context, IDataModel model);
    }
}