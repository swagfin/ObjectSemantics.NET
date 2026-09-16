using ObjectSemantics.NET.Engine.Extensions;
using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineTemplateRenderer
    {
        public static string Render<T>(T record, EngineRunnerTemplate template, Dictionary<string, object> parameters = null, TemplateMapperOptions options = null) where T : class
        {
            StringBuilder output = new StringBuilder(Math.Min(template.Template.Length, 4096));
            using (StringWriter writer = new StringWriter(output, CultureInfo.InvariantCulture))
            {
                RenderTo(writer, record, typeof(T), template, parameters, options);
                return output.ToString();
            }
        }

        public static void RenderTo(
            TextWriter writer,
            object record,
            Type type,
            EngineRunnerTemplate template,
            Dictionary<string, object> parameters,
            TemplateMapperOptions options)
        {
            TemplateMapperOptions activeOptions = (options ?? new TemplateMapperOptions()).Snapshot();
            CheckSource(template.Template, activeOptions);
            CheckTemplate(template, activeOptions);
            EngineRenderContext context = new EngineRenderContext(writer, activeOptions);
            RenderNodes(template.Nodes, new RenderScope(record, type, parameters, false, activeOptions) { Context = context }, context, 0);
        }

        internal static void CheckSource(string source, TemplateMapperOptions options)
        {
            options.CancellationToken.ThrowIfCancellationRequested();
            if (options.MaximumTemplateCharacters > 0 && source.Length > options.MaximumTemplateCharacters)
                throw new TemplateLimitExceededException("Template source length limit exceeded.");
        }

        internal static void CheckTemplate(EngineRunnerTemplate template, TemplateMapperOptions options)
        {
            options.CancellationToken.ThrowIfCancellationRequested();
            if (options.MaximumNestingDepth > 0 && template.MaximumDepth > options.MaximumNestingDepth)
                throw new TemplateLimitExceededException("Template nesting limit exceeded.");
            if (options.StrictMode && template.Diagnostics.Length > 0)
            {
                TemplateDiagnostic diagnostic = template.Diagnostics[0];
                throw new FormatException(diagnostic.Message + " Line " + diagnostic.Line + ", column " + diagnostic.Column + ".");
            }
        }

        private static void RenderNodes(TemplateNode[] nodes, RenderScope scope, EngineRenderContext context, int depth)
        {
            context.Check(depth);
            for (int i = 0; i < nodes.Length; i++)
            {
                context.Check(depth);
                TemplateNode node = nodes[i];
                if (node.Kind == TemplateNodeKind.Literal)
                {
                    context.Write(node.Text);
                    continue;
                }
                if (node.Kind == TemplateNodeKind.Value && scope.IsRow && node.Path.Text == ".")
                {
                    ExtractedObjProperty current = new ExtractedObjProperty { Type = scope.Type, OriginalValue = scope.Model };
                    context.Write(current.GetPropertyDisplayString(node.Format, context.Options));
                    continue;
                }
                bool found = EnginePropertyResolver.TryResolveProperty(scope, node.Path, out ExtractedObjProperty property);
                if (node.Kind == TemplateNodeKind.Condition)
                {
                    if (!found)
                    {
                        if (context.Options.StrictMode) throw new FormatException("Unknown condition property: " + node.Path.Text);
                        context.Write("[IF-CONDITION EXCEPTION]: unrecognized property: [" + node.Path.Text + "]");
                        continue;
                    }
                    bool passed = property.IsPropertyValueConditionPassed(node.Comparison, node.Operator, context);
                    // Preserve eager getter evaluation on branch entry for existing callers.
                    if (!passed && node.Alternative.Length == 0) continue;
                    RenderScope branch = context.Options.LazyPropertyAccess ? scope : new RenderScope(scope.Model, scope.Type, scope.Parameters, scope.IsRow, context.Options) { Context = context };
                    RenderNodes(passed ? node.Children : node.Alternative, branch, context, depth + 1);
                }
                else if (node.Kind == TemplateNodeKind.Loop)
                {
                    if (found && property.OriginalValue is IEnumerable rows)
                    {
                        foreach (object row in rows)
                        {
                            context.CountIteration();
                            RenderScope child = new RenderScope(row, row == null ? typeof(object) : row.GetType(), null, true, context.Options) { Context = context };
                            RenderNodes(node.Children, child, context, depth + 1);
                        }
                    }
                    else if (!found && context.Options.StrictMode)
                        throw new FormatException("Unknown collection: " + node.Path.Text);
                }
                else if (found)
                    context.Write(property.GetPropertyDisplayString(node.Format, context.Options));
                else if (EngineExpressionEvaluator.TryEvaluate(node.Expression, scope, out ExtractedObjProperty value, out bool emptyOnFailure, out bool isExpression))
                    context.Write(value.GetPropertyDisplayString(node.Format, context.Options));
                else
                {
                    if (context.Options.StrictMode) throw new FormatException("Unable to evaluate: " + node.Path.Text);
                    if (!(isExpression && emptyOnFailure))
                        context.Write(scope.IsRow ? node.Text : "{{ " + node.Text + " }}");
                }
            }
        }
    }
}
