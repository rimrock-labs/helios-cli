namespace Rimrock.Helios.Common.Graph
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Node extensions class.
    /// </summary>
    public static class NodeExtensions
    {
        /// <summary>
        /// Enumerates the graph breadth-first.
        /// </summary>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <param name="node">The node.</param>
        /// <returns>The enumerable.</returns>
        public static IEnumerable<TNode> EnumerateBreadthFirst<TNode>(this TNode node)
            where TNode : class?, INode<TNode>
        {
            var queue = Pool<Queue<TNode>>.Borrow();
            queue.Clear();

            yield return node;

            foreach (var child in node.EnumerateChildren())
            {
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                int level = queue.Count;
                while (level-- > 0 &&
                       queue.TryDequeue(out TNode? node2))
                {
                    yield return node2;

                    foreach (var child in node2.EnumerateChildren())
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            Debug.Assert(queue.Count == 0, "Queue not empty.");
            Pool<Queue<TNode>>.Return(ref queue);
        }

        /// <summary>
        /// Enumerates the siblings.
        /// </summary>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <param name="node">The node.</param>
        /// <returns>The enumerable.</returns>
        public static IEnumerable<TNode> EnumerateSiblings<TNode>(this TNode node)
            where TNode : class?, INode<TNode>
        {
            node = node.Parent?.Child ?? node;
            while (node.Sibling != null)
            {
                yield return node;
                node = node.Sibling;
            }
        }

        /// <summary>
        /// Enumerates the children.
        /// </summary>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <param name="node">The node.</param>
        /// <returns>The enumerable.</returns>
        public static IEnumerable<TNode> EnumerateChildren<TNode>(this TNode node)
            where TNode : class?, INode<TNode>
        {
            TNode? child = node.Child;
            while (child != null)
            {
                yield return child;
                child = child.Sibling;
            }
        }

        /// <summary>
        /// Clones the stack from the leaf.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <returns>The leaf of the stack.</returns>
        public static TNode CloneStackFromLeaf<TNode>(this TNode node)
            where TNode : class?, INode<TNode>
        {
            TNode? result = null;
            TNode? previous = null;
            foreach (TNode parent in node.EnumerateParentStack())
            {
                TNode clone = parent.Clone();
                result ??= clone;

                if (previous != null)
                {
                    clone.AddChild(previous);
                }

                previous = clone;
            }

            return result!;
        }

        /// <summary>
        /// Clones the stack from the root.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <returns>The root of the stack.</returns>
        public static TNode CloneStackFromRoot<TNode>(this TNode node)
            where TNode : class?, INode<TNode>
        {
            TNode? result = null;
            TNode? previous = null;
            foreach (TNode parent in node.EnumerateChildStack())
            {
                TNode clone = parent.Clone();
                result ??= clone;

                previous?.AddChild(clone);
                previous = clone;
            }

            return result!;
        }

        /// <summary>
        /// Enumerates up the parents.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <returns>The enumerable.</returns>
        public static IEnumerable<TNode> EnumerateParentStack<TNode>(this TNode node)
            where TNode : INode<TNode>
        {
            yield return node;
            while (node.Parent != null)
            {
                node = node.Parent;
                yield return node;
            }
        }

        /// <summary>
        /// Enumerates down the children.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <returns>The enumerable.</returns>
        public static IEnumerable<TNode> EnumerateChildStack<TNode>(this TNode node)
            where TNode : INode<TNode>
        {
            yield return node;
            while (node.Child != null)
            {
                node = node.Child;
                yield return node;
            }
        }

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

            while (node != null)
            {
                stack.Push(node);
                node = node.Sibling!;
            }

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