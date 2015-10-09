using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.AST
{
    public class InterpolatedStringExpression : VelocityExpression
    {
        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object[]) }, null);


        public IReadOnlyList<Expression> Parts { get; set; }

        public override Type Type => typeof(string);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.InterpolatedString;

        public InterpolatedStringExpression(IReadOnlyList<Expression> parts)
        {
            Parts = parts;
        }

        public override Expression Reduce()
        {
            if (Parts.Count == 0)
                return Expression.Constant(string.Empty);
            if (Parts.Count == 1)
            {
                var element = Parts[0];
                if (element.Type != typeof(void))
                {
                    if (element.Type == typeof(string))
                        return element;

                    var toStringExpr = Expression.Call(element, MethodHelpers.ToStringMethodInfo);

                    if (element.Type.IsValueType)
                        return toStringExpr;

                    return Expression.Condition(
                        Expression.Equal(element, Expression.Default(element.Type))
                        , Expression.Constant(string.Empty)
                        , toStringExpr);
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

            return new TemporaryVariableScopeExpression(
                    outputParam,
                    Expression.Block(
                        Expression.Assign(outputParam, Expression.New(Constants.OutputParameter.Type)),
                        new RenderedBlock(Parts),
                        Expression.Call(outputParam, "ToString", Type.EmptyTypes)
                    )
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
