using NVelocity.Context;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.RuntimeHelpers
{
    public static class VelocityStrings
    {
        private const string DictStart = "%{";
        private const string DictEnd = "}";

        public static bool IsDictionaryString(string str)
        {
            return str.StartsWith(DictStart) && str.EndsWith(DictEnd);
        }


        /// <summary>
        /// Interpolates the dictionary string.
        /// dictionary string is any string in the format
        /// "%{ key='value' [,key2='value2' }"		
        /// "%{ key='value' [,key2='value2'] }"		
        /// </summary>
        /// <param name="str">If valid input a HybridDictionary with zero or more items,
        ///	otherwise the input string</param>
        /// <param name="context">NVelocity runtime context</param>
        public static Expression InterpolateDictionaryString(string str)
        {
            char[] contents = str.ToCharArray();
            int lastIndex;

            return RecursiveBuildDictionary(contents, 2, out lastIndex);
        }

        private static Expression RecursiveBuildDictionary(char[] contents, int fromIndex, out int lastIndex)
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
                            Expression nestedHash = RecursiveBuildDictionary(contents, i + 1, out i);
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
                    if (sbKeyBuilder.ToString().Trim() == String.Empty)
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

            //return hash;
            return VelocityExpressions.Dictionary(hash);
        }

        private static void ProcessDictEntry(IDictionary<string, Expression> map, StringBuilder keyBuilder, Expression value)
        {
            var key = keyBuilder.ToString().Trim();

            if (key.StartsWith("$"))
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
                                      bool isTextContent)
        {

            ProcessDictEntry(map, keyBuilder, Expression.Constant(value.ToString()));
            value.Length = 0;


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
