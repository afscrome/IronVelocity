using NVelocity.Exception;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IronVelocity.Compilation.AST
{
    public class InterpolatedStringExpression : VelocityExpression
    {
        private readonly VelocityExpressionBuilder _builder;
        public IReadOnlyList<Expression> Parts { get; set; }

        public override Type Type { get { return typeof(string); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.InterpolatedString; } }

        public InterpolatedStringExpression(VelocityExpressionBuilder builder, params Expression[] parts)
        {
            Parts = parts.ToArray();
            _builder = builder;
        }

        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object[]) }, null);

        public override Expression Reduce()
        {
            if (Parts.Count == 1)
            {
                var element = Parts[0];
                if (element.Type != typeof(void))
                {
                    return element.Type == typeof(string)
                        ? element
                        : Expression.Call(element, MethodHelpers.ToStringMethodInfo);
                }
            }

            var outputParam = _builder.OutputParameter;
            var body = new RenderedBlock(Parts,_builder);

            var result = Expression.Block(
                    new[] { outputParam },
                    Expression.Assign(outputParam, Expression.New(typeof(StringBuilder))),
                    body,
                    Expression.Call(outputParam, "ToString", Type.EmptyTypes)
                );
            return result;
        }

        public InterpolatedStringExpression Update(params Expression[] parts)
        {
            return parts == Parts
                ? this
                : new InterpolatedStringExpression(_builder, parts);
        }

    }
}
