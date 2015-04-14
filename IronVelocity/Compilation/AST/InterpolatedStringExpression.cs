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
        public IReadOnlyList<Expression> Parts { get; set; }

        public override Type Type { get { return typeof(string); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.InterpolatedString; } }

        public InterpolatedStringExpression(params Expression[] parts)
        {
            Parts = parts.ToArray();
        }

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

            //Create a new scope, in which the Output parameter points to a different StringBuilder
            //So we can get the result, without writing it to the output stream.
            var outputParam = Constants.OutputParameter;

            return Expression.Block(
                    new[] { outputParam },
                    Expression.Assign(outputParam, Expression.New(typeof(StringBuilder))),
                    new RenderedBlock(Parts),
                    Expression.Call(outputParam, "ToString", Type.EmptyTypes)
                );
        }

        public InterpolatedStringExpression Update(params Expression[] parts)
        {
            return parts == Parts
                ? this
                : new InterpolatedStringExpression(parts);
        }

    }
}
