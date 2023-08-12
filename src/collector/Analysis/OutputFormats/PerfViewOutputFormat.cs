namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using Rimrock.Helios.Analysis.Analyzers;

    [OutputFormat(Name = "PerfView")]
    internal sealed class PerfViewOutputFormat : IOutputFormat
    {
        public Type ModelType => throw new NotImplementedException();

        public void Save(AnalyzerContext analyzer, AnalysisContext context, IDataModel model)
        {
            throw new NotImplementedException();
        }
    }
}