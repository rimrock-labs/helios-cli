namespace Rimrock.Helios.Analysis
{
    /// <summary>
    /// Process class.
    /// </summary>
    public sealed class Process
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public required string Name { get; init; }
    }
}