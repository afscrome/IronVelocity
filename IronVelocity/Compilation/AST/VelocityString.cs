using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class VelocityString : VelocityExpression
    {
        public VelocityStringType StringType { get; private set; }
        public string Value { get; set; }

        public VelocityString(INode node)
            : base(node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!(node is ASTStringLiteral))
                throw new ArgumentOutOfRangeException("node");

            Value = node.Literal.Substring(1, node.Literal.Length - 2);

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
                    return new DictionaryString(Value);
                case VelocityStringType.Interpolated:
                    return new InterpolatedString(Value);
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

    }

    public enum VelocityStringType
    {
        Constant,
        Dictionary,
        Interpolated
    }
}
