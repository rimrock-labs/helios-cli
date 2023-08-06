namespace Rimrock.Helios.Collector
{
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Common.Commands;

    /// <summary>
    /// Collector command class.
    /// </summary>
    public class CollectorCommand : ICommand
    {
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
            command.AddOption(new Option<string[]>("--data-analyzers", description: "Data sets to collect.", getDefaultValue: () => new[] { "CPU" }).FromAmong("CPU"));
            command.AddOption(new Option<int[]>("--process-ids", description: "Process identifiers to focus the data to."));
            command.AddOption(new Option<string[]>("--output-format", description: "Output format.", getDefaultValue: () => new[] { "CSV" }).FromAmong("CSV"));
            command.SetHandler(this.Collect);
            return new[] { command };
        }

        private void Collect(InvocationContext context)
        {
            context.Console.WriteLine("Collect!");
            this.logger.LogInformation("[Collect] Collector!");
        }
    }
}