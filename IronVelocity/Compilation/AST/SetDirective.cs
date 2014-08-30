using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class SetDirective : VelocityBinaryExpression
    {
        public override Type Type { get { return typeof(void); } }


        public SetDirective(Expression left, Expression right, SymbolInformation symbols)
            : base(left,right,symbols)
        {
        }


        public override VelocityBinaryExpression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;

            if (left is GlobalVariableExpression)
                throw new NotSupportedException("Cannot assign to a global variable");

            return new SetDirective(left, right, Symbols);
        }
        

        public override Expression Reduce()
        {
            var left = Left;
            var right = Right;

  
            var getMember = left as PropertyAccessExpression;
            if (getMember != null)
            {
                    return new SetMemberExpression(getMember.Name, getMember.Target, right);
            }

            if (left.Type != right.Type)
            {
                //This shouldn't really be happening as we should only be assigning to objects, but just in case...
                if (!left.Type.IsAssignableFrom(right.Type))
                    throw new InvalidOperationException("Cannot assign from type '{0}' to '{1}'");

                right = VelocityExpressions.ConvertIfNeeded(right, left.Type);
            }

            if (left is ReferenceExpression)
                left = left.Reduce();

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
            if (isVariableExpression || !ReflectionHelper.IsNullableType(right.Type))
                return Expression.Block(typeof(void), Expression.Assign(left, right));

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

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            var left = visitor.Visit(Left);
            if (left is GlobalVariableExpression)
                throw new InvalidOperationException("Cannot assign to a global variable");

            var right = visitor.Visit(Right);

            return Update(left, right);
        }
    }
}
