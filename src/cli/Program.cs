﻿namespace Rimrock.Helios.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Builder;
    using System.CommandLine.Parsing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.Options;
    using Rimrock.Helios.Cli.Configuration;
    using Rimrock.Helios.Common;
    using Rimrock.Helios.Common.Commands;

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
        public static Task<int> Main(string[] args)
        {
            Option<bool> verboseOptions = new("--verbose", "Enable verbose logging.");
            RootCommand command = new("Command line interface for performance analysis.");
            command.AddGlobalOption(verboseOptions);

            bool verbose = args.Any(_ => _.Contains(verboseOptions.Name, StringComparison.OrdinalIgnoreCase));
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(
                    config =>
                    {
                        config.AddJsonFile("helios-cli.json", optional: false);
                        config.AddJsonFile($"helios-cli.{Environment.MachineName}.json", optional: true);
                        config.AddEnvironmentVariables();
                    })
                .ConfigureLogging(
                    logging =>
                    {
                        logging.ClearProviders();
                        logging.AddFilter("*", LogLevel.None);
                        logging.AddFilter("Rimrock", verbose ? LogLevel.Trace : LogLevel.Warning);
                        //logging.AddSimpleConsole(ConfigureConsole);
                        logging.AddConsole(_ => _.FormatterName = nameof(CustomConsoleFormatter));
                        logging.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatter.Options>();
                    })
                .ConfigureServices(
                    services =>
                    {
                        services.AddSingleton<FileSystem>();
                        services.AddSingleton<HeliosEnvironment>();
                        services.AddOptions<AppSettings>().BindConfiguration(string.Empty).ValidateDataAnnotations();
                    })
                .Build();

            ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
            IOptions<AppSettings> options = host.Services.GetRequiredService<IOptions<AppSettings>>();
            AppSettings settings = options.Value;
            foreach (string commandAssembly in settings.Commands)
            {
                Assembly? assembly = null;
                try
                {
                    assembly = Assembly.Load(commandAssembly);
                }
                catch (FileNotFoundException)
                {
                    // silently skip things we cannot load?
                }

                if (assembly != null)
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.IsAssignableTo(typeof(ICommand)))
                        {
                            ICommand childCommand = (ICommand)ActivatorUtilities.CreateInstance(host.Services, type);
                            AddCommands(command, childCommand.GetCommand(host.Services));
                        }
                    }
                }
            }

            return new CommandLineBuilder(command)
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
        }

        private static void ConfigureConsole(SimpleConsoleFormatterOptions console)
        {
            console.IncludeScopes = false;
            console.TimestampFormat = "yyyy-MM-dd hh:mm:ssZ ";
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