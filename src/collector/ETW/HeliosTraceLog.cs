namespace Rimrock.Helios.Collection.ETW
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;

    internal class HeliosTraceLog : IDisposable
    {
        private TraceLog? log;
        private bool disposed;

        public IEnumerable<TraceEvent> Events => this.log?.Events ?? Enumerable.Empty<TraceEvent>();

        public void Open(string path) =>
            this.log = TraceLog.OpenOrConvert(path);

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