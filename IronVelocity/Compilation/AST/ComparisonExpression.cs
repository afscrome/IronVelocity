using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ComparisonExpression : VelocityBinaryExpression
    {
        public ComparisonOperation Operation { get; }

        public override Type Type => typeof(bool);
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.Comparison;

        public ComparisonExpression(Expression left, Expression right, SourceInfo sourceInfo, ComparisonOperation op)
            : base(left, right, sourceInfo)
        {
            Operation = op;
        }

        public override Expression Reduce()
        {
            var binder = BinderHelper.Instance.GetComparisonOperationBinder(Operation);

            return Expression.Dynamic(
                binder,
                binder.ReturnType,
                Left,
                Right
            );
        }

        public override VelocityBinaryExpression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;
            else
                return new ComparisonExpression(left, right, SourceInfo, Operation);
        }
    }
}
