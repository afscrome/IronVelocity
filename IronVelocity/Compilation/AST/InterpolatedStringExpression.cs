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
        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object[]) }, null);


        public IReadOnlyList<Expression> Parts { get; set; }

        public override Type Type { get { return typeof(string); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.InterpolatedString; } }

        public InterpolatedStringExpression(params Expression[] parts)
        {
            Parts = parts.ToArray();
        }

        public override Expression Reduce()
        {
            if (Parts.Count == 0)
                return Expression.Constant("");
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
            //If we don't have any void expressions (i.e. Macros), use String.Concat as it produces less IL and so JIT compiles faster
            else if (Parts.All(x => x.Type != typeof(void) ))
            {
                var objParts = Parts.Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)));
                return Expression.Call(_stringConcatMethodInfo, Expression.NewArrayInit(typeof(object),objParts));

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
