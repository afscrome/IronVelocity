using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class SetDirective : BinaryExpression
    {
        public SetDirective(INode node)
            : base(node)
        {
        }

        public override Expression Reduce()
        {
            var left = Left;
            var right = Right;

            var reference = left as DynamicReference;
            if (reference != null)
            {
                left = reference.Reduce();
                var member = left as DynamicGetMemberExpression;
                if (member != null)
                {
                    return new DynamicSetMemberExpression(member.Name, member.Target, right);
                }
            }


            if (left.Type != right.Type)
            {
                //This shouldn't really be happening as we should only be assigning to objects, but just in case...
                if (!left.Type.IsAssignableFrom(right.Type))
                    throw new InvalidOperationException("Cannot assign from type '{0}' to '{1}'");

                right = Expression.Convert(right, left.Type);
            }
            if (left is VariableReference)
                left = left.Reduce();


            /* One of the nuances of velocity is that if the right evaluates to null,
                * Thus we can't simply return an assignment expression.
                * The resulting expression looks as follows:P
                *     .set $tempResult = right;
                *     .if ($tempResult != null){
                *         Assign(left, right)
                *     }
                */

            //However, if the expression is guaranteed to be a value type, why bother?
            if (right.Type.IsValueType)
                return Expression.Assign(left, right);

            var tempResult = Expression.Parameter(right.Type);
            return Expression.Block(new[] { tempResult },
                //Store the result of the right hand side in to a temporary variable
                Expression.Assign(tempResult, right),
                Expression.IfThen(
                //If the temporary variable is not equal to null
                    Expression.NotEqual(tempResult, Expression.Constant(null, right.Type)),
                //Make the assignment
                    Expression.Assign(left, tempResult)
                )
            );
        }

        public override Type Type { get { return typeof(void); } }
    }
}
