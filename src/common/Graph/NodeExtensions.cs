namespace Rimrock.Helios.Common.Graph
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Node extensions class.
    /// </summary>
    public static class NodeExtensions
    {
        /// <summary>
        /// Adds child.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="child">The child.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        public static void AddChild<TNode>(this TNode node, TNode child)
            where TNode : INode<TNode>
        {
            child.Sibling = node.Child;
            node.Child = child;
            child.Parent = node;
        }

        /// <summary>
        /// Adds a sibling.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="sibling">The sibling.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        public static void AddSibling<TNode>(this TNode node, TNode sibling)
            where TNode : INode<TNode>
        {
            TNode next = node;
            while (true)
            {
                if (next.Sibling == null)
                {
                    next.Sibling = sibling;
                    sibling.Parent = next.Parent;
                    break;
                }

                next = next.Sibling;
            }
        }

        /// <summary>
        /// Tries to find a child based on specified comparer.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="target">The target.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="child">The child.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <returns>true if successful, false otherwise.</returns>
        public static bool TryFindChild<TNode>(this TNode parent, TNode target, IEqualityComparer<TNode> comparer, [NotNullWhen(true)] out TNode? child)
            where TNode : class?, INode<TNode>
        {
            bool result = false;
            child = default;

            int targetHashCode = comparer.GetHashCode(target);
            TNode? node = parent.Child;
            while (node != null)
            {
                if (targetHashCode == comparer.GetHashCode(node) &&
                    comparer.Equals(target, node))
                {
                    child = node;
                    result = true;
                    break;
                }

                node = node.Sibling;
            }

            return result;
        }

        /// <summary>
        /// Tries to find a sibling based on the specified comparer.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="target">The target.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="sibling">The sibling.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <returns>true if successful, false otherwise.</returns>
        public static bool TryFindSibling<TNode>(this TNode node, TNode target, IEqualityComparer<TNode> comparer, [NotNullWhen(true)] out TNode? sibling)
            where TNode : class?, INode<TNode>
        {
            bool result = false;
            sibling = default;

            int targetHashCode = comparer.GetHashCode(target);

            // get the first sibling from the parent's view or this node it self
            TNode? anotherNode = node.Parent?.Sibling ?? node;
            while (anotherNode != null)
            {
                if (targetHashCode == comparer.GetHashCode(anotherNode) &&
                    comparer.Equals(target, anotherNode))
                {
                    sibling = anotherNode;
                    result = true;
                    break;
                }

                anotherNode = anotherNode.Sibling;
            }

            return result;
        }

        /// <summary>
        /// Enumerates depth first through the node graph.
        /// </summary>
        /// <param name="node">The start node.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <returns>The nodes.</returns>
        public static IEnumerable<TNode> EnumerateDepthFirst<TNode>(this TNode node)
            where TNode : class?, INode<TNode>
        {
            Stack<TNode> stack = new();
            stack.Clear();
            stack.Push(node);
            while (stack.TryPop(out TNode? stackNode))
            {
                yield return stackNode;

                for (stackNode = stackNode.Child; stackNode != null; stackNode = stackNode.Sibling)
                {
                    stack.Push(stackNode);
                }
            }

            stack.Clear();
        }
    }
}