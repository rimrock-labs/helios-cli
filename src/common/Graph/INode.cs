namespace Rimrock.Helios.Common.Graph
{
    /// <summary>
    /// Node interface.
    /// </summary>
    public interface INode<TNode>
        where TNode : INode<TNode>
    {
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        TNode? Parent { get; set; }

        /// <summary>
        /// Gets or sets the child.
        /// </summary>
        TNode? Child { get; set; }

        /// <summary>
        /// Gets or sets the sibling.
        /// </summary>
        TNode? Sibling { get; set; }
    }
}