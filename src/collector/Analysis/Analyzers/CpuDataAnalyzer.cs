namespace Rimrock.Helios.Analysis.Analyzers
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Analysis.OutputFormats;

    [DataAnalyzer(Name = "CPU")]
    [WindowsProfilingDefinition(
        ClrEvents = new[] { "Stack" },
        KernelEvents = new[] { "Profile" })]
    internal class CpuDataAnalyzer : BaseDataAnalyzer
    {
        public CpuDataAnalyzer(ILogger<CpuDataAnalyzer> logger, IServiceProvider services)
            : base(logger, services)
        {
        }

        public override bool OnData(AnalysisContext context, Process process, TraceEvent traceEvent)
        {
            bool result = false;
            if (traceEvent is SampledProfileTraceData &&
                context.Symbols.TryResolve(traceEvent.CallStack(), out Frame? frame))
            {
                StackData data = new(frame);

                // TODO: add additional predefined tags
                data.AddProcessTag(process.Name);

                this.AddData(data);
                result = true;
            }

            return result;
        }
    }
}