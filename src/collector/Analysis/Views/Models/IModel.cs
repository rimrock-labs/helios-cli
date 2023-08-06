namespace Rimrock.Helios.Analysis.Views
{
    /// <summary>
    /// Model interface.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Adds data.
        /// </summary>
        /// <param name="data">The data.</param>
        void AddData(IData data);
    }
}