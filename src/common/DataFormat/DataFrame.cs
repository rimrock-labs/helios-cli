namespace Rimrock.Helios.Common
{
    /// <summary>
    /// Data frame class.
    /// </summary>
    public class DataFrame
    {
        /// <summary>
        /// Gets or sets the modules name.
        /// </summary>
        public string? ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the method name.
        /// </summary>
        public string? MethodName { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public DataFrame? Parent { get; set; }

        /// <summary>
        /// Gets or sets the child.
        /// </summary>
        public DataFrame? Child { get; set; }

        /// <summary>
        /// Gets or sets the sibling.
        /// </summary>
        public DataFrame? Sibling { get; set; }

        /// <summary>
        /// Gets or sets the first value.
        /// </summary>
        public ulong Value1 { get; set; }

        /// <summary>
        /// Gets or sets the second value.
        /// </summary>
        public ulong Value2 { get; set; }

        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="child">The child.</param>
        public void AddChild(DataFrame child)
        {
            child.Sibling = this.Child;
            this.Child = child;
            child.Parent = this;
        }
    }
}