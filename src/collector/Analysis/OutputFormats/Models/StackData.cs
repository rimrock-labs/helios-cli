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
        public StackData(Frame stackLeaf, Frame stackRoot)
        {
            this.StackLeaf = stackLeaf;
            this.StackRoot = stackRoot;
        }

        /// <summary>
        /// Gets the stack leaf.
        /// </summary>
        public Frame StackLeaf { get; }

        /// <summary>
        /// Gets the stack root.
        /// </summary>
        public Frame StackRoot { get; }
    }
}