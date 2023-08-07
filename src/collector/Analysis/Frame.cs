namespace Rimrock.Helios.Analysis
{
    using System;

    /// <summary>
    /// Frame class.
    /// </summary>
    public sealed class Frame : IEquatable<Frame>
    {
        private readonly int hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Frame(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="moduleName">The module name.</param>
        /// <param name="methodName">The method name.</param>
        public Frame(string moduleName, string? methodName)
        {
            this.ModuleName = moduleName;
            this.MethodName = methodName;

            this.hashCode = HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(this.ModuleName),
                this.MethodName?.GetHashCode() ?? 0);
        }

        /// <summary>
        /// Gets the module name.
        /// </summary>
        public string ModuleName { get; }

        /// <summary>
        /// Gets the method name.
        /// </summary>
        public string? MethodName { get; }

        /// <summary>
        /// Gets or sets the caller.
        /// </summary>
        public Frame? Caller { get; set; }

        /// <summary>
        /// Gets or sets the callee.
        /// </summary>
        public Frame? Callee { get; set; }

        /// <inheritdoc />
        public override bool Equals(object? obj) =>
            this.Equals(obj as Frame);

        /// <inheritdoc />
        public bool Equals(Frame? other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null || (this.hashCode != other.hashCode))
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(this.ModuleName, other.ModuleName) &&
                   string.Equals(this.MethodName, other.MethodName);
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.hashCode;
    }
}