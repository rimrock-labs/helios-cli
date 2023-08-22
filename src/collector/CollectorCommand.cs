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
        private static readonly Option<string> OutputDirectoryOption = new("--output-directory", "Output directory.") { IsRequired = true };
        private static readonly Option<TimeSpan> DurationOption = new("--duration", description: "Duration of collection.", getDefaultValue: () => TimeSpan.FromMinutes(1));
        private static readonly Option<string> SymbolStoreCacheOption = new("--symbol-store-cache", "Path to directory where symbols are cached.");
        private static readonly Option<string[]> OutputFormatOption = new Option<string[]>("--output-format", description: "Output format.", getDefaultValue: () => new[] { OutputFormatAttribute.GetName(typeof(CsvOutputFormat)) }).FromAmong(OutputFormatAttribute.GetViews().Keys.ToArray());
        private static readonly Option<string[]> DataAnalyzerOption = new Option<string[]>("--data-analyzer", description: "Data sets to collect.", getDefaultValue: () => new[] { DataAnalyzerAttribute.GetName(typeof(CpuDataAnalyzer)) }).FromAmong(DataAnalyzerAttribute.GetAnalyzers().Keys.ToArray());
        private static readonly Option<int[]> ProcessIdOption = new("--process-id", "Process identifiers to focus the data to.");
        private static readonly Option<string> TracePathOption = new("--trace-path", "Path to existing trace.");
        private static readonly Option<bool> ResolveNativeSymbolsOption = new("--resolve-native-symbols", "Resolve native symbols.");

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
            Command command = new(this.Name, description: "Collects and analyzes data from the current machine's running processes.");
            command.AddOption(OutputDirectoryOption);
            command.AddOption(DataAnalyzerOption);
            command.AddOption(ProcessIdOption);
            command.AddOption(OutputFormatOption);
            command.AddOption(DurationOption);
            command.AddOption(SymbolStoreCacheOption);
            command.AddOption(TracePathOption);
            command.AddOption(ResolveNativeSymbolsOption);
            command.SetHandler(this.Collect);
            return new[] { command };
        }

        private void Collect(InvocationContext context)
        {
            string workingDirectory = this.GetOutputDirectory(context);
            this.fileSystem.CreateDirectory(workingDirectory);
            this.logger.LogInformation("Set working directory, {dir}", workingDirectory);

            string? tracePath = context.ParseResult.GetValueForOption(TracePathOption);
            if (tracePath == null || !this.fileSystem.FileExists(tracePath))
            {
                HashSet<Type> analyzerTypes = new(DataAnalyzerAttribute.GetAnalyzersByName(context.ParseResult.GetValueForOption(DataAnalyzerOption)));

                HashSet<string> kernelEvents = new(StringComparer.OrdinalIgnoreCase) { "Process", "ImageLoad" };
                HashSet<string> clrEvents = new(StringComparer.OrdinalIgnoreCase) { "Loader" };
                kernelEvents.AddRange(PerfViewAgent.ProfilingDefinitionAttribute.GetKernelEvents(analyzerTypes));
                clrEvents.AddRange(PerfViewAgent.ProfilingDefinitionAttribute.GetClrEvents(analyzerTypes));

                PerfViewAgent.Configuration configuration = new()
                {
                    PerfViewPath = Path.Combine(this.environment.ApplicationDirectory, "PerfView", "PerfView.exe"),
                    WorkingDirectory = workingDirectory,
                    OutputName = $"Helios-{Environment.MachineName}-{DateTimeOffset.UtcNow:yyyyMMdd-hhmmss}",
                    Duration = context.ParseResult.GetValueForOption(DurationOption),
                    KernelEvents = kernelEvents.ToArray(),
                    ClrEvents = clrEvents.ToArray(),
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

            this.Analyze(context, workingDirectory, tracePath);
        }

        private void Analyze(InvocationContext context, string workingDirectory, string tracePath)
        {
            string symbolPath = $";SRV*https://msdl.microsoft.com/download/symbols;SRV*https://nuget.smbsrc.net;SRV*https://referencesource.microsoft.com/symbols";
            string? symbolStoreCache = context.ParseResult.GetValueForOption(SymbolStoreCacheOption);
            if (!string.IsNullOrEmpty(symbolStoreCache))
            {
                symbolPath = symbolPath.Replace("*", $"*{symbolStoreCache}*");
            }

            SymbolStore symbolStore = ActivatorUtilities.CreateInstance<SymbolStore>(this.services, Path.ChangeExtension(tracePath, "symbol.log"), symbolPath);
            symbolStore.ResolveNativeSymbols = context.ParseResult.GetValueForOption(ResolveNativeSymbolsOption);
            AnalysisContext analysisContext = new()
            {
                TracePath = tracePath,
                WorkingDirectory = workingDirectory,
                Symbols = symbolStore,
                OutputFormats = new HashSet<Type>(OutputFormatAttribute.GetViewsByName(context.ParseResult.GetValueForOption(OutputFormatOption))),
            };

            HashSet<Type> analyzerTypes = new(DataAnalyzerAttribute.GetAnalyzersByName(context.ParseResult.GetValueForOption(DataAnalyzerOption)));
            foreach (Type analyzerType in analyzerTypes)
            {
                IDataAnalyzer analyzer = (IDataAnalyzer)ActivatorUtilities.CreateInstance(this.services, analyzerType);
                analysisContext.Analyzers.Add(analyzer);
                this.logger.LogInformation("Constructed {analyzer} analyzer.", analyzerType.Name);
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

            HeliosTraceLog traceLog = new(tracePath);

            this.logger.LogInformation("Opened trace, {trace}", tracePath);

            long eventCount = 0;
            long processedEventCount = 0;

            // OnData
            foreach (TraceEvent data in traceLog.Events)
            {
                if (processIds.Count == 0 || processIds.Contains(data.ProcessID))
                {
                    if (traceLog.ProcessMap.TryGetProcess(data.ProcessID, data.TimeStampRelativeMSec, out Process? process))
                    {
                        for (int i = 0; i < analysisContext.Analyzers.Count; i++)
                        {
                            IDataAnalyzer analyzer = analysisContext.Analyzers[i];
                            if (analyzer.OnData(analysisContext, process, data))
                            {
                                processedEventCount++;
                            }

                            eventCount++;
                        }
                    }
                }
            }

            this.logger.LogInformation("Iterated through {count1} events, processed {count2} events.", eventCount, processedEventCount);

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