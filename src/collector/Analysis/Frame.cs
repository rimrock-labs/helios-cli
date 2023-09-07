namespace Rimrock.Helios.Analysis;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Rimrock.Helios.DataAccess;

/// <summary>
/// Frame class.
/// </summary>
public sealed class Frame : ISerializableNode<Frame>
{
    private readonly int hashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="Frame"/> class.
    /// </summary>
    /// <param name="moduleName">The module name.</param>
    /// <param name="methodName">The method name.</param>
    public Frame(string moduleName, string? methodName = null)
    {
        this.ModuleName = moduleName;
        this.MethodName = methodName ?? string.Empty;

        this.hashCode = HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.ModuleName),
            this.MethodName.GetHashCode());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Frame"/> class.
    /// </summary>
    /// <param name="other">The other frame.</param>
    public Frame(Frame other)
        : this(other.ModuleName, other.MethodName)
    {
        this.InclusiveCount = other.InclusiveCount;
        this.InclusiveWeight = other.InclusiveWeight;
        this.ExclusiveCount = other.ExclusiveCount;
        this.ExclusiveWeight = other.ExclusiveWeight;
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// Gets the method name.
    /// </summary>
    public string MethodName { get; }

    /// <inheritdoc />
    public Frame? Parent { get; set; }

    /// <inheritdoc />
    public Frame? Child { get; set; }

    /// <inheritdoc />
    public Frame? Sibling { get; set; }

    /// <summary>
    /// Gets or sets the incusive count.
    /// </summary>
    public ulong InclusiveCount { get; set; }

    /// <summary>
    /// Gets or sets the inclusive weight.
    /// </summary>
    public ulong InclusiveWeight { get; set; }

    /// <summary>
    /// Gets or sets the excusive count.
    /// </summary>
    public ulong ExclusiveCount { get; set; }

    /// <summary>
    /// Gets or sets the exclusive weight.
    /// </summary>
    public ulong ExclusiveWeight { get; set; }

    /// <inheritdoc />
    public Frame Clone() =>
        new(this);

    /// <summary>
    /// Checks whether the frame module name and method name match provided values.
    /// </summary>
    /// <param name="moduleName">The module name.</param>
    /// <param name="methodName">The method name.</param>
    /// <returns>true if equal, false otherwise.</returns>
    public bool Equals(string moduleName, string methodName) =>
        StringComparer.OrdinalIgnoreCase.Equals(this.ModuleName, moduleName) &&
        string.Equals(this.MethodName, methodName);

    /// <summary>
    /// Metric struct.
    /// </summary>
    public readonly struct Metric
    {
        /// <summary>
        /// Gets the inclusive count.
        /// </summary>
        public required ulong Inclusive { get; init; }

        /// <summary>
        /// Gets the exclusive count.
        /// </summary>
        public required ulong Exclusive { get; init; }
    }

    /// <summary>
    /// <see cref="Frame" /> name equality comparer.
    /// </summary>
    public sealed class NameEqualityComparer : IEqualityComparer<Frame>
    {
        /// <summary>
        /// An instance of the comparer.
        /// </summary>
        public static readonly NameEqualityComparer Instance = new();

        private NameEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(Frame? x, Frame? y) =>
            object.ReferenceEquals(x, y) ||
            (x != null && y != null &&
             x.Equals(y.ModuleName, y.MethodName));

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] Frame obj) =>
            obj.hashCode;
    }

    /// <summary>
    /// Default formatter class.
    /// </summary>
    public sealed class DefaultFormatter : IFrameFormatter
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly DefaultFormatter Instance = new();

        private DefaultFormatter()
        {
        }

        /// <inheritdoc />
        public void Write(StreamWriter writer, Frame frame, Func<string, string>? escaper = null)
        {
            writer.Write(escaper?.Invoke(frame.ModuleName) ?? frame.ModuleName);
            if (!string.IsNullOrEmpty(frame.MethodName))
            {
                writer.Write('!');
                writer.Write(escaper?.Invoke(frame.MethodName) ?? frame.MethodName);
            }
        }
    }
}