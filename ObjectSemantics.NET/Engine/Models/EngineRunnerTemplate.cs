using System;

namespace ObjectSemantics.NET.Engine.Models
{
    internal class EngineRunnerTemplate
    {
        public string Template { get; set; }
        public int MaximumDepth { get; set; }
        public TemplateNode[] Nodes { get; set; } = Array.Empty<TemplateNode>();
        public TemplateDiagnostic[] Diagnostics { get; set; } = Array.Empty<TemplateDiagnostic>();
    }

    internal enum TemplateNodeKind
    {
        Literal,
        Value,
        Condition,
        Loop
    }

    internal class TemplateNode
    {
        public TemplateNodeKind Kind { get; set; }
        public string Text { get; set; }
        public string Format { get; set; }
        public string Operator { get; set; }
        public string Comparison { get; set; }
        public int Position { get; set; }
        public PropertyPath Path { get; set; }
        public EngineExpressionEvaluator.ExpressionPlan Expression { get; set; }
        public TemplateNode[] Children { get; set; } = Array.Empty<TemplateNode>();
        public TemplateNode[] Alternative { get; set; } = Array.Empty<TemplateNode>();
    }

    internal class PropertyPath
    {
        public PropertyPath(string text)
        {
            Text = text.Trim();
            int dot = Text.IndexOf('.');
            Root = dot < 0 ? Text : Text.Substring(0, dot).Trim();
            string tail = dot < 0 ? string.Empty : Text.Substring(dot + 1);
            string[] segments = tail.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i].Trim();
                if (segment.Length > 0) segments[count++] = segment;
            }
            if (count != segments.Length) Array.Resize(ref segments, count);
            Segments = segments;
        }

        public string Text { get; set; }
        public string Root { get; set; }
        public string[] Segments { get; set; }
    }
}
