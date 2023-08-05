namespace Rimrock.Helios.Analysis.Views
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Graph model class.
    /// </summary>
    public sealed class GraphModel : IModel
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public GraphModel(ILogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public void AddData(IData data)
        {
            if (data is WeightedGraphData weightedData)
            {
                this.AddData(weightedData);
            }
            else
            {
                this.logger.LogWarning("Received data of '{type}' type but not could not be processed.", data.GetType().FullName);
            }
        }

        public void AddData(WeightedGraphData data)
        {

        }
    }
}