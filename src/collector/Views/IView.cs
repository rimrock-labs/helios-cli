namespace Rimrock.Helios.Analysis.Views
{
    using System;

    /// <summary>
    /// View interface.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Gets the model type.
        /// </summary>
        Type ModelType { get; }

        /// <summary>
        /// Saves this view.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="model">The model.</param>
        void Save(AnalysisContext context, IModel model);
    }
}