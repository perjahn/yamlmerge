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
            var inroot = ParseRows(rows);
            trees.Add(inroot);
        }

        TreeNode outroot = new();

        foreach (var tree in trees)
        {
            MergeTreeNodes(outroot, tree);
        }

        List<string> outrows = [];
        FlattenTree(outroot, outrows);

        return [.. outrows.Skip(1)];
    }

    static TreeNode ParseRows(string[] rows)
    {
        TreeNode root = new();
        List<TreeNode> parsedNodes = [];

        foreach (var row in rows)
        {
            if (row.Trim().Length == 0)
            {
                continue;
            }

            var indent = LeadingSpaces(row);

            TreeNode? parent = null;
            for (var j = parsedNodes.Count - 1; j >= 0; j--)
            {
                var prevIndent = LeadingSpaces(parsedNodes[j].Row);
                if (prevIndent < indent || (prevIndent == indent && row.TrimStart().StartsWith('-') && !parsedNodes[j].Row.TrimStart().StartsWith('-')))
                {
                    parent = parsedNodes[j];
                    break;
                }
            }

            if (parent == null)
            {
                TreeNode node = new() { Row = row };
                root.Children.Add(node);
                parsedNodes.Add(node);
            }
            else
            {
                if (row.TrimStart().StartsWith('-'))
                {
                    TreeNode node = new() { Row = row[..(indent + 1)] };
                    parent.Children.Add(node);
                    parsedNodes.Add(node);

                    TreeNode n2 = new() { Row = $"{row[0..indent]} {row[(indent + 1)..]}" };
                    node.Children.Add(n2);
                    parsedNodes.Add(n2);
                }
                else
                {
                    TreeNode node = new() { Row = row };
                    parent.Children.Add(node);
                    parsedNodes.Add(node);
                }
            }
        }

        return root;
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
            var key = GetKey(sourceChild.Row);
            var targetChild = target.Children.Find(n => GetKey(n.Row) == key);
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

    static string GetKey(string row)
    {
        var index = row.IndexOf(':');
        return index < 0 ? row : row[..index].Trim();
    }

    static void FlattenTree(TreeNode node, List<string> rows)
    {
        if (node.Row.Trim() == "-" && node.Children.Count >= 1 && node.Children[0].Row.StartsWith("  "))
        {
            node.Children[0].Row = $"{node.Row} {node.Children[0].Row.TrimStart()}";
        }
        else
        {
            rows.Add(node.Row);
        }

        foreach (var child in node.Children)
        {
            FlattenTree(child, rows);
        }
    }
}
