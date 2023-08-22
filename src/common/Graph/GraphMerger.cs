namespace Rimrock.Helios.Common.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// <see cref="INode{TNode}"/> graph merger class.
    /// </summary>
    /// <typeparam name="TNode">The node type.</typeparam>
    /// <typeparam name="TContext">The merge context type.</typeparam>
    public class GraphMerger<TNode, TContext>
        where TNode : class?, INode<TNode>
        where TContext : class?
    {
        /// <summary>
        /// Merge the specified graphs.
        /// </summary>
        /// <param name="sourceRoot">The source graph root.</param>
        /// <param name="targetRoot">The target graph root (accumulator).</param>
        /// <param name="comparer">The comparer to use for node equality.</param>
        /// <param name="mergeContext">The merge operation context.</param>
        public void MergeGraph(TNode sourceRoot, TNode targetRoot, IEqualityComparer<TNode>? comparer = null, TContext? mergeContext = null)
        {
            comparer ??= EqualityComparer<TNode>.Default;
            Stack<(TNode Source, TNode Target)> stack = new();
            stack.Push((sourceRoot, targetRoot));
            while (stack.Count > 0)
            {
                (TNode sourceIterator, TNode targetIterator) = stack.Pop();
                bool matched = false;
                if (comparer.GetHashCode(targetIterator) == comparer.GetHashCode(sourceIterator) &&
                    comparer.Equals(targetIterator, sourceIterator))
                {
                    // nodes are identical
                    matched = true;
                }
                else if (targetIterator.TryFindSibling(sourceIterator, comparer, out TNode? match))
                {
                    // we found a matching sibling
                    targetIterator = match;
                    matched = true;
                }
                else
                {
                    // no match at this level
                    // add source as subtree
                    targetIterator.AddSibling(this.OnInsert(sourceIterator));
                }

                if (matched)
                {
                    this.MergeNode(sourceIterator, targetIterator, mergeContext);

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
                            targetIterator.AddChild(this.OnInsert(sourceIterator.Child));
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

        protected virtual TNode OnInsert(TNode node, TContext? context = null) =>
            node;

        /// <summary>
        /// Performs the merge between two nodes.
        /// </summary>
        /// <param name="source">The source node.</param>
        /// <param name="target">The target node.</param>
        /// <param name="mergeContext">The merge operation context.</param>
        protected virtual void MergeNode(TNode source, TNode target, TContext? mergeContext = null)
        {
            // implement this to accumulate node level data
        }
    }
}