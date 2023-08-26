namespace Rimrock.Helios.Analysis.Analyzers
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Analysis.OutputFormats;
    using Rimrock.Helios.Collection;

    [DataAnalyzer(Name = "CPU")]
    [PerfViewAgent.ProfilingDefinition(
        ClrEvents = new[] { "Stack" },
        KernelEvents = new[] { "Profile" })]
    internal class CpuDataAnalyzer : BaseDataAnalyzer
    {
        public CpuDataAnalyzer(ILogger<CpuDataAnalyzer> logger, IServiceProvider services)
            : base(logger, services)
        {
        }

        public override bool OnData(AnalysisContext context, Process process, TraceEvent data)
        {
            bool result = false;
            if (data is SampledProfileTraceData &&
                context.Symbols.TryResolve(data.CallStack(), process, out Frame? stackLeaf, out Frame? stackRoot))
            {
                ApplyInclusiveMetrics(stackLeaf);
                stackLeaf.ExclusiveCount += 1;
                stackLeaf.ExclusiveWeight += 1;

                stackRoot = AddTagsAsFrames(stackRoot, context.Tags);
                this.AddData(new StackData(stackLeaf, stackRoot));
                result = true;
            }

            return result;
        }
    }
}