using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineExpressionEvaluator
    {
        private static readonly Regex FunctionRegex = new Regex(@"^\s*_*(?<fn>sum|avg|count|min|max|calc)\s*\(\s*(?<arg>.*)\s*\)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        internal class ExpressionPlan
        {
            public string Command { get; set; }
            public string Function { get; set; }
            public PropertyPath Argument { get; set; }
            public ExpressionToken[] Instructions { get; set; }
        }

        public static ExpressionPlan Prepare(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return null;
            Match match = FunctionRegex.Match(command.Trim());
            if (!match.Success)
                return null;
            ExpressionPlan plan = new ExpressionPlan
            {
                Command = command,
                Function = match.Groups["fn"].Value.Trim().ToLowerInvariant(),
                Argument = new PropertyPath(match.Groups["arg"].Value.Trim())
            };
            if (plan.Function == "calc" && TryTokenize(plan.Argument.Text, out List<ExpressionToken> tokens) && TryToRpn(tokens, out List<ExpressionToken> instructions))
            {
                int stackDepth = 0;
                bool valid = true;
                for (int i = 0; i < instructions.Count; i++)
                {
                    ExpressionToken token = instructions[i];
                    if (token.Kind == ExpressionTokenKind.Number || token.Kind == ExpressionTokenKind.Identifier) stackDepth++;
                    else if (token.Kind == ExpressionTokenKind.UnaryMinus) valid &= stackDepth >= 1;
                    else { valid &= stackDepth >= 2; stackDepth--; }
                }
                if (valid && stackDepth == 1) plan.Instructions = instructions.ToArray();
            }
            return plan;
        }

        public static bool TryEvaluate(ExpressionPlan plan, RenderScope propMap, out ExtractedObjProperty evaluatedProperty, out bool renderEmptyOnFailure, out bool isExpressionCommand)
        {
            evaluatedProperty = null;
            renderEmptyOnFailure = false;
            isExpressionCommand = plan != null;
            if (plan == null)
                return false;
            string expressionCommand = plan.Command;
            string fn = plan.Function;
            PropertyPath arg = plan.Argument;

            try
            {
                switch (fn)
                {
                    case "sum":
                        if (!TryAggregateNumeric(arg, propMap, AggregateMode.Sum, out decimal sum))
                        {
                            renderEmptyOnFailure = true;
                            return false;
                        }
                        evaluatedProperty = CreateDecimalProperty(expressionCommand, sum);
                        return true;

                    case "avg":
                        if (!TryAggregateNumeric(arg, propMap, AggregateMode.Average, out decimal avg))
                        {
                            renderEmptyOnFailure = true;
                            return false;
                        }
                        evaluatedProperty = CreateDecimalProperty(expressionCommand, avg);
                        return true;

                    case "count":
                        if (!TryCount(arg, propMap, out int count))
                        {
                            renderEmptyOnFailure = true;
                            return false;
                        }
                        evaluatedProperty = new ExtractedObjProperty
                        {
                            Name = expressionCommand,
                            Type = typeof(int),
                            OriginalValue = count
                        };
                        return true;

                    case "min":
                        if (!TryAggregateNumeric(arg, propMap, AggregateMode.Min, out decimal min))
                        {
                            renderEmptyOnFailure = true;
                            return false;
                        }
                        evaluatedProperty = CreateDecimalProperty(expressionCommand, min);
                        return true;

                    case "max":
                        if (!TryAggregateNumeric(arg, propMap, AggregateMode.Max, out decimal max))
                        {
                            renderEmptyOnFailure = true;
                            return false;
                        }
                        evaluatedProperty = CreateDecimalProperty(expressionCommand, max);
                        return true;

                    case "calc":
                        if (!TryEvaluateArithmetic(plan.Instructions, propMap, out decimal calcResult))
                        {
                            renderEmptyOnFailure = true;
                            return false;
                        }
                        evaluatedProperty = CreateDecimalProperty(expressionCommand, calcResult);
                        return true;
                }

                return false;
            }
            catch (OverflowException)
            {
                renderEmptyOnFailure = true;
                return false;
            }
        }

        private static ExtractedObjProperty CreateDecimalProperty(string name, decimal value)
        {
            return new ExtractedObjProperty
            {
                Name = name,
                Type = typeof(decimal),
                OriginalValue = value
            };
        }

        private static bool TryCount(PropertyPath path, RenderScope propMap, out int count)
        {
            count = 0;
            int nonNullCount = 0;
            if (string.IsNullOrWhiteSpace(path.Text))
                return false;

            bool found = false;
            VisitPathValues(path, propMap, value =>
            {
                found = true;
                if (value != null) nonNullCount++;
            });
            count = nonNullCount;
            return found;
        }

        private static bool TryAggregateNumeric(PropertyPath path, RenderScope propMap, AggregateMode mode, out decimal result)
        {
            result = 0m;
            if (string.IsNullOrWhiteSpace(path.Text))
                return false;

            bool found = false;
            bool hasAny = false;
            bool hasInvalidNonNumeric = false;
            decimal running = 0m;
            int numericCount = 0;

            VisitPathValues(path, propMap, rawValue =>
            {
                found = true;
                if (rawValue == null)
                    return;

                if (!TryConvertToDecimal(rawValue, out decimal numeric))
                {
                    hasInvalidNonNumeric = true;
                    return;
                }

                if (!hasAny)
                {
                    running = numeric;
                    hasAny = true;
                }
                else
                {
                    switch (mode)
                    {
                        case AggregateMode.Sum:
                        case AggregateMode.Average:
                            running += numeric;
                            break;
                        case AggregateMode.Min:
                            if (numeric < running) running = numeric;
                            break;
                        case AggregateMode.Max:
                            if (numeric > running) running = numeric;
                            break;
                    }
                }

                numericCount++;
            });

            if (!found || hasInvalidNonNumeric)
                return false;

            if (!hasAny)
            {
                result = 0m;
                return true;
            }

            if (mode == AggregateMode.Average)
                result = numericCount == 0 ? 0m : running / numericCount;
            else
                result = running;

            return true;
        }

        private static void VisitPathValues(PropertyPath path, RenderScope scope, Action<object> visit)
        {
            if (scope.TryGetValue(path.Text, out ExtractedObjProperty direct)) { visit(direct.OriginalValue); return; }
            if (!scope.TryGetValue(path.Root, out ExtractedObjProperty root)) return;
            if (path.Segments.Length == 0) { visit(root.OriginalValue); return; }
            if (scope.Options.UseStreamingEvaluation)
            {
                WalkPath(root.OriginalValue, path.Segments, 0, scope.Context, visit, 0);
                return;
            }

            // Compatibility mode preserves breadth-first getter evaluation.
            List<object> current = new List<object> { root.OriginalValue };
            for (int i = 0; i < path.Segments.Length; i++)
            {
                List<object> next = new List<object>();
                for (int j = 0; j < current.Count; j++)
                    ExpandSegment(current[j], path.Segments[i], next, scope.Context, 0);
                current = next;
            }
            for (int i = 0; i < current.Count; i++) visit(current[i]);
        }

        private static void WalkPath(
            object value,
            string[] segments,
            int index,
            EngineRenderContext context,
            Action<object> visit,
            int depth)
        {
            context.Check(depth);
            if (index == segments.Length || value == null) { visit(value); return; }
            if (value is IEnumerable rows && !(value is string))
            {
                foreach (object row in rows)
                {
                    context.CountIteration();
                    WalkPath(row, segments, index, context, visit, depth + 1);
                }
            }
            else if (EngineTypeMetadataCache.TryGetPropertyAccessor(value.GetType(), segments[index], out PropertyAccessor accessor))
                WalkPath(accessor.Getter(value), segments, index + 1, context, visit, depth + 1);
        }

        private static void ExpandSegment(object value, string segment, List<object> next, EngineRenderContext context, int depth)
        {
            context.Check(depth);
            if (value == null) { next.Add(null); return; }
            if (value is IEnumerable rows && !(value is string))
            {
                foreach (object row in rows)
                {
                    context.CountIteration();
                    ExpandSegment(row, segment, next, context, depth + 1);
                }
            }
            else if (EngineTypeMetadataCache.TryGetPropertyAccessor(value.GetType(), segment, out PropertyAccessor accessor))
                next.Add(accessor.Getter(value));
        }

        private static bool TryConvertToDecimal(object value, out decimal number)
        {
            number = 0m;
            if (value == null)
                return false;

            switch (value)
            {
                case decimal d:
                    number = d;
                    return true;
                case int i:
                    number = i;
                    return true;
                case long l:
                    number = l;
                    return true;
                case short s:
                    number = s;
                    return true;
                case byte b:
                    number = b;
                    return true;
                case double db:
                    number = Convert.ToDecimal(db, CultureInfo.InvariantCulture);
                    return true;
                case float f:
                    number = Convert.ToDecimal(f, CultureInfo.InvariantCulture);
                    return true;
                case string str:
                    return decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out number);
            }

            if (value is IConvertible convertible)
            {
                try
                {
                    number = convertible.ToDecimal(CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private static bool TryEvaluateArithmetic(ExpressionToken[] rpn, RenderScope propMap, out decimal result)
        {
            result = 0m;
            if (rpn == null)
                return false;

            Stack<decimal> stack = new Stack<decimal>();
            bool hasNullOperand = false;
            for (int i = 0; i < rpn.Length; i++)
            {
                ExpressionToken token = rpn[i];
                switch (token.Kind)
                {
                    case ExpressionTokenKind.Number:
                        stack.Push(token.NumberValue);
                        break;
                    case ExpressionTokenKind.Identifier:
                        IdentifierResolveMode identifierResolveMode = TryResolveIdentifierToNumber(token.Path, propMap, out decimal identifierValue);
                        if (identifierResolveMode == IdentifierResolveMode.UnknownPath || identifierResolveMode == IdentifierResolveMode.NonNumeric || identifierResolveMode == IdentifierResolveMode.Ambiguous)
                            return false;

                        if (identifierResolveMode == IdentifierResolveMode.NullValue)
                            hasNullOperand = true;

                        stack.Push(identifierResolveMode == IdentifierResolveMode.NullValue ? 0m : identifierValue);
                        break;
                    case ExpressionTokenKind.UnaryMinus:
                        if (stack.Count < 1)
                            return false;
                        stack.Push(-stack.Pop());
                        break;
                    case ExpressionTokenKind.Operator:
                        if (stack.Count < 2)
                            return false;
                        decimal right = stack.Pop();
                        decimal left = stack.Pop();
                        switch (token.OperatorChar)
                        {
                            case '+':
                                stack.Push(left + right);
                                break;
                            case '-':
                                stack.Push(left - right);
                                break;
                            case '*':
                                stack.Push(left * right);
                                break;
                            case '/':
                                if (right == 0m)
                                    return false;
                                stack.Push(left / right);
                                break;
                            default:
                                return false;
                        }
                        break;
                    default:
                        return false;
                }
            }

            if (stack.Count != 1)
                return false;

            decimal computed = stack.Pop();
            result = hasNullOperand ? 0m : computed;
            return true;
        }

        private static IdentifierResolveMode TryResolveIdentifierToNumber(PropertyPath identifier, RenderScope propMap, out decimal number)
        {
            number = 0m;
            if (string.IsNullOrWhiteSpace(identifier.Text))
                return IdentifierResolveMode.UnknownPath;

            object singleValue = null;
            int count = 0;
            VisitPathValues(identifier, propMap, value =>
            {
                singleValue = value;
                count++;
            });
            if (count == 0) return IdentifierResolveMode.UnknownPath;
            if (count > 1) return IdentifierResolveMode.Ambiguous;

            if (singleValue == null)
                return IdentifierResolveMode.NullValue;

            if (!TryConvertToDecimal(singleValue, out decimal parsedValue))
                return IdentifierResolveMode.NonNumeric;

            number = parsedValue;
            return IdentifierResolveMode.Success;
        }

        private static bool TryTokenize(string expression, out List<ExpressionToken> tokens)
        {
            tokens = new List<ExpressionToken>();
            int i = 0;

            while (i < expression.Length)
            {
                char ch = expression[i];
                if (char.IsWhiteSpace(ch))
                {
                    i++;
                    continue;
                }

                if (char.IsDigit(ch) || (ch == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])))
                {
                    int start = i;
                    bool seenDot = false;
                    while (i < expression.Length)
                    {
                        char c = expression[i];
                        if (char.IsDigit(c))
                        {
                            i++;
                            continue;
                        }

                        if (c == '.' && !seenDot)
                        {
                            seenDot = true;
                            i++;
                            continue;
                        }

                        break;
                    }

                    string numberText = expression.Substring(start, i - start);
                    if (!decimal.TryParse(numberText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedNumber))
                        return false;

                    tokens.Add(ExpressionToken.Number(parsedNumber));
                    continue;
                }

                if (char.IsLetter(ch) || ch == '_')
                {
                    int start = i;
                    while (i < expression.Length)
                    {
                        char c = expression[i];
                        if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
                            i++;
                        else
                            break;
                    }

                    string identifier = expression.Substring(start, i - start).Trim();
                    if (identifier.Length == 0)
                        return false;

                    tokens.Add(ExpressionToken.Identifier(identifier));
                    continue;
                }

                if (ch == '+' || ch == '-' || ch == '*' || ch == '/')
                {
                    tokens.Add(ExpressionToken.Operator(ch));
                    i++;
                    continue;
                }

                if (ch == '(')
                {
                    tokens.Add(ExpressionToken.LeftParenthesis());
                    i++;
                    continue;
                }

                if (ch == ')')
                {
                    tokens.Add(ExpressionToken.RightParenthesis());
                    i++;
                    continue;
                }

                return false;
            }

            return tokens.Count > 0;
        }

        private static bool TryToRpn(List<ExpressionToken> tokens, out List<ExpressionToken> rpn)
        {
            rpn = new List<ExpressionToken>(tokens.Count);
            Stack<ExpressionToken> operators = new Stack<ExpressionToken>();
            ExpressionToken previousToken = default;
            bool hasPrevious = false;
            bool expectsOperand = true;

            for (int i = 0; i < tokens.Count; i++)
            {
                ExpressionToken token = tokens[i];
                if (token.Kind == ExpressionTokenKind.Number || token.Kind == ExpressionTokenKind.Identifier)
                {
                    if (!expectsOperand) return false;
                    expectsOperand = false;
                }
                else if (token.Kind == ExpressionTokenKind.LeftParenthesis)
                {
                    if (!expectsOperand) return false;
                }
                else if (token.Kind == ExpressionTokenKind.RightParenthesis)
                {
                    if (expectsOperand) return false;
                }
                else if (token.Kind == ExpressionTokenKind.Operator)
                {
                    if (expectsOperand && token.OperatorChar != '-') return false;
                    expectsOperand = true;
                }
                switch (token.Kind)
                {
                    case ExpressionTokenKind.Number:
                    case ExpressionTokenKind.Identifier:
                        rpn.Add(token);
                        break;
                    case ExpressionTokenKind.Operator:
                        bool isUnary = token.OperatorChar == '-' && (!hasPrevious || previousToken.Kind == ExpressionTokenKind.Operator || previousToken.Kind == ExpressionTokenKind.UnaryMinus || previousToken.Kind == ExpressionTokenKind.LeftParenthesis);

                        ExpressionToken currentOperator = isUnary ? ExpressionToken.UnaryMinus() : token;

                        while (operators.Count > 0 && IsOperatorToken(operators.Peek()))
                        {
                            ExpressionToken top = operators.Peek();
                            int currentPrecedence = GetPrecedence(currentOperator);
                            int topPrecedence = GetPrecedence(top);
                            bool currentRightAssociative = currentOperator.Kind == ExpressionTokenKind.UnaryMinus;

                            if ((!currentRightAssociative && currentPrecedence <= topPrecedence) ||
                                (currentRightAssociative && currentPrecedence < topPrecedence))
                            {
                                rpn.Add(operators.Pop());
                                continue;
                            }

                            break;
                        }

                        operators.Push(currentOperator);
                        break;
                    case ExpressionTokenKind.LeftParenthesis:
                        operators.Push(token);
                        break;
                    case ExpressionTokenKind.RightParenthesis:
                        bool foundLeft = false;
                        while (operators.Count > 0)
                        {
                            ExpressionToken top = operators.Pop();
                            if (top.Kind == ExpressionTokenKind.LeftParenthesis)
                            {
                                foundLeft = true;
                                break;
                            }

                            rpn.Add(top);
                        }

                        if (!foundLeft)
                            return false;
                        break;
                    default:
                        return false;
                }

                previousToken = token;
                hasPrevious = true;
            }

            while (operators.Count > 0)
            {
                ExpressionToken top = operators.Pop();
                if (top.Kind == ExpressionTokenKind.LeftParenthesis || top.Kind == ExpressionTokenKind.RightParenthesis)
                    return false;

                rpn.Add(top);
            }

            return !expectsOperand && rpn.Count > 0;
        }

        private static bool IsOperatorToken(ExpressionToken token)
        {
            return token.Kind == ExpressionTokenKind.Operator || token.Kind == ExpressionTokenKind.UnaryMinus;
        }

        private static int GetPrecedence(ExpressionToken token)
        {
            if (token.Kind == ExpressionTokenKind.UnaryMinus)
                return 3;
            if (token.Kind != ExpressionTokenKind.Operator)
                return 0;

            return token.OperatorChar == '*' || token.OperatorChar == '/' ? 2 : 1;
        }

        private enum IdentifierResolveMode
        {
            Success,
            UnknownPath,
            NullValue,
            NonNumeric,
            Ambiguous
        }

        private enum AggregateMode
        {
            Sum,
            Average,
            Min,
            Max
        }

        internal enum ExpressionTokenKind
        {
            Number,
            Identifier,
            Operator,
            LeftParenthesis,
            RightParenthesis,
            UnaryMinus
        }

        internal struct ExpressionToken
        {
            public ExpressionTokenKind Kind;
            public decimal NumberValue;
            public PropertyPath Path;
            public char OperatorChar;

            public static ExpressionToken Number(decimal value)
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.Number, NumberValue = value };
            }

            public static ExpressionToken Identifier(string value)
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.Identifier, Path = new PropertyPath(value) };
            }

            public static ExpressionToken Operator(char op)
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.Operator, OperatorChar = op };
            }

            public static ExpressionToken LeftParenthesis()
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.LeftParenthesis };
            }

            public static ExpressionToken RightParenthesis()
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.RightParenthesis };
            }

            public static ExpressionToken UnaryMinus()
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.UnaryMinus, OperatorChar = '-' };
            }
        }
    }
}
