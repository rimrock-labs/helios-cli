namespace Rimrock.Helios.Analysis.Analyzers
{
    using System;
    using System.Collections.Generic;
    using Rimrock.Helios.Analysis.Views;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Base data analyzer class.
    /// </summary>
    public abstract class BaseDataAnalyzer : IDataAnalyzer
    {
        private readonly Dictionary<Type, IModel> models;
        private readonly List<IView> views;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataAnalyzer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public BaseDataAnalyzer(ILogger logger)
        {
            this.Logger = logger;

            this.models = new Dictionary<Type, IModel>();
            this.views = new List<IView>();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc />
        public virtual void OnStart(AnalysisContext context)
        {
            this.InitializeViews(context.Views);
        }

        /// <inheritdoc />
        public abstract void OnData(AnalysisContext context, TraceEvent data);

        /// <inheritdoc />
        public virtual void OnEnd(AnalysisContext context)
        {
            foreach (IView view in this.views)
            {
                if (this.models.TryGetValue(view.ModelType, out IModel? model))
                {
                    view.Save(context, model);
                }
            }
        }

        /// <summary>
        /// Adds data to the underlying view-models.
        /// </summary>
        /// <param name="data">The data.</param>
        protected void AddData(IData data)
        {
            foreach (IModel model in this.models.Values)
            {
                model.AddData(data);
            }
        }

        private void InitializeViews(IReadOnlySet<Type> views)
        {
            HashSet<Type> modelTypes = new();
            foreach (Type viewType in views)
            {
                IView view = (IView)Activator.CreateInstance(viewType)!;
                Type modelType = view.ModelType;
                modelTypes.Add(modelType);

                this.views.Add(view);
            }

            foreach (Type modelType in modelTypes)
            {
                IModel model = (IModel)Activator.CreateInstance(modelType)!;
                this.models[modelType] = model;
            }
        }
    }
}