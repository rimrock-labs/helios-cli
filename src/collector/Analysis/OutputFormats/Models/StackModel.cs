namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Common.Graph;

    /// <summary>
    /// Stack model class.
    /// </summary>
    public sealed class StackModel : IDataModel
    {
        private readonly ILogger<StackModel> logger;
        private readonly Dictionary<StackData, Statistics> data;

        /// <summary>
        /// Initializes a new instance of the <see cref="StackModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public StackModel(ILogger<StackModel> logger)
        {
            this.logger = logger;
            this.data = new Dictionary<StackData, Statistics>(DataComparer.Instance);
        }

        /// <summary>
        /// Gets the model data.
        /// </summary>
        /// <returns>The data.</returns>
        public IReadOnlyDictionary<StackData, Statistics> GetData() => this.data;

        /// <inheritdoc />
        public void AddData(IData data)
        {
            if (data is StackData callData)
            {
                this.AddData(callData);
            }
            else
            {
                this.logger.LogWarning("Ignoring received data of '{type}' type but that could not be processed.", data.GetType().FullName);
            }
        }

        private void AddData(StackData data)
        {
            if (!this.data.TryGetValue(data, out Statistics? statistics))
            {
                this.data[data] = statistics = new Statistics();
            }

            statistics.Count += data.StackRoot.InclusiveCount;
            statistics.Weight += data.StackRoot.InclusiveWeight;
        }

        /// <summary>
        /// Data comparer class.
        /// </summary>
        public class DataComparer : EqualityComparer<StackData>
        {
            /// <summary>
            /// The comparer instance.
            /// </summary>
            public static readonly DataComparer Instance = new();

            /// <summary>
            /// Prevents a default instance of the <see cref="DataComparer"/> class from being created.
            /// </summary>
            private DataComparer()
            {
            }

            /// <inheritdoc/>
            public override bool Equals(StackData? x, StackData? y)
            {
                bool result = (x == null && y == null) || (x != null && y != null);
                if (result)
                {
                    Frame? xFrame = x?.StackLeaf!;
                    Frame? yFrame = y?.StackLeaf!;
                    do
                    {
                        result = object.Equals(xFrame, yFrame);
                        xFrame = xFrame.Parent;
                        yFrame = yFrame.Parent;
                    }
                    while (result && xFrame != null && yFrame != null);
                }

                return result;
            }

            /// <inheritdoc/>
            public override int GetHashCode(StackData obj)
            {
                return HashCode.Combine(
                    obj.StackLeaf.EnumerateParentStack().Select(_ => _.GetHashCode()));
            }
        }

        /// <summary>
        /// Statistics class.
        /// </summary>
        public sealed class Statistics
        {
            /// <summary>
            /// Gets or sets the count.
            /// </summary>
            public ulong Count { get; set; } = 0;

            /// <summary>
            /// Gets or sets the weight.
            /// </summary>
            public ulong Weight { get; set; } = 0;
        }
    }
}