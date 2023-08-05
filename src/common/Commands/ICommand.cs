namespace Rimrock.Helios.Common.Commands
{
    using System.Collections.Generic;
    using System.CommandLine;

    /// <summary>
    /// Command interface.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the commands supported by this command.
        /// </summary>
        /// <returns>A collection of commands.</returns>
        public IReadOnlyList<Command> GetCommand();
    }
}