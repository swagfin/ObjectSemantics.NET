using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Generic;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineTemplateParser
    {
        public static EngineRunnerTemplate Parse(string templateContent)
        {
            string source = templateContent ?? string.Empty;
            List<TemplateNode> roots = new List<TemplateNode>();
            List<TemplateNode> active = roots;
            List<TemplateDiagnostic> diagnostics = new List<TemplateDiagnostic>();
            Stack<BlockFrame> blocks = new Stack<BlockFrame>();
            int offset = 0;
            int maximumDepth = 0;
            List<int> lineStarts = null;
            while (offset < source.Length)
            {
                int start = source.IndexOf("{{", offset, StringComparison.Ordinal);
                if (start < 0)
                {
                    active.Add(new TemplateNode { Kind = TemplateNodeKind.Literal, Text = source.Substring(offset), Position = offset });
                    break;
                }
                if (start > offset)
                    active.Add(new TemplateNode { Kind = TemplateNodeKind.Literal, Text = source.Substring(offset, start - offset), Position = offset });
                int end = source.IndexOf("}}", start + 2, StringComparison.Ordinal);
                if (end < 0)
                {
                    AddDiagnostic(diagnostics, source, ref lineStarts, start, "Unclosed template expression.");
                    active.Add(new TemplateNode { Kind = TemplateNodeKind.Literal, Text = source.Substring(start), Position = start });
                    break;
                }
                string command = source.Substring(start + 2, end - start - 2).Trim();
                offset = end + 2;
                string directive = command.StartsWith("#", StringComparison.Ordinal) ? command.Substring(1).Trim() : null;
                if (directive != null && (directive.Equals("else", StringComparison.OrdinalIgnoreCase) || directive.Equals("endif", StringComparison.OrdinalIgnoreCase) || directive.Equals("endforeach", StringComparison.OrdinalIgnoreCase)))
                {
                    bool isElse = directive.Equals("else", StringComparison.OrdinalIgnoreCase);
                    TemplateNodeKind expected = directive.Equals("endforeach", StringComparison.OrdinalIgnoreCase) ? TemplateNodeKind.Loop : TemplateNodeKind.Condition;
                    if (blocks.Count == 0 || blocks.Peek().Node.Kind != expected || (isElse && blocks.Peek().InAlternative))
                    {
                        AddDiagnostic(diagnostics, source, ref lineStarts, start, "Unexpected block directive: " + command);
                        active.Add(new TemplateNode { Kind = TemplateNodeKind.Literal, Text = source.Substring(start, offset - start), Position = start });
                        continue;
                    }
                    BlockFrame frame = blocks.Peek();
                    if (isElse)
                    {
                        frame.Node.Children = active.ToArray();
                        frame.InAlternative = true;
                        active = new List<TemplateNode>();
                    }
                    else
                    {
                        if (frame.InAlternative) frame.Node.Alternative = active.ToArray();
                        else frame.Node.Children = active.ToArray();
                        blocks.Pop();
                        active = frame.Parent;
                    }
                    continue;
                }
                TemplateNode node = new TemplateNode { Kind = TemplateNodeKind.Value, Text = command, Position = start };
                int open = directive == null ? -1 : directive.IndexOf('(');
                if (open >= 0 && directive.EndsWith(")", StringComparison.Ordinal))
                {
                    string keyword = directive.Substring(0, open).Trim();
                    string argument = directive.Substring(open + 1, directive.Length - open - 2).Trim();
                    if (keyword.Equals("foreach", StringComparison.OrdinalIgnoreCase) && IsPath(argument))
                    {
                        node.Kind = TemplateNodeKind.Loop;
                        node.Path = new PropertyPath(argument);
                    }
                    else if (keyword.Equals("if", StringComparison.OrdinalIgnoreCase))
                    {
                        int comparison = argument.IndexOfAny(new[] { '=', '!', '>', '<' });
                        if (comparison > 0)
                        {
                            string path = argument.Substring(0, comparison).Trim();
                            int length = comparison + 1 < argument.Length && argument[comparison + 1] == '=' ? 2 : 1;
                            string operation = argument.Substring(comparison, length);
                            string value = argument.Substring(comparison + length).Trim();
                            if (IsPath(path) && value.Length > 0 && (operation == "==" || operation == "!=" || operation == ">" || operation == "<" || operation == ">=" || operation == "<="))
                            {
                                node.Kind = TemplateNodeKind.Condition;
                                node.Path = new PropertyPath(path);
                                node.Operator = operation;
                                node.Comparison = value;
                            }
                        }
                    }
                }
                active.Add(node);
                if (node.Kind == TemplateNodeKind.Loop || node.Kind == TemplateNodeKind.Condition)
                {
                    blocks.Push(new BlockFrame { Node = node, Parent = active });
                    maximumDepth = Math.Max(maximumDepth, blocks.Count);
                    active = new List<TemplateNode>();
                }
                else
                {
                    if (directive != null)
                        AddDiagnostic(diagnostics, source, ref lineStarts, start, "Invalid block directive: " + command);
                    int colon = command.IndexOf(':');
                    string target = colon > 0 ? command.Substring(0, colon).Trim() : command;
                    node.Format = colon > 0 ? command.Substring(colon + 1).Trim() : string.Empty;
                    node.Path = new PropertyPath(target);
                    node.Expression = EngineExpressionEvaluator.Prepare(target);
                    if (node.Expression != null && node.Expression.Function == "calc" && node.Expression.Instructions == null)
                        AddDiagnostic(diagnostics, source, ref lineStarts, start, "Invalid arithmetic expression.");
                }
            }
            while (blocks.Count > 0)
            {
                BlockFrame frame = blocks.Pop();
                AddDiagnostic(diagnostics, source, ref lineStarts, frame.Node.Position, "Unclosed block: " + frame.Node.Text);
                if (frame.InAlternative) frame.Node.Alternative = active.ToArray();
                else frame.Node.Children = active.ToArray();
                active = frame.Parent;
            }
            return new EngineRunnerTemplate { Template = source, MaximumDepth = maximumDepth, Nodes = roots.ToArray(), Diagnostics = diagnostics.ToArray() };
        }

        private static bool IsPath(string path)
        {
            if (path.Length == 0) return false;
            for (int i = 0; i < path.Length; i++)
            {
                char character = path[i];
                if (!char.IsLetterOrDigit(character) && character != '_' && character != '.') return false;
            }
            return true;
        }

        private static void AddDiagnostic(List<TemplateDiagnostic> diagnostics, string source, ref List<int> lineStarts, int position, string message)
        {
            if (lineStarts == null)
            {
                lineStarts = new List<int> { 0 };
                for (int i = 0; i < source.Length; i++)
                    if (source[i] == '\n') lineStarts.Add(i + 1);
            }
            int line = lineStarts.BinarySearch(position);
            if (line < 0) line = ~line - 1;
            diagnostics.Add(new TemplateDiagnostic { Message = message, Position = position, Line = line + 1, Column = position - lineStarts[line] + 1 });
        }

        private class BlockFrame
        {
            public TemplateNode Node { get; set; }
            public List<TemplateNode> Parent { get; set; }
            public bool InAlternative { get; set; }
        }
    }
}
