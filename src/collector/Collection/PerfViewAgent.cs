namespace Rimrock.Helios.Collection
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Common;

    internal class PerfViewAgent
    {
        private readonly ILogger logger;
        private readonly FileSystem fileSystem;

        public PerfViewAgent(
            ILogger<PerfViewAgent> logger,
            FileSystem fileSystem)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
        }

        public void Run(Configuration configuration)
        {
            using Process? process = Process.Start(new ProcessStartInfo(configuration.PerfViewPath)
            {
                Arguments = $"Collect {configuration.OutputName} /LogFile:{configuration.OutputName}.log /MaxCollectSec:{configuration.Duration.TotalSeconds} /Circular:{configuration.MaxOutputSize} /BufferSize:{configuration.Buffer} /CpuSampleMSec:{configuration.CpuSamplingRate} /RundownTimeout:{configuration.RundownTimeout} /RundownMaxMB:{configuration.MaxRundownOutputSize} /NoNGenRundown /Merge:false /Zip:false /TrustPdbs /AcceptEULA /SafeMode /EnableEventsInContainers /KernelEvents={string.Join(',', configuration.KernelEvents)} /ClrEvents={string.Join(',', configuration.ClrEvents)}",
                WorkingDirectory = configuration.WorkingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
            });

            bool successful = true;
            if (!(process?.WaitForExit(configuration.Timeout) ?? false))
            {
                process?.Kill(true);
                this.logger.LogError("Collection agent exceeded runtime timeout and was killed.");
                successful = false;
            }

            string logFile = Path.Combine(configuration.WorkingDirectory, configuration.OutputName + ".log");
            if (this.fileSystem.FileExists(logFile))
            {
                this.logger.LogInformation("Collection agent log...{NewLine}{Log}", Environment.NewLine, this.fileSystem.FileReadAllText(logFile));
            }

            if (!successful)
            {
                throw new ApplicationException("Collection agent did not exit in time (or did not start).");
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

            [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00", ErrorMessage = "The Duration must be greater than zero.")]
            public TimeSpan Duration { get; init; } = TimeSpan.FromMinutes(1);

            [Range(1024, uint.MaxValue)]
            public uint MaxOutputSize { get; set; } = 1024;

            [Range(1024, uint.MaxValue)]
            public uint MaxRundownOutputSize { get; set; } = 1024;

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