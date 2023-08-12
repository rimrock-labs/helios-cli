namespace Rimrock.Helios.Collection.ETW
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Rimrock.Helios.Analysis;

    internal class HeliosTraceLog : IDisposable
    {
        private readonly TraceLog log;

        private bool disposed;

        public HeliosTraceLog(string tracePath)
        {
            this.log = TraceLog.OpenOrConvert(tracePath);
            this.ProcessMap = new ProcessNameMappingService();

            foreach (TraceProcess? process in this.log.Processes)
            {
                if (process != null)
                {
                    this.ProcessMap.OnProcess(process);
                }
            }
        }

        public IEnumerable<TraceEvent> Events => this.log.Events;

        public ProcessNameMappingService ProcessMap { get; }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.log?.Dispose();

                this.disposed = true;
            }
        }
    }
}