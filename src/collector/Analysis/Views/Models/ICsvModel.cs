namespace Rimrock.Helios.Analysis.Views
{
    using System.Collections.Generic;

    /// <summary>
    /// CSV model class.
    /// </summary>
    public interface ICsvModel : IModel
    {
        /// <summary>
        /// Gets the column names.
        /// </summary>
        /// <returns>The column names.</returns>
        IReadOnlyList<string> GetColumnNames();

        /// <summary>
        /// Gets the data rows.
        /// </summary>
        /// <returns>The data rows.</returns>
        IEnumerable<IReadOnlyList<string>> GetDataRows();
    }
}