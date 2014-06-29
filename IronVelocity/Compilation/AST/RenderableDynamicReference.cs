using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class RenderableDynamicReference : VelocityExpression
    {
        public DynamicReference Reference { get; private set; }

        public RenderableDynamicReference(DynamicReference reference)
        {
            Reference = reference;
        }

        public override Expression Reduce()
        {
            if (Reference.Metadata.Escaped)
            {
                return Expression.Condition(
                    Expression.NotEqual(Reference, Expression.Constant(null, Reference.Type)),
                    Expression.Constant(Reference.Metadata.EscapePrefix + Reference.Metadata.NullString),
                    Expression.Constant(Reference.Metadata.EscapePrefix + "\\" + Reference.Metadata.NullString)
                );
            }
            else
            {
                var prefix = Reference.Metadata.EscapePrefix + Reference.Metadata.MoreString;
                var NullValue = Expression.Constant(Reference.Metadata.EscapePrefix + prefix + Reference.Metadata.NullString);

                //If the literal has not been escaped (has an empty prefix), then we can return a simple Coalesce expression
                if (String.IsNullOrEmpty(prefix))
                    return Expression.Coalesce(Reference, NullValue);

                //Otherwise we have to do a slightly more complicated result
                var _evaulatedResult = Expression.Parameter(typeof(object), "tempEvaulatedResult");
                return Expression.Block(
                    new[] { _evaulatedResult },
                    Expression.Assign(_evaulatedResult, Reference),
                    Expression.Condition(
                        Expression.NotEqual(_evaulatedResult, Expression.Constant(null, _evaulatedResult.Type)),
                        Expression.Call(
                            MethodHelpers.StringConcatMethodInfo,
                            Expression.Convert(Expression.Constant(prefix), typeof(object)),
                            VelocityExpressions.ConvertIfNeeded(_evaulatedResult, typeof(object))
                        ),
                        NullValue
                    )
                );
            }
        }

        //public override Type Type { get { return typeof(string); } }
    }

}
