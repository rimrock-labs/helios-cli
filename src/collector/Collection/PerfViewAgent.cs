namespace Rimrock.Helios.Collection
{
    using System;
    using System.Diagnostics;

    internal class PerfViewAgent : IDisposable
    {
        private readonly Process? process;
        private readonly Configuration configuration;
        private bool disposed;

        private PerfViewAgent(Process? process, Configuration configuration)
        {
            this.process = process;
            this.configuration = configuration;
        }

        public static PerfViewAgent Start(Configuration configuration)
        {
            Process? process = Process.Start(new ProcessStartInfo(configuration.PerfViewPath)
            {
                Arguments = $"Collect {configuration.OutputName} /LogFile:{configuration.OutputName}.log /MaxCollectSec:{configuration.Duration.TotalSeconds} /Circular:{configuration.MaxOutputSize} /BufferSize:{configuration.Buffer} /CpuSampleMSec:${configuration.CpuSamplingRate} /RundownTimeout:${configuration.RundownTimeout} /NoNGenRundown /Merge:false /Zip:false /TrustPdbs /AcceptEULA /SafeMode /EnableEventsInContainers /KernelEvents={string.Join(',', configuration.KernelEvents)} /ClrEvents={string.Join(',', configuration.ClrEvents)}",
                WorkingDirectory = configuration.WorkingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
            });

            return new PerfViewAgent(process, configuration);
        }

        public void Wait()
        {
            if (!(this.process?.WaitForExit(this.configuration.Timeout) ?? false))
            {
                this.process?.Kill(true);
                throw new ApplicationException("Collection agent did not exit in time (or was not created).");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.process?.Dispose();

                this.disposed = true;
            }
        }

        public class Configuration
        {
            public required string OutputName { get; set; }

            public required string PerfViewPath { get; init; }

            public required string WorkingDirectory { get; init; }

            public TimeSpan Duration { get; init; } = TimeSpan.FromMinutes(1);

            public uint MaxOutputSize { get; set; } = 1024;

            public uint Buffer { get; set; } = 1024;

            public uint RundownTimeout { get; set; } = 120;

            public uint CpuSamplingRate { get; set; } = 10;

            public required string[] ClrEvents { get; init; }

            public required string[] KernelEvents { get; init; }

            public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
        }
    }
}