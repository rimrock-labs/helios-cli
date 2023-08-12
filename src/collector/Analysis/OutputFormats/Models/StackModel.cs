namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Stack model class.
    /// </summary>
    public sealed class StackModel : IDataModel, ICsvModel
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
        public IEnumerable<KeyValuePair<StackData, Statistics>> GetData() => this.data;

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

        /// <inheritdoc />
        public IReadOnlyList<string> GetColumnNames() => new[] { "Stack", "Tags", "Count", "Weight" };

        /// <inheritdoc />
        public IEnumerable<IReadOnlyList<string>> GetDataRows()
        {
            List<string> row = new(4);
            foreach (KeyValuePair<StackData, Statistics> data in this.data)
            {
                row.Clear();
                StringBuilder builder = new();
                Frame? frame = data.Key.CallStack;
                while (frame != null)
                {
                    builder.Append(frame.ModuleName);
                    if (!string.IsNullOrEmpty(frame.MethodName))
                    {
                        builder.Append('!').Append(frame.MethodName);
                    }

                    builder.Append(';');
                    frame = frame.Caller;
                }

                row.Add(builder.ToString());
                builder.Clear();
                foreach (string tag in data.Key.Tags)
                {
                    builder.Append(tag).Append(';');
                }

                row.Add(builder.ToString());
                builder.Clear();

                row.Add(data.Value.Count.ToString());
                row.Add(data.Value.Weight.ToString());

                yield return row;
            }
        }

        private void AddData(StackData data)
        {
            if (!this.data.TryGetValue(data, out Statistics? statistics))
            {
                this.data[data] = statistics = new Statistics();
            }

            statistics.Count++;
            statistics.Weight += data.Weight;
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
                return (x == null && y == null) ||
                    (x != null && y != null &&
                    x.Tags.SetEquals(y.Tags) &&
                    object.Equals(x.CallStack, y.CallStack));
            }

            /// <inheritdoc/>
            public override int GetHashCode(StackData obj)
            {
                return HashCode.Combine(
                    obj.CallStack.GetHashCode(),
                    obj.Tags.Select(_ => StringComparer.OrdinalIgnoreCase.GetHashCode(_)).Aggregate((hash, _) => HashCode.Combine(hash, _)));
            }
        }
    }
}