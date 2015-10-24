using IronVelocity.Binders;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public class ComparisonExpression : VelocityBinaryExpression
    {
        private readonly VelocityComparisonOperationBinder _binder;
        public ComparisonOperation Operation => _binder.Operation;

        public override Type Type => _binder.ReturnType;
        public override VelocityExpressionType VelocityExpressionType => VelocityExpressionType.Comparison;

        public ComparisonExpression(Expression left, Expression right, SourceInfo sourceInfo, VelocityComparisonOperationBinder binder)
            : base(left, right, sourceInfo)
        {
            _binder = binder;
        }

        public override Expression Reduce()
        {
            return Expression.Dynamic(
                _binder,
                _binder.ReturnType,
                Left,
                Right
            );
        }

        public override VelocityBinaryExpression Update(Expression left, Expression right)
        {
            if (Left == left && Right == right)
                return this;
            else
                return new ComparisonExpression(left, right, SourceInfo, _binder);
        }
    }
}
