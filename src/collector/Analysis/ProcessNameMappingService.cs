namespace Rimrock.Helios.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Diagnostics.Tracing.Etlx;

    internal sealed partial class ProcessNameMappingService
    {
        private static readonly Dictionary<string, IParser> Parsers = GetParsers();

        private readonly Dictionary<int, List<ProcessInterval>> processes = new();

        /// <summary>
        /// Parser interface.
        /// </summary>
        private interface IParser
        {
            /// <summary>
            /// Gets the target process name.
            /// </summary>
            string TargetProcessName { get; }

            /// <summary>
            /// Parses the command line.
            /// </summary>
            /// <param name="name">The process name.</param>
            /// <param name="commandLine">The command line.</param>
            /// <returns>The parsed process name.</returns>
            string Parse(string name, string commandLine);
        }

        public void OnProcess(TraceProcess process)
        {
            if (!this.processes.TryGetValue(process.ProcessID, out List<ProcessInterval>? processList))
            {
                this.processes[process.ProcessID] = processList = new List<ProcessInterval>(2);
            }

            string processName = process.Name;
            if (Parsers.TryGetValue(processName, out IParser? parser))
            {
                processName = parser.Parse(processName, process.CommandLine);
            }

            processList.Add(new ProcessInterval()
            {
                Process = new Process()
                {
                    Id = process.ProcessID,
                    Name = processName,
                },
                StartTime = process.StartTimeRelativeMsec,
                EndTime = process.EndTimeRelativeMsec,
            });
        }

        public bool TryGetProcess(int id, double time, [NotNullWhen(true)] out Process? process)
        {
            process = default;
            if (this.processes.TryGetValue(id, out List<ProcessInterval>? processList))
            {
                if (processList.Count == 1)
                {
                    process = processList[0].Process;
                }
                else
                {
                    process = processList.Where(_ => _.StartTime >= time && time <= _.EndTime).FirstOrDefault()?.Process;
                }
            }

            return process != null;
        }

        private static Dictionary<string, IParser> GetParsers()
        {
            Dictionary<string, IParser> parsers = new(StringComparer.OrdinalIgnoreCase);
            foreach (Type type in typeof(ProcessNameMappingService).GetNestedTypes(System.Reflection.BindingFlags.NonPublic).Where(_ => !_.IsInterface && _.IsAssignableTo(typeof(IParser))))
            {
                IParser parser = (IParser)Activator.CreateInstance(type)!;
                parsers.Add(parser.TargetProcessName, parser);
            }

            return parsers;
        }

        private class ProcessInterval
        {
            public required Process Process { get; init; }

            public required double StartTime { get; init; }

            public required double EndTime { get; init; }
        }

        private partial class DotnetParser : IParser
        {
            public string TargetProcessName => "dotnet";

            public string Parse(string name, string commandLine)
            {
                Match match = GetPattern().Match(commandLine);
                if (match.Success)
                {
                    name = string.Concat(name, "#", match.Groups[1].Value);
                }

                return name;
            }

            [GeneratedRegex(@"([^/\\]+)(?:\.dll)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
            private static partial Regex GetPattern();
        }

        private partial class W3wpParser : IParser
        {
            public string TargetProcessName => "w3wp";

            public string Parse(string name, string commandLine)
            {
                Match match = GetPattern().Match(commandLine);
                if (match.Success)
                {
                    name = string.Concat(name, "#", match.Groups[1].Value);
                }

                return name;
            }

            [GeneratedRegex("-ap \"([^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
            private static partial Regex GetPattern();
        }

        private partial class PowershellParser : IParser
        {
            private static readonly string[] Options = new string[] { "-EncodedCommand", "-Command", "-File" };

            public virtual string TargetProcessName => "powershell";

            public string Parse(string name, string commandLine)
            {
                name = this.TargetProcessName + "#Shell";
                foreach (string option in Options)
                {
                    if (commandLine.IndexOf(option, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        name = string.Concat(name, "#", option[1..]);
                        break;
                    }
                }

                return name;
            }
        }

        private partial class PowershellCoreParser : PowershellParser
        {
            public override string TargetProcessName => "pwsh";
        }
    }
}