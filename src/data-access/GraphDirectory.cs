namespace Rimrock.Helios.DataAccess;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Graph directory class.
/// </summary>
public sealed class GraphDirectory : IEnumerable<GraphDirectory.Entry>
{
    private readonly Dictionary<string, Entry> directory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphDirectory"/> class.
    /// </summary>
    public GraphDirectory()
    {
        this.directory = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds an entry.
    /// </summary>
    /// <param name="entry">The entry.</param>
    public void AddEntry(Entry entry) =>
        this.directory.Add(entry.Name, entry);

    /// <inheritdoc />
    public IEnumerator<Entry> GetEnumerator() =>
        this.directory.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();

    /// <summary>
    /// Entry class.
    /// </summary>
    public sealed class Entry
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// Gets or sets the node count.
        /// </summary>
        public int NodeCount { get; set; }
    }
}