namespace Rimrock.Helios.Analysis.OutputFormats
{
    /// <summary>
    /// Stack data class.
    /// </summary>
    public class StackData : IData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackData"/> class.
        /// </summary>
        /// <param name="stackLeaf">The stack leaf.</param>
        /// /// <param name="stackRoot">The stack root.</param>
        /// <param name="count">The count.</param>
        /// <param name="weight">The weight.</param>
        public StackData(Frame stackLeaf, Frame stackRoot, ulong count = 1, ulong weight = 1)
        {
            this.StackLeaf = stackLeaf;
            this.StackRoot = stackRoot;
            this.Count = count;
            this.Weight = weight;
        }

        /// <summary>
        /// Gets the stack leaf.
        /// </summary>
        public Frame StackLeaf { get; }

        /// <summary>
        /// Gets the stack root.
        /// </summary>
        public Frame StackRoot { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public ulong Count { get; }

        /// <summary>
        /// Gets the weight.
        /// </summary>
        public ulong Weight { get; }
    }
}