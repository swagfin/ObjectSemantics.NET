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
        private static readonly Regex FunctionRegex = new Regex(@"^\s*_*(?<fn>sum|avg|count|min|max|calc)\s*\(\s*(?<arg>.*)\s*\)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool TryEvaluate(string expressionCommand, object rootRecord, Dictionary<string, ExtractedObjProperty> propMap, out ExtractedObjProperty evaluatedProperty, out bool renderEmptyOnFailure, out bool isExpressionCommand)
        {
            evaluatedProperty = null;
            renderEmptyOnFailure = false;
            isExpressionCommand = false;
            if (string.IsNullOrWhiteSpace(expressionCommand))
                return false;

            Match match = FunctionRegex.Match(expressionCommand.Trim());
            if (!match.Success)
                return false;
            isExpressionCommand = true;

            string fn = match.Groups["fn"].Value.Trim().ToLowerInvariant();
            string arg = match.Groups["arg"].Value.Trim();

            switch (fn)
            {
                case "sum":
                    if (!TryAggregateNumeric(arg, rootRecord, propMap, AggregateMode.Sum, out decimal sum))
                    {
                        renderEmptyOnFailure = true;
                        return false;
                    }
                    evaluatedProperty = CreateDecimalProperty(expressionCommand, sum);
                    return true;

                case "avg":
                    if (!TryAggregateNumeric(arg, rootRecord, propMap, AggregateMode.Average, out decimal avg))
                    {
                        renderEmptyOnFailure = true;
                        return false;
                    }
                    evaluatedProperty = CreateDecimalProperty(expressionCommand, avg);
                    return true;

                case "count":
                    if (!TryCount(arg, rootRecord, propMap, out int count))
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
                    if (!TryAggregateNumeric(arg, rootRecord, propMap, AggregateMode.Min, out decimal min))
                    {
                        renderEmptyOnFailure = true;
                        return false;
                    }
                    evaluatedProperty = CreateDecimalProperty(expressionCommand, min);
                    return true;

                case "max":
                    if (!TryAggregateNumeric(arg, rootRecord, propMap, AggregateMode.Max, out decimal max))
                    {
                        renderEmptyOnFailure = true;
                        return false;
                    }
                    evaluatedProperty = CreateDecimalProperty(expressionCommand, max);
                    return true;

                case "calc":
                    if (!TryEvaluateArithmetic(arg, rootRecord, propMap, out decimal calcResult))
                    {
                        renderEmptyOnFailure = true;
                        return false;
                    }
                    evaluatedProperty = CreateDecimalProperty(expressionCommand, calcResult);
                    return true;
            }

            return false;
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

        private static bool TryCount(string path, object rootRecord, Dictionary<string, ExtractedObjProperty> propMap, out int count)
        {
            count = 0;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            List<object> values = ResolvePathValues(path, rootRecord, propMap);
            if (values.Count == 0)
                return false;

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] != null)
                    count++;
            }
            return true;
        }

        private static bool TryAggregateNumeric(string path, object rootRecord, Dictionary<string, ExtractedObjProperty> propMap, AggregateMode mode, out decimal result)
        {
            result = 0m;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            List<object> values = ResolvePathValues(path, rootRecord, propMap);

            if (values.Count == 0)
                return false;

            bool hasAny = false;
            bool hasInvalidNonNumeric = false;
            decimal running = 0m;
            int numericCount = 0;

            for (int i = 0; i < values.Count; i++)
            {
                object rawValue = values[i];
                if (rawValue == null)
                    continue;

                if (!TryConvertToDecimal(rawValue, out decimal numeric))
                {
                    hasInvalidNonNumeric = true;
                    continue;
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
            }

            if (hasInvalidNonNumeric)
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

        private static List<object> ResolvePathValues(string path, object rootRecord, Dictionary<string, ExtractedObjProperty> propMap)
        {
            List<object> empty = new List<object>();
            if (string.IsNullOrWhiteSpace(path) || propMap == null)
                return empty;

            string normalizedPath = path.Trim();
            if (propMap.TryGetValue(normalizedPath, out ExtractedObjProperty directProperty))
                return new List<object> { directProperty?.OriginalValue };

            int dotIndex = normalizedPath.IndexOf('.');
            string rootName = dotIndex >= 0 ? normalizedPath.Substring(0, dotIndex).Trim() : normalizedPath;
            string nestedPath = dotIndex >= 0 ? normalizedPath.Substring(dotIndex + 1).Trim() : string.Empty;

            if (!propMap.TryGetValue(rootName, out ExtractedObjProperty rootProperty))
                return empty;

            if (string.IsNullOrEmpty(nestedPath))
                return new List<object> { rootProperty?.OriginalValue };

            string[] segments = SplitPathSegments(nestedPath);
            if (segments.Length == 0)
                return new List<object> { rootProperty?.OriginalValue };

            List<object> currentValues = new List<object> { rootProperty?.OriginalValue };
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                List<object> nextValues = new List<object>();

                for (int j = 0; j < currentValues.Count; j++)
                    ExpandSegmentValues(currentValues[j], segment, nextValues);

                currentValues = nextValues;
                if (currentValues.Count == 0)
                    break;
            }

            return currentValues;
        }

        private static void ExpandSegmentValues(object current, string segment, List<object> nextValues)
        {
            if (string.IsNullOrWhiteSpace(segment))
                return;

            if (current == null)
            {
                nextValues.Add(null);
                return;
            }

            if (current is IEnumerable enumerable && !(current is string))
            {
                foreach (object item in enumerable)
                    ExpandSegmentValues(item, segment, nextValues);
                return;
            }

            Type type = current.GetType();
            if (!EngineTypeMetadataCache.TryGetPropertyAccessor(type, segment, out PropertyAccessor accessor))
                return;

            nextValues.Add(accessor.Getter(current));
        }

        private static string[] SplitPathSegments(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Array.Empty<string>();

            string[] raw = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (raw.Length == 0)
                return raw;

            int count = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                string trimmed = raw[i].Trim();
                if (trimmed.Length == 0)
                    continue;
                raw[count] = trimmed;
                count++;
            }

            if (count == raw.Length)
                return raw;

            string[] trimmedSegments = new string[count];
            Array.Copy(raw, trimmedSegments, count);
            return trimmedSegments;
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

        private static bool TryEvaluateArithmetic(string expression, object rootRecord, Dictionary<string, ExtractedObjProperty> propMap, out decimal result)
        {
            result = 0m;
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            if (!TryTokenize(expression, out List<ExpressionToken> tokens))
                return false;
            if (!TryToRpn(tokens, out List<ExpressionToken> rpn))
                return false;

            Stack<decimal> stack = new Stack<decimal>();
            bool hasNullOperand = false;
            for (int i = 0; i < rpn.Count; i++)
            {
                ExpressionToken token = rpn[i];
                switch (token.Kind)
                {
                    case ExpressionTokenKind.Number:
                        stack.Push(token.NumberValue);
                        break;
                    case ExpressionTokenKind.Identifier:
                        IdentifierResolveMode identifierResolveMode = TryResolveIdentifierToNumber(token.TextValue, rootRecord, propMap, out decimal identifierValue);
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

        private static IdentifierResolveMode TryResolveIdentifierToNumber(string identifier, object rootRecord, Dictionary<string, ExtractedObjProperty> propMap, out decimal number)
        {
            number = 0m;
            if (string.IsNullOrWhiteSpace(identifier))
                return IdentifierResolveMode.UnknownPath;

            List<object> values = ResolvePathValues(identifier, rootRecord, propMap);
            if (values.Count == 0)
                return IdentifierResolveMode.UnknownPath;

            if (values.Count > 1)
                return IdentifierResolveMode.Ambiguous;

            object singleValue = values[0];
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

            for (int i = 0; i < tokens.Count; i++)
            {
                ExpressionToken token = tokens[i];
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

            return rpn.Count > 0;
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

        private enum ExpressionTokenKind
        {
            Number,
            Identifier,
            Operator,
            LeftParenthesis,
            RightParenthesis,
            UnaryMinus
        }

        private struct ExpressionToken
        {
            public ExpressionTokenKind Kind;
            public decimal NumberValue;
            public string TextValue;
            public char OperatorChar;

            public static ExpressionToken Number(decimal value)
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.Number, NumberValue = value };
            }

            public static ExpressionToken Identifier(string value)
            {
                return new ExpressionToken { Kind = ExpressionTokenKind.Identifier, TextValue = value };
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
