using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class TreeNode
{
    public string Row = string.Empty;
    public List<TreeNode> Children = [];
}

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: <outfile> <infile1> <infile2> ...");
            return 1;
        }

        var outfile = args[0];
        var infiles = args[1..];

        var result = MergeFiles(outfile, infiles);

        return result ? 0 : 1;
    }

    static bool MergeFiles(string outfile, string[] infiles)
    {
        var result = true;
        foreach (var infile in infiles)
        {
            if (!File.Exists(infile))
            {
                Console.WriteLine($"File not found: '{infile}'");
                result = false;
            }
        }
        if (!result)
        {
            return false;
        }

        List<string[]> inrows = [];

        foreach (var infile in infiles)
        {
            var rows = File.ReadAllLines(infile);
            inrows.Add(rows);
        }

        var outrows = MergeRows(inrows);

        File.WriteAllLines(outfile, outrows);

        return true;
    }

    static string[] MergeRows(List<string[]> inrows)
    {
        List<TreeNode> trees = [];

        foreach (var rows in inrows)
        {
            var inroot = new TreeNode();
            ParseRows(inroot, rows);
            trees.Add(inroot);
        }

        var outroot = new TreeNode();

        foreach (var tree in trees)
        {
            MergeTreeNodes(outroot, tree);
        }

        List<string> outrows = [];
        FlattenTree(outroot, outrows);

        return [.. outrows.Skip(1)];
    }

    static void ParseRows(TreeNode root, string[] rows)
    {
        List<TreeNode> parsedNodes = [];

        for (var i = 0; i < rows.Length; i++)
        {
            var indent = LeadingSpaces(rows[i]);
            TreeNode node = new() { Row = rows[i] };

            TreeNode? parent = null;
            for (var j = i - 1; j >= 0; j--)
            {
                var prevIndent = LeadingSpaces(rows[j]);
                if (prevIndent < indent ||
                    (prevIndent == indent && rows[i].TrimStart().StartsWith('-') && !rows[j].TrimStart().StartsWith('-')))
                {
                    parent = parsedNodes[j];
                    //Console.WriteLine($"{i} {j}: '{rows[i]}' '{rows[j]}'");
                    break;
                }
            }

            parent?.Children.Add(node);

            if (parent == null)
            {
                root.Children.Add(node);
            }

            parsedNodes.Add(node);
        }
    }

    static int LeadingSpaces(string row)
    {
        var count = 0;
        foreach (var c in row)
        {
            if (char.IsWhiteSpace(c))
            {
                count++;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    static void MergeTreeNodes(TreeNode target, TreeNode source)
    {
        foreach (var sourceChild in source.Children)
        {
            var targetChild = target.Children.Find(n => n.Row == sourceChild.Row);
            if (targetChild == null)
            {
                targetChild = new TreeNode
                {
                    Row = sourceChild.Row
                };
                target.Children.Add(targetChild);
            }
            else if (sourceChild.Row.TrimStart().StartsWith('-'))
            {
                target.Children.Add(sourceChild);
                continue;
            }
            MergeTreeNodes(targetChild, sourceChild);
        }
    }

    static void FlattenTree(TreeNode node, List<string> rows)
    {
        rows.Add(node.Row);

        foreach (var child in node.Children)
        {
            FlattenTree(child, rows);
        }
    }
}
