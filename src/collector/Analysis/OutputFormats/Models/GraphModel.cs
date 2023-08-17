namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Common.Graph;

    /// <summary>
    /// Graph model class.
    /// </summary>
    public class GraphModel : IDataModel
    {
        private readonly ILogger<GraphModel> logger;
        private Node? root;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public GraphModel(ILogger<GraphModel> logger)
        {
            this.logger = logger;
        }

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
            this.root ??= new Node();
            foreach (Frame frame in data.CallStack.EnumerateDownStack().Last().EnumerateUpStack())
            {
                // merge starting at root
            }
        }

        private class Node : INode
        {
            public INode? Parent { get; set; }

            public INode? Child { get; set; }

            public INode? Sibling { get; set; }

            public ulong InclusiveCount { get; set; }

            public ulong InclusiveWeight { get; set; }

            public ulong ExclusiveCount { get; set; }

            public ulong ExclusiveWeight { get; set; }
        }
    }
}