namespace Rimrock.Helios.Collector
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Analysis;
    using Rimrock.Helios.Analysis.Analyzers;
    using Rimrock.Helios.Analysis.OutputFormats;
    using Rimrock.Helios.Collection;
    using Rimrock.Helios.Collection.ETW;
    using Rimrock.Helios.Common;
    using Rimrock.Helios.Common.Commands;

    /// <summary>
    /// Collector command class.
    /// </summary>
    public class CollectorCommand : ICommand
    {
        private static readonly Option<string> OutputDirectoryOption = new("--output-directory", description: "Output directory.") { IsRequired = true };
        private static readonly Option<TimeSpan> DurationOption = new("--duration", description: "Duration of collection.", getDefaultValue: () => TimeSpan.FromMinutes(1));
        private static readonly Option<string> SymbolStoreCacheOption = new("--symbol-store-cache", description: "Path to directory where symbols are cached.");
        private static readonly Option<string[]> OutputFormatOption = new Option<string[]>("--output-format", description: "Output format.", getDefaultValue: () => new[] { OutputFormatAttribute.GetName(typeof(CsvOutputFormat)) }).FromAmong(OutputFormatAttribute.GetViews().Keys.ToArray());
        private static readonly Option<string[]> DataAnalyzerOption = new Option<string[]>("--data-analyzer", description: "Data sets to collect.", getDefaultValue: () => new[] { DataAnalyzerAttribute.GetName(typeof(CpuDataAnalyzer)) }).FromAmong(DataAnalyzerAttribute.GetAnalyzers().Keys.ToArray());
        private static readonly Option<int[]> ProcessIdOption = new("--process-id", description: "Process identifiers to focus the data to.");
        private static readonly Option<string> TracePathOption = new("--trace-path", description: "Path to existing trace.");

        private readonly ILogger<CollectorCommand> logger;
        private readonly HeliosEnvironment environment;
        private readonly FileSystem fileSystem;
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="services">The services.</param>
        public CollectorCommand(
            ILogger<CollectorCommand> logger,
            HeliosEnvironment environment,
            FileSystem fileSystem,
            IServiceProvider services)
        {
            this.logger = logger;
            this.environment = environment;
            this.fileSystem = fileSystem;
            this.services = services;
        }

        /// <inheritdoc />
        public string Name => "collect";

        /// <inheritdoc />
        public IReadOnlyList<Command> GetCommand()
        {
            Command command = new(this.Name, description: "Collects data from the current machine.");
            command.AddOption(OutputDirectoryOption);
            command.AddOption(DataAnalyzerOption);
            command.AddOption(ProcessIdOption);
            command.AddOption(OutputFormatOption);
            command.AddOption(DurationOption);
            command.AddOption(SymbolStoreCacheOption);
            command.AddOption(TracePathOption);
            command.SetHandler(this.Collect);
            return new[] { command };
        }

        private void Collect(InvocationContext context)
        {
            string? tracePath = context.ParseResult.GetValueForOption(TracePathOption);
            if (tracePath == null || !this.fileSystem.FileExists(tracePath))
            {
                PerfViewAgent.Configuration configuration = new()
                {
                    PerfViewPath = Path.Combine(this.environment.ApplicationDirectory, "PerfView", "PerfView.exe"),
                    WorkingDirectory = this.GetOutputDirectory(context),
                    OutputName = $"Helios-{Environment.MachineName}-{DateTimeOffset.UtcNow:yyyyMMdd-hhmmss}",
                    Duration = context.ParseResult.GetValueForOption(DurationOption),
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

                PerfViewAgent agent = ActivatorUtilities.CreateInstance<PerfViewAgent>(this.services);
                this.logger.LogInformation("Started collection agent...");
                agent.Run(configuration);
                tracePath = Path.Combine(configuration.WorkingDirectory, configuration.OutputName) + ".etl";
            }
            else
            {
                this.logger.LogInformation("Skipping collection, path to existing trace was specified.");
            }

            this.Analyze(context, tracePath);
        }

        private void Analyze(InvocationContext context, string tracePath)
        {
            HeliosTraceLog traceLog = new();
            string symbolPath = $";SRV*https://msdl.microsoft.com/download/symbols;SRV*https://nuget.smbsrc.net;SRV*https://referencesource.microsoft.com/symbols";
            string? symbolStoreCache = context.ParseResult.GetValueForOption(SymbolStoreCacheOption);
            if (!string.IsNullOrEmpty(symbolStoreCache))
            {
                symbolPath = symbolPath.Replace("*", $"*{symbolStoreCache}*");
            }

            AnalysisContext analysisContext = new()
            {
                TracePath = tracePath,
                WorkingDirectory = this.GetOutputDirectory(context),
                Symbols = ActivatorUtilities.CreateInstance<SymbolStore>(this.services, tracePath + ".log", symbolPath),
                OutputFormats = new HashSet<Type>(OutputFormatAttribute.GetViewsByName(context.ParseResult.GetValueForOption(OutputFormatOption))),
            };

            HashSet<Type> analyzerTypes = new(DataAnalyzerAttribute.GetAnalyzersByName(context.ParseResult.GetValueForOption(DataAnalyzerOption)));
            foreach (Type analyzerType in analyzerTypes)
            {
                var analyzer = (IDataAnalyzer)ActivatorUtilities.CreateInstance(this.services, analyzerType);
                analysisContext.Analyzers.Add(analyzer);
            }

            this.logger.LogInformation("Created {number} analyzers.", analysisContext.Analyzers.Count);

            HashSet<int> processIds = new(context.ParseResult.GetValueForOption(ProcessIdOption) ?? Enumerable.Empty<int>());

            this.logger.LogInformation("Starting analysis...");

            // OnStart
            for (int i = 0; i < analysisContext.Analyzers.Count; i++)
            {
                IDataAnalyzer analyzer = analysisContext.Analyzers[i];
                analyzer.OnStart(analysisContext);
            }

            // OnData
            foreach (TraceEvent data in traceLog.Events)
            {
                if (processIds.Count == 0 || processIds.Contains(data.ProcessID))
                {
                    for (int i = 0; i < analysisContext.Analyzers.Count; i++)
                    {
                        IDataAnalyzer analyzer = analysisContext.Analyzers[i];
                        analyzer.OnData(analysisContext, data);
                    }
                }
            }

            // OnEnd
            for (int i = 0; i < analysisContext.Analyzers.Count; i++)
            {
                IDataAnalyzer analyzer = analysisContext.Analyzers[i];
                analyzer.OnEnd(analysisContext);
            }
        }

        private string GetOutputDirectory(InvocationContext context)
        {
            StringMacros.DefaultValues macroDefaults = new(this.environment);
            StringMacros pathMacros = new(macroDefaults);
            string outputDirectory = context.ParseResult.GetValueForOption(OutputDirectoryOption)!;
            outputDirectory = Environment.ExpandEnvironmentVariables(outputDirectory);
            return pathMacros.Expand(outputDirectory);
        }
    }
}