namespace Rimrock.Helios.Common.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// <see cref="INode{TNode}"/> graph merger class.
    /// </summary>
    public class GraphMerger<TNode>
        where TNode : class?, INode<TNode>
    {
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
                    this.MergeNode(sourceIterator, targetIterator);
                    matched = true;
                }
                else if (targetIterator.TryFindSibling(sourceIterator, comparer, out TNode? match))
                {
                    targetIterator = match;
                    this.MergeNode(sourceIterator, targetIterator);
                    matched = true;
                }
                else
                {
                    // add source as subtree
                    targetIterator.AddSibling(sourceIterator);
                }

                if (matched)
                {
                    if (sourceIterator.Child != null)
                    {
                        if (targetIterator.Child != null)
                        {
                            stack.Push((sourceIterator.Child, targetIterator.Child));
                        }
                        else
                        {
                            targetIterator.AddChild(sourceIterator.Child);
                        }
                    }

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
        }
    }
}