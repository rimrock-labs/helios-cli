namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Stack data class.
    /// </summary>
    public class StackData : IData
    {
        private readonly HashSet<string> tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="StackData"/> class.
        /// </summary>
        /// <param name="callStack">The call stack.</param>
        public StackData(Frame callStack)
        {
            this.CallStack = callStack;
            this.tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the call stack.
        /// </summary>
        public Frame CallStack { get; }

        /// <summary>
        /// Gets or sets the weight.
        /// </summary>
        public ulong Weight { get; set; } = 1;

        /// <summary>
        /// Gets the tags.
        /// </summary>
        public IReadOnlySet<string> Tags => this.tags;

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