namespace Rimrock.Helios.DataAccess;

using System.IO;
using Rimrock.Helios.Common.Graph;

/// <summary>
/// Serializable node interface.
/// </summary>
/// <typeparam name="TNode">The node type.</typeparam>
public interface ISerializableNode<TNode> : INode<TNode>
    where TNode : INode<TNode>
{
    /// <summary>
    /// Gets the branch name.
    /// </summary>
    /// <returns>The directory name.</returns>
    string GetBranchName();

    /// <summary>
    /// Writes the instance to binary stream.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="stringIndexFactory">The string index factory.</param>
    void Write(BinaryWriter writer, IndexedSetFactory<string> stringIndexFactory);
}