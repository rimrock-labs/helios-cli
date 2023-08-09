namespace Rimrock.Helios.Collection
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Common;

    internal class PerfViewAgent : IDisposable
    {
        private readonly ILogger logger;
        private readonly FileSystem fileSystem;
        private readonly Process? process;
        private readonly Configuration configuration;
        private bool disposed;

        private PerfViewAgent(ILogger logger, FileSystem fileSystem, Process? process, Configuration configuration)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.process = process;
            this.configuration = configuration;
        }

        public static PerfViewAgent Start(Configuration configuration)
        {
            // Process? process = Process.Start(new ProcessStartInfo(configuration.PerfViewPath)
            // {
            //     Arguments = $"Collect {configuration.OutputName} /LogFile:{configuration.OutputName}.log /MaxCollectSec:{configuration.Duration.TotalSeconds} /Circular:{configuration.MaxOutputSize} /BufferSize:{configuration.Buffer} /CpuSampleMSec:${configuration.CpuSamplingRate} /RundownTimeout:${configuration.RundownTimeout} /NoNGenRundown /Merge:false /Zip:false /TrustPdbs /AcceptEULA /SafeMode /EnableEventsInContainers /KernelEvents={string.Join(',', configuration.KernelEvents)} /ClrEvents={string.Join(',', configuration.ClrEvents)}",
            //     WorkingDirectory = configuration.WorkingDirectory,
            //     CreateNoWindow = true,
            //     UseShellExecute = false,
            // });

            return null; // new PerfViewAgent(logger, fileSystem, process, configuration);
        }

        public void Wait()
        {
            bool successful = true;
            if (!(this.process?.WaitForExit(this.configuration.Timeout) ?? false))
            {
                this.process?.Kill(true);
                successful = false;
            }

            string logFile = this.configuration.OutputName + ".log";
            if (this.fileSystem.FileExists(logFile))
            {
                this.logger.LogDebug("PerfView log,{NewLine}{Log}", Environment.NewLine, this.fileSystem.FileReadAllText(logFile));
            }

            if (!successful)
            {
                throw new ApplicationException("Collection agent did not exit in time (or did not start successfully).");
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
            [Required]
            public required string OutputName { get; set; }

            [Required]
            public required string PerfViewPath { get; init; }

            [Required]
            public required string WorkingDirectory { get; init; }

            [Range(1, int.MaxValue, ErrorMessage = "The Duration must be greater than zero.")]
            public TimeSpan Duration { get; init; } = TimeSpan.FromMinutes(1);

            [Range(1024, uint.MaxValue)]
            public uint MaxOutputSize { get; set; } = 1024;

            [Range(1024, uint.MaxValue)]
            public uint Buffer { get; set; } = 1024;

            [Range(60, uint.MaxValue)]
            public uint RundownTimeout { get; set; } = 120;

            [Range(10, 1000)]
            public uint CpuSamplingRate { get; set; } = 10;

            public required string[] ClrEvents { get; init; }

            public required string[] KernelEvents { get; init; }

            public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

            public bool TryValidate([NotNullWhen(false)] out IReadOnlyList<string?>? errors)
            {
                bool result = true;
                errors = default;

                List<ValidationResult> validationResults = new();
                if (!Validator.TryValidateObject(this, new ValidationContext(this), validationResults, true))
                {
                    List<string?> errorMessages = new(validationResults.Count);
                    foreach (ValidationResult validation in validationResults)
                    {
                        errorMessages.Add(validation.ErrorMessage);
                    }

                    errors = errorMessages;
                    result = false;
                }

                return result;
            }
        }
    }
}