using NVelocity.Exception;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IronVelocity.RuntimeHelpers
{
    /// <remarks>
    /// The String Dictionary & Interpolation ("%{key=value,key2=value}" & "hello $name" respectively)
    /// appear to be NVelocity specific.  Rather than being properly supported in the parser's grammar,
    /// they were implemented as hacks on ASTStringLiteral.
    /// 
    /// I would love to be able to redefine the grammar and hence parser to include these properly in the AST
    /// but for now I'm keeping the code as similar as possible to the NVelocity hack, only making minimal changes
    /// (i.e. returning Expressions building a dictionary, rather than an actual dictionary object)
    /// </remarks>
    public static class VelocityStrings
    {
        private const string DictStart = "%{";
        private const string DictEnd = "}";


        private static MethodInfo _escapeQuoteMethodInfo = typeof(VelocityStrings).GetMethod("EscapeQuotes", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object), typeof(char) }, null);

        private static string EscapeQuotes(object obj, char quoteChar)
        {
            var type = obj.GetType();
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return string.Concat(quoteChar, obj.ToString().Replace(quoteChar.ToString(), "\\" + quoteChar), quoteChar);
            else
                return null;

        }

        public static Expression EscapeSingleQuote(Expression content)
        {
            return Expression.Call(
                _escapeQuoteMethodInfo,
                VelocityExpressions.ConvertIfNeeded(content, typeof(object)),
                Expression.Constant('\'')
            );
        }

        public static Expression EscapeDoubleQuote(Expression content)
        {
            return Expression.Call(
                _escapeQuoteMethodInfo,
                VelocityExpressions.ConvertIfNeeded(content, typeof(object)),
                Expression.Constant('"')
            );
        }

        public enum StringType
        {
            Constant,
            Dictionary,
            Interpolated
        }

        public static StringType DetermineStringType(string value)
        {
            if (value == null)
                return StringType.Constant;
            if (value.StartsWith(DictStart, StringComparison.OrdinalIgnoreCase) && value.EndsWith(DictEnd, StringComparison.OrdinalIgnoreCase))
                return StringType.Dictionary;
            if (value.IndexOfAny(new[] { '$', '#' }) != -1)
                return StringType.Interpolated;
            else
                return StringType.Constant;
        }


        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object[]) }, null);
        public static Expression InterpolateString(string value, VelocityASTConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            //TODO; Refactor to share with VelocityExpressionTreeBuilder, or reuse the same parser
            var parser = new NVelocity.Runtime.RuntimeInstance().CreateNewParser();
            using (var reader = new System.IO.StringReader(value))
            {
                SimpleNode ast;
                try
                {
                    ast = parser.Parse(reader, null);
                }
                catch (ParseErrorException)
                {
                    ast = null;
                }

                //If we fail to parse, the ast returned will be null, so just return our normal string
                if (ast == null)
                    return Expression.Constant(value);

                var expressions = converter.GetBlockExpressions(ast, false)
                    .Where(x => x.Type != typeof(void))
                    .ToArray();

                if (expressions.Length == 1)
                    return expressions[0];
                else
                    return Expression.Call(_stringConcatMethodInfo, Expression.NewArrayInit(typeof(object), expressions));
            }
        }


        /// <summary>
        /// Interpolates the dictionary string.
        /// dictionary string is any string in the format
        /// "%{ key='value' [,key2='value2' }"		
        /// "%{ key='value' [,key2='value2'] }"		
        /// </summary>
        /// <param name="value">If valid input a HybridDictionary with zero or more items,
        ///	otherwise the input string</param>
        /// <param name="context">NVelocity runtime context</param>
        public static Expression InterpolateDictionaryString(string value, VelocityASTConverter converter)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            char[] contents = value.ToCharArray();
            int lastIndex;

            return RecursiveBuildDictionary(contents, 2, out lastIndex, converter);
        }

        private static Expression RecursiveBuildDictionary(char[] contents, int fromIndex, out int lastIndex, VelocityASTConverter converter)
        {
            // key=val, key='val', key=$val, key=${val}, key='id$id'

            lastIndex = 0;

            var hash = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);

            bool inKey, valueStarted, expectSingleCommaAtEnd, inTransition;
            int inEvaluationContext = 0;
            inKey = false;
            inTransition = true;
            valueStarted = expectSingleCommaAtEnd = false;
            StringBuilder sbKeyBuilder = new StringBuilder();
            StringBuilder sbValBuilder = new StringBuilder();

            for (int i = fromIndex; i < contents.Length; i++)
            {
                char c = contents[i];

                if (inTransition)
                {
                    // Eat all insignificant chars
                    if (c == ',' || c == ' ')
                    {
                        continue;
                    }
                    else if (c == '}') // Time to stop
                    {
                        lastIndex = i;
                        break;
                    }
                    else
                    {
                        inTransition = false;
                        inKey = true;
                    }
                }

                if (c == '=' && inKey)
                {
                    inKey = false;
                    valueStarted = true;
                    continue;
                }

                if (inKey)
                {
                    sbKeyBuilder.Append(c);
                }
                else
                {
                    if (valueStarted && c == ' ') continue;

                    if (valueStarted)
                    {
                        valueStarted = false;

                        if (c == '\'')
                        {
                            expectSingleCommaAtEnd = true;
                            continue;
                        }
                        else if (c == '{')
                        {
                            Expression nestedHash = RecursiveBuildDictionary(contents, i + 1, out i, converter);
                            ProcessDictEntry(hash, sbKeyBuilder, nestedHash);
                            inKey = false;
                            valueStarted = false;
                            inTransition = true;
                            expectSingleCommaAtEnd = false;
                            continue;
                        }
                    }

                    if (c == '\\')
                    {
                        char ahead = contents[i + 1];

                        // Within escape

                        switch (ahead)
                        {
                            case 'r':
                                i++;
                                sbValBuilder.Append('\r');
                                continue;
                            case '\'':
                                i++;
                                sbValBuilder.Append('\'');
                                continue;
                            case '"':
                                i++;
                                sbValBuilder.Append('"');
                                continue;
                            case 'n':
                                i++;
                                sbValBuilder.Append('\n');
                                continue;
                        }
                    }

                    if ((c == '\'' && expectSingleCommaAtEnd) ||
                        (!expectSingleCommaAtEnd && c == ',') ||
                        (inEvaluationContext == 0 && c == '}'))
                    {
                        ProcessDictEntry(hash, sbKeyBuilder, sbValBuilder, expectSingleCommaAtEnd, converter);

                        inKey = false;
                        valueStarted = false;
                        inTransition = true;
                        expectSingleCommaAtEnd = false;

                        if (inEvaluationContext == 0 && c == '}')
                        {
                            lastIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        if (c == '{')
                        {
                            inEvaluationContext++;
                        }
                        else if (inEvaluationContext != 0 && c == '}')
                        {
                            inEvaluationContext--;
                        }

                        sbValBuilder.Append(c);
                    }
                }

                if (i == contents.Length - 1)
                {
                    if (String.IsNullOrWhiteSpace(sbKeyBuilder.ToString()))
                    {
                        break;
                    }

                    lastIndex = i;

                    ProcessDictEntry(hash, sbKeyBuilder, sbValBuilder, expectSingleCommaAtEnd, converter);

                    inKey = false;
                    valueStarted = false;
                    inTransition = true;
                    expectSingleCommaAtEnd = false;
                }
            }

            //return hash;
            return VelocityExpressions.Dictionary(hash);
        }

        private static void ProcessDictEntry(IDictionary<string, Expression> map, StringBuilder keyBuilder, Expression value)
        {
            var key = keyBuilder.ToString().Trim();

            if (key.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Dictionary keys must be strings");
                /*
                object keyVal = EvaluateInPlace(key.ToString());

                if (keyVal == null)
                {
                    throw new ArgumentException(
                        string.Format("The dictionary entry {0} evaluated to null, but null is not a valid dictionary key", key));
                }

                key = keyVal;*/
            }

            map[key] = value;

            keyBuilder.Length = 0;
        }

        private static void ProcessDictEntry(IDictionary<string, Expression> map,
                                      StringBuilder keyBuilder, StringBuilder value,
                                      bool isTextContent, VelocityASTConverter converter)
        {
            Expression expr;
            var content = value.ToString();
            if (DetermineStringType(content) == StringType.Interpolated)
                expr = InterpolateString(content, converter);
            else
            {
                if (isTextContent)
                {
                    expr = Expression.Constant(content);
                }
                else
                {
                    if (content.Contains('.'))
                    {
                        try
                        {
                            expr = Expression.Constant(Convert.ToSingle(content, CultureInfo.InvariantCulture));
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException(
                                string.Format(CultureInfo.InvariantCulture,
                                    "Could not convert dictionary value for entry {0} with value {1} to Single. If the value is supposed to be a string, it must be enclosed with '' (single quotes)",
                                    keyBuilder, content));
                        }
                    }
                    else
                    {
                        try
                        {
                            expr = Expression.Constant(Convert.ToInt32(content, CultureInfo.InvariantCulture));
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException(
                                string.Format(CultureInfo.InvariantCulture,
                                    "Could not convert dictionary value for entry {0} with value {1} to Int32. If the value is supposed to be a string, it must be enclosed with '' (single quotes)",
                                    keyBuilder, content));
                        }
                    }
                }
            }

            ProcessDictEntry(map, keyBuilder, expr);
            value.Length = 0;
            //If contains $, evaluate
            // else if not text content
            //{
            //    If contains .
            //           try parse as single
            //    else
            //          try parse as int
            //}

            /*
            object val = value.ToString().Trim();

            // Is it a reference?
            if (val.ToString().StartsWith("$") || val.ToString().IndexOf('$') != -1)
            {
                val = EvaluateInPlace(val.ToString());
            }
            else if (!isTextContent)
            {
                // Is it a Int32 or Single?

                if (val.ToString().IndexOf('.') == -1)
                {
                    try
                    {
                        val = Convert.ToInt32(val);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Could not convert dictionary value for entry {0} with value {1} to Int32. If the value is supposed to be a string, it must be enclosed with '' (single quotes)",
                                keyBuilder, val));
                    }
                }
                else
                {
                    try
                    {
                        val = Convert.ToSingle(val);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Could not convert dictionary value for entry {0} with value {1} to Single. If the value is supposed to be a string, it must be enclosed with '' (single quotes)",
                                keyBuilder, val));
                    }
                }
            }

            */
            // Reset buffers

        }

        /*
        private Expression EvaluateInPlace(string content)
        {
            throw new NotImplementedException();
        }

        private object Evaluate(SimpleNode inlineNode, IInternalContextAdapter context)
        {
            if (inlineNode.ChildrenCount == 1)
            {
                INode child = inlineNode.GetChild(0);
                return child.Value(context);
            }
            else
            {
                StringBuilder result = new StringBuilder();

                for (int i = 0; i < inlineNode.ChildrenCount; i++)
                {
                    INode child = inlineNode.GetChild(i);

                    if (child.Type == ParserTreeConstants.REFERENCE)
                    {
                        result.Append(child.Value(context));
                    }
                    else
                    {
                        result.Append(child.Literal);
                    }
                }

                return result.ToString();
            }
        }
        */

    }
}
