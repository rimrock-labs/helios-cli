namespace Rimrock.Helios.Analysis.OutputFormats
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security;
    using Rimrock.Helios.Analysis.Analyzers;
    using Rimrock.Helios.Common;
    using Rimrock.Helios.Common.Graph;

    [OutputFormat(Name = "PerfView")]
    internal sealed class PerfViewOutputFormat : IOutputFormat
    {
        private readonly FileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerfViewOutputFormat"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public PerfViewOutputFormat(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Type ModelType => typeof(GraphModel);

        public void Save(AnalyzerContext analyzer, AnalysisContext context, IDataModel model) =>
            this.Save(analyzer, context, (GraphModel)model);

        private void Save(AnalyzerContext analyzer, AnalysisContext context, GraphModel model)
        {
            string path = Path.Combine(context.WorkingDirectory, analyzer.AnalyzerName) + ".perfview.xml";
            using Stream fileStream = this.fileSystem.FileOpen(path, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new(fileStream);

            Dictionary<Frame, int> frameLookup = new(Frame.NameEqualityComparer.Instance);
            Dictionary<Frame, int> stackLookup = new();

            List<(int StackId, int CallerId, int FrameId)> stacks = new();
            List<(int SampleId, int StackId, ulong Count, ulong Weight)> samples = new();

            int sampleId = 0;
            foreach (Frame frame in model.GraphRoot?.EnumerateDepthFirst() ?? Enumerable.Empty<Frame>())
            {
                int stackId = stackLookup.Count;
                stackLookup.Add(frame, stackId);

                if (!frameLookup.TryGetValue(frame, out int frameId))
                {
                    frameLookup[frame] = frameId = frameLookup.Count;
                }

                int callerId = -1;
                if (frame.Parent != null)
                {
                    if (!stackLookup.TryGetValue(frame.Parent, out callerId))
                    {
                        callerId = -1;
                    }
                }

                stacks.Add((stackId, callerId, frameId));

                Debug.Assert(frame.Metrics != null, "Frame has unset metrics.");
                if (frame.Metrics != null && frame.Child == null)
                {
                    // only collect leaf node metrics
                    ulong metric1 = frame.Metrics[0].Exclusive;
                    ulong metric2 = frame.Metrics[1].Exclusive;
                    samples.Add((sampleId++, stackId, metric1, metric2));
                }
            }

            writer.WriteLine("""<?xml version="1.0" encoding="utf-8"?>""");
            writer.WriteLine("<StackWindow>");
            writer.WriteLine("<StackSource>");

            writer.WriteLine($"""<Frames Count="{frameLookup.Count}">""");
            foreach (var frame in frameLookup)
            {
                writer.WriteLine($"""<Frame ID="{frame.Value}">{SecurityElement.Escape(frame.Key.ModuleName)}!{SecurityElement.Escape(frame.Key.MethodName)}</Frame>""");
            }

            writer.WriteLine("""</Frames>""");

            writer.WriteLine($"""<Stacks Count="{stacks.Count}">""");
            foreach (var stack in stacks)
            {
                writer.WriteLine($"""<Stack ID="{stack.StackId}" CallerID="{stack.CallerId}" FrameID="{stack.FrameId}" />""");
            }

            writer.WriteLine("""</Stacks>""");

            writer.WriteLine($"""<Samples Count="{samples.Count}">""");

            foreach (var sample in samples)
            {
                writer.WriteLine($"""<Sample ID="{sample.SampleId}" Count="{sample.Count}" StackID="{sample.StackId}" Metric="{sample.Weight}" />""");
            }

            writer.WriteLine("</Samples>");
            writer.WriteLine("</StackSource>");
            writer.WriteLine("</StackWindow>");
        }
    }
}