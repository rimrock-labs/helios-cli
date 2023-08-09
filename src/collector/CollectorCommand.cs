namespace Rimrock.Helios.Collector
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Collection;
    using Rimrock.Helios.Common;
    using Rimrock.Helios.Common.Commands;

    /// <summary>
    /// Collector command class.
    /// </summary>
    public class CollectorCommand : ICommand
    {
        private static readonly Option<string> OutputDirectory = new("--output-directory", description: "Output directory.") { IsRequired = true };
        private static readonly Option<TimeSpan> Duration = new("--duration", description: "Duration of collection.", getDefaultValue: () => TimeSpan.FromMinutes(1));

        private readonly ILogger<CollectorCommand> logger;
        private readonly FileSystem fileSystem;
        private readonly HeliosEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        public CollectorCommand(
            ILogger<CollectorCommand> logger,
            FileSystem fileSystem,
            HeliosEnvironment environment)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.environment = environment;
        }

        /// <inheritdoc />
        public string Name => "collect";

        /// <inheritdoc />
        public IReadOnlyList<Command> GetCommand(IServiceProvider services)
        {
            Command command = new(this.Name, description: "Collects data from the current machine.");
            command.AddOption(OutputDirectory);
            command.AddOption(new Option<string[]>("--data-analyzers", description: "Data sets to collect.", getDefaultValue: () => new[] { "CPU" }).FromAmong("CPU"));
            command.AddOption(new Option<int[]>("--process-ids", description: "Process identifiers to focus the data to."));
            command.AddOption(new Option<string[]>("--output-format", description: "Output format.", getDefaultValue: () => new[] { "CSV" }).FromAmong("CSV"));
            command.AddOption(Duration);
            command.SetHandler(this.Collect);
            return new[] { command };
        }

        private void Collect(InvocationContext context)
        {
            StringMacros.DefaultValues macroDefaults = new(this.environment);
            StringMacros pathMacros = new(macroDefaults);
            string outputDirectory = context.ParseResult.GetValueForOption(OutputDirectory)!;
            outputDirectory = Environment.ExpandEnvironmentVariables(outputDirectory);
            PerfViewAgent.Configuration configuration = new()
            {
                PerfViewPath = Path.Combine(this.environment.ApplicationDirectory, "PerfView", "PerfView.exe"),
                WorkingDirectory = pathMacros.Expand(outputDirectory),
                OutputName = $"Helios-{Environment.MachineName}-{DateTimeOffset.UtcNow:yyyyMMdd-hhmmss}",
                Duration = context.ParseResult.GetValueForOption(Duration),
                KernelEvents = Array.Empty<string>(),
                ClrEvents = Array.Empty<string>(),
            };

            Console.WriteLine(JsonSerializer.Serialize(configuration, new JsonSerializerOptions() { WriteIndented = true }));

            if (!configuration.TryValidate(out var errors))
            {
                foreach (string? validationError in errors)
                {
                    this.logger.LogError(validationError);
                }

                throw new CommandLineConfigurationException("Invalid arguments specified.");
            }

            using PerfViewAgent agent = PerfViewAgent.Start(configuration);
            //agent.Wait();

            this.Analyze(context);
        }

        private void Analyze(InvocationContext context)
        {

        }
    }
}