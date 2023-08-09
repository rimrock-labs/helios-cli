namespace Rimrock.Helios.Collector
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Text.Json;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Analysis;
    using Rimrock.Helios.Analysis.Views;
    using Rimrock.Helios.Collection;
    using Rimrock.Helios.Collection.ETW;
    using Rimrock.Helios.Common;
    using Rimrock.Helios.Common.Commands;

    /// <summary>
    /// Collector command class.
    /// </summary>
    public class CollectorCommand : ICommand
    {
        private static readonly Option<string> OutputDirectory = new("--output-directory", description: "Output directory.") { IsRequired = true };
        private static readonly Option<TimeSpan> Duration = new("--duration", description: "Duration of collection.", getDefaultValue: () => TimeSpan.FromMinutes(1));
        private static readonly Option<string> SymbolStoreCache = new("--symbol-store-cache", description: "Path to directory where symbols are cached.");
        private static readonly Option<string[]> OutputFormat = new Option<string[]>("--output-format", description: "Output format.", getDefaultValue: () => new[] { "CSV" }).FromAmong("CSV");

        private readonly ILogger<CollectorCommand> logger;
        private readonly HeliosEnvironment environment;
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="services">The services.</param>
        public CollectorCommand(
            ILogger<CollectorCommand> logger,
            HeliosEnvironment environment,
            IServiceProvider services)
        {
            this.logger = logger;
            this.environment = environment;
            this.services = services;
        }

        /// <inheritdoc />
        public string Name => "collect";

        /// <inheritdoc />
        public IReadOnlyList<Command> GetCommand()
        {
            Command command = new(this.Name, description: "Collects data from the current machine.");
            command.AddOption(OutputDirectory);
            command.AddOption(new Option<string[]>("--data-analyzers", description: "Data sets to collect.", getDefaultValue: () => new[] { "CPU" }).FromAmong("CPU"));
            command.AddOption(new Option<int[]>("--process-ids", description: "Process identifiers to focus the data to."));
            command.AddOption(OutputFormat);
            command.AddOption(Duration);
            command.AddOption(SymbolStoreCache);
            command.SetHandler(this.Collect);
            return new[] { command };
        }

        private void Collect(InvocationContext context)
        {
            PerfViewAgent.Configuration configuration = new()
            {
                PerfViewPath = Path.Combine(this.environment.ApplicationDirectory, "PerfView", "PerfView.exe"),
                WorkingDirectory = this.GetOutputDirectory(context),
                OutputName = $"Helios-{Environment.MachineName}-{DateTimeOffset.UtcNow:yyyyMMdd-hhmmss}",
                Duration = context.ParseResult.GetValueForOption(Duration),
                KernelEvents = Array.Empty<string>(),
                ClrEvents = Array.Empty<string>(),
            };

            if (!configuration.TryValidate(out var errors))
            {
                foreach (string? validationError in errors)
                {
                    this.logger.LogError("Validation error: {error}", validationError);
                }

                this.logger.LogError("Specified configuration, {config}", JsonSerializer.Serialize(configuration, new JsonSerializerOptions() { WriteIndented = true }));

                throw new CommandLineConfigurationException("Invalid configuration specified.");
            }

            var agent = (PerfViewAgent)ActivatorUtilities.CreateInstance<PerfViewAgent>(this.services);
            this.logger.LogInformation("Started collection agent...");
            agent.Run(configuration);

            this.Analyze(context, Path.Combine(configuration.WorkingDirectory, configuration.OutputName) + ".etl");
        }

        private void Analyze(InvocationContext context, string tracePath)
        {
            HeliosTraceLog traceLog = new();
            string symbolPath = $";SRV*https://msdl.microsoft.com/download/symbols;SRV*https://nuget.smbsrc.net;SRV*https://referencesource.microsoft.com/symbols";
            string? symbolStoreCache = context.ParseResult.GetValueForOption(SymbolStoreCache);
            if (!string.IsNullOrEmpty(symbolStoreCache))
            {
                symbolPath = symbolPath.Replace("*", $"*{symbolStoreCache}*");
            }

            AnalysisContext analysisContext = new()
            {
                TracePath = tracePath,
                WorkingDirectory = this.GetOutputDirectory(context),
                Symbols = ActivatorUtilities.CreateInstance<SymbolStore>(this.services, tracePath + ".log", symbolPath),
                Views = new HashSet<Type>() { typeof(CsvView) },
            };

            foreach (var data in traceLog.Events)
            {

            }
        }

        private string GetOutputDirectory(InvocationContext context)
        {
            StringMacros.DefaultValues macroDefaults = new(this.environment);
            StringMacros pathMacros = new(macroDefaults);
            string outputDirectory = context.ParseResult.GetValueForOption(OutputDirectory)!;
            outputDirectory = Environment.ExpandEnvironmentVariables(outputDirectory);
            return pathMacros.Expand(outputDirectory);
        }
    }
}