using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class SetDirective : VelocityBinaryExpression
    {
        private static readonly ParameterExpression _objectTemp = Expression.Parameter(typeof(object), "setDirectiveTemp");

        public override Type Type { get { return typeof(void); } }
        public override VelocityExpressionType VelocityExpressionType { get { return VelocityExpressionType.SetDirective; } }


        public SetDirective(Expression left, Expression right, SourceInfo sourceInfo)
            : base(left, right, sourceInfo)
        {
        }


        public override VelocityBinaryExpression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;

            if (left is GlobalVariableExpression)
                throw new NotSupportedException("Cannot assign to a global variable");

            return new SetDirective(left, right, SourceInfo);
        }
        

        public override Expression Reduce()
        {
            var left = Left;
            var right = Right;

            if (left is ReferenceExpression)
                left = left.Reduce();
  
            var getMember = left as PropertyAccessExpression;
            if (getMember != null)
            {
                    return new SetMemberExpression(getMember.Name, getMember.Target, right);
            }

            bool rightIsNullableType = ReflectionHelper.IsNullableType(right.Type);

            if (left.Type != right.Type)
            {
                //This shouldn't really be happening as we should only be assigning to objects, but just in case...
                if (!left.Type.IsAssignableFrom(right.Type))
                    throw new InvalidOperationException(String.Format("Cannot assign from type '{0}' to '{1}'", left.Type, right.Type));

                right = VelocityExpressions.ConvertIfNeeded(right, left.Type);
            }


            bool isVariableExpression = left is VariableExpression;
            if (isVariableExpression)
                left = left.Reduce();


            /* One of the nuances of velocity is that if the right evaluates to null,
                * Thus we can't simply return an assignment expression.
                * The resulting expression looks as follows:P
                *     .set $tempResult = right;
                *     .if ($tempResult != null){
                *         Assign(left, right)
                *     }
                */

            //However, if the expression is guaranteed to be a value type (i.e. not nullable), why bother?
            //Similarly if it's a variable expression, the null handling is handled in side the setter
            if (isVariableExpression || !rightIsNullableType)
                return Expression.Block(typeof(void), Expression.Assign(left, right));

            var tempResult = right.Type == typeof(object)
                ? _objectTemp
                : Expression.Parameter(right.Type, "setDirectiveTemp");
            return Expression.Block(new[] { tempResult },
                Expression.IfThen(
                //Store the result of the right hand side in to a temporary variable
                //If the temporary variable is not equal to null
                    Expression.NotEqual(Expression.Assign(tempResult, right), Expression.Constant(null, right.Type)),
                //Make the assignment
                    Expression.Assign(left, tempResult)
                )
            );
        }

    }
}
