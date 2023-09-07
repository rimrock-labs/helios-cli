namespace Rimrock.Helios.DataAccess;

using System;
using System.Collections.Generic;
using System.IO;
using Rimrock.Helios.Common;
using Rimrock.Helios.Common.Graph;

/// <summary>
/// Graph writer class.
/// </summary>
/// <typeparam name="TNode">The node type.</typeparam>
public class GraphWriter<TNode> : IDisposable
    where TNode : class?, ISerializableNode<TNode>
{
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphWriter{TNode}"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="options">The options.</param>
    public GraphWriter(Stream stream, Options? options = default)
    {
        this.Writer = new BinaryWriter(stream);
    }

    /// <summary>
    /// Gets the writer.
    /// </summary>
    protected BinaryWriter Writer { get; }

    /// <summary>
    /// Writes the specified graph out.
    /// </summary>
    /// <param name="graph">The graph.</param>
    public virtual void Write(TNode graph)
    {
        GraphDirectory directory = new();
        IndexedSetFactory<string> stringSets = new();
        foreach (var branch in graph.EnumerateSiblings())
        {
            long streamOffset = this.Writer.BaseStream.Position;
            int nodeCount = 0;

            var ids = Pool<Dictionary<TNode, uint>>.Borrow(() => new Dictionary<TNode, uint>(ReferenceEqualityComparer.Instance));
            foreach (var node in branch.EnumerateBreadthFirst())
            {
                byte nodeTypeFlag = 0x0;

                // TODO: write flag
                node.Write(this.Writer, stringSets);

                nodeCount++;
            }

            directory.AddEntry(new GraphDirectory.Entry()
            {
                Name = branch.GetBranchName(),
                Offset = streamOffset,
                NodeCount = nodeCount,
            });
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!this.disposed)
        {
            this.Writer?.Dispose();

            this.disposed = true;
        }
    }

    /// <summary>
    /// Options class.
    /// </summary>
    public sealed class Options
    {

    }
}
