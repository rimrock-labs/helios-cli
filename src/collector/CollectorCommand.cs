namespace Rimrock.Helios.Collector
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Collection;
    using Rimrock.Helios.Common.Commands;

    /// <summary>
    /// Collector command class.
    /// </summary>
    public class CollectorCommand : ICommand
    {
        private static readonly Option<string> OutputDirectory = new("--output-directory", description: "Output directory.") { IsRequired = true };
        private static readonly Option<TimeSpan> Duration = new("--duration", description: "Duration of collection.", getDefaultValue: () => TimeSpan.FromMinutes(1));

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CollectorCommand(ILogger logger)
        {
            this.logger = logger;
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
            command.AddOption(new Option<string[]>("--output-format", description: "Output format.", getDefaultValue: () => new[] { "CSV" }).FromAmong("CSV"));
            command.AddOption(Duration);
            command.SetHandler(this.Collect);
            return new[] { command };
        }

        private void Collect(InvocationContext context)
        {
            context.Console.WriteLine("Collect!");
            this.logger.LogInformation("[Collect] Collector!");

            PerfViewAgent.Configuration configuration = new()
            {
                PerfViewPath = Path.Combine(Directory.GetCurrentDirectory(), "PerfView", "PerfView.exe"),
                WorkingDirectory = context.ParseResult.GetValueForOption(OutputDirectory)!,
                OutputName = $"Helios-{Environment.MachineName}-{DateTimeOffset.UtcNow:u}",
                Duration = context.ParseResult.GetValueForOption(Duration),
                KernelEvents = Array.Empty<string>(),
                ClrEvents = Array.Empty<string>(),
            };

            using PerfViewAgent agent = PerfViewAgent.Start(configuration);
            agent.Wait();

            this.Analyze(context);
        }

        private void Analyze(InvocationContext context)
        {

        }
    }
}