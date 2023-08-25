namespace Rimrock.Helios.Analysis.OutputFormats
{
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

        private void AddData(StackData data)
        {
            if (this.graph == null)
            {
                this.graph = data.StackRoot.CloneStackFromRoot();
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
                return clone;
            }

            protected override void MergeNode(Frame source, Frame target, StackData? context = null)
            {
                target.InclusiveCount += source.InclusiveCount;
                target.InclusiveWeight += source.InclusiveWeight;
                target.ExclusiveCount += source.ExclusiveCount;
                target.ExclusiveWeight += source.ExclusiveWeight;
            }
        }
    }
}