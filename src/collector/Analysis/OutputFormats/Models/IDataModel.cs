namespace Rimrock.Helios.Analysis.OutputFormats
{
    /// <summary>
    /// Model interface.
    /// </summary>
    public interface IDataModel
    {
        /// <summary>
        /// Adds data.
        /// </summary>
        /// <param name="data">The data.</param>
        void AddData(IData data);
    }
}