namespace Rimrock.Helios.Analysis.Views
{
    using System;
    using System.Collections.Generic;
    using Rimrock.Helios.Common;

    /// <summary>
    /// Weighted graph data class.
    /// </summary>
    public class WeightedGraphData : IData
    {
        /// <summary>
        /// The tags.
        /// </summary>
        private readonly HashSet<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeightedGraphData"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="weight1">The first weight.</param>
        /// <param name="weight2">The second weight.</param>
        public WeightedGraphData(
            DataFrame frame,
            uint weight1 = 1,
            uint weight2 = 1)
        {
            this.Frame = frame;
            this.Weight1 = weight1;
            this.Weight2 = weight2;
            this.tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the root frame.
        /// </summary>
        public DataFrame Frame { get; }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        public IReadOnlySet<string> Tags => this.tags;

        /// <summary>
        /// Gets the first weight.
        /// </summary>
        public uint Weight1 { get; }

        /// <summary>
        /// Gets the second weight.
        /// </summary>
        public uint Weight2 { get; }

        /// <summary>
        /// Adds a tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public void AddTag(string tag) =>
            this.tags.Add(tag);

        /// <summary>
        /// Adds a process tag.
        /// </summary>
        /// <param name="processName">The process name.</param>
        public void AddProcessTag(string processName) =>
            this.AddTag($"process: {processName}");
    }
}