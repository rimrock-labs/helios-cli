namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Rimrock.Helios.Analysis.Analyzers;
    using Rimrock.Helios.Common;
    using Rimrock.Helios.Common.Graph;

    [OutputFormat(Name = "speedscope")]
    internal sealed class SpeedScopeOutputFormat : IOutputFormat
    {
        private readonly FileSystem fileSystem;

        public SpeedScopeOutputFormat(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Type ModelType => typeof(GraphModel);

        public void Save(AnalyzerContext analyzer, AnalysisContext context, IDataModel model) =>
            this.Save(analyzer, context, (GraphModel)model);

        private void Save(AnalyzerContext analyzer, AnalysisContext context, GraphModel model)
        {
            string path = Path.Combine(context.WorkingDirectory, analyzer.AnalyzerName) + ".speedscope.json";
            using Stream fileStream = this.fileSystem.FileOpen(path, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new(fileStream);

            Frame? graphRoot = model.GraphRoot;
            Dictionary<Frame, int> frames = new(Frame.NameEqualityComparer.Instance);
            foreach (Frame frame in graphRoot?.EnumerateDepthFirst() ?? Enumerable.Empty<Frame>())
            {
                if (!frames.TryGetValue(frame, out _))
                {
                    frames[frame] = frames.Count;
                }
            }

            writer.WriteLine('{');
            writer.WriteLine("""  "version": "0.0.1",""");
            writer.WriteLine("""  "$schema": "https://www.speedscope.app/file-format-schema.json",""");
            writer.WriteLine("""  "shared": { "frames": [""");

            bool first1 = true;
            foreach (var framePair in frames.OrderBy(_ => _.Value))
            {
                if (!first1)
                {
                    writer.WriteLine(',');
                }

                writer.Write($$"""{"name": "{{framePair.Key.ModuleName}}!{{framePair.Key.MethodName}}"}""");
                first1 = false;
            }

            writer.WriteLine("]},");
            writer.WriteLine($$""" "profiles": [{ "type": "sampled", "name": "{{analyzer.AnalyzerName}}", "unit": "none", "startValue": 0, "endValue": 0, "samples": [""");

            List<ulong> weights = new();
            bool first2 = true;
            foreach (Frame frame in graphRoot?.EnumerateDepthFirst() ?? Enumerable.Empty<Frame>())
            {
                if (frame.Child == null)
                {
                    weights.Add(frame.ExclusiveCount);
                    if (!first2)
                    {
                        writer.WriteLine(',');
                    }

                    writer.Write('[');
                    first1 = true;
                    foreach (var f in frame.EnumerateParentStack())
                    {
                        if (frames.TryGetValue(f, out int frameIndex))
                        {
                            if (!first1)
                            {
                                writer.Write(',');
                            }

                            writer.Write(frameIndex);
                            first1 = false;
                        }
                    }

                    writer.WriteLine(']');
                    first2 = false;
                }
            }

            writer.WriteLine("]");
            writer.WriteLine(""", "weights": [""");

            first1 = true;
            foreach (ulong weight in weights)
            {
                if (!first1)
                {
                    writer.Write(',');
                }

                writer.Write(weight);
                first1 = false;
            }

            writer.WriteLine("]}]}");
        }
    }
}