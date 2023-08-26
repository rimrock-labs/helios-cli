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

    [DataAnalyzer(Name = "MemAllocs")]
    [PerfViewAgent.ProfilingDefinition(
        ClrEvents = new[] { "GC", "Stack" },
        KernelEvents = new string[0])]
    internal sealed class MemoryAllocationDataAnalyzer : BaseDataAnalyzer
    {
        public MemoryAllocationDataAnalyzer(ILogger<MemoryAllocationDataAnalyzer> logger, IServiceProvider services)
            : base(logger, services)
        {
        }

        public override bool OnData(AnalysisContext context, Process process, TraceEvent data)
        {
            bool result = false;
            if (data is GCAllocationTickTraceData gcData &&
                context.Symbols.TryResolve(data.CallStack(), process, out Frame? stackLeaf, out Frame? stackRoot))
            {
                Frame type = new(gcData.TypeName);
                stackLeaf.AddChild(type);

                ulong allocAmount = (ulong)gcData.AllocationAmount64;
                stackLeaf.ExclusiveCount += 1;
                stackLeaf.ExclusiveWeight += allocAmount;
                stackLeaf = type;
                stackLeaf.ExclusiveCount += 1;
                stackLeaf.ExclusiveWeight += allocAmount;

                ApplyInclusiveMetrics(stackLeaf, weight: allocAmount);
                stackRoot = AddTagsAsFrames(stackRoot, context.Tags);

                this.AddData(new StackData(stackLeaf, stackRoot));
                result = true;
            }

            return result;
        }
    }
}