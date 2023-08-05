namespace Rimrock.Helios.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Builder;
    using System.CommandLine.Parsing;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Rimrock.Helios.Cli.Configuration;
    using Rimrock.Helios.Common.Commands;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;

    /// <summary>
    /// Program class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A completion task.</returns>
        public static async Task<int> Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Options options = new();
            configuration.Bind(options);

            Option<bool> verboseOptions = new("--verbose", "Enable verbose logging.");
            RootCommand command = new("Command line interface for performance analysis.");
            command.AddGlobalOption(verboseOptions);

            bool verbose = new Parser(command).Parse(args).GetValueForOption(verboseOptions);
            ILoggerFactory loggerFactory = LoggerFactory.Create(_ =>
            {
                _.AddFilter("App", verbose ? LogLevel.Information : LogLevel.Error);
                _.AddFilter("Command", verbose ? LogLevel.Information : LogLevel.Error);
                _.AddSimpleConsole(ConfigureConsole);
            });

            ILogger logger = loggerFactory.CreateLogger("App");

            foreach (string commandAssembly in options.Commands)
            {
                Assembly? assembly = null;
                try
                {
                    assembly = Assembly.Load(commandAssembly);
                    logger.LogInformation("Loaded '{name}' command assembly.", commandAssembly);
                }
                catch (FileNotFoundException)
                {
                    logger.LogError("Unable to load the '{name}' command assembly.", commandAssembly);
                }

                if (assembly != null)
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.IsAssignableTo(typeof(ICommand)))
                        {
                            ILogger commandLogger = loggerFactory.CreateLogger("Command");
                            ICommand childCommand = (ICommand)Activator.CreateInstance(type, commandLogger)!;
                            logger.LogInformation("Enabled '{command}' command.", childCommand.Name);

                            AddCommands(command, childCommand.GetCommand());
                        }
                    }
                }
            }

            CommandLineBuilder commandLineBuilder = new(command);
            commandLineBuilder.UseDefaults();
            Parser parser = commandLineBuilder.Build();
            return await parser.InvokeAsync(args);

            // Future commands,
            // collect
            // analyze
            // monitor
            // view
            // upload
        }

        private static void ConfigureConsole(SimpleConsoleFormatterOptions console)
        {
            console.IncludeScopes = false;
            console.SingleLine = true;
            console.TimestampFormat = "u";
        }

        private static void AddCommands(RootCommand rootCommand, IReadOnlyCollection<Command> subCommands)
        {
            foreach (Command command in subCommands)
            {
                rootCommand.AddCommand(command);
            }
        }
    }
}