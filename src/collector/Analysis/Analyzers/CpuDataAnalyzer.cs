namespace Rimrock.Helios.Analysis.Analyzers
{
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Analysis.OutputFormats;

    [DataAnalyzer(Name = "CPU")]
    [WindowsProfilingDefinition(
        ClrEvents = new[] { "Loader", "Stack" },
        KernelEvents = new[] { "Process", "Profile", "ImageLoad" })]
    internal class CpuDataAnalyzer : BaseDataAnalyzer
    {
        public CpuDataAnalyzer(ILogger<CpuDataAnalyzer> logger)
            : base(logger)
        {
        }

        public override void OnData(AnalysisContext context, TraceEvent traceEvent)
        {
            if (traceEvent is SampledProfileTraceData &&
                context.Symbols.TryResolve(traceEvent.CallStack(), out Frame? frame))
            {
                CallStackData data = new(frame);

                // TODO: add additional predefined tags
                data.AddProcessTag(traceEvent.ProcessName);

                this.AddData(data);
            }
        }
    }
}