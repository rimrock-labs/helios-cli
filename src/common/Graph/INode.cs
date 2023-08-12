namespace Rimrock.Helios.Common
{
    /// <summary>
    /// Node interface.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        INode? Parent { get; set; }

        /// <summary>
        /// Gets or sets the child.
        /// </summary>
        INode? Child { get; set; }

        /// <summary>
        /// Gets or sets the sibling.
        /// </summary>
        INode? Sibling { get; set; }
    }
}