namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Common.Graph;

    /// <summary>
    /// Graph model class.
    /// </summary>
    public class GraphModel : IDataModel
    {
        private readonly ILogger<GraphModel> logger;
        private readonly Merger merger;
        private Frame? graph;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public GraphModel(ILogger<GraphModel> logger)
        {
            this.logger = logger;
            this.merger = new Merger();
        }

        /// <summary>
        /// Gets the graph root.
        /// </summary>
        public Frame? GraphRoot => this.graph;

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

        private static Func<Frame, int, Frame> SetMetrics(StackData data)
        {
            Frame Function(Frame frame, int index)
            {
                bool exclusive = frame.Child == null;
                frame.Metrics = new Frame.Metric[2]
                {
                    new() { Inclusive = data.Count, Exclusive = exclusive ? data.Count : 0 },
                    new() { Inclusive = data.Weight, Exclusive = exclusive ? data.Weight : 0 },
                };

                return frame;
            }

            return Function;
        }

        private void AddData(StackData data)
        {
            if (this.graph == null)
            {
                this.graph = data.StackRoot.CloneStackFromRoot();
                this.graph.EnumerateChildStack().Select(SetMetrics(data)).Iterate();
            }
            else
            {
                this.merger.MergeGraph(data.StackRoot, this.graph, context: data);
            }
        }

        private class Merger : GraphMerger<Frame, StackData>
        {
            protected override Frame OnInsert(Frame node, StackData? context = null)
            {
                Frame clone = node.CloneStackFromRoot();
                if (context != null)
                {
                    clone.EnumerateChildStack().Select(SetMetrics(context)).Iterate();
                }

                return clone;
            }

            protected override void MergeNode(Frame source, Frame target, StackData? context = null)
            {
                int metricCount = target.Metrics?.Length ?? 0;
                if (metricCount > 0)
                {
                    for (int m = 0; m < metricCount; m++)
                    {
                        target.Metrics![m] = new Frame.Metric()
                        {
                            Inclusive = source.Metrics?[m].Inclusive ?? 0,
                            Exclusive = source.Metrics?[m].Exclusive ?? 0,
                        };
                    }
                }
            }
        }
    }
}