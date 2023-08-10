namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Rimrock.Helios.Analysis.Analyzers;
    using Rimrock.Helios.Common;

    /// <summary>
    /// CSV output format class.
    /// </summary>
    [OutputFormat(Name = "CSV")]
    public class CsvOutputFormat : IOutputFormat
    {
        private readonly FileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvOutputFormat"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public CsvOutputFormat(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <inheritdoc />
        public Type ModelType => typeof(ICsvModel);

        /// <inheritdoc />
        public void Save(AnalyzerContext analyzer, AnalysisContext context, IDataModel model) =>
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