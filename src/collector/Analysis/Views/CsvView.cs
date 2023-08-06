namespace Rimrock.Helios.Analysis.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Rimrock.Helios.Analysis.Analyzers;
    using Rimrock.Helios.Common;

    /// <summary>
    /// CSV view base class.
    /// </summary>
    public class CsvView : IView
    {
        private readonly FileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvView"/> class.
        /// </summary>
        public CsvView()
            : this(FileSystem.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvView"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public CsvView(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <inheritdoc />
        public Type ModelType => typeof(ICsvModel);

        /// <inheritdoc />
        public void Save(AnalyzerContext analyzer, AnalysisContext context, IModel model) =>
            this.Save(analyzer, context, (ICsvModel)model);

        private void Save(AnalyzerContext analyzer, AnalysisContext context, ICsvModel model)
        {
            const char Comma = ',';

            string path = Path.Combine(context.WorkingDirectory, analyzer.AnalyzerName) + ".csv";
            using Stream fileStream = this.fileSystem.FileOpen(path, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new(fileStream);
            foreach (string column in model.GetColumnNames())
            {
                writer.Write(column);
                writer.Write(Comma);
            }

            writer.WriteLine();

            foreach (IReadOnlyList<string> row in model.GetDataRows())
            {
                foreach (string column in row)
                {
                    writer.Write(column);
                    writer.Write(Comma);
                }

                writer.WriteLine();
            }
        }
    }
}