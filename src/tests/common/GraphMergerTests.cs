using System.Diagnostics.CodeAnalysis;
using System.Text;
using Rimrock.Helios.Common.Graph;

namespace Rimrock.Helios.Tests.Common;

[TestClass]
public class GraphMergerTests
{
    [TestMethod]
    public void MergeTest()
    {
        var graph1 = GetGraph("A>B>C");
        var graph2 = GetGraph("A>B>D");
        var graph3 = GetGraph("A>B>E");

        Comparer comparer = new();
        Merger merger = new();
        merger.MergeGraph(graph2, graph1, comparer);
        merger.MergeGraph(graph3, graph1, comparer);

        Assert.AreEqual("ABC", Navigate(">>>", graph1));
        Assert.AreEqual(1, GetNodeValue(">>>", graph1));
        Assert.AreEqual("ABC", Navigate(">>>>", graph1));
        Assert.AreEqual("ABCD", Navigate(">>^", graph1));
        Assert.AreEqual(1, GetNodeValue(">>^", graph1));
        Assert.AreEqual("ABCDE", Navigate(">>^^", graph1));
        Assert.AreEqual(1, GetNodeValue(">>^^", graph1));
        Assert.AreEqual(3, GetNodeValue("", graph1));
        Assert.AreEqual(3, GetNodeValue(">", graph1));

        graph2 = GetGraph("A>B>E>F");
        merger.MergeGraph(graph2, graph1, comparer);
        Assert.AreEqual("ABCDEF", Navigate(">>^^>", graph1));

        graph2 = GetGraph("X>Y>Z");
        merger.MergeGraph(graph2, graph1, comparer);
        Assert.AreEqual("AXYZ", Navigate("^>>", graph1));

        graph3 = GetGraph("X>Y>0");
        merger.MergeGraph(graph3, graph2, comparer);
        merger.MergeGraph(graph2, graph1, comparer);
        Assert.AreEqual("AXYZ", Navigate("^>>", graph1));
        Assert.AreEqual("AXYZ0", Navigate("^>>^", graph1));
        Assert.AreEqual(4, GetNodeValue("", graph1));
    }

    private static string Navigate(string instruction, Node graph)
    {
        StringBuilder builder = new();

        Node? node = graph;
        using StringReader reader = new(instruction);
        while (node != null)
        {
            builder.Append(node.Name);
            node = reader.Read() switch
            {
                '>' => node.Child,
                '^' => node.Sibling,
                '<' => node.Parent,
                _ => null,
            };
        }

        return builder.ToString();
    }

    private static int GetNodeValue(string instruction, Node graph)
    {
        int value = 0;
        Node? node = graph;
        using StringReader reader = new(instruction);
        while (node != null)
        {
            value = node.Value;
            node = reader.Read() switch
            {
                '>' => node.Child,
                '^' => node.Sibling,
                '<' => node.Parent,
                _ => null,
            };
        }

        return value;
    }

    private static Node GetGraph(string value, int metric = 1)
    {
        string[] parts = value.Split('>');
        Node? previous = null;
        foreach (string part in parts)
        {
            Node node = new()
            {
                Name = part,
                Value = metric,
            };

            previous?.AddChild(node);
            previous = node;
        }

        Node? root = null;
        while (previous != null)
        {
            root = previous;
            previous = previous.Parent;
        }

        return root!;
    }

    private class Comparer : IEqualityComparer<Node>
    {
        public bool Equals(Node? x, Node? y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x?.Name, y?.Name);

        public int GetHashCode([DisallowNull] Node obj) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
    }

    private class Merger : GraphMerger<Node, object>
    {
        protected override void MergeNode(Node source, Node target, object? context = null)
        {
            target.Value += source.Value;
        }
    }

    private class Node : INode<Node>
    {
        public Node? Parent { get; set; }
        public Node? Child { get; set; }
        public Node? Sibling { get; set; }
        public required string Name { get; init; }
        public int Value { get; set; }

        public Node Clone() =>
            new()
            {
                Name = this.Name,
                Value = this.Value,
            };
    }
}