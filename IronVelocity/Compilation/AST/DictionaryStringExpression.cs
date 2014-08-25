﻿using IronVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IronVelocity.Compilation.AST
{
    public class DictionaryStringExpression : VelocityExpression
    {
        public string Value { get; set; }
        public override Type Type { get { return typeof(RuntimeDictionary); } }

        public DictionaryStringExpression(string value)
        {
            Value = value;
        }

        public override Expression Reduce()
        {
            char[] contents = Value.ToCharArray();
            int lastIndex;
            return RecursiveBuildDictionary(contents, 2, out lastIndex);
        }

        private DictionaryExpression RecursiveBuildDictionary(char[] contents, int fromIndex, out int lastIndex)
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
                            var value = RecursiveBuildDictionary(contents, i + 1, out i);
                            ProcessDictEntry(hash, sbKeyBuilder, value);
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
                        ProcessDictEntry(hash, sbKeyBuilder, sbValBuilder, expectSingleCommaAtEnd);

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

                    ProcessDictEntry(hash, sbKeyBuilder, sbValBuilder, expectSingleCommaAtEnd);

                    inKey = false;
                    valueStarted = false;
                    inTransition = true;
                    expectSingleCommaAtEnd = false;
                }
            }

            return new DictionaryExpression(hash);
        }

        private static void ProcessDictEntry(IDictionary<string, Expression> map, StringBuilder keyBuilder, Expression value)
        {
            var key = keyBuilder.ToString().Trim();

            if (key.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotImplementedException("Dictionary keys must be constants");
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
                                      bool isTextContent)
        {
            Expression expr;
            var content = value.ToString().Trim();
            if (content.Contains('$'))
            {
                var interpolated = new InterpolatedStringExpression(content);
                if (isTextContent)
                    expr = interpolated;
                else if (interpolated.Parts.Count == 1)
                    expr = interpolated.Parts.First();
                else
                    throw new InvalidOperationException();
            }
            else
            {
                if (isTextContent)
                {
                    expr = Expression.Constant(content);
                }
                else
                {
                    content = content.ToString();
                    if (content.Contains('.'))
                    {
                        try
                        {
                            expr = Expression.Constant(System.Convert.ToSingle(content, CultureInfo.InvariantCulture));
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
                            expr = Expression.Constant(System.Convert.ToInt32(content, CultureInfo.InvariantCulture));
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


    }
}
