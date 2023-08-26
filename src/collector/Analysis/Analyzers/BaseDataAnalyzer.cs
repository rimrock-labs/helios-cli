namespace Rimrock.Helios.Analysis.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rimrock.Helios.Analysis.OutputFormats;
    using Rimrock.Helios.Common.Graph;

    /// <summary>
    /// Base data analyzer class.
    /// </summary>
    public abstract class BaseDataAnalyzer : IDataAnalyzer
    {
        private readonly Dictionary<Type, IDataModel> models;
        private readonly List<IOutputFormat> views;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataAnalyzer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="services">The services.</param>
        public BaseDataAnalyzer(ILogger logger, IServiceProvider services)
        {
            this.Logger = logger;
            this.Services = services;

            this.models = new Dictionary<Type, IDataModel>();
            this.views = new List<IOutputFormat>();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the services.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <inheritdoc />
        public virtual void OnStart(AnalysisContext context)
        {
            this.InitializeViews(context.OutputFormats);
        }

        /// <inheritdoc />
        public abstract bool OnData(AnalysisContext context, Process process, TraceEvent data);

        /// <inheritdoc />
        public virtual void OnEnd(AnalysisContext context)
        {
            AnalyzerContext analyzerContext = new()
            {
                AnalyzerName = this.GetAnalyzerName(),
            };

            foreach (IOutputFormat view in this.views)
            {
                if (this.models.TryGetValue(view.ModelType, out IDataModel? model))
                {
                    view.Save(analyzerContext, context, model);
                }
            }
        }

        /// <summary>
        /// Adds the tags as frames.
        /// </summary>
        /// <param name="stackRoot">The stack root.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The new stack root.</returns>
        protected static Frame AddTagsAsFrames(Frame stackRoot, IReadOnlySet<string> tags)
        {
            Frame previous = stackRoot;
            foreach (string tag in tags)
            {
                Frame frame = new(tag);
                frame.AddChild(previous);
                previous = frame;
            }

            return previous;
        }

        /// <summary>
        /// Applies inclusive metrics to stack.
        /// </summary>
        /// <param name="leaf">The stack leaf.</param>
        /// <param name="count">The count.</param>
        /// <param name="weight">The weight.</param>
        protected static void ApplyInclusiveMetrics(Frame leaf, ulong count = 1, ulong weight = 1)
        {
            foreach (Frame frame in leaf.EnumerateParentStack())
            {
                frame.InclusiveCount += count;
                frame.InclusiveWeight += weight;
            }
        }

        /// <summary>
        /// Adds data to the underlying view-models.
        /// </summary>
        /// <param name="data">The data.</param>
        protected void AddData(IData data)
        {
            foreach (IDataModel model in this.models.Values)
            {
                model.AddData(data);
            }
        }

        private void InitializeViews(IReadOnlySet<Type> views)
        {
            HashSet<Type> modelTypes = new();
            foreach (Type viewType in views)
            {
                IOutputFormat view = (IOutputFormat)ActivatorUtilities.CreateInstance(this.Services, viewType)!;
                Type modelType = view.ModelType;
                modelTypes.Add(modelType);

                this.views.Add(view);
            }

            foreach (Type modelType in modelTypes)
            {
                IDataModel model = (IDataModel)ActivatorUtilities.CreateInstance(this.Services, modelType)!;
                this.models[modelType] = model;
            }
        }

        private string GetAnalyzerName()
        {
            CustomAttributeData attribute = this.GetType().
                GetCustomAttributesData().
                Where(_ => _.AttributeType == typeof(DataAnalyzerAttribute)).
                FirstOrDefault() ??
                    throw new InvalidOperationException("Analyzer is missing [DataAnalyzer] attribute.");
            return ((string?)attribute.NamedArguments[0].TypedValue.Value) ?? "Unknown";
        }
    }
}