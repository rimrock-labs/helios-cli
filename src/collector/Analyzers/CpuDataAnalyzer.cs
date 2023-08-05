namespace Rimrock.Helios.Analysis.Analyzers
{
    using Rimrock.Helios.Analysis.Views;
    using Rimrock.Helios.Common;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Extensions.Logging;

    [WindowsProfilingDefinition(
        "CPU",
        ClrEvents = new[] { "Loader", "Stack" },
        KernelEvents = new[] { "Process", "Profile", "ImageLoad" })]
    internal class CpuDataAnalyzer : BaseDataAnalyzer
    {
        public CpuDataAnalyzer(ILogger logger)
            : base(logger)
        {
        }

        public override void OnData(AnalysisContext context, TraceEvent traceEvent)
        {
            if (traceEvent is SampledProfileTraceData &&
                context.Symbols.TryResolve(traceEvent.CallStack(), out DataFrame? frame))
            {
                WeightedGraphData data = new(frame);

                // TODO: add additional predefined tags
                data.AddProcessTag(traceEvent.ProcessName);
            }
        }
    }
}