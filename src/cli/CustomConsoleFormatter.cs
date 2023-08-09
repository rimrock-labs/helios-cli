namespace Rimrock.Helios.Cli
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Logging.Console;

    internal sealed class CustomConsoleFormatter : ConsoleFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomConsoleFormatter"/> class.
        /// </summary>
        public CustomConsoleFormatter()
            : base(nameof(CustomConsoleFormatter))
        {
        }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider? scopeProvider,
            TextWriter textWriter)
        {
            string? message =
                logEntry.Formatter?.Invoke(
                    logEntry.State, logEntry.Exception);

            if (message is null)
            {
                return;
            }

            if (logEntry.LogLevel == LogLevel.Error)
            {
                // red
                textWriter.Write("\x1b[1;31m");
            }
            else if (logEntry.LogLevel == LogLevel.Warning)
            {
                // yellow
                textWriter.Write("\x1b[1;33m");
            }

            textWriter.Write('[');
            textWriter.Write(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ"));
            textWriter.Write("] ");
            textWriter.Write(logEntry.Category);
            textWriter.Write(": ");
            textWriter.WriteLine(message);

            // clear formatting
            textWriter.Write("\x1b[0m");
        }

        public class Options : ConsoleFormatterOptions
        {
        }
    }
}