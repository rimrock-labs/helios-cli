namespace Rimrock.Helios.Collector
{
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Rimrock.Helios.Common.Commands;
    using Microsoft.Extensions.Logging;

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
            Command command = new(this.Name, description: "Collects a profile.");
            command.AddOption(new Option<string[]>("--data-set", description: "Data sets to collect.") { IsRequired = true });
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