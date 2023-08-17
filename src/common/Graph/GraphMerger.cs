namespace Rimrock.Helios.Common.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// <see cref="INode{TNode}"/> graph merger class.
    /// </summary>
    /// <typeparam name="TNode">The node type.</typeparam>
    public class GraphMerger<TNode>
        where TNode : class?, INode<TNode>
    {
        /// <summary>
        /// Merge the specified graphs.
        /// </summary>
        /// <param name="source">The source graph.</param>
        /// <param name="target">The target graph (accumulator).</param>
        /// <param name="comparer">The comparer to use for node equality.</param>
        public void MergeGraph(TNode source, TNode target, IEqualityComparer<TNode> comparer)
        {
            Stack<(TNode Source, TNode Target)> stack = new();
            stack.Push((source, target));
            while (stack.Count > 0)
            {
                (TNode sourceIterator, TNode targetIterator) = stack.Pop();
                bool matched = false;
                if (comparer.GetHashCode(targetIterator) == comparer.GetHashCode(sourceIterator) &&
                    comparer.Equals(targetIterator, sourceIterator))
                {
                    // nodes are identical
                    this.MergeNode(sourceIterator, targetIterator);
                    matched = true;
                }
                else if (targetIterator.TryFindSibling(sourceIterator, comparer, out TNode? match))
                {
                    // we found a matching sibling
                    targetIterator = match;
                    this.MergeNode(sourceIterator, targetIterator);
                    matched = true;
                }
                else
                {
                    // no match at this level
                    // add source as subtree
                    targetIterator.AddSibling(sourceIterator);
                }

                if (matched)
                {
                    // more children to merge
                    if (sourceIterator.Child != null)
                    {
                        if (targetIterator.Child != null)
                        {
                            // target has children so need to be merged
                            stack.Push((sourceIterator.Child, targetIterator.Child));
                        }
                        else
                        {
                            // target has no children so reparent source
                            targetIterator.AddChild(sourceIterator.Child);
                        }
                    }

                    // more siblings to merge and accumulate
                    if (sourceIterator.Sibling != null)
                    {
                        stack.Push((sourceIterator.Sibling, targetIterator));
                    }
                }
            }
        }

        /// <summary>
        /// Performs the merge between two nodes.
        /// </summary>
        /// <param name="source">The source node.</param>
        /// <param name="target">The target node.</param>
        protected virtual void MergeNode(TNode source, TNode target)
        {
            // implement this to accumulate node level data
        }
    }
}