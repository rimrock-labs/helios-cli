namespace Rimrock.Helios.Analysis.Analyzers
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Diagnostics.Tracing.Parsers.Clr;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Analysis.OutputFormats;
    using Rimrock.Helios.Collection;
    using Rimrock.Helios.Common.Graph;

    [DataAnalyzer(Name = "Exceptions")]
    [PerfViewAgent.ProfilingDefinition(
        ClrEvents = new[] { "Exception", "Stack" },
        KernelEvents = new string[0])]
    internal sealed class ExceptionsDataAnalyzer : BaseDataAnalyzer
    {
        public ExceptionsDataAnalyzer(ILogger<ExceptionsDataAnalyzer> logger, IServiceProvider services)
            : base(logger, services)
        {
        }

        public override bool OnData(AnalysisContext context, Process process, TraceEvent data)
        {
            bool result = false;
            if (data is ExceptionTraceData exceptionData &&
                context.Symbols.TryResolve(data.CallStack(), process, out Frame? stackLeaf, out Frame? stackRoot))
            {
                Frame type = new(exceptionData.ExceptionType);
                stackLeaf.AddChild(type);

                stackLeaf.ExclusiveCount += 1;
                stackLeaf.ExclusiveWeight += 1;
                stackLeaf = type;
                stackLeaf.ExclusiveCount += 1;
                stackLeaf.ExclusiveWeight += 1;

                ApplyInclusiveMetrics(stackLeaf);
                stackRoot = AddTagsAsFrames(stackRoot, context.Tags);

                this.AddData(new StackData(stackLeaf, stackRoot));
                result = true;
            }

            return result;
        }
    }
}