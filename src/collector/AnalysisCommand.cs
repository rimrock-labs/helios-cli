namespace Rimrock.Helios.Analysis
{
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Rimrock.Helios.Common.Commands;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Analysis command class.
    /// </summary>
    public class AnalysisCommand : ICommand
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisCommand"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AnalysisCommand(ILogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public string Name => "analyze";

        /// <inheritdoc />
        public IReadOnlyList<Command> GetCommand()
        {
            Command command = new(this.Name, "Analyzes a data set collection.");
            command.AddOption(new Option<string>("--path", "Path to the data set collection.") { IsRequired = true });
            command.SetHandler(this.Analyze);
            return new[] { command };
        }

        private void Analyze(InvocationContext context)
        {
        }
    }
}
