using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.AST
{
    public class InterpolatedStringExpression : VelocityExpression
    {
        public IImmutableList<Expression> Parts { get; set; }

        public override Type Type => typeof(string);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.InterpolatedString;

        public InterpolatedStringExpression(IImmutableList<Expression> parts)
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
                    Expression nullValue = null;
                    var reference = element as ReferenceExpression;
                    if (reference !=  null && !reference.IsSilent)
                        nullValue = Expression.Constant(reference.Raw);

                    if (element.Type == typeof(string))
                        return element;

                    var toStringExpr = Expression.Call(element, MethodHelpers.ToStringMethodInfo);

                    if (!TypeHelper.IsNullableType(element.Type))
                        return toStringExpr;

                    return Expression.Condition(
                        Expression.Equal(element, Expression.Default(element.Type))
                        , nullValue ?? Expression.Constant(string.Empty)
                        , toStringExpr);
                }
            }
            //If we don't have any void expressions (i.e. Macros), use String.Concat as it produces less IL and so JIT compiles faster
            //TODO: This optimisation doesn't work as it fails with unsilenced null references.
            /*
            else if (Parts.All(x => x.Type != typeof(void) ))
            {
                var objParts = Parts.Select(x => VelocityExpressions.ConvertIfNeeded(x, typeof(object)));
                return Expression.Call(_stringConcatMethodInfo, Expression.NewArrayInit(typeof(object),objParts));

            }*/

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

        public InterpolatedStringExpression Update(ImmutableList<Expression> parts)
        {
            return parts == Parts
                ? this
                : new InterpolatedStringExpression(parts);
        }
    }
}
