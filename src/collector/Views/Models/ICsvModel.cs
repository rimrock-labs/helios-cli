namespace Rimrock.Helios.Analysis.Views
{
    using System.Collections.Generic;

    /// <summary>
    /// CSV model class.
    /// </summary>
    public interface ICsvModel : IModel
    {
        IEnumerable<string> GetColumnNames();


    }
}