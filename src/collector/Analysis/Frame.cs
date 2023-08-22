namespace Rimrock.Helios.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Rimrock.Helios.Common.Graph;

    /// <summary>
    /// Frame class.
    /// </summary>
    public sealed class Frame : INode<Frame>
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
            if (other.Metrics != null)
            {
                this.Metrics = new Metric[other.Metrics.Length];
                for (int m = 0; m < other.Metrics.Length; m++)
                {
                    this.Metrics[m] = other.Metrics[m].Clone();
                }
            }
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
        /// Gets or sets the metrics.
        /// </summary>
        public Metric[]? Metrics { get; set; }

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

            /// <summary>
            /// Clones this instance.
            /// </summary>
            /// <returns>The metric.</returns>
            public Metric Clone() =>
                new()
                {
                    Inclusive = this.Inclusive,
                    Exclusive = this.Exclusive,
                };
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
    }
}