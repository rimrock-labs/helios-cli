namespace Rimrock.Helios.Analysis.Views
{
    using System;

    /// <summary>
    /// CSV view base class.
    /// </summary>
    public abstract class CsvView : IView
    {
        /// <inheritdoc />
        public abstract Type ModelType { get; }

        /// <inheritdoc />
        public abstract void Save(AnalysisContext context, IModel model);
    }
}