using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class VelocityString : VelocityExpression
    {
        private readonly VelocityASTConverter _converter;
        public VelocityStringType StringType { get; private set; }
        public string Value { get; set; }

        public VelocityString(INode node, VelocityASTConverter converter)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException("node");

            Value = node.Literal.Substring(1, node.Literal.Length - 2);
            _converter = converter;

            var isDoubleQuoted = node.Literal.StartsWith("\"", StringComparison.Ordinal);
            StringType = isDoubleQuoted
                ? DetermineStringType(Value)
                : VelocityStringType.Constant;
        }

        protected override Expression ReduceInternal()
        {
            switch (StringType)
            {
                case VelocityStringType.Constant:
                    return Expression.Constant(Value);
                case VelocityStringType.Dictionary:
                    return new DictionaryString(Value, _converter);
                case VelocityStringType.Interpolated:
                    return new InterpolatedString(Value, _converter);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static VelocityStringType DetermineStringType(string value)
        {
            if (value == null)
                return VelocityStringType.Constant;
            if (value.StartsWith("%{", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal))
                return VelocityStringType.Dictionary;
            if (value.IndexOfAny(new[] { '$', '#' }) != -1)
                return VelocityStringType.Interpolated;
            else
                return VelocityStringType.Constant;
        }

        public override string ToString()
        {
            return Value;
        }

        private string DebugView { get { return "todo"; } }

    }

    public enum VelocityStringType
    {
        Constant,
        Dictionary,
        Interpolated
    }
}
